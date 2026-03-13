import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  protected baseUrl = environment.apiUrl;

  constructor(protected http: HttpClient) {}

  getAll<T>(endpoint: string, page = 1, pageSize = 10, filters?: Record<string, string>): Observable<PagedResult<T>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== null && value !== undefined && value !== '') {
          params = params.set(key, value);
        }
      });
    }

    return this.http.get<PagedResult<T>>(`${this.baseUrl}${endpoint}`, { params });
  }

  getById<T>(endpoint: string, id: number | string): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}${endpoint}/${id}`);
  }

  create<T>(endpoint: string, body: unknown): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}${endpoint}`, body);
  }

  update<T>(endpoint: string, id: number | string, body: unknown): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}${endpoint}/${id}`, body);
  }

  delete(endpoint: string, id: number | string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}${endpoint}/${id}`);
  }

  search<T>(endpoint: string, query: string, page = 1, pageSize = 10): Observable<PagedResult<T>> {
    const params = new HttpParams()
      .set('q', query)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<T>>(`${this.baseUrl}${endpoint}/search`, { params });
  }

  export(endpoint: string, format: 'csv' | 'excel' = 'excel'): Observable<Blob> {
    return this.http.get(`${this.baseUrl}${endpoint}/export`, {
      params: new HttpParams().set('format', format),
      responseType: 'blob'
    });
  }
}
