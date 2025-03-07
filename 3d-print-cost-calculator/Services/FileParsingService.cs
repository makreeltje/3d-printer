using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using ThreeDPrintCostCalculator.Models;

namespace ThreeDPrintCostCalculator.Services
{

    public interface IFileParsingService
    {
        Task<ThreeMFModel> Parse3MFFileAsync(string filePath);
        Task<ThreeMFModel> Parse3MFFileAsync(Stream fileStream);
        Task<bool> Validate3MFFileAsync(Stream fileStream);
    }

    public class FileParsingService : IFileParsingService
    {
        private readonly ILogger<FileParsingService> _logger;
        private const string ModelPath = "3D/3dmodel.model";
        private const string Namespace3MF = "http://schemas.microsoft.com/3dmanufacturing/core/2015/02";
        private const string NamespaceProduction = "http://schemas.microsoft.com/3dmanufacturing/production/2015/06";
        private const string BambuStudioNamespace = "http://schemas.bambulab.com/package/2021";

        public FileParsingService(ILogger<FileParsingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ThreeMFModel> Parse3MFFileAsync(string filePath)
        {
            _logger.LogInformation("Parsing 3MF file from {FilePath}", filePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            try
            {
                using var fileStream = File.OpenRead(filePath);
                var model = await Parse3MFFileAsync(fileStream);
                
                // Set the filename property based on the file path
                model.FileName = Path.GetFileName(filePath);
                
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing 3MF file from path {FilePath}", filePath);
                throw;
            }
        }

        public async Task<ThreeMFModel> Parse3MFFileAsync(Stream fileStream)
        {
            _logger.LogInformation("Parsing 3MF file from stream");

            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            try
            {
                using var archive = new ZipArchive(new PositionRestoringStream(fileStream), ZipArchiveMode.Read, true);
                
                // Create model instance
                var model = new ThreeMFModel
                {
                    // Set default values
                    Name = "Unnamed 3D Model",
                    ModelWarnings = "No warnings detected"
                };
                
                // Parse model file
                await ParseModelFileAsync(archive, model);
                
                // Parse metadata file if available
                await ParseMetadataFileAsync(archive, model);
                
                // Parse materials if any (previously components)
                await ParseComponentsAsync(archive, model);
                
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing 3MF file from stream");
                throw;
            }
        }

        public async Task<bool> Validate3MFFileAsync(Stream fileStream)
        {
            _logger.LogInformation("Validating 3MF file");

            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            try
            {
                using var archive = new ZipArchive(new PositionRestoringStream(fileStream), ZipArchiveMode.Read, true);
                var modelEntry = archive.GetEntry(ModelPath);
                
                if (modelEntry == null)
                {
                    _logger.LogWarning("Model file not found in 3MF archive");
                    return false;
                }

                using var modelStream = modelEntry.Open();
                var modelDoc = await XDocument.LoadAsync(modelStream, LoadOptions.None, default);
                
                // Get the root element
                if (modelDoc.Root == null)
                {
                    _logger.LogWarning("Root element not found in 3MF model file");
                    return false;
                }
                
                // Get the default namespace as a string
                string defaultNamespace = modelDoc.Root.GetDefaultNamespace().NamespaceName;
                _logger.LogInformation("Found default 3MF namespace: {Namespace}", defaultNamespace);
                
                // Log all xmlns attributes to help with debugging
                var allNamespaces = modelDoc.Root.Attributes()
                    .Where(a => a.IsNamespaceDeclaration)
                    .Select(a => $"{(string.IsNullOrEmpty(a.Name.LocalName) ? "default" : a.Name.LocalName)}: {a.Value}");
                
                _logger.LogInformation("All declared namespaces: {Namespaces}", string.Join(", ", allNamespaces));
                
                // Check if the default namespace matches Namespace3MF exactly
                bool isValid = string.Equals(defaultNamespace, Namespace3MF, StringComparison.OrdinalIgnoreCase);
                
                // Optionally check for BambuStudio namespace but don't require it
                bool hasBambuStudioNamespace = modelDoc.Root.Attributes()
                    .Where(a => a.IsNamespaceDeclaration)
                    .Any(a => string.Equals(a.Value, BambuStudioNamespace, StringComparison.OrdinalIgnoreCase));
                
                if (hasBambuStudioNamespace)
                {
                    _logger.LogInformation("BambuStudio namespace found");
                }
                
                if (!isValid)
                {
                    _logger.LogWarning("Invalid 3MF namespace in model file. Expected: {ExpectedNamespace}, Found: {ActualNamespace}", 
                                       Namespace3MF, defaultNamespace);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating 3MF file");
                return false;
            }
        }

        private async Task ParseModelFileAsync(ZipArchive archive, ThreeMFModel model)
        {
            var modelEntry = archive.GetEntry(ModelPath);
            if (modelEntry == null)
            {
                throw new InvalidOperationException("Model file not found in 3MF archive");
            }

            using var modelStream = modelEntry.Open();
            var modelDoc = await XDocument.LoadAsync(modelStream, LoadOptions.None, default);
            
            // Parse basic model information
            var resources = modelDoc.Root?.Element(XName.Get("resources", Namespace3MF));
            if (resources == null)
            {
                throw new InvalidOperationException("Resources element not found in model file");
            }

            // Parse object dimensions if available
            var objects = resources.Elements(XName.Get("object", Namespace3MF));
            foreach (var obj in objects)
            {
                CalculateObjectDimensions(obj, model);
                break; // Only process the first object for now
            }

            // Try to extract metadata from the model file
            var metadata = modelDoc.Root?
                .Elements(XName.Get("metadata", Namespace3MF))
                .ToDictionary(
                    x => x.Attribute("name")?.Value ?? "unknown",
                    x => x.Value);

            if (metadata != null && metadata.Any())
            {
                // Extract common metadata values if available
                if (metadata.TryGetValue("PrintTime", out var printTimeStr) && 
                    double.TryParse(printTimeStr, out var printTime))
                {
                    model.EstimatedPrintTime = printTime;
                }

                if (metadata.TryGetValue("FilamentUsed", out var filamentStr) && 
                    double.TryParse(filamentStr, out var filament))
                {
                    model.EstimatedMaterialUsage = filament;
                }

                if (metadata.TryGetValue("Name", out var name))
                {
                    model.Name = name;
                }
                
                // Add other relevant metadata to model properties
                if (metadata.TryGetValue("LayerHeight", out var layerHeightStr) &&
                    double.TryParse(layerHeightStr, out var layerHeight))
                {
                    model.RecommendedLayerHeight = layerHeight;
                }
                
                if (metadata.TryGetValue("Units", out var units))
                {
                    model.Units = units;
                }
                
                // Add any warnings to ModelWarnings
                if (metadata.TryGetValue("Warnings", out var warnings) && !string.IsNullOrEmpty(warnings))
                {
                    model.ModelWarnings = warnings;
                }
            }
        }

        private void CalculateObjectDimensions(XElement objElement, ThreeMFModel model)
        {
            // In a real implementation, this would parse the mesh data to calculate dimensions
            // Here we just set some placeholder values
            model.Width = 100;
            model.Height = 100;
            model.Depth = 100;
            model.Volume = 1000000;
            
            // Calculate surface area
            model.SurfaceArea = CalculateSurfaceArea(objElement);
        }

        private double CalculateSurfaceArea(XElement objElement)
        {
            // In a real implementation, this would calculate the surface area from the mesh
            // Here we just return a placeholder value
            return 60000; // 6 sides of 100x100
        }

        private async Task ParseMetadataFileAsync(ZipArchive archive, ThreeMFModel model)
        {
            // This would parse a metadata file if present in the 3MF archive
            // For simplicity, we'll just set some example values if they haven't been set yet
            if (string.IsNullOrEmpty(model.Name) || model.Name == "Unnamed 3D Model")
            {
                model.Name = "Default Model Name";
            }
            
            // Set default materials if none were found
            if (string.IsNullOrEmpty(model.Materials) || model.Materials == "Unknown Material")
            {
                model.Materials = "PLA";
                model.MaterialCount = 1;
            }
            
            // Set upload time
            model.UploadedAt = DateTime.UtcNow;
            
            await Task.CompletedTask; // Placeholder for async operation
        }

        private async Task ParseComponentsAsync(ZipArchive archive, ThreeMFModel model)
        {
            // In a real implementation, this would parse component files to extract materials
            // Here we just add placeholder materials
            model.Materials = "PLA, PETG";
            model.MaterialCount = 2;

            await Task.CompletedTask; // Placeholder for async operation
        }

        private async Task<XDocument> ParseComponentModelAsync(ZipArchive archive, string componentPath)
        {
            var componentEntry = archive.GetEntry(componentPath);
            if (componentEntry == null)
            {
                throw new InvalidOperationException($"Component file not found: {componentPath}");
            }

            using var componentStream = componentEntry.Open();
            return await XDocument.LoadAsync(componentStream, LoadOptions.None, default);
        }

        private void ProcessComponentFile(XDocument componentDoc, ThreeMFModel model)
        {
            // Process the component file and update the model
            // This is a simplified placeholder
            var materialName = componentDoc.Root?.Attribute("name")?.Value ?? "Unknown Material";
            
            // Update the Materials string with the new material
            if (string.IsNullOrEmpty(model.Materials) || model.Materials == "Unknown Material")
            {
                model.Materials = materialName;
                model.MaterialCount = 1;
            }
            else
            {
                // Only add if it's not already in the list
                var materials = model.Materials.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (!materials.Contains(materialName))
                {
                    model.Materials += $", {materialName}";
                    model.MaterialCount++;
                }
            }
            
            // Add information to warnings if there's anything unusual
            if (componentDoc.Root?.Element(XName.Get("warnings", Namespace3MF)) != null)
            {
                var warnings = componentDoc.Root.Element(XName.Get("warnings", Namespace3MF))?.Value;
                if (!string.IsNullOrEmpty(warnings))
                {
                    if (model.ModelWarnings == "No warnings detected")
                    {
                        model.ModelWarnings = warnings;
                    }
                    else
                    {
                        model.ModelWarnings += Environment.NewLine + warnings;
                    }
                }
            }
        }

        private string? FindComponentPathForObjectId(XElement modelRoot, string objectId)
        {
            // Find the component path for a given object ID
            // This is a simplified placeholder
            return $"/3D/Components/{objectId}.model";
        }

        // Helper class to restore stream position when disposing
        private class PositionRestoringStream : Stream
        {
            private readonly Stream _baseStream;
            private readonly long _initialPosition;

            public PositionRestoringStream(Stream baseStream)
            {
                _baseStream = baseStream;
                _initialPosition = baseStream.Position;
            }

            public override bool CanRead => _baseStream.CanRead;
            public override bool CanSeek => _baseStream.CanSeek;
            public override bool CanWrite => _baseStream.CanWrite;
            public override long Length => _baseStream.Length;

            public override long Position
            {
                get => _baseStream.Position;
                set => _baseStream.Position = value;
            }

            public override void Flush() => _baseStream.Flush();

            public override int Read(byte[] buffer, int offset, int count) =>
                _baseStream.Read(buffer, offset, count);

            public override long Seek(long offset, SeekOrigin origin) =>
                _baseStream.Seek(offset, origin);

            public override void SetLength(long value) =>
                _baseStream.SetLength(value);

            public override void Write(byte[] buffer, int offset, int count) =>
                _baseStream.Write(buffer, offset, count);

            public override int ReadByte() => 
                _baseStream.ReadByte();

            public override void WriteByte(byte value) => 
                _baseStream.WriteByte(value);

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => 
                _baseStream.ReadAsync(buffer, offset, count, cancellationToken);

            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => 
                _baseStream.ReadAsync(buffer, cancellationToken);

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => 
                _baseStream.WriteAsync(buffer, offset, count, cancellationToken);

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => 
                _baseStream.WriteAsync(buffer, cancellationToken);

            public override void CopyTo(Stream destination, int bufferSize) => 
                _baseStream.CopyTo(destination, bufferSize);

            public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => 
                _baseStream.CopyToAsync(destination, bufferSize, cancellationToken);

            public override Task FlushAsync(CancellationToken cancellationToken) => 
                _baseStream.FlushAsync(cancellationToken);

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // Restore position but don't dispose the base stream
                    if (_baseStream.CanSeek)
                    {
                        _baseStream.Position = _initialPosition;
                    }
                }

                base.Dispose(disposing);
            }
        }
    }
}

