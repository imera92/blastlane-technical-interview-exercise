import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('logs in through the API and stores the current user', () => {
    service
      .login({ email: 'reviewer@example.com', password: 'Reviewer123' })
      .subscribe();

    const initialAntiforgeryRequest = http.expectOne('/api/auth/antiforgery');
    expect(initialAntiforgeryRequest.request.method).toBe('GET');
    initialAntiforgeryRequest.flush(null);

    const request = http.expectOne('/api/auth/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      email: 'reviewer@example.com',
      password: 'Reviewer123',
    });

    request.flush({
      user: {
        id: 'f385b965-9de6-44a0-b0ed-336a3d97335b',
        displayName: 'Reviewer',
        email: 'reviewer@example.com',
      },
    });

    const authenticatedAntiforgeryRequest = http.expectOne('/api/auth/antiforgery');
    expect(authenticatedAntiforgeryRequest.request.method).toBe('GET');
    authenticatedAntiforgeryRequest.flush(null);

    expect(service.currentUser()?.email).toBe('reviewer@example.com');
  });
});
