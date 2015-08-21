using System;
using System.Globalization;

namespace TM.UI.MVC.Helpers
{
   public static class StringExtensions
   {
      /// <exception cref="ArgumentNullException"><paramref name="source" /> is null. </exception>
      public static string ToTitleCaseInvariant(this string source)
      {
         var atTitleCase = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(source);
         return atTitleCase;
      }
   }
}