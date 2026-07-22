import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TransactionService } from './transaction.service';

describe('TransactionService', () => {
  let service: TransactionService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(TransactionService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('creates a signed transaction inside the selected budget', () => {
    const payload = { name: 'Groceries', amount: -74.5, date: '2026-07-22' };

    service.create(42, payload).subscribe();

    const request = http.expectOne('/api/budgets/42/transactions');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush({});
  });

  it('deletes a transaction through its nested resource route', () => {
    service.delete(42, 7).subscribe();

    const request = http.expectOne('/api/budgets/42/transactions/7');
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });
});
