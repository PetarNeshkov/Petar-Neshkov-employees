import { APP_CONSTANTS } from '../../core/constants/app.constants';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { AnalysisResponse } from '../models/analysis-response';

@Injectable({ providedIn: 'root' })
export class EmployeePairsService {
  constructor(private readonly api: ApiService) { }

  analyze(file: File): Observable<AnalysisResponse> {
    const form = new FormData();
    form.append(APP_CONSTANTS.FILE_FIELD, file);

    return this.api.post<AnalysisResponse>(APP_CONSTANTS.ANALYZE_URL, form);
  }
}
