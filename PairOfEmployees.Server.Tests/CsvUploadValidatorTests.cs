using PairOfEmployees.Server.Constants;
using PairOfEmployees.Server.Exceptions;
using PairOfEmployees.Server.Validation;

namespace PairOfEmployees.Server.Tests;

public sealed class CsvUploadValidatorTests
{
    private readonly CsvUploadValidator _validator = new();

    [Theory]
    [InlineData(null, 1, AppConstants.FileRequired)]
    [InlineData("", 1, AppConstants.FileRequired)]
    [InlineData("employees.csv", 0, AppConstants.FileRequired)]
    [InlineData("employees.txt", 1, AppConstants.CsvExtensionRequired)]
    [InlineData("employees.csv.exe", 1, AppConstants.CsvExtensionRequired)]
    [InlineData("employees.csv", AppConstants.MaxFileBytes + 1, AppConstants.FileTooLarge)]
    [InlineData("employees.csv", 1, null)]
    [InlineData("employees.CSV", AppConstants.MaxFileBytes, null)]
    public void Validate_FileMetadata_ReturnsExpectedError(string? name, long length, string? expected)
    {
        Assert.Equal(expected, _validator.Validate(name, length));
    }

    [Fact]
    public void ValidateAndThrow_MissingFile_ReportsFileField()
    {
        var error = Assert.Throws<RequestValidationException>(() => _validator.ValidateAndThrow(null));
        
        Assert.Equal(new[] { AppConstants.FileRequired }, error.Errors[AppConstants.FileField]);
    }
}
