import { APP_CONSTANTS as C } from '../../core/constants/app.constants';
import { validateCsvFile } from './csv-file.validation';

describe('validateCsvFile', () => {
  const cases: [string, number, string | null][] = [
    ['employees.csv', 1, null],
    ['employees.CSV', C.MAX_FILE_BYTES, null],
    ['employees.txt', 1, C.CSV_EXTENSION_REQUIRED],
    ['employees.csv.exe', 1, C.CSV_EXTENSION_REQUIRED],
    ['employees.csv', 0, C.EMPTY_FILE],
    ['employees.csv', C.MAX_FILE_BYTES + 1, C.FILE_TOO_LARGE]
  ];
  for (const [name, size, expected] of cases) {
    it(`validates ${name} with ${size} bytes`, () => {
      expect(validateCsvFile(new File([new Uint8Array(size)], name))).toBe(expected);
    });
  }
});
