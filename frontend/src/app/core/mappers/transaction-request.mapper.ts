export type TransactionType = 'income' | 'expense';

export function toSignedAmount(amount: number, type: TransactionType): number {
  const absoluteAmount = Math.abs(amount);
  return type === 'income' ? absoluteAmount : -absoluteAmount;
}
