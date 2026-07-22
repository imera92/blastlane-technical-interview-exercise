import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { BudgetSummary } from '../../../core/budgets/budget.models';
import { BudgetService } from '../../../core/budgets/budget.service';
import { ErrorAlertService } from '../../../core/errors/error-alert.service';

@Component({
  selector: 'app-budget-list',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './budget-list.component.html',
  styleUrl: './budget-list.component.css',
})
export class BudgetListComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly budgetService = inject(BudgetService);
  private readonly errorAlert = inject(ErrorAlertService);
  private readonly router = inject(Router);

  readonly budgets = signal<BudgetSummary[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.loadBudgets();
  }

  deleteBudget(event: Event, budgetId: number): void {
    event.stopPropagation();
    this.budgetService.delete(budgetId).subscribe({
      next: () => this.budgets.update((budgets) => budgets.filter((item) => item.id !== budgetId)),
      error: (error) => this.errorAlert.show(error),
    });
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => void this.router.navigateByUrl('/login'),
      error: (error) => this.errorAlert.show(error),
    });
  }

  private loadBudgets(): void {
    this.loading.set(true);
    this.budgetService
      .list()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (budgets) => this.budgets.set(budgets),
        error: (error) => this.errorAlert.show(error),
      });
  }
}
