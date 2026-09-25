import { APP_CONSTANTS } from '../../../core/constants/app.constants';
import {Component, DestroyRef, inject} from '@angular/core';
import {HttpErrorResponse} from '@angular/common/http';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {finalize} from 'rxjs';
import {AnalysisResponse} from '../../models/analysis-response';
import {getAnalysisErrorMessages} from '../../services/analysis-error-messages';
import {EmployeePairsService} from '../../services/employee-pairs.service';
import {validateCsvFile} from '../../validation/csv-file.validation';

@Component({
  selector: 'app-employee-upload',
  templateUrl: './employee-upload.component.html',
  styleUrl: './employee-upload.component.css'
})
export class EmployeeUploadComponent {
  readonly labels = APP_CONSTANTS;
  readonly acceptedFileTypes = APP_CONSTANTS.ACCEPTED_FILE_TYPES;
  private readonly destroyRef = inject(DestroyRef);
  private dragDepth = 0;

  loading = false;
  isDragging = false;
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
    this.uploadFile(file);
  }

  onDragEnter(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    if (this.loading) return;

    this.dragDepth++;
    this.isDragging = true;
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragDepth = Math.max(0, this.dragDepth - 1);
    this.isDragging = this.dragDepth > 0;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragDepth = 0;
    this.isDragging = false;
    if (this.loading) return;

    const files = event.dataTransfer?.files;
    if (!files?.length) return;

    if (files.length !== 1) {
      this.fileName = '';
      this.response = null;
      this.errors = [APP_CONSTANTS.SINGLE_FILE_REQUIRED];
      return;
    }

    this.uploadFile(files[0]);
  }

  private uploadFile(file: File): void {
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
