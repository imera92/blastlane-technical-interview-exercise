import { toSignedAmount } from './transaction-request.mapper';

describe('toSignedAmount', () => {
  it('returns a positive amount for income', () => {
    expect(toSignedAmount(125.5, 'income')).toBe(125.5);
  });

  it('returns a negative amount for an expense', () => {
    expect(toSignedAmount(125.5, 'expense')).toBe(-125.5);
  });
});
