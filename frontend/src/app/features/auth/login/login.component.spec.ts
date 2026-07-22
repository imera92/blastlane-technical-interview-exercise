import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  it('logs in with the form values and navigates to budgets', () => {
    const authService = {
      login: jasmine.createSpy().and.returnValue(
        of({
          id: 'f385b965-9de6-44a0-b0ed-336a3d97335b',
          displayName: 'Reviewer',
          email: 'reviewer@example.com',
        }),
      ),
    };

    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authService },
      ],
    });

    const fixture = TestBed.createComponent(LoginComponent);
    const router = TestBed.inject(Router);
    const navigate = spyOn(router, 'navigateByUrl');
    fixture.componentInstance.form.setValue({
      email: 'reviewer@example.com',
      password: 'Reviewer123',
    });

    fixture.componentInstance.submit();

    expect(authService.login).toHaveBeenCalledWith({
      email: 'reviewer@example.com',
      password: 'Reviewer123',
    });
    expect(navigate).toHaveBeenCalledWith('/budgets');
  });
});
