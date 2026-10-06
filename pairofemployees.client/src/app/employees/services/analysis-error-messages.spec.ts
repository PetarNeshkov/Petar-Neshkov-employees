import { HttpErrorResponse } from '@angular/common/http';
import { APP_CONSTANTS as C } from '../../core/constants/app.constants';
import { getAnalysisErrorMessages } from './analysis-error-messages';

describe('getAnalysisErrorMessages', () => {
  for (const [status, message] of [[0, C.CONNECTION_FAILED], [413, C.UPLOAD_TOO_LARGE],
    [500, C.SERVER_ERROR], [503, C.SERVER_ERROR]] as const) {
    it(`uses a safe message for HTTP ${status}`, () => {
      expect(getAnalysisErrorMessages(new HttpErrorResponse({
        status, error: { detail: 'internal detail', errors: { file: ['internal error'] } }
      }))).toEqual([message]);
    });
  }

  it('collects nonempty string validation errors across fields before detail or title', () => {
    const error = new HttpErrorResponse({ status: 400, error: {
      errors: { file: ['first', '', '  ', 42, null], other: ['second'], invalid: 'ignored' },
      detail: 'detail', title: 'title'
    } });
    expect(getAnalysisErrorMessages(error)).toEqual(['first', 'second']);
  });

  it('falls back to detail when validation errors contain no usable messages', () => {
    expect(getAnalysisErrorMessages(new HttpErrorResponse({ status: 400,
      error: { errors: { file: [] }, detail: 'detail', title: 'title' }
    }))).toEqual(['detail']);
  });

  it('falls back to title when detail is blank', () => {
    expect(getAnalysisErrorMessages(new HttpErrorResponse({ status: 400,
      error: { detail: ' ', title: 'title' }
    }))).toEqual(['title']);
  });

  for (const body of [null, '<html>proxy error</html>', [], {}, { detail: 3, title: ' ' },
    { errors: 'invalid' }, { errors: null }]) {
    it(`handles malformed response ${JSON.stringify(body)}`, () => {
      expect(getAnalysisErrorMessages(new HttpErrorResponse({ status: 400, error: body })))
        .toEqual([C.ANALYSIS_FAILED]);
    });
  }
});
