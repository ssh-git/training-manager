using System;
using System.Collections.Generic;
using TM.Shared;

namespace TM.Data
{
   public static class EnumExtensions
   {
      public static IEnumerable<TEnum> GetFlags<TEnum>(this Enum source) where TEnum 
         : struct, IConvertible
      {
         var sourceValue = Convert.ToUInt64(source);

         var enumType = source.GetType();
         var bitCount = GetEnumUnderlineTypeBitCount(source);

         for (int i = 0; i < bitCount; i++)
         {
            var number = (ulong)1 << i;
            if ((sourceValue & number) == number)
            {
               yield return (TEnum)Enum.Parse(enumType, number.ToString());
            }
         }
      }

      public static IEnumerable<Specializations> GetFlags(this Specializations specializations) 
      {
         var bitCount = GetEnumUnderlineTypeBitCount(specializations);
        
         for (var i = 0; i < bitCount; i++)
         {
            var number = 1 << i;
            var numberAsSpecializations = (Specializations)number;
            if ((specializations & numberAsSpecializations) == numberAsSpecializations)
            {
               yield return numberAsSpecializations;
            }
         }
      }


      private static int GetEnumUnderlineTypeBitCount(Enum @enum)
      {
         var bitCount = 0;

         switch (@enum.GetTypeCode())
         {
            case TypeCode.SByte:
            case TypeCode.Byte:
               bitCount = 8;
               break;
            case TypeCode.Int16:
               break;
            case TypeCode.UInt16:
               bitCount = 16;
               break;
            case TypeCode.Int32:
               break;
            case TypeCode.UInt32:
               bitCount = 32;
               break;
            case TypeCode.Int64:
               break;
            case TypeCode.UInt64:
               bitCount = 64;
               break;
            default:
               throw new ArgumentOutOfRangeException();
         }

         return bitCount;
      }
   }
}
