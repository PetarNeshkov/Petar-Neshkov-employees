import { APP_CONSTANTS } from '../../core/constants/app.constants';
import {HttpErrorResponse} from '@angular/common/http';
import {ApiProblem} from '../models/api-problem';

export function getAnalysisErrorMessages(error: HttpErrorResponse): string[] {
  if (error.status === APP_CONSTANTS.NETWORK_ERROR) {
    return [APP_CONSTANTS.CONNECTION_FAILED];
  }

  if (error.status === APP_CONSTANTS.PAYLOAD_TOO_LARGE) {
    return [APP_CONSTANTS.UPLOAD_TOO_LARGE];
  }

  if (error.status >= APP_CONSTANTS.SERVER_ERROR_START) {
    return [APP_CONSTANTS.SERVER_ERROR];
  }

  // A proxy may return plain text or HTML instead of the API's JSON error format.
  const body: unknown = error.error;
  if (body && typeof body === 'object' && !Array.isArray(body)) {
    const problem = body as ApiProblem;
    if (problem.errors && typeof problem.errors === 'object') {
      const messages = Object.values(problem.errors)
        .filter(Array.isArray)
        .flat()
        .filter((message): message is string => typeof message === 'string' && message.trim().length > 0);

      if (messages.length > 0){
        return messages;
      }
    }

    if (typeof problem.detail === 'string' && problem.detail.trim()) {
      return [problem.detail];
    }
    if (typeof problem.title === 'string' && problem.title.trim()){
      return [problem.title];
    }
  }

  return [APP_CONSTANTS.ANALYSIS_FAILED];
}
