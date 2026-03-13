import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';

export interface PagedResult<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface QueryParams {
  page?: number;
  pageSize?: number;
  sortField?: string;
  sortDirection?: 'asc' | 'desc';
  [key: string]: unknown;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /** GET /resource  – returns paginated list */
  getAll<T>(resource: string, params?: QueryParams): Observable<PagedResult<T>> {
    let httpParams = new HttpParams();
    if (params) {
      Object.entries(params).forEach(([k, v]) => {
        if (v !== null && v !== undefined) {
          httpParams = httpParams.set(k, String(v));
        }
      });
    }
    return this.http.get<PagedResult<T>>(`${this.baseUrl}/${resource}`, { params: httpParams });
  }

  /** GET /resource/:id */
  getById<T>(resource: string, id: number | string): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}/${resource}/${id}`);
  }

  /** POST /resource */
  create<TRequest, TResponse = TRequest>(resource: string, body: TRequest): Observable<TResponse> {
    return this.http.post<TResponse>(`${this.baseUrl}/${resource}`, body);
  }

  /** PUT /resource/:id */
  update<TRequest, TResponse = TRequest>(
    resource: string,
    id: number | string,
    body: TRequest
  ): Observable<TResponse> {
    return this.http.put<TResponse>(`${this.baseUrl}/${resource}/${id}`, body);
  }

  /** PATCH /resource/:id */
  patch<TRequest, TResponse = TRequest>(
    resource: string,
    id: number | string,
    body: Partial<TRequest>
  ): Observable<TResponse> {
    return this.http.patch<TResponse>(`${this.baseUrl}/${resource}/${id}`, body);
  }

  /** DELETE /resource/:id */
  delete(resource: string, id: number | string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${resource}/${id}`);
  }

  /** GET /resource/search?q=... */
  search<T>(resource: string, query: string, params?: QueryParams): Observable<PagedResult<T>> {
    let httpParams = new HttpParams().set('q', query);
    if (params) {
      Object.entries(params).forEach(([k, v]) => {
        if (v !== null && v !== undefined) {
          httpParams = httpParams.set(k, String(v));
        }
      });
    }
    return this.http.get<PagedResult<T>>(`${this.baseUrl}/${resource}/search`, { params: httpParams });
  }

  /** GET /resource/export – returns blob (e.g. Excel/CSV) */
  export(resource: string, params?: QueryParams): Observable<Blob> {
    let httpParams = new HttpParams();
    if (params) {
      Object.entries(params).forEach(([k, v]) => {
        if (v !== null && v !== undefined) {
          httpParams = httpParams.set(k, String(v));
        }
      });
    }
    return this.http.get(`${this.baseUrl}/${resource}/export`, {
      params: httpParams,
      responseType: 'blob'
    });
  }
}
