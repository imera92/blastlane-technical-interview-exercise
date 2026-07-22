export interface TransactionItem {
  id: number;
  name: string;
  amount: number;
  createdAtUtc: string;
}

export interface TransactionGroup {
  date: string;
  transactions: TransactionItem[];
}

export interface TransactionResponse extends TransactionItem {
  date: string;
}

export interface CreateTransactionRequest {
  name: string;
  amount: number;
  date: string;
}
