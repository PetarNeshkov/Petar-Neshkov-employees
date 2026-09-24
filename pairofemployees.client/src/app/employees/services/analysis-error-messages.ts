import {HttpErrorResponse} from '@angular/common/http';
import {ApiProblem} from '../models/api-problem';

export function getAnalysisErrorMessages(error: HttpErrorResponse): string[] {
  if (error.status === 0) {
    return ['Could not reach the server. Check your connection and choose the file again.'];
  }

  if (error.status === 413) {
    return ['The upload is too large. Choose a CSV file no larger than 1 MB.'];
  }

  if (error.status >= 500) {
    return ['The server could not analyze the file. Please try again.'];
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

  return ['The file could not be analyzed. Check its contents and try again.'];
}
