using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TM.Shared;

namespace TM.Data
{
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   public class TrainingProvider
   {
      public int Id { get; set; }

      [Required, StringLength(50)]
      public string Name { get; set; }

      [StringLength(1000), DataType(DataType.MultilineText)]
      public string Description { get; set; }

      [Required, StringLength((int)StringLengthConstraint.Url), DataType(DataType.Url)]
      public string SiteUrl { get; set; }

      [Required, StringLength(50)]
      public string LogoFileName { get; set; }

      public short UpdateFrequencyHours { get; set; }

      [DataType(DataType.Duration)]
      public TimeSpan UpdateFrequency
      {
         get { return TimeSpan.FromHours(UpdateFrequencyHours); }
         set { UpdateFrequencyHours = Convert.ToInt16(value.TotalHours); }
      }

      [Required, StringLength(100)]
      public string AllowedUpdateUtcHoursString { get; set; }


      /// <exception cref="InvalidOperationException" accessor="get"><see cref="AllowedUpdateUtcHoursString"/> cannot be null or whitespace</exception>
      /// <exception cref="ArgumentNullException" accessor="set"><paramref name="value"/> is <see langword="null" />.</exception>
      public List<int> AllowedUpdateUtcHours
      {
         get
         {
            if (string.IsNullOrWhiteSpace(AllowedUpdateUtcHoursString))
            {
               throw new InvalidOperationException("AllowedUpdateUtcHoursString property is null or whitespace");
            }

            var hoursList = AllowedUpdateUtcHoursString
               .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(x => int.Parse(x.Trim()))
               .ToList();

            return hoursList;
         }
         set
         {
            if (value == null)
            {
               throw new ArgumentNullException("value");
            }

            var allowedHoursString = string.Join(";", value);
            AllowedUpdateUtcHoursString = allowedHoursString;

         }
      }

      [Required, StringLength((int)StringLengthConstraint.Url), DataType(DataType.Url)]
      public string SourceUrl { get; set; }

      public LocationType SourceLocation { get; set; }

      [Required, StringLength(200)]
      public string AssemblyType { get; set; }

      public bool IsDeleted { get; set; }

      public virtual ICollection<Category> Categories { get; set; }
      public virtual ICollection<Course> Courses { get; set; }
      public virtual ICollection<TrainingProviderAuthor> TrainingProviderAuthors { get; set; }
   }
}
