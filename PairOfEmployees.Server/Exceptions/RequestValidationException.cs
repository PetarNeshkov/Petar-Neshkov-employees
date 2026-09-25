using System.Collections.ObjectModel;
using PairOfEmployees.Server.Constants;

namespace PairOfEmployees.Server.Exceptions;

public class RequestValidationException : Exception
{
    public RequestValidationException(string field, params string[] errors)
        : this(new Dictionary<string, string[]> { [field] = errors })
    {
    }

    public RequestValidationException(IDictionary<string, string[]> errors)
        : base(AppConstants.InvalidRequest)
    {
        Errors = new ReadOnlyDictionary<string, string[]>(
            errors.ToDictionary(entry => entry.Key, entry => entry.Value.ToArray()));
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
