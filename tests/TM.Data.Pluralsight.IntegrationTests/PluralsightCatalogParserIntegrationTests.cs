using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using TM.Data.Parse;
using TM.Data.Update;
using TM.Shared;
using TM.Shared.HtmlContainer;
using Xunit;

namespace TM.Data.Pluralsight.IntegrationTests
{
   [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
   [SuppressMessage("ReSharper", "StringLiteralTypo")]
   public class PluralsightCatalogParserIntegrationTests
   {

      private Dictionary<string, PluralsightAuthor> _authorsDictionary;
      private Dictionary<string, PluralsightAuthor> _fullnamesakesAuthorsDictionary;

      private Dictionary<string, PluralsightCourse> _coursesDictionary;
      private Dictionary<string, PluralsightCategory> _categoriesDictionary;

      private Dictionary<string, PluralsightAuthor> ExpectedFullnamesakesAuthorsDictionary
      {
         get
         {
            if (_fullnamesakesAuthorsDictionary != null)
            {
               return _fullnamesakesAuthorsDictionary;
            }

            _fullnamesakesAuthorsDictionary = new Dictionary<string, PluralsightAuthor>
            {
               {
                  "Scott Lowe", new PluralsightAuthor
                  {
                     FullName = "Scott Lowe",
                     SiteUrl =
                        "http://www.pluralsight.com/author/otherscott-lowe;http://www.pluralsight.com/author/scott-lowe",
                     UrlName = "otherscott-lowe;scott-lowe"
                  }
               }
            };


            return _fullnamesakesAuthorsDictionary;
         }
      }

      private Dictionary<string, PluralsightAuthor> ExpectedAuthorsDictionary
      {
         get
         {
            if (_authorsDictionary != null)
            {
               return _authorsDictionary;
            }

            _authorsDictionary = new Dictionary<string, PluralsightAuthor>
            {
               {
                  "Scott Lowe", new PluralsightAuthor
                  {
                     FullName = "Scott Lowe",
                     SiteUrl = "http://www.pluralsight.com/author/scott-lowe",
                     UrlName = "scott-lowe"
                  }
               },
               {
                  "Scott Lowe other", new PluralsightAuthor
                  {
                     FullName = "Scott Lowe",
                     SiteUrl = "http://www.pluralsight.com/author/otherscott-lowe",
                     UrlName = "otherscott-lowe"
                  }
               },
               {
                  "Robert Horvick", new PluralsightAuthor
                  {
                     FullName = "Robert Horvick",
                     SiteUrl = "http://www.pluralsight.com/author/robert-horvick",
                     UrlName = "robert-horvick"
                  }
               },
               {
                  "Jan-Erik Sandberg", new PluralsightAuthor
                  {
                     FullName = "Jan-Erik Sandberg",
                     SiteUrl = "http://www.pluralsight.com/author/janerik-sandberg",
                     UrlName = "janerik-sandberg"
                  }
               },
               {
                  "Jeff Hurd", new PluralsightAuthor
                  {
                     FullName = "Jeff Hurd",
                     SiteUrl = "http://www.pluralsight.com/author/jeff-hurd",
                     UrlName = "jeff-hurd"
                  }
               },
               {
                  "Scott Allen", new PluralsightAuthor
                  {
                     FullName = "Scott Allen",
                     SiteUrl = "http://www.pluralsight.com/author/scott-allen",
                     UrlName = "scott-allen"
                  }
               },
               {
                  "Keith Sparkjoy", new PluralsightAuthor
                  {
                     FullName = "Keith Sparkjoy"
                  }
               }
            };

            return _authorsDictionary;
         }
      }

      private Dictionary<string, PluralsightCourse> ExpectedCoursesDictionary
      {
         get
         {
            if (_coursesDictionary != null)
            {
               return _coursesDictionary;
            }

            _coursesDictionary = new Dictionary<string, PluralsightCourse>
            {
               {
                  ".NET Reflector by Example", new PluralsightCourse
                  {
                     Title = ".NET Reflector by Example",
                     SiteUrl = "http://www.pluralsight.com/courses/dotnet-reflector-by-example",
                     UrlName = "dotnet-reflector-by-example",
                     Description =
                        "Learn how to effectively use Redgate's .NET Reflector through a series of sample demonstrations.",
                     HasClosedCaptions = false,
                     Level = CourseLevel.Advanced,
                     Rating = new CourseRating {Raters = 118, Rating = 4.0m},
                     Duration = TimeSpan.Parse("01:04:19"),
                     ReleaseDate = DateTime.Parse("16 Mar 2010", DateTimeFormatInfo.InvariantInfo),
                     CourseAuthors = new List<PluralsightCourseAuthor>
                     {
                        new PluralsightCourseAuthor
                        {
                           IsAuthorCoAuthor = false,
                           HasFullnamesake = false,
                           Author = ExpectedAuthorsDictionary["Scott Lowe"]
                        }
                     }
                  }
               },

               {
                  "Algorithms and Data Structures - Part 1", new PluralsightCourse
                  {
                     Title = "Algorithms and Data Structures - Part 1",
                     SiteUrl = "http://www.pluralsight.com/courses/ads-part1",
                     UrlName = "ads-part1",
                     Description =
                        "In this course we will look at the core data structures and algorithms used in everyday applications. We will discuss the trade-offs involved with choosing each data structure, along with traversal, retrieval, and update algorithms. This is part 1 of a two-part series of courses covering algorithms and data structures. In this part we cover linked lists, stacks, queues, binary trees, and hash tables.",
                     HasClosedCaptions = true,
                     Level = CourseLevel.Intermediate,
                     Rating = new CourseRating {Raters = 615, Rating = 4.5m},
                     Duration = TimeSpan.Parse("03:13:48"),
                     ReleaseDate = DateTime.Parse("15 Aug 2011", DateTimeFormatInfo.InvariantInfo),
                     CourseAuthors = new List<PluralsightCourseAuthor>
                     {
                        new PluralsightCourseAuthor
                        {
                           IsAuthorCoAuthor = false,
                           Author = ExpectedAuthorsDictionary["Robert Horvick"]
                        },
                        new PluralsightCourseAuthor
                        {
                           IsAuthorCoAuthor = true,
                           Author = ExpectedAuthorsDictionary["Scott Allen"]
                        },
                        new PluralsightCourseAuthor
                        {
                           IsAuthorCoAuthor = true,
                           HasFullnamesake = true,
                           Author = ExpectedFullnamesakesAuthorsDictionary["Scott Lowe"]
                        },
                        new PluralsightCourseAuthor
                        {
                           IsAuthorCoAuthor = true,
                           Author = ExpectedAuthorsDictionary["Jan-Erik Sandberg"]
                        },
                        new PluralsightCourseAuthor
                        {
                           IsAuthorCoAuthor = true,
                           Author = ExpectedAuthorsDictionary["Keith Sparkjoy"]
                        }
                     }
                  }
               },

               {
                  "Becoming a .NET Developer", new PluralsightCourse
                  {
                     Title = "Becoming a .NET Developer",
                     SiteUrl = "http://www.pluralsight.com/courses/becoming-dotnet-developer",
                     UrlName = "becoming-dotnet-developer",
                     Description =
                        "In this course, you will learn all you need to begin your journey to become a .NET developer. This course is for those who are transitioning from being an IT-professional or are moving from other technologies. Filled with practical exercises and real world examples, you will be taken through all the major areas of .NET development. This course is also packed with tips and tricks to ensure that you become as productive as possible, as fast as possible.",
                     HasClosedCaptions = false,
                     Level = CourseLevel.Beginner,
                     Rating = new CourseRating {Raters = 199, Rating = 4.3m},
                     Duration = TimeSpan.Parse("04:41:15"),
                     ReleaseDate = DateTime.Parse("28 Jan 2015", DateTimeFormatInfo.InvariantInfo),
                     CourseAuthors = new List<PluralsightCourseAuthor>
                     {
                        new PluralsightCourseAuthor
                        {
                           IsAuthorCoAuthor = false,
                           Author = ExpectedAuthorsDictionary["Jan-Erik Sandberg"]
                        },
                        new PluralsightCourseAuthor
                        {
                           IsAuthorCoAuthor = false,
                           HasFullnamesake = false,
                           Author = ExpectedAuthorsDictionary["Scott Lowe other"]
                        }
                     }
                  }
               },
               {
                  "Beginning After Effects CC", new PluralsightCourse
                  {
                     Title = "Beginning After Effects CC",
                     SiteUrl = "http://www.pluralsight.com/courses/beginning-after-effects-cc",
                     UrlName = "beginning-after-effects-cc",
                     Description =
                        "Adobe's After Effects is the industry-leading solution for creating animations, motion graphics and visual effects. Learn the basics and more by being hands-on with real industry animations and following an entertaining scenario story arc. Beginning After Effects CC will walk you through the essential skills you need to animate everything from text and shapes to simple character animations. This course will also help you build a solid foundation in common workflows and easily break down a complex animation into manageable pieces.",
                     HasClosedCaptions = true,
                     Level = CourseLevel.Beginner,
                     Rating = new CourseRating {Raters = 103, Rating = 4.8m},
                     Duration = TimeSpan.Parse("03:21:09"),
                     ReleaseDate = DateTime.Parse("13 Feb 2014", DateTimeFormatInfo.InvariantInfo),
                     CourseAuthors = new List<PluralsightCourseAuthor>
                     {
                        new PluralsightCourseAuthor
                        {
                           IsAuthorCoAuthor = false,
                           Author = ExpectedAuthorsDictionary["Jeff Hurd"]
                        }
                     }
                  }
               },
               {
                  "HTTP Fundamentals", new PluralsightCourse
                  {
                     Title = "HTTP Fundamentals",
                     SiteUrl = "http://www.pluralsight.com/courses/xhttp-fund",
                     UrlName = "xhttp-fund",
                     Description =
                        "HTTP is the protocol of the web, and this course will look at HTTP from a web developer's perspective. We'll cover resources, messages, cookies, and authentication protocols. We'll look at how HTTP clients can use persistent and parallel connections to improve performance, and see how the web scales to meet demand using cache headers and proxy servers. By the end of the course you will have the knowledge to build better web applications and web services.",
                     HasClosedCaptions = true,
                     Level = CourseLevel.Beginner,
                     Rating = new CourseRating {Raters = 1757, Rating = 4.7m},
                     Duration = TimeSpan.Parse("02:50:08"),
                     ReleaseDate = DateTime.Parse("17 Feb 2012", DateTimeFormatInfo.InvariantInfo),
                     CourseAuthors = new List<PluralsightCourseAuthor>
                     {
                        new PluralsightCourseAuthor
                        {
                           IsAuthorCoAuthor = false,
                           Author = ExpectedAuthorsDictionary["Scott Allen"]
                        }
                     }
                  }
               }
            };

            return _coursesDictionary;
         }
      }

      private Dictionary<string, PluralsightCategory> ExpectedCategoriesDictionary
      {
         get
         {
            if (_categoriesDictionary != null)
            {
               return _categoriesDictionary;
            }

            _categoriesDictionary = new Dictionary<string, PluralsightCategory>
            {
               {
                  "dotnet", new PluralsightCategory
                  {
                     UrlName = "dotnet",
                     Title = ".NET",
                     LogoUrl = "http://s.pluralsight.com/mn/img/cs/dotnet-v1.png",
                     LogoFileName = "dotnet-v1.png",
                     Courses = new List<PluralsightCourse>
                     {
                        ExpectedCoursesDictionary[".NET Reflector by Example"],
                        ExpectedCoursesDictionary["Algorithms and Data Structures - Part 1"],
                        ExpectedCoursesDictionary["Becoming a .NET Developer"]
                     }
                  }
               },
               {
                  "adobe", new PluralsightCategory
                  {
                     UrlName = "adobe",
                     Title = "Adobe",
                     LogoUrl = "http://s.pluralsight.com/mn/img/cs/adobe-v1.png",
                     LogoFileName = "adobe-v1.png",
                     Courses = new List<PluralsightCourse>
                     {
                        ExpectedCoursesDictionary["Beginning After Effects CC"]
                     }
                  }
               },
               {
                  "http", new PluralsightCategory
                  {
                     UrlName = "http",
                     Title = "HTTP",
                     LogoUrl = "http://s.pluralsight.com/mn/img/cs/http-v1.png",
                     LogoFileName = "http-v1.png",
                     Courses = new List<PluralsightCourse>
                     {
                        ExpectedCoursesDictionary["HTTP Fundamentals"]
                     }
                  }
               }
            };

            return _categoriesDictionary;
         }
      }

      

      [Fact]
      public void Should_ParseCatalogContent()
      {
         // Arrange
         const string host = "http://www.pluralsight.com/";

         var nodeSelector = new PluralsightNodeSelector();
         var nodeParser = new PluralsightNodeParser(host, nodeSelector);

         var sut = new PluralsightCatalogParser(nodeSelector, nodeParser);

         // add category reference for each course
         foreach (var expectedCategory in ExpectedCategoriesDictionary.Values)
         {
            foreach (var expectedCourse in expectedCategory.Courses)
            {
               expectedCourse.Category = expectedCategory;
            }
         }

         const string catalogPath = "./../../TestData/PluralsighCatalogTestData.html";
         var loader = new HtmlLoader<HtmlAgilityPackHtmlContainer>();
         var catalog = loader.Load(catalogPath, LocationType.Local);


         // Act
         var result = sut.Parse(catalog);

         // Assert

         // Authors
         var resultAuthors = result.AuthorsParseResult.AuthorsExceptWhoseUrlNullContainer.Values
            .OrderBy(x => x.UrlName);

         var expectedAuthors = ExpectedAuthorsDictionary.Values
            .Where(x => x.UrlName != null)
            .OrderBy(x => x.UrlName);

         Assert.Equal(expectedAuthors, resultAuthors, PluralsightAuthor.PropertiesComparer);

         // Courses
         var resultCourses = result.CoursesParseResult.CourseContainer.Values;
         var expectedCourses = ExpectedCoursesDictionary.Values;

         CheckCoursesEquality(expectedCourses, resultCourses);

         // Categories
         var resultCategories = result.CategoriesParseResult.CategoryContainer.Values;
         var expectedCategories = ExpectedCategoriesDictionary.Values;

         Assert.Equal(expectedCategories.Count, resultCategories.Count);
         foreach (var resultCategory in resultCategories)
         {
            var expectedCategory = expectedCategories
               .Single(x => x.UrlName == resultCategory.UrlName);

            Assert.Equal(expectedCategory, resultCategory, PluralsightCategory.PropertiesComparer);

            CheckCoursesEquality(expectedCategory.Courses, resultCategory.Courses, resultCategory);
         }
      }


      private void CheckCoursesEquality(ICollection<PluralsightCourse> expectedCourses,
         ICollection<PluralsightCourse> resultCourses, PluralsightCategory resultCoursesCategory = null)
      {
         Assert.Equal(expectedCourses.Count, resultCourses.Count);

         foreach (var resultCourse in resultCourses)
         {
            if (resultCoursesCategory != null)
            {
               Assert.Same(resultCourse.Category, resultCoursesCategory);
            }

            var expectedCourse = expectedCourses
               .Single(x => x.UrlName == resultCourse.UrlName);

            Assert.Equal(expectedCourse.Category, resultCourse.Category, PluralsightCategory.PropertiesComparer);
            Assert.Equal(expectedCourse, resultCourse, PluralsightCourse.PropertiesComparer);

            CheckCourseAuthorsEquality(expectedCourse, resultCourse);
         }
      }


      private void CheckCourseAuthorsEquality(PluralsightCourse expectedCourse,
         PluralsightCourse resultCourse)
      {
         Assert.Equal(expectedCourse.CourseAuthors.Count, resultCourse.CourseAuthors.Count);

         foreach (var resultCourseAuthor in resultCourse.CourseAuthors)
         {
            Assert.Same(resultCourseAuthor.Course, resultCourse);

            var expectedCourseAuthor = expectedCourse.CourseAuthors
               .First(x => x.Author.FullName == resultCourseAuthor.Author.FullName &&
                           x.Author.UrlName == resultCourseAuthor.Author.UrlName);

            Assert.Equal(expectedCourseAuthor, resultCourseAuthor, PluralsightCourseAuthor.PropertiesComparer);

            Assert.Equal(expectedCourseAuthor.Author, resultCourseAuthor.Author, PluralsightAuthor.PropertiesComparer);
         }

      }
   }
}
