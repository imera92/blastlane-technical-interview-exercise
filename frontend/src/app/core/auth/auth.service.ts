import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, finalize, map, switchMap, tap } from 'rxjs';
import { AuthenticationResponse, LoginRequest, User } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  readonly currentUser = signal<User | null>(null);

  login(request: LoginRequest): Observable<User> {
    return this.refreshAntiforgeryToken().pipe(
      switchMap(() => this.http.post<AuthenticationResponse>('/api/auth/login', request)),
      map((response) => response.user),
      tap((user) => this.currentUser.set(user)),
      switchMap((user) => this.refreshAntiforgeryToken().pipe(map(() => user))),
    );
  }

  loadCurrentUser(): Observable<User> {
    return this.http.get<AuthenticationResponse>('/api/auth/me').pipe(
      map((response) => response.user),
      tap((user) => this.currentUser.set(user)),
    );
  }

  logout(): Observable<void> {
    return this.http
      .post<void>('/api/auth/logout', null)
      .pipe(finalize(() => this.currentUser.set(null)));
  }

  private refreshAntiforgeryToken(): Observable<void> {
    return this.http.get<void>('/api/auth/antiforgery');
  }
}
