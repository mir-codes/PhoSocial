import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { jwtDecode } from 'jwt-decode';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenKey = 'pho_token';

  constructor(private http: HttpClient) {}

  signup(userName: string, email: string, password: string) {
    return this.http.post(`${environment.apiUrl}/Auth/signup`, { userName, email, password });
  }

  login(email: string, password: string) {
    return this.http.post<{ token: string }>(`${environment.apiUrl}/Auth/login`, { email, password });
  }

  saveToken(token: string) {
    localStorage.setItem(this.tokenKey, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
  }

  getUserIdFromToken(): string | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const decoded: any = jwtDecode(token);
      return decoded.id || decoded.userId || null;
    } catch {
      return null;
    }
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}
