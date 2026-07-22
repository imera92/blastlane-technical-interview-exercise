import { HttpErrorResponse } from '@angular/common/http';

const genericErrorMessage = 'Sorry, something went wrong.';

export function getApiErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse) || error.status !== 400) {
    return genericErrorMessage;
  }

  const response = error.error as { errors?: Record<string, unknown> } | null;
  const validationErrors = response?.errors;

  if (!validationErrors) {
    return genericErrorMessage;
  }

  const messages = Object.values(validationErrors).flatMap((value) =>
    Array.isArray(value)
      ? value.filter((message): message is string => typeof message === 'string')
      : [],
  );

  return messages.length > 0 ? messages.join('\n') : genericErrorMessage;
}
