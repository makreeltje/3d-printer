using System.Net.Http;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace _3d_printer_cost_calculator.Tests.Controllers;

public class GcodeControllerTests : IClassFixture<WebApplicationFactory<IApiMarker>>
{
    private readonly HttpClient _client;

    public GcodeControllerTests(WebApplicationFactory<IApiMarker> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Upload_Parses_Bambu_Gcode()
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(File.ReadAllBytes("../../../Examples/bambu_lab.gcode"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", "bambu_lab.gcode");

        var response = await _client.PostAsync("/api/gcode/parse", content);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        body.Should().Contain("\"slicerName\":\"Bambu Studio\"");
    }
    
    [Fact]
    public async Task Upload_Bambu_Gcode_Returns_Full_Parsed_Data()
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(File.ReadAllBytes("../../../Examples/bambu_lab.gcode"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", "bambu_lab.gcode");

        var response = await _client.PostAsync("/api/gcode/parse", content);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain("\"slicerName\":\"Bambu Studio\"");
        json.Should().Contain("\"filamentUsedMm\":1503.61");
        json.Should().Contain("\"filamentUsedGrams\":4.48");
        json.Should().Contain("\"nozzleTemperature\":220");
        json.Should().Contain("\"bedTemperature\":60");
        json.Should().Contain("\"layerCount\":25");
    }
    
    [Fact]
    public async Task Upload_Parses_Creality_Gcode()
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(File.ReadAllBytes("../../../Examples/creality_print.gcode"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", "creality_print.gcode");

        var response = await _client.PostAsync("/api/gcode/parse", content);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        body.Should().Contain("\"slicerName\":\"Creality Print\"");
    }
    
    [Fact]
    public async Task Upload_Creality_Gcode_Returns_Full_Parsed_Data()
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(File.ReadAllBytes("../../../Examples/creality_print.gcode"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", "creality_print.gcode");

        var response = await _client.PostAsync("/api/gcode/parse", content);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        json.Should().Contain("\"slicerName\":\"Creality Print\"");
        json.Should().Contain("\"filamentUsedMm\":10882.04");
        json.Should().Contain("\"filamentUsedGrams\":32.46");
        json.Should().Contain("\"nozzleTemperature\":220");
        json.Should().Contain("\"bedTemperature\":60");
        json.Should().Contain("\"layerCount\":20");
    }

    [Fact]
    public async Task Upload_Invalid_File_Returns_400()
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes("not real gcode"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "fake.txt");

        var response = await _client.PostAsync("/api/gcode/parse", content);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}