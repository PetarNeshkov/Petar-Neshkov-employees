const maxFileBytes = 1_048_576;

export function validateCsvFile(file: File): string | null {
  if (!file.name.toLowerCase().endsWith('.csv')) {
    return 'Choose a file with a .csv extension.';
  }

  if (file.size === 0) {
    return 'The selected file is empty. Choose a CSV containing employee assignments.';
  }

  if (file.size > maxFileBytes) {
    return 'The file is too large. Choose a CSV no larger than 1 MiB.';
  }

  return null;
}
