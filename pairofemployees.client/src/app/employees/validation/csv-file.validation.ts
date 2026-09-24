import { ERROR_MESSAGES } from '../../core/constants/error-messages';
import { UPLOAD_CONSTANTS } from '../../core/constants/upload.constants';

export function validateCsvFile(file: File): string | null {
  if (!file.name.toLowerCase().endsWith(UPLOAD_CONSTANTS.CSV_EXTENSION)) {
    return ERROR_MESSAGES.CSV_EXTENSION_REQUIRED;
  }

  if (file.size === 0) {
    return ERROR_MESSAGES.EMPTY_FILE;
  }

  if (file.size > UPLOAD_CONSTANTS.MAX_FILE_BYTES) {
    return ERROR_MESSAGES.FILE_TOO_LARGE;
  }

  return null;
}
