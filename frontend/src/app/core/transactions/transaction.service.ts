import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateTransactionRequest, TransactionResponse } from './transaction.models';

@Injectable({ providedIn: 'root' })
export class TransactionService {
  private readonly http = inject(HttpClient);

  create(
    budgetId: number,
    request: CreateTransactionRequest,
  ): Observable<TransactionResponse> {
    return this.http.post<TransactionResponse>(
      `/api/budgets/${budgetId}/transactions`,
      request,
    );
  }

  delete(budgetId: number, transactionId: number): Observable<void> {
    return this.http.delete<void>(
      `/api/budgets/${budgetId}/transactions/${transactionId}`,
    );
  }
}
