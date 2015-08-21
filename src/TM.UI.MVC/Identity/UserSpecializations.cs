using System.Collections.Generic;
using TM.Shared;

namespace TM.UI.MVC
{
   public class UserSpecializations
   {
      public string UserId { get; set; }
      public Specializations Specialization { get; set; }

      private sealed class SpecializationEqualityComparer : IEqualityComparer<UserSpecializations>
      {
         public bool Equals(UserSpecializations x, UserSpecializations y)
         {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            
            return x.Specialization == y.Specialization;
         }

         public int GetHashCode(UserSpecializations obj)
         {
            return (int) obj.Specialization;
         }
      }

      private static readonly IEqualityComparer<UserSpecializations> SpecializationComparerInstance = new SpecializationEqualityComparer();

      public static IEqualityComparer<UserSpecializations> SpecializationComparer
      {
         get { return SpecializationComparerInstance; }
      }
   }
}