import { HttpParams, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ApiService } from './api.service';

describe('ApiService', () => {
  let api: ApiService;
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    api = TestBed.inject(ApiService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  for (const method of ['get', 'delete'] as const) {
    it(`forwards ${method} query parameters and response`, () => {
      const next = jasmine.createSpy('next');
      api[method]('/resource', new HttpParams().set('id', '2')).subscribe(next);
      const request = http.expectOne('/resource?id=2');
      expect(request.request.method).toBe(method.toUpperCase());
      request.flush({ ok: true });
      expect(next).toHaveBeenCalledOnceWith({ ok: true });
    });
  }

  for (const method of ['post', 'patch'] as const) {
    for (const body of [undefined, { name: 'example' }]) {
      it(`forwards ${method} with ${body ? 'explicit' : 'default'} body`, () => {
        api[method]('/resource', body).subscribe();
        const request = http.expectOne('/resource');
        expect(request.request.method).toBe(method.toUpperCase());
        expect(request.request.body).toEqual(body ?? {});
        request.flush(null);
      });
    }
  }
});
