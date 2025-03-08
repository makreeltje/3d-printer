import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

// Define interfaces for our data models
export interface GCodeFile {
  id?: string;
  filename: string;
  filamentUsageLength: number;  // in mm
  filamentUsageWeight: number;  // in g
  estimatedPrintTime: string;  // formatted time string
  layerCount: number;
  layerHeight: number;
  nozzleTemperature: number;
  bedTemperature: number;
  // Add any other properties from the backend model
}

export interface CostCalculation {
  materialCost: number;
  electricityCost: number;
  depreciationCost: number;
  laborCost: number;
  totalCost: number;
  currency: string;
  gCodeFile?: GCodeFile;
}

export interface CostCalculationRequest {
  gcodeFileId: string;
  materialCostPerKg: number;
  electricityCostPerKwh: number;
  printerCost: number;
  printerLifespan: number;
  printerPowerConsumption: number;
  laborCostPerHour: number;
  laborTime: number;
  currency: string;
}

@Injectable({
  providedIn: 'root'
})
export class GcodeService {
  private baseUrl = 'api/gcode';  // Base URL to web API

  constructor(private http: HttpClient) { }

  // Upload a GCODE file
  uploadGCodeFile(file: File): Observable<GCodeFile> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<GCodeFile>(`${this.baseUrl}/upload`, formData);
  }

  // Get a specific GCODE file by ID
  getGCodeFile(id: string): Observable<GCodeFile> {
    return this.http.get<GCodeFile>(`${this.baseUrl}/${id}`);
  }

  // Get all GCODE files (if we implement this functionality later)
  getAllGCodeFiles(): Observable<GCodeFile[]> {
    return this.http.get<GCodeFile[]>(`${this.baseUrl}`);
  }

  // Calculate costs based on a GCODE file and parameters
  calculateCosts(request: CostCalculationRequest): Observable<CostCalculation> {
    return this.http.post<CostCalculation>(`${this.baseUrl}/calculate-cost`, request);
  }
}

