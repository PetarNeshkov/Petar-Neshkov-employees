import { provideHttpClient, HttpErrorResponse } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { APP_CONSTANTS as C } from '../../core/constants/app.constants';
import { EmployeePairsService } from './employee-pairs.service';

describe('EmployeePairsService', () => {
  let service: EmployeePairsService;
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(EmployeePairsService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('posts the file as multipart data and returns the server response', () => {
    const file = new File(['csv'], 'employees.csv');
    const next = jasmine.createSpy('next');
    service.analyze(file).subscribe(next);
    const request = http.expectOne(C.ANALYZE_URL);
    expect(request.request.method).toBe('POST');
    expect(request.request.body instanceof FormData).toBeTrue();
    expect(request.request.body.get(C.FILE_FIELD)).toBe(file);
    expect(request.request.headers.has('Content-Type')).toBeFalse();
    request.flush({ pair: null });
    expect(next).toHaveBeenCalledOnceWith({ pair: null });
  });

  it('preserves the status and validation body on errors', () => {
    const error = jasmine.createSpy('error');
    service.analyze(new File(['csv'], 'employees.csv')).subscribe({ error });
    const body = { errors: { file: ['invalid row'] } };
    http.expectOne(C.ANALYZE_URL).flush(body, { status: 400, statusText: 'Bad Request' });
    const actual = error.calls.mostRecent().args[0] as HttpErrorResponse;
    expect(actual.status).toBe(400);
    expect(actual.error).toEqual(body);
  });

  it('cancels the HTTP request when the subscriber unsubscribes', () => {
    const subscription = service.analyze(new File(['csv'], 'employees.csv')).subscribe();
    const request = http.expectOne(C.ANALYZE_URL);
    subscription.unsubscribe();
    expect(request.cancelled).toBeTrue();
  });
});
