import { Component, OnInit } from '@angular/core';
import { GcodeService, GCodeParseResult } from '../services/gcode.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-gcode-parser',
  templateUrl: './gcode-parser.component.html',
  styleUrls: ['./gcode-parser.component.css']
})
export class GcodeParserComponent implements OnInit {
  // Store the cost calculation results for each parsed file
  costResults: CostResult[] = [];
  isUploading = false;
  uploadProgress = 0;
  uploadError = '';
  selectedFiles: File[] = [];
  parsedResults: { filename: string, result: GCodeParseResult | null, error?: string }[] = [];

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
      profitPercentage: [0.15, [Validators.required, Validators.min(0)]], // 15% default
      currency: ['USD', Validators.required]
    });
  }

  ngOnInit(): void {
    // Any initialization logic here
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input?.files) {
      this.selectedFiles = Array.from(input.files);
    } else {
      this.selectedFiles = [];
    }
    this.parsedResults = [];
    this.uploadError = '';
    this.costResults = [];
  }

  uploadFiles(): void {
    if (!this.selectedFiles || this.selectedFiles.length === 0) {
      this.uploadError = 'Please select one or more files first';
      return;
    }

    this.isUploading = true;
    this.uploadProgress = 0;
    this.uploadError = '';
    this.parsedResults = [];
    this.costResults = [];

    let completed = 0;
    const total = this.selectedFiles.length;

    this.selectedFiles.forEach(file => {
      if (!file.name.toLowerCase().endsWith('.gcode') && !file.name.toLowerCase().endsWith('.bgcode')) {
        this.parsedResults.push({ filename: file.name, result: null, error: 'Not a GCODE file' });
        completed++;
        if (completed === total) this.isUploading = false;
        return;
      }
      this.gcodeService.parseGCodeFile(file).subscribe({
        next: (result: GCodeParseResult) => {
          this.parsedResults.push({ filename: file.name, result });
          completed++;
          this.uploadProgress = Math.round((completed / total) * 100);
          if (completed === total) this.isUploading = false;
        },
        error: (error: any) => {
          const backendError = error?.error?.error;
          this.parsedResults.push({ filename: file.name, result: null, error: backendError || error?.message || 'Parse error' });
          completed++;
          this.uploadProgress = Math.round((completed / total) * 100);
          if (completed === total) this.isUploading = false;
        }
      });
    });
  }

  // Calculate costs for all parsed results
  calculateCosts(): void {
    this.costResults = this.parsedResults.map(r => {
      if (!r.result) return null;
      return this.calculateCostForResult(r.result);
    }).filter(c => !!c) as CostResult[];
  }

  // Helper: calculate cost for a single GCodeParseResult
  calculateCostForResult(result: GCodeParseResult): CostResult {
    const form = this.costForm.value;
    const materialCost = (result.filamentUsedGrams / 1000) * form.materialCostPerKg;
    // Parse estimatedPrintTime (e.g. '5h 23m' or '2h') to hours
    let printTimeHours = 0;
    if (result.estimatedPrintTime) {
      const match = result.estimatedPrintTime.match(/(\d+)h\s*(\d+)?m?/);
      if (match) {
        printTimeHours = parseInt(match[1], 10);
        if (match[2]) printTimeHours += parseInt(match[2], 10) / 60;
      }
    }
    const electricityCost = form.printerPowerConsumption * printTimeHours * form.electricityCostPerKwh;
    const depreciationCost = (form.printerCost * printTimeHours) / form.printerLifespan;
    const laborCost = form.laborTime * form.laborCostPerHour;
    const subtotal = materialCost + electricityCost + depreciationCost + laborCost;
    const profitMargin = subtotal * form.profitPercentage;
    const totalCost = subtotal + profitMargin;
    return {
      materialCost,
      electricityCost,
      depreciationCost,
      laborCost,
      profitMargin,
      totalCost,
      currency: form.currency
    };
  }

  hasAnyError(): boolean {
    return this.parsedResults.some(r => !!r.error);
  }

  // Format filament length to display in meters when > 1000mm
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

// Cost breakdown interface
interface CostResult {
  materialCost: number;
  electricityCost: number;
  depreciationCost: number;
  laborCost: number;
  profitMargin: number;
  totalCost: number;
  currency: string;
}