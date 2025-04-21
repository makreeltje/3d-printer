import { Component } from '@angular/core';
import { GcodeService, GCodeParseResult } from '../services/gcode.service';

@Component({
  selector: 'app-gcode-uploader',
  templateUrl: './gcode-uploader.component.html',
  styleUrls: ['./gcode-uploader.component.css']
})
export class GcodeUploaderComponent {
  selectedFile: File | null = null;
  parseResult: GCodeParseResult | null = null;
  error: string = '';

  // Cost parameters
  filamentPrice = 25; // $/kg
  electricityPrice = 0.25; // $/kWh
  printerPower = 0.1; // kW

  // Calculated cost
  totalCost: number | null = null;

  constructor(private gcodeService: GcodeService) { }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      this.parseResult = null;
      this.totalCost = null;
      this.error = '';
    }
  }

  uploadAndParse() {
    if (!this.selectedFile) return;
    this.gcodeService.parseGCodeFile(this.selectedFile).subscribe({
      next: (result) => {
        this.parseResult = result;
        this.calculateCost();
      },
      error: (err) => {
        this.error = 'Failed to parse G-code file.';
      }
    });
  }

  calculateCost() {
    if (!this.parseResult) return;
    // Material cost
    const filamentKg = (this.parseResult.filamentUsedGrams || 0) / 1000;
    const materialCost = filamentKg * this.filamentPrice;
    // Electricity cost
    let printHours = 0;
    if (this.parseResult.estimatedPrintTime) {
      const match = this.parseResult.estimatedPrintTime.match(/(\d+):(\d+)/);
      if (match) {
        printHours = parseInt(match[1]) + parseInt(match[2]) / 60;
      }
    }
    const electricityCost = this.printerPower * printHours * this.electricityPrice;
    this.totalCost = materialCost + electricityCost;
  }
}
