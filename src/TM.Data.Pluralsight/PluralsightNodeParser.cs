using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using TM.Data.Pluralsight.Properties;
using TM.Shared;
using TM.Shared.HtmlContainer;

namespace TM.Data.Pluralsight
{
   internal interface INodeParser
   {
      PluralsightCategory ParseCategoryNode(INode categoryNode);
      Sketch ParseSketchNode(INode sketchNode);
      CourseInfo ParseCourseInfo(INode infoNode);
      PluralsightAuthor ParseAuthor(INode authorNode);
      string ParseAuthorFullName(INode authorNode);
      bool IsCoAuthorNode(INode authorNode);
      IEnumerable<PluralsightAuthor> ParseCoAuthors(INode coAuthorNode);
      CourseLevel ParseCourseLevel(INode levelNode);
      CourseRating ParseCourseRating(INode ratingNode);
      TimeSpan ParseCourseDuration(INode durationNode);
      DateTime ParseCourseReleaseDate(INode releaseDateNode);
   }

   internal class PluralsightNodeParser : INodeParser
   {
      private const NumberStyles PositiveIntegerNumberStyle = NumberStyles.Integer ^ NumberStyles.AllowLeadingSign;

      private const NumberStyles PositiveFloatNumberStyle = NumberStyles.Float ^ NumberStyles.AllowLeadingSign ^ NumberStyles.AllowExponent;

      private readonly Uri _host;
      private readonly INodeSelector _nodeSelector;


      /// <exception cref="ArgumentNullException">
      /// <paramref name="host"/> or
      /// <paramref name="nodeSelector"/> is <see langword="null" />.</exception>
      public PluralsightNodeParser(string host, INodeSelector nodeSelector)
      {
         if (host == null) 
            throw new ArgumentNullException("host");

         if (nodeSelector == null)
            throw new ArgumentNullException("nodeSelector");

         _host = new Uri(host);

         _nodeSelector = nodeSelector;
      }


      /// <exception cref="ArgumentNullException"><paramref name="categoryNode"/> is <see langword="null" />.</exception>
      public PluralsightCategory ParseCategoryNode(INode categoryNode)
      {
         if (categoryNode == null) 
            throw new ArgumentNullException("categoryNode");

         var idInHtml = categoryNode.GetAttributeValue(Constants.CategoryIdAttribute);

         var categoryName = _nodeSelector.SelectNode(categoryNode, Constants.CategoryNameXPath).InnerText;

         var courseCategory = new PluralsightCategory
         {
            UrlName = HttpUtility.HtmlDecode(idInHtml),
            Title = HttpUtility.HtmlDecode(categoryName)
         };

         return courseCategory;
      }


      /// <exception cref="ArgumentNullException"><paramref name="categoryNode"/> is <see langword="null" />.</exception>
      public string ParseCategoryIdInHtmlDocument(INode categoryNode)
      {
         if (categoryNode == null) 
            throw new ArgumentNullException("categoryNode");

         var categoryIdInHtmlDocument = categoryNode.GetAttributeValue(Constants.CategoryIdAttribute);

         return HttpUtility.HtmlDecode(categoryIdInHtmlDocument);
      }


      /// <exception cref="ArgumentNullException"><paramref name="sketchNode"/> is <see langword="null" />.</exception>
      public Sketch ParseSketchNode(INode sketchNode)
      {
         if (sketchNode == null) 
            throw new ArgumentNullException("sketchNode");

         var sketchUrl = sketchNode.GetAttributeValue(Constants.SketchUrlAttribute);
         var sketchUriBuilder = new UriBuilder(new Uri(sketchUrl))
         {
            Scheme = Uri.UriSchemeHttp
         };

         var sketchUri = sketchUriBuilder.Uri;

         var sketch = new Sketch
         {
            Url = sketchUri.ToString(),
            FileName = sketchUri.Segments.Last().EndsWith("/", StringComparison.Ordinal) ? null : sketchUri.Segments.Last()
         };

         return sketch;
      }


      /// <exception cref="ArgumentNullException"><paramref name="infoNode"/> is <see langword="null" />.</exception>
      public CourseInfo ParseCourseInfo(INode infoNode)
      {
         if (infoNode == null) 
            throw new ArgumentNullException("infoNode");

         var description = infoNode.GetAttributeValue(Constants.CourseDescriptionAttribute);

         var relativeUrl = infoNode.GetAttributeValue(Constants.CourseUrlAttribute);
         var amendedUrl = AmendCourseUrl(relativeUrl);

         var courseUri = new Uri(_host, amendedUrl);

         var courseInfo = new CourseInfo
         {
            Name = HttpUtility.HtmlDecode(infoNode.InnerText),
            Description = HttpUtility.HtmlDecode(description),
            SiteUrl = courseUri.ToString(),
            UrlName = courseUri.Segments.Last()
         };

         return courseInfo;
      }
      
     
      private string AmendCourseUrl(string sourceUrl, string oldValue = "training/Courses/TableOfContents", string newValue = "courses")
      {
         return sourceUrl.Replace(oldValue, newValue);
      }


      /// <exception cref="ArgumentNullException"><paramref name="authorNode"/> is <see langword="null" />.</exception>
      public PluralsightAuthor ParseAuthor(INode authorNode)
      {
         if (authorNode == null) 
            throw new ArgumentNullException("authorNode");

         var authorFullName = authorNode.GetAttributeValue(Constants.AuthorFullNameAttribute);
         var authorUrl = authorNode.GetAttributeValue(Constants.AuthorUrlAttribute);
         var authorUriBuilder = new UriBuilder(new Uri(authorUrl))
         {
            Scheme = Uri.UriSchemeHttp
         };

         var authorUri = authorUriBuilder.Uri;

         var author = new PluralsightAuthor
         {
            FullName = HttpUtility.HtmlDecode(authorFullName),
            SiteUrl = authorUri.ToString(),
            UrlName = authorUri.Segments.Last()
         };

         return author;
      }


      /// <exception cref="ArgumentNullException"><paramref name="authorNode"/> is <see langword="null" />.</exception>
      public string ParseAuthorFullName(INode authorNode)
      {
         if (authorNode == null) 
            throw new ArgumentNullException("authorNode");

         var authorFullName = authorNode.GetAttributeValue(Constants.AuthorFullNameAttribute);

         return HttpUtility.HtmlDecode(authorFullName);
      }


      /// <exception cref="ArgumentNullException"><paramref name="authorNode"/> is <see langword="null" />.</exception>
      public bool IsCoAuthorNode(INode authorNode)
      {
         if (authorNode == null) throw new ArgumentNullException("authorNode");

         return authorNode.InnerText
            .Equals(Constants.CoAuthorMarker, StringComparison.OrdinalIgnoreCase);
      }


      /// <exception cref="ArgumentNullException"><paramref name="coAuthorNode"/> is <see langword="null" />.</exception>
      public IEnumerable<PluralsightAuthor> ParseCoAuthors(INode coAuthorNode)
      {
         if (coAuthorNode == null) 
            throw new ArgumentNullException("coAuthorNode");

         var coAuthors = coAuthorNode.GetAttributeValue(Constants.AuthorFullNameAttribute)
            .Split(new[] {", "}, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new PluralsightAuthor
            {
               FullName = HttpUtility.HtmlDecode(x.Trim())
            });

         return coAuthors;
      }


      /// <exception cref="ArgumentNullException"><paramref name="levelNode"/> is <see langword="null" />.</exception>
      public CourseLevel ParseCourseLevel(INode levelNode)
      {
         if (levelNode == null) 
            throw new ArgumentNullException("levelNode");

         CourseLevel courseLevel;
         Enum.TryParse(levelNode.InnerText, ignoreCase: true, result: out courseLevel);
         return courseLevel;
      }


      /// <exception cref="NodeParseException">Incorrect data in <paramref name="ratingNode"/>.</exception>
      /// <exception cref="ArgumentNullException"><paramref name="ratingNode"/> is <see langword="null" />.</exception>
      public CourseRating ParseCourseRating(INode ratingNode)
      {
         if (ratingNode == null) 
            throw new ArgumentNullException("ratingNode");

         try
         {
            var ratingString = ratingNode.GetAttributeValue(Constants.CourseRatingAttribute);
            if (ratingString.Equals("Not enough course ratings", StringComparison.OrdinalIgnoreCase))
            {
               return new CourseRating();
            }
            // context: "X.X stars from Y users" where X - Rating; Y - RaterCount
            var stringParts = ratingString.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            var raters = int.Parse(stringParts[3], PositiveIntegerNumberStyle);
            var rating = decimal.Parse(stringParts[0], PositiveFloatNumberStyle, NumberFormatInfo.InvariantInfo);

            var courseRating = new CourseRating
            {
               Raters = raters,
               Rating = rating
            };
            return courseRating;
         }
         // ReSharper disable once CatchAllClause
         catch (Exception ex)
         {
            var message = string.Format(Resources.CatalogNodeParseException_Message, "ratingNode");
            throw new NodeParseException(message, ratingNode.OuterText, ex);
         }
      }


      /// <exception cref="NodeParseException">Incorrect data in <paramref name="durationNode"/>. </exception>
      /// <exception cref="ArgumentNullException"><paramref name="durationNode"/> is <see langword="null" />.</exception>
      public TimeSpan ParseCourseDuration(INode durationNode)
      {
         if (durationNode == null) 
            throw new ArgumentNullException("durationNode");

         var timeParts = durationNode.InnerText
            .Split(new[] { "[", ":", "]" }, StringSplitOptions.RemoveEmptyEntries);
         try
         {
            var hours = int.Parse(timeParts[0], PositiveIntegerNumberStyle);
            var minutes = int.Parse(timeParts[1], PositiveIntegerNumberStyle);
            var seconds = int.Parse(timeParts[2], PositiveIntegerNumberStyle);
            return new TimeSpan(hours, minutes, seconds);
         }
         // ReSharper disable once CatchAllClause
         catch (Exception ex)
         {
            var message = string.Format(Resources.CatalogNodeParseException_Message, "durationNode");
            throw new NodeParseException(message, durationNode.OuterText, ex);
         }
      }


      /// <exception cref="NodeParseException">Incorrect data in <paramref name="releaseDateNode"/>. </exception>
      /// <exception cref="ArgumentNullException"><paramref name="releaseDateNode"/> is <see langword="null" />.</exception>
      public DateTime ParseCourseReleaseDate(INode releaseDateNode)
      {
         if (releaseDateNode == null) 
            throw new ArgumentNullException("releaseDateNode");

         try
         {
            var date = DateTime.ParseExact(releaseDateNode.InnerText, "d MMM yyyy", DateTimeFormatInfo.InvariantInfo,
               DateTimeStyles.AssumeUniversal|DateTimeStyles.AdjustToUniversal);
            return date;
         }
         // ReSharper disable once CatchAllClause
         catch (Exception ex)
         {
            var message = string.Format(Resources.CatalogNodeParseException_Message, "releaseDateNode");
            throw new NodeParseException(message, releaseDateNode.OuterText, ex);
         }
      }


      internal static class Constants
      {
         public const string CategoryNameXPath = @"./div[@class='title']";

         public const string AuthorUrlAttribute = "href";
         public const string AuthorFullNameAttribute = "title";

         public const string CategoryIdAttribute = "id";

         public const string SketchUrlAttribute = "src";

         public const string CourseDescriptionAttribute = "title";
         public const string CourseRatingAttribute = "title";
         public const string CourseUrlAttribute = "href";

         public const string CoAuthorMarker = "et al.";
      }
   }
}

