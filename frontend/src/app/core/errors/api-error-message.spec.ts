import { HttpErrorResponse } from '@angular/common/http';
import { getApiErrorMessage } from './api-error-message';

describe('getApiErrorMessage', () => {
  it('joins validation messages returned by the API', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        errors: {
          BudgetValidation: ['Name is required.', 'Starting balance is invalid.'],
        },
      },
    });

    expect(getApiErrorMessage(error)).toBe(
      'Name is required.\nStarting balance is invalid.',
    );
  });

  it('uses the generic message for non-validation errors', () => {
    const error = new HttpErrorResponse({ status: 500 });

    expect(getApiErrorMessage(error)).toBe('Sorry, something went wrong.');
  });
});
