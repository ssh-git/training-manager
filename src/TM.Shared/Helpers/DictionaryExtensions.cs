using System.Collections.Generic;

namespace TM.Shared
{
   public static class DictionaryExtensions
   {
      public static Dictionary<TKey, TValue> GetShallowCopy<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
      {
         var comparer = dictionary.Comparer;
         var shallowCopy = new Dictionary<TKey, TValue>(dictionary, comparer);
         return shallowCopy;
      }
   }
}
