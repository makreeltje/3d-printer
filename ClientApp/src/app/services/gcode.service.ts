import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// Define interfaces for our data models
export interface GCodeFile {
  id?: string;
  filename: string;
  filamentUsageLength: number;  // in mm
  filamentType: string;
  filamentUsageWeight: number;  // in g
  estimatedPrintTime: string;  // formatted time string
  layerCount: number;
  layerHeight: number;
  nozzleTemperature: number;
  bedTemperature: number;
  filamentDiameter: number;
  infillPercentage: number;
  hasSupport: boolean;
  slicerSoftware: string;
  thumbnailBase64: string;
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
}

