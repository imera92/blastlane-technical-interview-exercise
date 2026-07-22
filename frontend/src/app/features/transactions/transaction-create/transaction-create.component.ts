import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { BudgetService } from '../../../core/budgets/budget.service';
import { ErrorAlertService } from '../../../core/errors/error-alert.service';
import {
  TransactionType,
  toSignedAmount,
} from '../../../core/mappers/transaction-request.mapper';
import { TransactionService } from '../../../core/transactions/transaction.service';

@Component({
  selector: 'app-transaction-create',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './transaction-create.component.html',
  styleUrl: './transaction-create.component.css',
})
export class TransactionCreateComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly budgetService = inject(BudgetService);
  private readonly transactionService = inject(TransactionService);
  private readonly errorAlert = inject(ErrorAlertService);

  readonly budgetId = Number(this.route.snapshot.paramMap.get('budgetId'));
  readonly budgetName = signal('budget');
  readonly type = signal<TransactionType>('expense');
  readonly submitting = signal(false);
  readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    date: [new Date().toISOString().slice(0, 10), Validators.required],
  });

  ngOnInit(): void {
    this.budgetService.get(this.budgetId).subscribe({
      next: (budget) => this.budgetName.set(budget.name),
      error: (error) => this.errorAlert.show(error),
    });
  }

  toggleType(): void {
    this.type.update((type) => (type === 'income' ? 'expense' : 'income'));
  }

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);
    this.transactionService
      .create(this.budgetId, {
        name: value.name,
        amount: toSignedAmount(value.amount, this.type()),
        date: value.date,
      })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => void this.router.navigateByUrl(`/budgets/${this.budgetId}`),
        error: (error) => this.errorAlert.show(error),
      });
  }
}
