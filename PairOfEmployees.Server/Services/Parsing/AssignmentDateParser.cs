using System.Globalization;
using PairOfEmployees.Server.Constants;

namespace PairOfEmployees.Server.Services.Parsing;

public sealed class AssignmentDateParser
{
    public bool TryParse(string value, out DateOnly date)
    {
        var input = value.Trim();

        foreach (var format in AppConstants.DateFormats)
        {
            if (DateOnly.TryParseExact(input, format, 
                    AppConstants.BulgarianCulture, DateTimeStyles.AllowWhiteSpaces, out date))
            {
                return true;
            }
        }

        foreach (var formats in AppConstants.Groups)
        {
            foreach (var format in formats)
            {
                if (DateOnly.TryParseExact(input, format,
                        CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out date))
                {
                    return true;
                }
            }
        }

        date = default;
        return false;
    }
}
