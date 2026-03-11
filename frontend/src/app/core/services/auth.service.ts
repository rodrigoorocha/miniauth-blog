import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  UserDto,
} from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly API_URL = 'http://localhost:5167/api/auth';

  private _user$ = new BehaviorSubject<UserDto | null>(null);
  user$ = this._user$.asObservable();

  constructor(private _http: HttpClient) {
    this.loadFromStorage();
  }

  get isLoggedIn(): boolean {
    return !!this._user$.getValue();
  }

  get token(): string | null {
    return localStorage.getItem('access_token');
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this._http.post<AuthResponse>(`${this.API_URL}/login`, request).pipe(
      tap((response) => this.handleAuth(response))
    );
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this._http
      .post<AuthResponse>(`${this.API_URL}/register`, request)
      .pipe(tap((response) => this.handleAuth(response)));
  }

  logout(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user');
    this._user$.next(null);
  }

  private handleAuth(response: AuthResponse): void {
    localStorage.setItem('access_token', response.accessToken);
    localStorage.setItem('refresh_token', response.refreshToken);
    localStorage.setItem('user', JSON.stringify(response.user));
    this._user$.next(response.user);
  }

  private loadFromStorage(): void {
    const userJson = localStorage.getItem('user');
    if (userJson) {
      this._user$.next(JSON.parse(userJson));
    }
  }
}
