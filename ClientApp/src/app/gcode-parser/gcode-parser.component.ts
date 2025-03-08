import { Component, OnInit } from '@angular/core';
import { GcodeService, GCodeFile, CostCalculation, CostCalculationRequest } from '../services/gcode.service';
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
  costCalculation: CostCalculation | null = null;
  calculationError = '';

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
    this.costCalculation = null;
    this.uploadError = '';
    this.calculationError = '';
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

  calculateCost(): void {
    if (!this.parsedGCodeFile || !this.parsedGCodeFile.id) {
      this.calculationError = 'Please upload a GCODE file first';
      return;
    }

    if (!this.costForm.valid) {
      this.calculationError = 'Please enter valid values for all fields';
      return;
    }

    const request: CostCalculationRequest = {
      gcodeFileId: this.parsedGCodeFile.id,
      ...this.costForm.value
    };

    this.calculationError = '';

    this.gcodeService.calculateCosts(request).subscribe({
      next: (result) => {
        this.costCalculation = result;
      },
      error: (error) => {
        this.calculationError = `Error calculating costs: ${error.message || 'Unknown error'}`;
        console.error('Error calculating costs:', error);
      }
    });
  }

  // Helper method to format a number with 2 decimal places and the currency
  formatCurrency(value: number, currency: string = 'USD'): string {
    return new Intl.NumberFormat('en-US', { 
      style: 'currency', 
      currency: currency 
    }).format(value);
  }

  // Calculate cost percentages for visualization
  getMaterialCostPercentage(): number {
    if (!this.costCalculation || this.costCalculation.totalCost === 0) {
      return 0;
    }
    return (this.costCalculation.materialCost / this.costCalculation.totalCost) * 100;
  }

  getElectricityCostPercentage(): number {
    if (!this.costCalculation || this.costCalculation.totalCost === 0) {
      return 0;
    }
    return (this.costCalculation.electricityCost / this.costCalculation.totalCost) * 100;
  }

  getDepreciationCostPercentage(): number {
    if (!this.costCalculation || this.costCalculation.totalCost === 0) {
      return 0;
    }
    return (this.costCalculation.depreciationCost / this.costCalculation.totalCost) * 100;
  }

  getLaborCostPercentage(): number {
    if (!this.costCalculation || this.costCalculation.totalCost === 0) {
      return 0;
    }
    return (this.costCalculation.laborCost / this.costCalculation.totalCost) * 100;
  }

  // Export cost report as CSV
  exportCostReport(): void {
    if (!this.costCalculation || !this.parsedGCodeFile) {
      this.calculationError = 'No cost calculation available to export';
      return;
    }

    // Create CSV content
    const csvRows = [
      ['3D Printing Cost Report'],
      ['Generated on', new Date().toLocaleString()],
      [''],
      ['File Information'],
      ['Filename', this.parsedGCodeFile.filename],
      ['Filament Usage (Length)', `${this.parsedGCodeFile.filamentUsageLength.toFixed(2)} mm`],
      ['Filament Usage (Weight)', `${this.parsedGCodeFile.filamentUsageWeight.toFixed(2)} g`],
      ['Estimated Print Time', this.parsedGCodeFile.estimatedPrintTimeFormatted],
      ['Layer Count', `${this.parsedGCodeFile.layerCount}`],
      ['Layer Height', `${this.parsedGCodeFile.layerHeight} mm`],
      ['Nozzle Temperature', `${this.parsedGCodeFile.nozzleTemperature}°C`],
      ['Bed Temperature', `${this.parsedGCodeFile.bedTemperature}°C`],
      [''],
      ['Cost Parameters'],
      ['Material Cost per kg', `${this.formatCurrency(this.costForm.value.materialCostPerKg, this.costForm.value.currency)}`],
      ['Electricity Cost per kWh', `${this.formatCurrency(this.costForm.value.electricityCostPerKwh, this.costForm.value.currency)}`],
      ['Printer Cost', `${this.formatCurrency(this.costForm.value.printerCost, this.costForm.value.currency)}`],
      ['Printer Lifespan', `${this.costForm.value.printerLifespan} hours`],
      ['Printer Power Consumption', `${this.costForm.value.printerPowerConsumption} kW`],
      ['Labor Cost per Hour', `${this.formatCurrency(this.costForm.value.laborCostPerHour, this.costForm.value.currency)}`],
      ['Labor Time', `${this.costForm.value.laborTime} hours`],
      [''],
      ['Cost Breakdown'],
      ['Material Cost', `${this.formatCurrency(this.costCalculation.materialCost, this.costCalculation.currency)} (${this.getMaterialCostPercentage().toFixed(1)}%)`],
      ['Electricity Cost', `${this.formatCurrency(this.costCalculation.electricityCost, this.costCalculation.currency)} (${this.getElectricityCostPercentage().toFixed(1)}%)`],
      ['Depreciation Cost', `${this.formatCurrency(this.costCalculation.depreciationCost, this.costCalculation.currency)} (${this.getDepreciationCostPercentage().toFixed(1)}%)`],
      ['Labor Cost', `${this.formatCurrency(this.costCalculation.laborCost, this.costCalculation.currency)} (${this.getLaborCostPercentage().toFixed(1)}%)`],
      [''],
      ['Total Cost', this.formatCurrency(this.costCalculation.totalCost, this.costCalculation.currency)]
    ];

    // Convert to CSV format
    const csvContent = csvRows.map(row => row.join(',')).join('\n');

    // Create and download the file
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', `cost-report-${this.parsedGCodeFile.filename}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}
