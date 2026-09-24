import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private readonly http: HttpClient) { }

  get<T = unknown>(path: string, params?: HttpParams): Observable<T> {
    return this.http.get<T>(path, { params });
  }

  post<T = unknown>(path: string, body: unknown = {}): Observable<T> {
    // HttpClient handles JSON and FormData; the browser supplies multipart boundaries.
    return this.http.post<T>(path, body);
  }

  patch<T = unknown>(path: string, body: unknown = {}): Observable<T> {
    return this.http.patch<T>(path, body);
  }

  delete<T = unknown>(path: string, params?: HttpParams): Observable<T> {
    return this.http.delete<T>(path, { params });
  }

  // Errors propagate unchanged as HttpErrorResponse, preserving status and response body.
}
