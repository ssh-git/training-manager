using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;
using TM.Shared;
using TM.Shared.HtmlContainer;
using Xunit;

namespace TM.Data.Pluralsight.UnitTests
{
   [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
   [SuppressMessage("ReSharper", "PossibleUnintendedReferenceComparison")]
   [SuppressMessage("ReSharper", "RedundantBoolCompare")]
   [SuppressMessage("ReSharper", "StringLiteralTypo")]
   public class PluralsightCatalogParserTests
   {
      [Fact]
      public void Should_ReturnParsedAuthors()
      {
         // Arrange

         var data = new[]
         {
            new
            {
               Node = Mock.Of<INode>(),
               Authors = new List<PluralsightAuthor>
               {
                  new PluralsightAuthor
                  {
                     FullName = "Eric Burke",
                     SiteUrl = "http://www.pluralsight.com/author/eric-burke",
                     UrlName = "eric-burke"
                  }
               },
               IsCoAuthors = false
            },
            new
            {
               Node = Mock.Of<INode>(),
               Authors = new List<PluralsightAuthor>
               {
                  new PluralsightAuthor
                  {
                     FullName = "Eric Burke",
                     SiteUrl = "http://www.pluralsight.com/author/eric-burke-other",
                     UrlName = "eric-burke-other"
                  }
               },
               IsCoAuthors = false
            },
            new
            {
               Node = Mock.Of<INode>(),
               Authors = new List<PluralsightAuthor>
               {
                  new PluralsightAuthor
                  {
                     FullName = "Robert Horvick",
                     SiteUrl = "http://www.pluralsight.com/author/robert-horvick",
                     UrlName = "robert-horvick"
                  }
               },
               IsCoAuthors = false
            },
            new
            {
               Node = Mock.Of<INode>(),
               Authors = new List<PluralsightAuthor>
               {
                  new PluralsightAuthor
                  {
                     FullName = "Jan-Erik Sandberg",
                     SiteUrl = "http://www.pluralsight.com/author/janerik-sandberg",
                     UrlName = "janerik-sandberg"
                  }
               },
               IsCoAuthors = false
            },
            new
            {
               Node = Mock.Of<INode>(),
               Authors = new List<PluralsightAuthor>
               {
                  new PluralsightAuthor
                  {
                     FullName = "Eric Burke"
                  },
                  new PluralsightAuthor
                  {
                     FullName = "Jan-Erik Sandberg"
                  },
                  new PluralsightAuthor
                  {
                     FullName = "Jay Mcfarland"
                  },
                  new PluralsightAuthor
                  {
                     FullName = "Jeff Hurd"
                  }
               },
               IsCoAuthors = true
            }
         };


         var nodeSelector = Mock.Of<INodeSelector>(x => x.SelectAuthorNodes() == data.Select(d => d.Node));

         var nodeParser = Mock.Of<INodeParser>();

         Mock.Get(nodeParser).Setup(x => x.IsCoAuthorNode(It.IsAny<INode>()))
            .Returns((INode node) => data.Single(x => x.Node == node).IsCoAuthors);

         Mock.Get(nodeParser).Setup(x => x.ParseCoAuthors(It.IsAny<INode>()))
            .Returns((INode node) => data.Single(x => x.Node == node).Authors);

         Mock.Get(nodeParser).Setup(x => x.ParseAuthor(It.IsAny<INode>()))
            .Returns((INode node) => data.Single(x => x.Node == node).Authors.Single());


         var sut = new PluralsightCatalogParser(nodeSelector, nodeParser);

         // Act
         var result = sut.ParseAuthors();

         // Assert
         Assert.NotNull(result);

         var resultFullnamesakesAuthors = result.FullnamesakesAuthorsContainer.Values;
         Assert.Equal(1, resultFullnamesakesAuthors.Count);

         var resultAuthorsWithUrlNotNull = result.AllAuthorsExceptWhoseUrlNullContainer.Values;
         Assert.Equal(4, resultAuthorsWithUrlNotNull.Count);

         var resultAllAuthorsExceptFullnamesakes = result.AllAuthorsByFullNameExceptFullnamesakesContainer.Values;
         Assert.Equal(4, resultAllAuthorsExceptFullnamesakes.Count);
      }

      [Fact]
      public void Should_ReturnParsedCategories()
      {
         // Arrange
         var categoryCount = 4;

         var data = Enumerable.Range(1, categoryCount).Select(x => new
         {
            Node = Mock.Of<INode>(),
            Category = new PluralsightCategory { Title = "category" + x, UrlName = "c" + x },
            Sketch = new Sketch { FileName = "fileName" + x, Url = "http://example.com/fileName" + x }
         }).ToList();


         var nodeSelector =
            Mock.Of<INodeSelector>(x => x.SelectCategoryNodes(out categoryCount) == data.Select(c => c.Node));

         Mock.Get(nodeSelector).Setup(x => x.SelectSketchNode(It.IsAny<string>()))
            .Returns((string urlName) => data
               .Where(x => x.Category.UrlName == urlName)
               .Select(x => x.Node)
               .Single());

         var nodeParser = Mock.Of<INodeParser>();

         Mock.Get(nodeParser).Setup(x => x.ParseCategoryNode(It.IsAny<INode>()))
            .Returns((INode n) => data
               .Where(x => x.Node == n)
               .Select(x => x.Category)
               .Single());

         Mock.Get(nodeParser).Setup(x => x.ParseSketchNode(It.IsAny<INode>()))
            .Returns((INode n) => data
               .Where(x => x.Node == n)
               .Select(x => x.Sketch)
               .Single());


         var sut = new PluralsightCatalogParser(nodeSelector, nodeParser);

         // Act
         var result = sut.ParseCategories();

         // Assert
         Assert.NotNull(result);
         Assert.Equal(categoryCount, result.Count);

         foreach (var index in Enumerable.Range(0, data.Count))
         {
            var expectedCategory = new PluralsightCategory
            {
               Title = data[index].Category.Title,
               UrlName = data[index].Category.UrlName,
               LogoFileName = data[index].Sketch.FileName,
               LogoUrl = data[index].Sketch.Url
            };

            Assert.Equal(expectedCategory, result[index], PluralsightCategory.PropertiesComparer);
         }
      }


      [Fact]
      public void Should_ReturnCourseCoAuthorWithFullnamesakeMark_WhenAuthorNameInFullnamesakeContainer()
      {
         // Arrange
         var author = new PluralsightAuthor
         {
            FullName = "John Smith"
         };

         var course = new PluralsightCourse();

         var fullnamesakesAuthorsContainer =
            new Dictionary<IAuthorFullNameNaturalKey, PluralsightAuthor>(
               FullNameNaturalKeyEqualityComparer<IAuthorFullNameNaturalKey>.Instance);

         var allAuthorsExceptFullnamesakesContainer =
            new Dictionary<IAuthorFullNameNaturalKey, PluralsightAuthor>(
               FullNameNaturalKeyEqualityComparer<IAuthorFullNameNaturalKey>.Instance);

         fullnamesakesAuthorsContainer.Add(author, author);

         var nodeSelector = Mock.Of<INodeSelector>();
         var nodeParser = Mock.Of<INodeParser>();

         var sut = new PluralsightCatalogParser(nodeSelector, nodeParser);

         // Act
         var result = sut.GetCourseCoAuthor(course, author, allAuthorsExceptFullnamesakesContainer, fullnamesakesAuthorsContainer);

         // Assert
         Assert.NotNull(result);
         Assert.Same(course, result.Course);
         Assert.Same(author, result.Author);

         Assert.Equal(true, result.HasFullnamesake);
      }


      [Fact]
      public void Should_ReturnCourseCoAuthorWithoutFullnamesakeMark_WhenAuthorNotInFullnamesakeContainer()
      {
         // Arrange
         var author = new PluralsightAuthor
         {
            FullName = "John Smith"
         };

         var course = new PluralsightCourse();

         var fullnamesakesAuthorsContainer =
            new Dictionary<IAuthorFullNameNaturalKey, PluralsightAuthor>(
               FullNameNaturalKeyEqualityComparer<IAuthorFullNameNaturalKey>.Instance);

         var allAuthorsExceptFullnamesakesContainer =
            new Dictionary<IAuthorFullNameNaturalKey, PluralsightAuthor>(FullNameNaturalKeyEqualityComparer<IAuthorFullNameNaturalKey>.Instance)
            {
               {author, author}
            };


         var nodeSelector = Mock.Of<INodeSelector>();
         var nodeParser = Mock.Of<INodeParser>();

         var sut = new PluralsightCatalogParser(nodeSelector, nodeParser);

         // Act
         var result = sut.GetCourseCoAuthor(course, author, allAuthorsExceptFullnamesakesContainer, fullnamesakesAuthorsContainer);

         // Assert
         Assert.NotNull(result);
         Assert.Same(course, result.Course);
         Assert.Same(author, result.Author);

         Assert.Equal(false, result.HasFullnamesake);
      }


      [Fact]
      public void Should_ReturnCourseAuthors()
      {
         // Arrange
         var coAuthors = new[] { "Eric Burke", "Steve Evans", "Rob Windsor" }
            .Select(x => new PluralsightAuthor { FullName = x }).ToList();

         var author = new PluralsightAuthor { FullName = "John Smith", UrlName = "john-smith"};

         var coAuthorNode = Mock.Of<INode>();
         var authorNode = Mock.Of<INode>();

         var course = new PluralsightCourse();

         var fullnamesakesAuthorsContainer = new Dictionary<IAuthorFullNameNaturalKey, PluralsightAuthor>(
            FullNameNaturalKeyEqualityComparer<IAuthorFullNameNaturalKey>.Instance);

         fullnamesakesAuthorsContainer.Add(coAuthors[1], coAuthors[1]);


         var allAuthorsExceptFullnamesakesContainer = new Dictionary<IAuthorFullNameNaturalKey, PluralsightAuthor>(
            FullNameNaturalKeyEqualityComparer<IAuthorFullNameNaturalKey>.Instance);

         allAuthorsExceptFullnamesakesContainer.Add(coAuthors[0], coAuthors[0]);
         allAuthorsExceptFullnamesakesContainer.Add(coAuthors[2], coAuthors[2]);
         allAuthorsExceptFullnamesakesContainer.Add(author, author);

         var allAuthorsExceptWhoseUrlNullContainer = new Dictionary<IAuthorUrlNameNaturalKey, PluralsightAuthor>(
            UrlNameNaturalKeyEqualityComparer<IAuthorUrlNameNaturalKey>.Instance);
         allAuthorsExceptWhoseUrlNullContainer.Add(author, author);

         var authorsParseResult = new PluralsightCatalogParser.AuthorsParseResult
         {
            FullnamesakesAuthorsContainer = fullnamesakesAuthorsContainer,
            AllAuthorsByFullNameExceptFullnamesakesContainer = allAuthorsExceptFullnamesakesContainer,
            AllAuthorsExceptWhoseUrlNullContainer = allAuthorsExceptWhoseUrlNullContainer
         };


         var nodeSelector = Mock.Of<INodeSelector>();

         var nodeParser = Mock.Of<INodeParser>(x =>
            x.IsCoAuthorNode(coAuthorNode) == true &&
            x.ParseCoAuthors(coAuthorNode) == coAuthors &&

            x.IsCoAuthorNode(authorNode) == false &&
            x.ParseAuthor(authorNode) == author);

         var sut = new PluralsightCatalogParser(nodeSelector, nodeParser);

         // Act
         var result = sut.GetCourseAuthors(new[] { authorNode, coAuthorNode }, course, authorsParseResult);

         // Assert
         Assert.NotNull(result);
         Assert.Equal(4, result.Count);

         foreach (var courseAuthor in result)
         {
            Assert.Same(course, courseAuthor.Course);
         }
         
         Assert.Equal(coAuthors.OrderBy(x => x.FullName),
            result.Where(x => x.IsAuthorCoAuthor).Select(x => x.Author).OrderBy(x => x.FullName),
            ReferenceEqualityComparer.Instance);

         Assert.Same(author, result.Single(x => x.IsAuthorCoAuthor == false).Author);

         Assert.Same(coAuthors[1], result.Single(x => x.HasFullnamesake).Author);
      }
   }
}
