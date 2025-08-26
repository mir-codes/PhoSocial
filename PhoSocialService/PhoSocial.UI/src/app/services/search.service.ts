import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class SearchService {
  private base = environment.apiUrl + '/Search';
  constructor(private http: HttpClient) {}
  users(q: string) {
    if (!q || !q.trim()) return { subscribe: () => {} };
    return this.http.get<any[]>(`${this.base}/users?q=${encodeURIComponent(q)}`);
  }
}
