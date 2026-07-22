import { TransactionGroup } from '../transactions/transaction.models';

export interface BudgetSummary {
  id: number;
  name: string;
  startingBalance: number;
  currentBalance: number;
  createdAtUtc: string;
}

export interface BudgetDetails extends BudgetSummary {
  transactionGroups: TransactionGroup[];
}

export interface CreateBudgetRequest {
  name: string;
  startingBalance: number;
}
