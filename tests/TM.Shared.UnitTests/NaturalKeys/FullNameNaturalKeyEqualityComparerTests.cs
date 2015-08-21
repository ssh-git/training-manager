using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;

using Xunit;

namespace TM.Shared.UnitTests
{
   [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
   [SuppressMessage("ReSharper", "UnusedMember.Global")]
   public class FullNameNaturalKeyEqualityComparerTests
   {
      private FullNameNaturalKeyEqualityComparer<IFullNameNaturalKey> Comparer
      {
         get { return FullNameNaturalKeyEqualityComparer<IFullNameNaturalKey>.Instance; }
      }

      private static class EqualityResult
      {
         public const bool AreEqual = true;
         public const bool AreNotEqual = false;
      }
      
      public static IEnumerable<object[]> TestData
      {
         get
         {
            yield return new object[]
            {
               null,
               null,
               EqualityResult.AreEqual
            };

            yield return new object[]
            {
               Mock.Of<IFullNameNaturalKey>(x=> x.FullName == (string)null),
               Mock.Of<IFullNameNaturalKey>(x=> x.FullName == (string)null),
               EqualityResult.AreEqual
            };

            yield return new object[]
            {
               null,
               Mock.Of<IFullNameNaturalKey>(x=> x.FullName == "john"),
               EqualityResult.AreNotEqual
            };

            yield return new object[]
            {
               Mock.Of<IFullNameNaturalKey>(x=> x.FullName == (string)null),
               Mock.Of<IFullNameNaturalKey>(x=> x.FullName == "john"),
               EqualityResult.AreNotEqual
            };


            yield return new object[]
            {
               Mock.Of<IFullNameNaturalKey>(x=> x.FullName == "john"),
               null,
               EqualityResult.AreNotEqual
            };

            yield return new object[]
            {
               Mock.Of<IFullNameNaturalKey>(x=> x.FullName == "john"),
               Mock.Of<IFullNameNaturalKey>(x=> x.FullName == (string)null),
               EqualityResult.AreNotEqual
            };

            yield return new object[]
            {
               Mock.Of<IFullNameNaturalKey>(x=> x.FullName == "john"),
               Mock.Of<IFullNameNaturalKey>(x=> x.FullName == "john"),
               EqualityResult.AreEqual
            };

            yield return new object[]
            {
               Mock.Of<IFullNameNaturalKey>(x=> x.FullName == "john1"),
               Mock.Of<IFullNameNaturalKey>(x=> x.FullName == "john2"),
               EqualityResult.AreNotEqual
            };
         }
      }

      [Theory]
      [MemberData("TestData")]
      public void Should_ProduceValidComparisonForTestData(IFullNameNaturalKey first, IFullNameNaturalKey second, bool expectedResult)
      {
         var actualResult = Comparer.Equals(first, second);


         Assert.Equal(expectedResult, actualResult);

         // for equals objects hash code must be equals
         if (actualResult == EqualityResult.AreEqual)
         {
            Assert.Equal(Comparer.GetHashCode(first), Comparer.GetHashCode(second));
         }
      }
   }
}
