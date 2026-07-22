import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { BudgetService } from './budget.service';

describe('BudgetService', () => {
  let service: BudgetService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(BudgetService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('loads the current user budgets', () => {
    service.list().subscribe();

    const request = http.expectOne('/api/budgets');
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('creates a budget without sending user ownership', () => {
    service.create({ name: 'Monthly budget', startingBalance: 1000 }).subscribe();

    const request = http.expectOne('/api/budgets');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      name: 'Monthly budget',
      startingBalance: 1000,
    });
    request.flush({});
  });
});
