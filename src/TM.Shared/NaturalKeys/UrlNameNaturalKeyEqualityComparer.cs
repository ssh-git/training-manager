using System;
using System.Collections.Generic;

namespace TM.Shared
{
   public class UrlNameNaturalKeyEqualityComparer<TKey> : EqualityComparer<TKey>
      where TKey : IUrlNameNaturalKey
   {
      private static readonly UrlNameNaturalKeyEqualityComparer<TKey> Comparer = new UrlNameNaturalKeyEqualityComparer<TKey>();

      private UrlNameNaturalKeyEqualityComparer()
      {
      }

      public static UrlNameNaturalKeyEqualityComparer<TKey> Instance
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
            return string.Equals(x.UrlName, y.UrlName, StringComparison.Ordinal);
         }
         return false;
      }

      public override int GetHashCode(TKey obj)
      {
         return obj != null && obj.UrlName != null
            ? obj.UrlName.GetHashCode()
            : 0;
      }
   }
}
