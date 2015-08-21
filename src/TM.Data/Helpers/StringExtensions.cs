using System;
using System.Text.RegularExpressions;

namespace TM.Data
{
   public static class StringExtensions
   {
      /// <exception cref="ArgumentNullException"><paramref name="input" /> or <paramref name="separator" /> is null.</exception>
      public static string SeparateWords(this string input, string separator = " ")
      {
         return StringSeparator.Separate(input, separator);
      }

      private static class StringSeparator
      {
         private static readonly Regex Regex;

         static StringSeparator()
         {
            const string pattern = @"
                (?<!^) # Not start
                (
                    # Digit, not preceded by another digit
                    (?<!\d)\d 
                    |
                    # Upper-case letter, followed by lower-case letter if
                    # preceded by another upper-case letter, e.g. 'G' in HTMLGuide
                    (?(?<=[A-Z])[A-Z](?=[a-z])|[A-Z])
                )";
            Regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
         }

         /// <exception cref="ArgumentNullException"><paramref name="input" /> or <paramref name="separator" /> is null.</exception>
         public static string Separate(string input, string separator)
         {
            return Regex.Replace(input, separator + "$1");
         }
      }
   }
}
