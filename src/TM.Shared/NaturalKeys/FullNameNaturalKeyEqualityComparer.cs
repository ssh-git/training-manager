using System;
using System.Collections.Generic;

namespace TM.Shared
{
   public class FullNameNaturalKeyEqualityComparer<TKey> : EqualityComparer<TKey>
      where TKey:IFullNameNaturalKey
   {
      private static readonly FullNameNaturalKeyEqualityComparer<TKey> Comparer = new FullNameNaturalKeyEqualityComparer<TKey>();

      private FullNameNaturalKeyEqualityComparer()
      {
      }

      public static FullNameNaturalKeyEqualityComparer<TKey> Instance
      {
         get { return Comparer; }
      }

      public override bool Equals(TKey x, TKey y)
      {
         if (ReferenceEquals(x, y))
         {
            return true;
         }
         if ((x != null) && (y != null))
         {
            return string.Equals(x.FullName, y.FullName, StringComparison.Ordinal);
         }
         return false;
      }

      public override int GetHashCode(TKey obj)
      {
         return obj != null && obj.FullName != null
            ? obj.FullName.GetHashCode()
            : 0;
      }
   }
}
