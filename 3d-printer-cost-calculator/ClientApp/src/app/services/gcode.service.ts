import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// Define interfaces for our data models
export interface GCodeParseResult {
  slicer: string;
  filamentUsedGrams: number;
  filamentUsedMm: number;
  estimatedPrintTime: string | null;
  layerCount: number | null;
}

@Injectable({
  providedIn: 'root'
})
export class GcodeService {
  private baseUrl = 'api/gcode';

  constructor(private http: HttpClient) { }

  // Upload and parse a GCODE file
  parseGCodeFile(file: File): Observable<GCodeParseResult> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<GCodeParseResult>(`${this.baseUrl}/parse`, formData);
  }
}

