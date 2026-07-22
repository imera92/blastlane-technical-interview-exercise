import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { BudgetDetails, BudgetSummary, CreateBudgetRequest } from './budget.models';

@Injectable({ providedIn: 'root' })
export class BudgetService {
  private readonly http = inject(HttpClient);

  list(): Observable<BudgetSummary[]> {
    return this.http.get<BudgetSummary[]>('/api/budgets');
  }

  get(budgetId: number): Observable<BudgetDetails> {
    return this.http.get<BudgetDetails>(`/api/budgets/${budgetId}`);
  }

  create(request: CreateBudgetRequest): Observable<BudgetSummary> {
    return this.http.post<BudgetSummary>('/api/budgets', request);
  }

  delete(budgetId: number): Observable<void> {
    return this.http.delete<void>(`/api/budgets/${budgetId}`);
  }
}
