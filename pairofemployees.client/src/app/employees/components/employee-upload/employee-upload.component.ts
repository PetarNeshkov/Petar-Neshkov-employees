import {Component, DestroyRef, inject} from '@angular/core';
import {HttpErrorResponse} from '@angular/common/http';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {finalize} from 'rxjs';
import {AnalysisResponse} from '../../models/analysis-response';
import {getAnalysisErrorMessages} from '../../services/analysis-error-messages';
import {EmployeePairsService} from '../../services/employee-pairs.service';
import {validateCsvFile} from '../../validation/csv-file.validation';
import { UPLOAD_CONSTANTS } from '../../../core/constants/upload.constants';

@Component({
  selector: 'app-employee-upload',
  templateUrl: './employee-upload.component.html',
  styleUrl: './employee-upload.component.css'
})
export class EmployeeUploadComponent {
  readonly acceptedFileTypes = UPLOAD_CONSTANTS.ACCEPTED_FILE_TYPES;
  private readonly destroyRef = inject(DestroyRef);

  loading = false;
  fileName = '';
  errors: string[] = [];
  response: AnalysisResponse | null = null;

  constructor(private readonly employeePairs: EmployeePairsService) {
  }

  onFileSelected(event: Event): void {
    if (this.loading){
      return;
    }

    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    input.value = '';
    this.fileName = file.name;
    this.response = null;
    this.errors = [];

    const validationError = validateCsvFile(file);
    if (validationError) {
      this.errors = [validationError];

      return;
    }

    this.loading = true;
    this.employeePairs.analyze(file).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading = false)
    ).subscribe({
      next: response => this.response = response,
      error: (error: HttpErrorResponse) => this.errors = getAnalysisErrorMessages(error)
    });
  }
}
