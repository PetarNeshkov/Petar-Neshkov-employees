import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Subject } from 'rxjs';
import { APP_CONSTANTS as C } from '../../../core/constants/app.constants';
import { AnalysisResponse } from '../../models/analysis-response';
import { EmployeePairsService } from '../../services/employee-pairs.service';
import { EmployeeResultsComponent } from '../employee-results/employee-results.component';
import { EmployeeUploadComponent } from './employee-upload.component';

describe('EmployeeUploadComponent', () => {
  let fixture: ComponentFixture<EmployeeUploadComponent>;
  let component: EmployeeUploadComponent;
  let service: jasmine.SpyObj<EmployeePairsService>;
  let response: Subject<AnalysisResponse>;

  beforeEach(async () => {
    response = new Subject<AnalysisResponse>();
    service = jasmine.createSpyObj<EmployeePairsService>('EmployeePairsService', ['analyze']);
    service.analyze.and.returnValue(response);
    await TestBed.configureTestingModule({
      imports: [CommonModule],
      declarations: [EmployeeUploadComponent, EmployeeResultsComponent],
      providers: [{ provide: EmployeePairsService, useValue: service }]
    }).compileComponents();
    fixture = TestBed.createComponent(EmployeeUploadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  function select(file?: File): HTMLInputElement {
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input[type=file]');
    const transfer = new DataTransfer();
    if (file) transfer.items.add(file);
    input.files = transfer.files;
    input.dispatchEvent(new Event('change'));
    fixture.detectChanges();
    return input;
  }

  function drag(type: string, files: File[] = []): DragEvent {
    const dataTransfer = new DataTransfer();
    files.forEach(file => dataTransfer.items.add(file));
    const event = new DragEvent(type, { dataTransfer, bubbles: true, cancelable: true });
    fixture.nativeElement.querySelector('.drop-zone').dispatchEvent(event);
    fixture.detectChanges();
    expect(event.defaultPrevented).toBeTrue();
    return event;
  }

  const csv = () => new File(['1,10,2024-01-01,NULL'], 'employees.csv');

  it('starts an upload from file input, clears stale state and shows loading', () => {
    component.errors = ['old'];
    component.response = { pair: null };
    const file = csv();
    const input = select(file);
    expect(input.value).toBe('');
    expect(service.analyze).toHaveBeenCalledOnceWith(file);
    expect(component.errors).toEqual([]);
    expect(component.response).toBeNull();
    expect(component.fileName).toBe(file.name);
    expect(fixture.nativeElement.querySelector('button').disabled).toBeTrue();
    expect(fixture.nativeElement.querySelector('[role=status]')).not.toBeNull();
    response.next({ pair: null });
    response.complete();
    fixture.detectChanges();
    expect(component.loading).toBeFalse();
    expect(fixture.nativeElement.textContent).toContain('No shared working time found');
  });

  it('ignores a canceled file selection', () => {
    select();
    expect(service.analyze).not.toHaveBeenCalled();
    expect(component.errors).toEqual([]);
  });

  it('rejects an invalid file without calling the service', () => {
    select(new File(['x'], 'bad.txt'));
    expect(service.analyze).not.toHaveBeenCalled();
    expect(component.loading).toBeFalse();
    expect(fixture.nativeElement.querySelector('[role=alert]').textContent).toContain(C.CSV_EXTENSION_REQUIRED);
  });

  it('shows API validation errors, ends loading and allows retry', () => {
    select(csv());
    response.error(new HttpErrorResponse({ status: 400, error: { errors: { file: ['Row 2 is invalid'] } } }));
    fixture.detectChanges();
    expect(component.loading).toBeFalse();
    expect(fixture.nativeElement.querySelector('[role=alert]').textContent).toContain('Row 2 is invalid');
    response = new Subject<AnalysisResponse>();
    service.analyze.and.returnValue(response);
    select(csv());
    expect(service.analyze).toHaveBeenCalledTimes(2);
    expect(component.errors).toEqual([]);
  });

  it('ignores selections and drops while loading', () => {
    select(csv());
    select(csv());
    drag('dragenter');
    expect(component.isDragging).toBeFalse();
    drag('drop', [csv()]);
    expect(service.analyze).toHaveBeenCalledTimes(1);
  });

  it('keeps drag styling until the final nested drag leaves', () => {
    drag('dragenter');
    drag('dragenter');
    drag('dragover');
    drag('dragleave');
    expect(component.isDragging).toBeTrue();
    drag('dragleave');
    drag('dragleave');
    expect(component.isDragging).toBeFalse();
    drag('dragenter');
    expect(component.isDragging).toBeTrue();
  });

  it('uploads a single dropped file and clears drag styling', () => {
    const file = csv();
    drag('dragenter');
    drag('drop', [file]);
    expect(service.analyze).toHaveBeenCalledOnceWith(file);
    expect(component.isDragging).toBeFalse();
  });

  it('rejects multiple dropped files and clears previous results', () => {
    component.response = { pair: null };
    component.fileName = 'previous.csv';
    drag('drop', [csv(), csv()]);
    expect(service.analyze).not.toHaveBeenCalled();
    expect(component.errors).toEqual([C.SINGLE_FILE_REQUIRED]);
    expect(component.response).toBeNull();
    expect(component.fileName).toBe('');
  });

  it('ignores a drop with no files', () => {
    drag('drop');
    expect(service.analyze).not.toHaveBeenCalled();
  });

  it('unsubscribes from an in-flight analysis when destroyed', () => {
    select(csv());
    expect(response.observed).toBeTrue();
    fixture.destroy();
    expect(response.observed).toBeFalse();
    expect(component.loading).toBeFalse();
  });

  it('opens the file picker when the choose button is clicked', () => {
    const input: HTMLInputElement = fixture.nativeElement.querySelector('input');
    const click = spyOn(input, 'click');
    fixture.nativeElement.querySelector('button').click();
    expect(click).toHaveBeenCalledTimes(1);
  });
});
