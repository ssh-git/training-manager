using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TM.Shared
{
   public sealed class ReferenceEqualityComparer :IEqualityComparer<object>
   {
      private static readonly ReferenceEqualityComparer Comparer = new ReferenceEqualityComparer();

      private ReferenceEqualityComparer()
      {
      }

      public static ReferenceEqualityComparer Instance
      {
         get { return Comparer; }
      }

      bool IEqualityComparer<object>.Equals(object x, object y)
      {
        return ReferenceEquals(x, y);
      }

      int IEqualityComparer<object>.GetHashCode(object obj)
      {
         return RuntimeHelpers.GetHashCode(obj);
      }
   }
}
