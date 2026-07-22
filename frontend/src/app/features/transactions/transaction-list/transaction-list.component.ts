import { DecimalPipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { BudgetDetails } from '../../../core/budgets/budget.models';
import { BudgetService } from '../../../core/budgets/budget.service';
import { ErrorAlertService } from '../../../core/errors/error-alert.service';
import { TransactionService } from '../../../core/transactions/transaction.service';

@Component({
  selector: 'app-transaction-list',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './transaction-list.component.html',
  styleUrl: './transaction-list.component.css',
})
export class TransactionListComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly budgetService = inject(BudgetService);
  private readonly transactionService = inject(TransactionService);
  private readonly errorAlert = inject(ErrorAlertService);

  readonly budgetId = Number(this.route.snapshot.paramMap.get('budgetId'));
  readonly budget = signal<BudgetDetails | null>(null);
  readonly loading = signal(true);
  readonly transactionCount = computed(
    () =>
      this.budget()?.transactionGroups.reduce(
        (total, group) => total + group.transactions.length,
        0,
      ) ?? 0,
  );

  ngOnInit(): void {
    this.loadBudget();
  }

  deleteTransaction(event: Event, transactionId: number): void {
    event.stopPropagation();
    this.transactionService.delete(this.budgetId, transactionId).subscribe({
      next: () => this.loadBudget(),
      error: (error) => this.errorAlert.show(error),
    });
  }

  private loadBudget(): void {
    this.loading.set(true);
    this.budgetService
      .get(this.budgetId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (budget) => this.budget.set(budget),
        error: (error) => this.errorAlert.show(error),
      });
  }
}
