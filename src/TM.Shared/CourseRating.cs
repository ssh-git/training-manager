using System;
using System.Diagnostics;

namespace TM.Shared
{
   [DebuggerDisplay("Rating = {Rating} from {Raters} users")]
   public class CourseRating : IEquatable<CourseRating>
   {
      public int Raters { get; set; }
      public decimal Rating { get; set; }


      public bool Equals(CourseRating other)
      {
         if (ReferenceEquals(null, other)) return false;
         if (ReferenceEquals(this, other)) return true;
         return Raters == other.Raters && Rating == other.Rating;
      }

      public override bool Equals(object obj)
      {
         if (ReferenceEquals(null, obj)) return false;
         if (ReferenceEquals(this, obj)) return true;
         if (obj.GetType() != this.GetType()) return false;
         return Equals((CourseRating) obj);
      }

      public override int GetHashCode()
      {
         unchecked
         {
            return (Raters*397) ^ Rating.GetHashCode();
         }
      }

      public static bool operator ==(CourseRating left, CourseRating right)
      {
         return Equals(left, right);
      }

      public static bool operator !=(CourseRating left, CourseRating right)
      {
         return !Equals(left, right);
      }
   }
}