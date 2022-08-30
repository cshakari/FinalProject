using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Models
{
    public class ValidateISBN : ValidationAttribute
    {
        public ValidateISBN(string erroInfo)
            : base(erroInfo)
        {
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool result = false;
            if (value != null)
            {
                string isbn = value.ToString();

                if (!string.IsNullOrEmpty(isbn))
                {
                    switch (isbn.Length)
                    {
                        case 10: result = IsValidIsbn10(isbn); break;
                        case 13: result = IsValidIsbn13(isbn); break;
                        default: result = false; break;
                    }
        




                //long j;

                //    if (isbn.Contains('-'))
                //    {
                //        isbn = isbn.Replace("-", "");
                //    }
                //    // Check if it contains any non numeric chars, if yes, return false

                //    if (!Int64.TryParse(isbn, out j))
                //        result = false;
                //    int sum = 0;
                //    // The calculation of an valueString-13 check digit begins with the first 12 digits of the thirteen-digit valueString (thus excluding the check digit itself).
                //    // Each digit, from left to right, is alternately multiplied by 1 or 3, then those products are summed modulo 10 to give a value ranging from 0 to 9.
                //    // Subtracted from 10, that leaves a result from 1 to 10. A zero (0) replaces a ten (10), so, in all cases, a single check digit results.
                //    for (int i = 0; i < 12; i++)
                //    {
                //        sum += Int32.Parse(isbn[i].ToString()) * (i % 2 == 1 ? 3 : 1);
                //    }
                //    int remainder = sum % 10;
                //    int checkDigit = 10 - remainder;
                //    if (checkDigit == 10) checkDigit = 0;
                //    result = (checkDigit == int.Parse(isbn[12].ToString()));
                } 
            }

            if (result)
            {
                return ValidationResult.Success;
            }
            else
            {
                var errorMessage = FormatErrorMessage(validationContext.DisplayName);
                return new ValidationResult(this.ErrorMessage, new List<string>() { "ISBN"});
            }

        }

    private static bool IsValidIsbn10(string isbn)
    {
        int n = isbn.Length;
        if (n != 10)
            return false;

        // Computing weighted sum of
        // first 9 digits
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            int digit = isbn[i] - '0';

            if (0 > digit || 9 < digit)
                return false;

            sum += (digit * (10 - i));
        }

        // Checking last digit.
        char last = isbn[9];
        if (last != 'X' && (last < '0'
                            || last > '9'))
            return false;

        // If last digit is 'X', add 10
        // to sum, else add its value.
        sum += ((last == 'X') ? 10 :
                            (last - '0'));

        // Return true if weighted sum
        // of digits is divisible by 11.
        return (sum % 11 == 0);
    }

    private static bool IsValidIsbn13(string isbn13)
    {
            bool result = false;
            if (!string.IsNullOrEmpty(isbn13))
            {
                long j;

                if (isbn13.Contains('-'))
                {
                    isbn13 = isbn13.Replace("-", "");
                }
                // Check if it contains any non numeric chars, if yes, return false

                if (!Int64.TryParse(isbn13, out j))
                    return false;
                int sum = 0;
                // The calculation of an ISBN-13 check digit begins with the first 12 digits of the thirteen-digit ISBN (thus excluding the check digit itself).
                // Each digit, from left to right, is alternately multiplied by 1 or 3, then those products are summed modulo 10 to give a value ranging from 0 to 9.
                // Subtracted from 10, that leaves a result from 1 to 10. A zero (0) replaces a ten (10), so, in all cases, a single check digit results.
                for (int i = 0; i < 12; i++)
                {
                    sum += Int32.Parse(isbn13[i].ToString()) * (i % 2 == 1 ? 3 : 1);
                }
                int remainder = sum % 10;
                int checkDigit = 10 - remainder;
                if (checkDigit == 10) checkDigit = 0;
                result = (checkDigit == int.Parse(isbn13[12].ToString()));
            }
            return result;
        }

    }
}
