using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using Xunit;

namespace TM.Shared.UnitTests
{
   [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
   [SuppressMessage("ReSharper", "UnusedMember.Global")]
   public class UrlNameNaturalKeyEqualityComparerTests
   {
      private UrlNameNaturalKeyEqualityComparer<IUrlNameNaturalKey> Comparer
      {
         get { return UrlNameNaturalKeyEqualityComparer<IUrlNameNaturalKey>.Instance; }
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
               Mock.Of<IUrlNameNaturalKey>(x => x.UrlName == (string) null),
               Mock.Of<IUrlNameNaturalKey>(x => x.UrlName == (string) null),
               EqualityResult.AreEqual
            };


            yield return new object[]
            {
               null,
               Mock.Of<IUrlNameNaturalKey>(x=> x.UrlName == "http://example.com/john"),
               EqualityResult.AreNotEqual
            };

            yield return new object[]
            {
               Mock.Of<IUrlNameNaturalKey>(x => x.UrlName == (string) null),
               Mock.Of<IUrlNameNaturalKey>(x=> x.UrlName == "http://example.com/john"),
               EqualityResult.AreNotEqual
            };


            yield return new object[]
            {
               Mock.Of<IUrlNameNaturalKey>(x=> x.UrlName == "http://example.com/john"),
               null,
               EqualityResult.AreNotEqual
            };

            yield return new object[]
            {
               Mock.Of<IUrlNameNaturalKey>(x=> x.UrlName == "http://example.com/john"),
               Mock.Of<IUrlNameNaturalKey>(x => x.UrlName == (string) null),
               EqualityResult.AreNotEqual
            };


            yield return new object[]
            {
               Mock.Of<IUrlNameNaturalKey>(x=> x.UrlName == "http://example.com/john"),
               Mock.Of<IUrlNameNaturalKey>(x=> x.UrlName == "http://example.com/john"),
               EqualityResult.AreEqual
            };

            yield return new object[]
            {
               Mock.Of<IUrlNameNaturalKey>(x=> x.UrlName == "http://example.com/john1"),
               Mock.Of<IUrlNameNaturalKey>(x=> x.UrlName == "http://example.com/john2"),
               EqualityResult.AreNotEqual
            };
         }
      }

      [Theory]
      [MemberData("TestData")]
      public void Should_ProduceValidComparisonForTestData(IUrlNameNaturalKey first, IUrlNameNaturalKey second, bool expectedResult)
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
