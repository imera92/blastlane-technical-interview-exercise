import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { BudgetService } from '../../../core/budgets/budget.service';
import { ErrorAlertService } from '../../../core/errors/error-alert.service';

@Component({
  selector: 'app-budget-create',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './budget-create.component.html',
  styleUrl: './budget-create.component.css',
})
export class BudgetCreateComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly budgetService = inject(BudgetService);
  private readonly errorAlert = inject(ErrorAlertService);
  private readonly router = inject(Router);

  readonly submitting = signal(false);
  readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    startingBalance: [0, [Validators.required, Validators.min(0)]],
  });

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.budgetService
      .create(this.form.getRawValue())
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => void this.router.navigateByUrl('/budgets'),
        error: (error) => this.errorAlert.show(error),
      });
  }
}
