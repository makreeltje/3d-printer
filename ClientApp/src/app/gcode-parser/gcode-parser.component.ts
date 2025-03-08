import { Component, OnInit } from '@angular/core';
import { GcodeService, GCodeFile } from '../services/gcode.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-gcode-parser',
  templateUrl: './gcode-parser.component.html',
  styleUrls: ['./gcode-parser.component.css']
})
export class GcodeParserComponent implements OnInit {
  isUploading = false;
  uploadProgress = 0;
  uploadError = '';
  selectedFile: File | null = null;
  parsedGCodeFile: GCodeFile | null = null;

  // Form for cost calculation parameters
  costForm: FormGroup;

  constructor(
    private gcodeService: GcodeService,
    private formBuilder: FormBuilder
  ) {
    // Initialize the form with default values
    this.costForm = this.formBuilder.group({
      materialCostPerKg: [20, [Validators.required, Validators.min(0)]],
      electricityCostPerKwh: [0.15, [Validators.required, Validators.min(0)]],
      printerCost: [500, [Validators.required, Validators.min(0)]],
      printerLifespan: [1000, [Validators.required, Validators.min(0)]], // in hours
      printerPowerConsumption: [0.150, [Validators.required, Validators.min(0)]], // in kW
      laborCostPerHour: [15, [Validators.required, Validators.min(0)]],
      laborTime: [0.5, [Validators.required, Validators.min(0)]], // in hours
      currency: ['USD', Validators.required]
    });
  }

  ngOnInit(): void {
    // Any initialization logic here
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0] as File;
    // Reset any previous data
    this.parsedGCodeFile = null;
    this.uploadError = '';
  }

  uploadFile(): void {
    if (!this.selectedFile) {
      this.uploadError = 'Please select a file first';
      return;
    }

    // Check file type
    if (!this.selectedFile.name.toLowerCase().endsWith('.gcode')) {
      this.uploadError = 'Please select a valid GCODE file (.gcode)';
      return;
    }

    this.isUploading = true;
    this.uploadProgress = 0;
    this.uploadError = '';

    // Upload the file
    this.gcodeService.uploadGCodeFile(this.selectedFile).subscribe({
      next: (result) => {
        this.isUploading = false;
        this.uploadProgress = 100;
        this.parsedGCodeFile = result;
      },
      error: (error) => {
        this.isUploading = false;
        this.uploadError = `Error uploading file: ${error.message || 'Unknown error'}`;
        console.error('Error uploading GCODE file:', error);
      }
    });
  }

  // Format filament length to display in meters when > 1000mm
  // Input is in mm, convert to m for better readability when appropriate
  formatFilamentLength(length: number): string {
    if (length >= 1000) {
      return `${(length / 1000).toFixed(2)} m`;
    } else {
      return `${length.toFixed(2)} mm`;
    }
  }

  // Format filament weight with correct precision
  formatFilamentWeight(weight: number): string {
    return `${weight.toFixed(2)} g`;
  }

  // Format layer height with correct precision (mm)
  formatLayerHeight(height: number): string {
    return `${height.toFixed(2)} mm`;
  }

  // Format temperature with graceful handling of null values
  formatTemperature(temp: number | null): string {
    if (temp === null) {
      return 'N/A';
    }
    return `${temp}°C`;
  }
}
