import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then((component) => component.LoginComponent),
  },
  {
    path: 'budgets',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/budgets/budget-list/budget-list.component').then(
            (component) => component.BudgetListComponent,
          ),
      },
      {
        path: 'new',
        loadComponent: () =>
          import('./features/budgets/budget-create/budget-create.component').then(
            (component) => component.BudgetCreateComponent,
          ),
      },
      {
        path: ':budgetId',
        loadComponent: () =>
          import('./features/transactions/transaction-list/transaction-list.component').then(
            (component) => component.TransactionListComponent,
          ),
      },
      {
        path: ':budgetId/transactions/new',
        loadComponent: () =>
          import('./features/transactions/transaction-create/transaction-create.component').then(
            (component) => component.TransactionCreateComponent,
          ),
      },
    ],
  },
  { path: '', pathMatch: 'full', redirectTo: 'budgets' },
  { path: '**', redirectTo: 'budgets' },
];
