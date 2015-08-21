using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TM.Data.Properties;
using TM.Shared;

namespace TM.Data
{
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   public class Author
   {
      public Author()
      {
         Social = new Social();
         Badge = new AuthorBadge();
         Avatar = new AuthorAvatar();
      }

      public int Id { get; set; }

      public string FullName { get { return string.Concat(FirstName, " ", LastName); } }

      [Required, StringLength(100)]
      public string FirstName { get; set; }

      [Required, StringLength(100)]
      public string LastName { get; set; }

      [StringLength(3500)]
      public string Bio { get; set; }
      public Social Social { get; set; }
      public AuthorBadge Badge { get; set; }
      public AuthorAvatar Avatar { get; set; }

      public bool IsDeleted { get; set; }
      public byte[] RowVersion { get; set; }

      public virtual ICollection<TrainingProviderAuthor> AuthorTrainingProviders { get; set; }
      public virtual ICollection<CourseAuthor> AuthorCourses { get; set; }

      #region Helpers

      /// <exception cref="ArgumentNullException"><paramref name="uriString" /> is null. </exception>
      /// <exception cref="UriFormatException">NoteIn the .NET for Windows Store apps or the Portable Class Library, catch the base class exception, <see cref="T:System.FormatException" />, instead.<paramref name="uriString" /> is empty.-or- The scheme specified in <paramref name="uriString" /> is not correctly formed. See <see cref="M:System.Uri.CheckSchemeName(System.String)" />.-or- <paramref name="uriString" /> contains too many slashes.-or- The password specified in <paramref name="uriString" /> is not valid.-or- The host name specified in <paramref name="uriString" /> is not valid.-or- The file name specified in <paramref name="uriString" /> is not valid. -or- The user name specified in <paramref name="uriString" /> is not valid.-or- The host or authority name specified in <paramref name="uriString" /> cannot be terminated by backslashes.-or- The port number specified in <paramref name="uriString" /> is not valid or cannot be parsed.-or- The length of <paramref name="uriString" /> exceeds 65519 characters.-or- The length of the scheme specified in <paramref name="uriString" /> exceeds 1023 characters.-or- There is an invalid character sequence in <paramref name="uriString" />.-or- The MS-DOS path specified in <paramref name="uriString" /> must start with c:\\.</exception>
      /// <exception cref="ArgumentException"><paramref name="uriString" /> uri path contains empty file name</exception>
      public static string GetUrlName(string uriString)
      {
         var uri = new Uri(uriString);
         if (uri.Segments.Last().EndsWith("/"))
         {
            throw new ArgumentException(Resources.EmptyUriFileName, "uriString");
         }

         var urlName = uri.Segments.Last();

         return urlName;
      }


      public static string GetFullName(string firstName, string lastName)
      {
         var fullName = firstName.Trim() + " " + lastName.Trim();
         return fullName;
      }


      /// <exception cref="ArgumentNullException">fullName is null. </exception>
      /// <exception cref="InvalidOperationException">Invalid fullName</exception>
      public static string GetFirstName(string fullName)
      {
         var fullNameParts = GetFullNameParts(fullName);

         // ReSharper disable once ExceptionNotDocumented
         var partsCount = fullNameParts.Length;

         // last name must be single word; first name could be many word
         fullNameParts[partsCount - 1] = string.Empty;
         var firstName = string.Join(" ", fullNameParts);

         firstName = firstName.Trim();
         return firstName;
      }


      /// <exception cref="ArgumentException">fullName is null or whitespace.</exception>
      /// <exception cref="InvalidOperationException">Invalid fullName.</exception>
      public static string GetLastName(string fullName)
      {
         var fullNameParts = GetFullNameParts(fullName);

         // ReSharper disable once ExceptionNotDocumented
         var partsCount = fullNameParts.Length;

         // last name must be single word; first name could be many word
         var lastName = fullNameParts[partsCount - 1];

         lastName = lastName.Trim();
         return lastName;
      }


      /// <exception cref="ArgumentException">fullName is null or whitespace.</exception>
      /// <exception cref="InvalidOperationException">Invalid fullName.</exception>
      private static string[] GetFullNameParts(string fullName)
      {
         if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException(Resources.ArgumentNullOrWhitespace_FullName, fullName);

         var fullNameParts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
         var partsCount = fullNameParts.Length;

         if (partsCount < 2) throw new InvalidOperationException(Resources.InvalidOperation_InvalidFullName);


         if (fullNameParts[partsCount - 1].EndsWith(")", StringComparison.Ordinal))
         {
            var leftParenthesisIndex = fullName.IndexOf("(", StringComparison.Ordinal);
            fullNameParts = fullName.Substring(0, leftParenthesisIndex).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
         }
         return fullNameParts;
      }

      #endregion
   }
}