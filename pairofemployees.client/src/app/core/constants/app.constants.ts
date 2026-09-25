import { environment } from '../../../environments/environment';

export const APP_CONSTANTS = {
  ANALYZE_URL: `${environment.baseUrl}/api/EmployeePairs/Analyze`,

  FILE_FIELD: 'file',
  CSV_EXTENSION: '.csv',
  ACCEPTED_FILE_TYPES: '.csv,text/csv',
  MAX_FILE_BYTES: 1_048_576,
  DROP_ZONE_TITLE: 'Drag and drop your CSV file here',
  CHOOSE_FILE_LABEL: 'Choose CSV file',
  DROP_ZONE_HINT: 'Or use the button below. Analysis starts automatically.',
  SINGLE_FILE_REQUIRED: 'Choose or drop one CSV file at a time.',

  NETWORK_ERROR: 0,
  PAYLOAD_TOO_LARGE: 413,
  SERVER_ERROR_START: 500,

  CSV_EXTENSION_REQUIRED: 'Choose a file with a .csv extension.',
  EMPTY_FILE: 'The selected file is empty. Choose a CSV containing employee assignments.',
  FILE_TOO_LARGE: 'The file is too large. Choose a CSV no larger than 1 MiB.',
  CONNECTION_FAILED: 'Could not reach the server. Check your connection and choose the file again.',
  UPLOAD_TOO_LARGE: 'The upload is too large. Choose a CSV file no larger than 1 MiB.',
  SERVER_ERROR: 'The server could not analyze the file. Please try again.',
  ANALYSIS_FAILED: 'The file could not be analyzed. Check its contents and try again.'
};
