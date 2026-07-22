import { Injectable } from '@angular/core';
import { getApiErrorMessage } from './api-error-message';

@Injectable({ providedIn: 'root' })
export class ErrorAlertService {
  show(error: unknown): void {
    window.alert(getApiErrorMessage(error));
  }
}
