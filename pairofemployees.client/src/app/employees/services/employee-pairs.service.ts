import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { EMPLOYEE_PAIR_URLS } from '../../core/constants/api-routes';
import { ApiService } from '../../core/services/api.service';
import { AnalysisResponse } from '../models/analysis-response';

@Injectable({ providedIn: 'root' })
export class EmployeePairsService {
  constructor(private readonly api: ApiService) { }

  analyze(file: File): Observable<AnalysisResponse> {
    const form = new FormData();
    form.append('file', file);

    return this.api.post<AnalysisResponse>(EMPLOYEE_PAIR_URLS.ANALYZE, form);
  }
}
