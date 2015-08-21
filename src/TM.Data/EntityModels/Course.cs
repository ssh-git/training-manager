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
   public class Course : ICourseUrlNameNaturalKey
   {
      public Course()
      {
         CourseSpecializations = new List<CourseSpecialization>();
      }
      public int Id { get; set; }
      public int? ReplacementCourseId { get; set; }
      public int TrainingProviderId { get; set; }
      public int CategoryId { get; set; }

      [Required, StringLength(100)]
      public string Title { get; set; }

      [Required, StringLength((int)StringLengthConstraint.Url)]
      public string SiteUrl { get; set; }

      [Required, StringLength((int)StringLengthConstraint.UrlName)]
      public string UrlName { get; set; }

      [StringLength(3500)]
      public string Description { get; set; }

      [StringLength(3500)]
      public string ShortDescription { get; set; }
    
      public bool HasClosedCaptions { get; set; }
      public CourseLevel Level { get; set; }

      public CourseRating Rating { get; set; }
      
      public TimeSpan Duration { get; set; }
      
      public DateTime ReleaseDate { get; set; }

      public bool IsRetired { get; set; }
      
      public bool IsDeleted { get; set; }


      public Specializations Specializations
      {
         get
         {
            var specializations = CourseSpecializations
               .Aggregate(default(Specializations),
                  (accumulator, courseSpecialization) =>
                     accumulator | courseSpecialization.Specialization);

            return specializations;
         }

         set
         {
            var specializationSet = new HashSet<Specializations>(value.GetFlags());
            var specializationsCopy = new List<CourseSpecialization>(CourseSpecializations);

            foreach (var courseSpecialization in specializationsCopy)
            {
               if (specializationSet.Contains(courseSpecialization.Specialization))
               {
                  specializationSet.Remove(courseSpecialization.Specialization);
               } else
               {
                  CourseSpecializations.Remove(courseSpecialization);
               }
            }

            foreach (var specialization in specializationSet)
            {
               CourseSpecializations.Add(CourseSpecialization.Create(this, specialization));
            }
         }
      }

      public ICollection<CourseSpecialization> CourseSpecializations { get; set; }

      
      public virtual TrainingProvider TrainingProvider { get; set; }
      public virtual Category Category { get; set; }
      public virtual Course ReplacementCourse { get; set; }
      public virtual ICollection<CourseAuthor> CourseAuthors { get; set; }
      public virtual ICollection<Subscription> Subscriptions { get; set; }
      public virtual ICollection<Module> Modules { get; set; }

      #region Equality Comparers

      private sealed class PropertiesEqualityComparer : IEqualityComparer<Course>
      {
         public bool Equals(Course x, Course y)
         {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            return string.Equals(x.Title, y.Title) &&
               string.Equals(x.SiteUrl, y.SiteUrl) &&
                   string.Equals(x.UrlName, y.UrlName) &&
                   string.Equals(x.Description, y.Description) &&
                   string.Equals(x.ShortDescription, y.ShortDescription) &&
                   x.HasClosedCaptions == y.HasClosedCaptions &&
                   x.Level == y.Level &&
                   Equals(x.Rating, y.Rating) &&
                   x.Duration.Equals(y.Duration) &&
                   x.ReleaseDate.Equals(y.ReleaseDate) && 
                   x.IsRetired == y.IsRetired &&
                   x.IsDeleted == y.IsDeleted &&
                   x.Specializations == y.Specializations;
         }

         public int GetHashCode(Course obj)
         {
            unchecked
            {
               var hashCode = (obj.Title != null ? obj.Title.GetHashCode() : 0);
               hashCode = (hashCode*397) ^ (obj.SiteUrl != null ? obj.SiteUrl.GetHashCode() : 0);
               hashCode = (hashCode*397) ^ (obj.UrlName != null ? obj.UrlName.GetHashCode() : 0);
               hashCode = (hashCode*397) ^ (obj.Description != null ? obj.Description.GetHashCode() : 0);
               hashCode = (hashCode*397) ^ (obj.ShortDescription != null ? obj.ShortDescription.GetHashCode() : 0);
               hashCode = (hashCode*397) ^ obj.HasClosedCaptions.GetHashCode();
               hashCode = (hashCode*397) ^ (int) obj.Level;
               hashCode = (hashCode*397) ^ (obj.Rating != null ? obj.Rating.GetHashCode() : 0);
               hashCode = (hashCode*397) ^ obj.Duration.GetHashCode();
               hashCode = (hashCode*397) ^ obj.ReleaseDate.GetHashCode();
               hashCode = (hashCode*397) ^ obj.IsRetired.GetHashCode();
               hashCode = (hashCode*397) ^ obj.IsDeleted.GetHashCode();
               hashCode = (hashCode*397) ^ obj.Specializations.GetHashCode();
               return hashCode;
            }
         }
      }

      private static readonly IEqualityComparer<Course> PropertiesComparerInstance = new PropertiesEqualityComparer();

      public static IEqualityComparer<Course> PropertiesComparer
      {
         get { return PropertiesComparerInstance; }
      }

      #endregion
   }
}