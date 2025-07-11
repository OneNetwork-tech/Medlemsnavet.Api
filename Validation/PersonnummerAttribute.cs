// File: Validation/PersonnummerAttribute.cs
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Medlemsnavet.Validation;

public class PersonnummerAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var pnr = Convert.ToString(value);

        if (string.IsNullOrWhiteSpace(pnr))
        {
            return new ValidationResult("Personnummer is required.");
        }

        // Regex to match YYYYMMDD-XXXX or YYYYMMDDXXXX format
        var regex = new Regex(@"^(\d{8})[-]?(\d{4})$");
        var match = regex.Match(pnr);

        if (!match.Success)
        {
            return new ValidationResult("Invalid Personnummer format. Use YYYYMMDD-XXXX.");
        }

        // Use only the numeric digits for Luhn check
        var digitsOnly = pnr.Replace("-", "").Substring(2); // Remove century and hyphen

        int sum = 0;
        for (int i = 0; i < digitsOnly.Length; i++)
        {
            var digit = int.Parse(digitsOnly[i].ToString());
            if (i % 2 == 0) // Every other digit starting from the first
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit -= 9;
                }
            }
            sum += digit;
        }

        if (sum % 10 != 0)
        {
            return new ValidationResult("Invalid Personnummer (checksum failed).");
        }

        return ValidationResult.Success;
    }
}