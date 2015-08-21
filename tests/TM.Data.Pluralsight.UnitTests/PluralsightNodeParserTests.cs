using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Moq;
using TM.Shared;
using TM.Shared.HtmlContainer;
using Xunit;

namespace TM.Data.Pluralsight.UnitTests
{
   using ParserConstants = PluralsightNodeParser.Constants;

   [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
   public class PluralsightNodeParserTests
   {
      private const string SiteUrl = "http://www.pluralsight.com/";
      private INodeSelector _nodeSelector = Mock.Of<INodeSelector>();

      [Fact]
      public void Should_ThrowArgumentNullException_When_ArgumentIsNull()
      {
         global::System.Threading.Tasks.Task test = null;
         Assert.Throws<ArgumentNullException>(() => new PluralsightNodeParser(null, null));

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         Assert.Throws<ArgumentNullException>(() => sut.ParseCategoryNode(null));
         Assert.Throws<ArgumentNullException>(() => sut.ParseCategoryIdInHtmlDocument(null));
         Assert.Throws<ArgumentNullException>(() => sut.ParseSketchNode(null));
         Assert.Throws<ArgumentNullException>(() => sut.ParseCourseInfo(null));
         Assert.Throws<ArgumentNullException>(() => sut.ParseAuthor(null));
         Assert.Throws<ArgumentNullException>(() => sut.ParseAuthorFullName(null));
         Assert.Throws<ArgumentNullException>(() => sut.IsCoAuthorNode(null));
         Assert.Throws<ArgumentNullException>(() => sut.ParseCoAuthors(null));
         Assert.Throws<ArgumentNullException>(() => sut.ParseCourseLevel(null));
         Assert.Throws<ArgumentNullException>(() => sut.ParseCourseRating(null));
         Assert.Throws<ArgumentNullException>(() => sut.ParseCourseDuration(null));
         Assert.Throws<ArgumentNullException>(() => sut.ParseCourseReleaseDate(null));
      }

      [Fact]
      public void Should_ParseCategoryNode()
      {
         // Arrange
         var categoryNode = Mock.Of<INode>();
         var nodeSelector = Mock.Of<INodeSelector>(x => x.SelectNode(It.IsAny<INode>(), It.IsAny<string>()).InnerText == string.Empty);

         var sut = new PluralsightNodeParser(SiteUrl, nodeSelector);

         // Act
         var result = sut.ParseCategoryNode(categoryNode);

         // Assert
         Assert.NotNull(result);
         Mock.Get(nodeSelector).Verify(x => x.SelectNode(It.IsAny<INode>(), It.IsAny<string>()).InnerText, Times.Once);
         Mock.Get(categoryNode).Verify(x => x.GetAttributeValue(It.IsAny<string>()), Times.Once);
      }


      [Fact]
      public void Should_ParseCategoryIdInHtmlDocumentFromCategoryNode()
      {
         // Arrange
         const string expected = "dotnet";
         const string categoryIdAttribute = ParserConstants.CategoryIdAttribute;

         var categoryNode = Mock.Of<INode>(n => n.GetAttributeValue(categoryIdAttribute) == expected);

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         // Act
         var result = sut.ParseCategoryIdInHtmlDocument(categoryNode);

         // Assert
         Assert.Equal(expected, result);
      }

      [Fact]
      public void Should_ParseSketchNode()
      {
         // Arrange
         const string expectedUrl = "http://s.pluralsight.com/mn/img/cs/dotnet-v1.png";
         const string expectedFileName = "dotnet-v1.png";
         const string sketchUrlAttribute = ParserConstants.SketchUrlAttribute;

         var sketchNode = Mock.Of<INode>(n => n.GetAttributeValue(sketchUrlAttribute) == expectedUrl);

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         // Act
         var result = sut.ParseSketchNode(sketchNode);

         // Assert
         Assert.NotNull(result);
         Assert.Equal(expectedUrl, result.Url);
         Assert.Equal(expectedFileName, result.FileName);
      }

      [Fact]
      public void Should_ParseCourseInfoNode()
      {
         // Arrange
         const string courseName = "What’s New in PostSharp v3";
         const string sourceUrl = "/training/Courses/TableOfContents/becoming-dotnet-developer";
         const string expectedUrl = "http://www.pluralsight.com/courses/becoming-dotnet-developer";

         const string courseUrlAttribute = ParserConstants.CourseUrlAttribute;

         var infoNode = Mock.Of<INode>(n => n.InnerText == courseName && n.GetAttributeValue(courseUrlAttribute) == sourceUrl);

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         // Act
         var result = sut.ParseCourseInfo(infoNode);

         // Assert
         Assert.NotNull(result);
         Assert.Equal(courseName, result.Name);
         Assert.Equal(expectedUrl, result.SiteUrl);
      }

      [Fact]
      public void Should_ParseAuthorNode()
      {
         // Arrange
         const string sourceUrl = "http://www.pluralsight.com/author/eric-burke";
         const string expectedUrl = "http://www.pluralsight.com/author/eric-burke";
         const string expectedUrlName = "eric-burke";

         const string authoreUrlAttribute = ParserConstants.AuthorUrlAttribute;

         var authorNode = Mock.Of<INode>(n => n.GetAttributeValue(authoreUrlAttribute) == sourceUrl);

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         // Act
         var result = sut.ParseAuthor(authorNode);

         // Assert
         Assert.NotNull(result);
         Assert.Equal(expectedUrl, result.SiteUrl);
         Assert.Equal(expectedUrlName, result.UrlName);
      }

      [Fact]
      public void Should_ParseAuthorFullNameFromAuthorNode()
      {
         // Arrange
         var authorNode = Mock.Of<INode>();

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         // Act
         var result = sut.ParseAuthorFullName(authorNode);

         // Assert
         Assert.Null(result);
         Mock.Get(authorNode).Verify(x => x.GetAttributeValue(It.IsAny<string>()), Times.Once);
      }

      [Theory]
      [InlineData(ParserConstants.CoAuthorMarker, true)]
      [InlineData("some incorrect text", false)]
      public void Should_GetCorrectAnswer_For_CoAuthorNodeTypeQuestion(string nodeInnerText, bool expectedAnswer)
      {
         // Arrange
         var expected = expectedAnswer;

         var authorNode = Mock.Of<INode>(n => n.InnerText == nodeInnerText);

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         // Act
         var result = sut.IsCoAuthorNode(authorNode);

         // Assert
         Assert.Equal(expected, result);
      }

      [Fact]
      public void Should_ParseCoAuthorNode()
      {
         // Arrange
         var expected = new List<PluralsightAuthor>
               {
                  new PluralsightAuthor{FullName = "Scott Allen"},
                  new PluralsightAuthor{FullName = "Keith Sparkjoy"},
                  new PluralsightAuthor{FullName = "Dan Sullivan"}
               };

         const string authorFullNameAttribute = ParserConstants.AuthorFullNameAttribute;

         var coAuthorNode = Mock.Of<INode>(n =>
            n.GetAttributeValue(authorFullNameAttribute) == string.Join(", ", expected.Select(x=> x.FullName)));

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         // Act
         var result = sut.ParseCoAuthors(coAuthorNode);

         // Assert
         Assert.NotNull(result);
         Assert.Equal(expected.Select(x =>x.FullName), result.Select(x =>x.FullName));
      }

      [Fact]
      public void Should_ParseCourseLevelNode()
      {
         // Arrange
         const CourseLevel expected = CourseLevel.Advanced;
         var nodeData = expected.ToString();

         var levelNode = Mock.Of<INode>(n => n.InnerText == nodeData);

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         // Act
         var result = sut.ParseCourseLevel(levelNode);

         // Assert
         Assert.Equal(expected, result);
      }

      [Fact]
      public void Should_ParseCourseRatingNode()
      {
         // Arrange
         var expected = new CourseRating { Raters = 103, Rating = 1.25m };
         var nodeData = string.Format(CultureInfo.InvariantCulture, "{0} stars from {1} users", expected.Rating, expected.Raters);
         const string courseRatingAttribute = ParserConstants.CourseRatingAttribute;

         var ratingNode = Mock.Of<INode>(n => n.GetAttributeValue(courseRatingAttribute) == nodeData);

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         // Act
         var result = sut.ParseCourseRating(ratingNode);

         // Assert
         Assert.NotNull(result);
         Assert.Equal(expected.Rating, result.Rating);
         Assert.Equal(expected.Raters, result.Raters);
      }

      [Fact]
      public void Should_ReturnEmptyRating_When_RatingNotSpecified()
      {
         // Arrange
         var expected = new CourseRating { Raters = 0, Rating = 0m };
         var nodeData = "Not enough course ratings";
         const string courseRatingAttribute = ParserConstants.CourseRatingAttribute;

         var ratingNode = Mock.Of<INode>(n => n.GetAttributeValue(courseRatingAttribute) == nodeData);

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         // Act
         var result = sut.ParseCourseRating(ratingNode);

         // Assert
         Assert.NotNull(result);
         Assert.Equal(expected.Rating, result.Rating);
         Assert.Equal(expected.Raters, result.Raters);
      }

      [Theory]
      [InlineData("-2.5 stars from 50 users")]
      [InlineData("4.5 stars from -2 users")]
      [InlineData("some incorrect data")]
      public void Should_ThrowCatalogNodeParseException_When_CourseRatingNodeHasIncorrectData(string invalidData)
      {
         var ratingNode = Mock.Of<INode>(n => n.InnerText == invalidData);

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         Assert.Throws<NodeParseException>(() => sut.ParseCourseRating(ratingNode));
      }

      [Fact]
      public void Should_ParseCourseDurationNode()
      {
         // Arrange
         var expected = TimeSpan.FromHours(2.5);
         var nodeData = string.Format(DateTimeFormatInfo.InvariantInfo, "[{0:T}]", expected);

         var durationNode = Mock.Of<INode>(n => n.InnerText == nodeData);

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         // Act
         var result = sut.ParseCourseDuration(durationNode);

         // Assert
         Assert.Equal(expected, result);
      }

      [Theory]
      [InlineData("[-5:0:07]")]
      [InlineData("[5:-50:07]")]
      [InlineData("[00:50:-07]")]
      [InlineData("some incorrect data")]
      public void Should_ThrowCatalogNodeParseException_When_CourseDurationNodeHasIncorrectData(string invalidData)
      {
         var durationNode = Mock.Of<INode>(n => n.InnerText == invalidData);

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         Assert.Throws<NodeParseException>(() => sut.ParseCourseDuration(durationNode));
      }

      [Fact]
      public void Should_ParseCourseReleaseDateNode()
      {
         // Arrange
         var expected = DateTime.UtcNow.Date;
         var nodeData = expected.ToString("d MMM yyyy", DateTimeFormatInfo.InvariantInfo);

         var releaseDateNode = Mock.Of<INode>(n => n.InnerText == nodeData);

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         // Act
         var result = sut.ParseCourseReleaseDate(releaseDateNode);

         // Assert
         Assert.Equal(expected, result);
      }

      [Theory]
      [InlineData("05/12/2014")]
      [InlineData("05.12.2014")]
      [InlineData("2014/12/05")]
      [InlineData("some incorrect data")]
      public void Should_ThrowCatalogNodeParseException_When_CourseReleaseDateNodeHasIncorrectData(string invalidData)
      {
         var releaseDateNode = Mock.Of<INode>(n => n.InnerText == invalidData);

         var sut = new PluralsightNodeParser(SiteUrl, _nodeSelector);

         Assert.Throws<NodeParseException>(() => sut.ParseCourseReleaseDate(releaseDateNode));
      }
   }
}
