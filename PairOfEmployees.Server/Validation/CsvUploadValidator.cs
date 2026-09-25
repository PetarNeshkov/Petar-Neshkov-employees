using System.Diagnostics.CodeAnalysis;
using PairOfEmployees.Server.Constants;
using PairOfEmployees.Server.Exceptions;

namespace PairOfEmployees.Server.Validation;

public class CsvUploadValidator
{
    public void ValidateAndThrow([NotNull] IFormFile? file)
    {
        var error = Validate(file?.FileName, file?.Length ?? 0);
        if (file is null || error is not null)
        {
            throw new RequestValidationException(AppConstants.FileField, error ?? AppConstants.FileRequired);
        }
    }

    public string? Validate(string? fileName, long length)
    {
        if (string.IsNullOrEmpty(fileName) || length == 0)
        {
            return AppConstants.FileRequired;
        }

        if (!Path.GetExtension(fileName).Equals(AppConstants.Extension, StringComparison.OrdinalIgnoreCase))
        {
            return AppConstants.CsvExtensionRequired;
        }

        if (length > AppConstants.MaxFileBytes)
        {
            return AppConstants.FileTooLarge;
        }

        return null;
    }
}
