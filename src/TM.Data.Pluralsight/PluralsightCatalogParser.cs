using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TM.Shared;
using TM.Shared.HtmlContainer;
using TM.Shared.Parse;

namespace TM.Data.Pluralsight
{

   internal class PluralsightCatalogParser : ITrainingCatalogParser<PluralsightCategory, PluralsightCourse, PluralsightAuthor>
   {
      private readonly INodeSelector _nodeSelector;
      private readonly INodeParser _nodeParser;

      private IHtmlContainer _catalog;

      /// <exception cref="ArgumentNullException">
      /// <paramref name="nodeSelector"/> or
      /// <paramref name="nodeParser"/> is <see langword="null" />.</exception>
      public PluralsightCatalogParser(INodeSelector nodeSelector, INodeParser nodeParser)
      {
         if(nodeSelector == null) 
            throw new ArgumentNullException("nodeSelector");

         if (nodeParser == null) 
            throw new ArgumentNullException("nodeParser");

         _nodeSelector = nodeSelector;
         _nodeParser = nodeParser;
      }


      public IUpdateContentParseResult<PluralsightCategory, PluralsightCourse, PluralsightAuthor> Parse(IHtmlContainer catalog)
      {
         if (_catalog == null)
         {
            _catalog = catalog;
            _nodeSelector.SetContext(catalog);
         } else if (!ReferenceEquals(_catalog, catalog))
         {
            _nodeSelector.SetContext(catalog);
         }
         
         var authorsCatalogParseResult = ParseAuthors();

         var categories = ParseCategories();

         var coursesDictionary =
            new Dictionary<ICourseUrlNameNaturalKey, PluralsightCourse>(
               UrlNameNaturalKeyEqualityComparer<ICourseUrlNameNaturalKey>.Instance);

         foreach (var category in categories)
         {
            var coursesForCategory = ParseCoursesForCategory(category, authorsCatalogParseResult);

            category.Courses = coursesForCategory;

            foreach (var course in coursesForCategory)
            {
               coursesDictionary.Add(course, course);
            }
         }

         var categoriesDictionary = categories
            .ToDictionary(x => x, x => x, UrlNameNaturalKeyEqualityComparer<ICategoryUrlNameNaturalKey>.Instance);


         var categoriesParseResult = new PluralsightCategoriesParseResult(categoriesDictionary);

         var coursesParseResult = new PluralsightCoursesParseResult(coursesDictionary);

         var authorsParseResult = new PluralsightAuthorsParseResult(authorsCatalogParseResult.AllAuthorsExceptWhoseUrlNullContainer);

         var catalogParseResult = new PluralsightUpdateParseResult(categoriesParseResult, coursesParseResult, authorsParseResult);

         return catalogParseResult;
      }


      internal AuthorsParseResult ParseAuthors()
      {
         var authorsByUrlNameSet =
            new HashSet<PluralsightAuthor>(UrlNameNaturalKeyEqualityComparer<IAuthorUrlNameNaturalKey>.Instance);

         var coAuthorsByFullNameSet =
            new HashSet<PluralsightAuthor>(FullNameNaturalKeyEqualityComparer<IAuthorFullNameNaturalKey>.Instance);

         var authorNodes = _nodeSelector.SelectAuthorNodes();

         foreach (var node in authorNodes)
         {
            if (_nodeParser.IsCoAuthorNode(node))
            {
               var coAuthors = _nodeParser.ParseCoAuthors(node);
               coAuthorsByFullNameSet.UnionWith(coAuthors);
            } else
            {
               var author = _nodeParser.ParseAuthor(node);
               authorsByUrlNameSet.Add(author);
            }
         }

         var fullnamesakesAuthorsList = authorsByUrlNameSet
            .GroupBy(x => x.FullName)
            .Where(x => x.Count() > 1)
            .SelectMany(x => x).ToList();


         var fullnamesakesAuthorDictionary = fullnamesakesAuthorsList
            .GroupBy(x => x.FullName)
            .Select(x => new PluralsightAuthor
            {
               FullName = x.Key,
               SiteUrl = string.Join(";", x.OrderBy(n => n.UrlName).Select(a => a.SiteUrl)),
               UrlName = string.Join(";", x.OrderBy(n => n.UrlName).Select(a => a.UrlName))
            }).ToDictionary(x => x, x => x, FullNameNaturalKeyEqualityComparer<IAuthorFullNameNaturalKey>.Instance);

         var allAuthorsExceptWhoseUrlNullDictionary = authorsByUrlNameSet.ToDictionary(x => x, x => x,
            UrlNameNaturalKeyEqualityComparer<IAuthorUrlNameNaturalKey>.Instance);

         // remove authors with urlName != null
         coAuthorsByFullNameSet.ExceptWith(authorsByUrlNameSet);

         // remove fullnamesakes from authorsByUrlNameSet
         authorsByUrlNameSet.ExceptWith(fullnamesakesAuthorsList);

         var allAuthorsByFullNameExceptFullnamesakesContainer = authorsByUrlNameSet
            .Concat(coAuthorsByFullNameSet)
            .ToDictionary(x => x, x => x, FullNameNaturalKeyEqualityComparer<IAuthorFullNameNaturalKey>.Instance);

         var authorsParseResult = new AuthorsParseResult
         {
            FullnamesakesAuthorsContainer = fullnamesakesAuthorDictionary,
            AllAuthorsByFullNameExceptFullnamesakesContainer = allAuthorsByFullNameExceptFullnamesakesContainer,
            AllAuthorsExceptWhoseUrlNullContainer = allAuthorsExceptWhoseUrlNullDictionary
         };

         return authorsParseResult;
      }


      internal List<PluralsightCategory> ParseCategories()
      {
         int categoryCount;
         var categoryNodes = _nodeSelector.SelectCategoryNodes(out categoryCount);

         var categories = new List<PluralsightCategory>(categoryCount);

         foreach (var node in categoryNodes)
         {
            var category = _nodeParser.ParseCategoryNode(node);

            var sketchNode = _nodeSelector.SelectSketchNode(category.UrlName);
            var sketch = _nodeParser.ParseSketchNode(sketchNode);

            category.LogoUrl = sketch.Url;
            category.LogoFileName = sketch.FileName;

            categories.Add(category);
         }

         return categories;
      }


      internal List<PluralsightCourse> ParseCoursesForCategory(PluralsightCategory category, AuthorsParseResult authorsParseResult)
      {
         int courseCount;
         var courseNodes = _nodeSelector.SelectCourseNodes(category.UrlName, out courseCount);

         var courses = new List<PluralsightCourse>(courseCount);

         foreach (var courseNode in courseNodes)
         {
            var infoNode = _nodeSelector.SelectInfoNode(courseNode);
            var featureNode = _nodeSelector.SelectClosedCaptionsNode(courseNode);
            var authorNodes = _nodeSelector.SelectAuthorNodes(courseNode);
            var levelNode = _nodeSelector.SelectLevelNode(courseNode);
            var ratingNode = _nodeSelector.SelectRatingNode(courseNode);
            var durationNode = _nodeSelector.SelectDurationNode(courseNode);
            var releaseDateNode = _nodeSelector.SelectReleaseDateNode(courseNode);

            if (infoNode == null || authorNodes == null || levelNode == null || ratingNode == null || durationNode == null || releaseDateNode == null)
            {
               // TODO: Log issue

               continue;
            }


            var info = _nodeParser.ParseCourseInfo(infoNode);
            var hasClosedCaptions = featureNode != null;
            var level = _nodeParser.ParseCourseLevel(levelNode);
            var rating = _nodeParser.ParseCourseRating(ratingNode);
            var duration = _nodeParser.ParseCourseDuration(durationNode);
            var releaseDate = _nodeParser.ParseCourseReleaseDate(releaseDateNode);
            

            var course = new PluralsightCourse
            {
               Title = info.Name,
               SiteUrl = info.SiteUrl,
               UrlName = info.UrlName,
               Description = info.Description,

               HasClosedCaptions = hasClosedCaptions,
               Level = level,
               Duration = duration,
               Rating = rating,
               ReleaseDate = releaseDate,

               Category = category
            };

            course.CourseAuthors = GetCourseAuthors(authorNodes, course, authorsParseResult);

            courses.Add(course);
         }

         return courses;
      }


      internal List<PluralsightCourseAuthor> GetCourseAuthors(IEnumerable<INode> authorNodes, PluralsightCourse course,
         AuthorsParseResult authorsParseResult)
      {

         var courseAuthors = new List<PluralsightCourseAuthor>();

         foreach (var node in authorNodes)
         {
            if (_nodeParser.IsCoAuthorNode(node))
            {
               var coAuthors = _nodeParser.ParseCoAuthors(node);
               // ReSharper disable once LoopCanBeConvertedToQuery
               foreach (var coAuthor in coAuthors)
               {
                  var courseAuthor = GetCourseCoAuthor(course, coAuthor,
                     authorsParseResult.AllAuthorsByFullNameExceptFullnamesakesContainer,
                     authorsParseResult.FullnamesakesAuthorsContainer);

                  courseAuthors.Add(courseAuthor);
               }
            }
            else
            {
               var author = _nodeParser.ParseAuthor(node);

               // get author from container (preserve reference integrity)
               author = authorsParseResult.AllAuthorsExceptWhoseUrlNullContainer[author];

               var courseAuthor = new PluralsightCourseAuthor
               {
                  Course = course,
                  Author = author,
                  IsAuthorCoAuthor = false, // explicit set
                  HasFullnamesake = false // explicit set
               };
               courseAuthors.Add(courseAuthor);
            }
         }

         return courseAuthors;
      }


      internal PluralsightCourseAuthor GetCourseCoAuthor(
         PluralsightCourse course,
         PluralsightAuthor author,
         Dictionary<IAuthorFullNameNaturalKey, PluralsightAuthor> allAuthorsByFullNameExceptFullnamesakesContainer,
         Dictionary<IAuthorFullNameNaturalKey, PluralsightAuthor> fullnamesakesAuthorsContainer)
      {
         PluralsightAuthor authorInContainer;
         if (allAuthorsByFullNameExceptFullnamesakesContainer.TryGetValue(author, out authorInContainer))
         {
            return new PluralsightCourseAuthor
            {
               Course = course,
               Author = authorInContainer,
               IsAuthorCoAuthor = true
            };
         }

         authorInContainer = fullnamesakesAuthorsContainer[author];
         return new PluralsightCourseAuthor
         {
            Course = course,
            Author = authorInContainer,
            IsAuthorCoAuthor = true,
            HasFullnamesake = true
         };
      }


      internal class AuthorsParseResult
      {
         public Dictionary<IAuthorFullNameNaturalKey, PluralsightAuthor> FullnamesakesAuthorsContainer { get; set; }
         public Dictionary<IAuthorFullNameNaturalKey, PluralsightAuthor> AllAuthorsByFullNameExceptFullnamesakesContainer { get; set; }
         public Dictionary<IAuthorUrlNameNaturalKey, PluralsightAuthor> AllAuthorsExceptWhoseUrlNullContainer { get; set; }
      }
   }
}