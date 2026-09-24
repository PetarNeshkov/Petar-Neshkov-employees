import { APP_CONSTANTS } from '../../core/constants/app.constants';

export function validateCsvFile(file: File): string | null {
  if (!file.name.toLowerCase().endsWith(APP_CONSTANTS.CSV_EXTENSION)) {
    return APP_CONSTANTS.CSV_EXTENSION_REQUIRED;
  }

  if (file.size === 0) {
    return APP_CONSTANTS.EMPTY_FILE;
  }

  if (file.size > APP_CONSTANTS.MAX_FILE_BYTES) {
    return APP_CONSTANTS.FILE_TOO_LARGE;
  }

  return null;
}
