using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using TM.Data.Update;
using TM.Shared;
using Xunit;

namespace TM.Data.Pluralsight.IntegrationTests
{
   [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
   [SuppressMessage("ReSharper", "StringLiteralTypo")]
   [SuppressMessage("ReSharper", "IdentifierTypo")]
   [SuppressMessage("ReSharper", "RedundantArgumentNameForLiteralExpression")]
   [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
   public class PluralsightCatalogIntegrationTests
   {
      private const int TrainingProviderId = 1;
      private const string TrainingProviderName = "pluralsight";
      private const string Host = "http://www.pluralsight.com/";
      //private const string UpdateContentPath = @".\..\..\TestData\CoursesContent2.html";
      private const string UpdateContentPath = @".\..\..\TestData\PluralsighCatalogTestData.html";
      private readonly LocationType LocationType = LocationType.Local;

      private const string ArchiveDirectoryPath = @"e:\Projects\TrainingManager\archive\";


      private IDatabaseInitializer<UpdateDbContext> DbInitializer
      {
         get
         {
            return new TestInitializer(new DropCreateDatabaseAlways<UpdateDbContext>());
         }
      }

      public PluralsightCatalogIntegrationTests()
      {
         Database.SetInitializer(DbInitializer);
      }

      [Fact]
      public void Should_UpdateCatalogContent()
      {
         var mediaContentProcessor =
            Mock.Of<ITrainingCatalogMediaContentProcessor>(
               x => x.UpdateMediaContentAsync(It.IsAny<UpdateDbContext>()) == Task.FromResult(true));

         using (var sut = new PluralsightCatalog())
         {
            // Arrange
            var updateEvent = new UpdateEvent(TrainingProviderId, "update test", DateTime.Now);
            var mediaPath = new MediaPath();

            sut.Initialize(TrainingProviderName, TrainingProviderId, Host, UpdateContentPath, LocationType, mediaPath, ArchiveDirectoryPath);

            

            sut.SetMediaContentProcessor(mediaContentProcessor);

            // Act
            using (var context = new UpdateDbContext())
            {
               context.UpdateEvents.Add(updateEvent);
               context.SaveChanges();

               sut.UpdateAsync(updateEvent, context, useArchiveData:true, logUpdateToDb:true).Wait();
            }
         }

         // Assert
         Mock.Get(mediaContentProcessor)
               .Verify(x => x.UpdateMediaContentAsync(It.IsAny<UpdateDbContext>()), Times.Once);

         // authors
         using (var context = new UpdateDbContext())
         {
            var addedAuthors = new List<TrainingProviderAuthor>
            {
               new TrainingProviderAuthor
               {
                  FullName = "Scott Lowe",
                  SiteUrl = "http://www.pluralsight.com/author/otherscott-lowe",
                  UrlName = "otherscott-lowe"
               },

               new TrainingProviderAuthor
               {
                  FullName = "Jan-Erik Sandberg",
                  SiteUrl = "http://www.pluralsight.com/author/janerik-sandberg",
                  UrlName = "janerik-sandberg"
               },

               new TrainingProviderAuthor
               {
                  FullName = "Jeff Hurd",
                  SiteUrl = "http://www.pluralsight.com/author/jeff-hurd",
                  UrlName = "jeff-hurd"
               },

               new TrainingProviderAuthor
               {
                  FullName = "Scott Allen",
                  SiteUrl = "http://www.pluralsight.com/author/scott-allen",
                  UrlName = "scott-allen"
               }
            };

            var deletedAuthors = new List<TrainingProviderAuthor>
            {
               new TrainingProviderAuthor
               {
                  FullName = "Rob Windsor",
                  SiteUrl = "http://www.pluralsight.com/author/rob-windsor",
                  UrlName = "rob-windsor",
                  IsDeleted = true
               }
            };

            var modifiedAuthorsData = new[]
            {
               new
               {
                  Author = new TrainingProviderAuthor
                  {
                     FullName = "Robert Horvick",
                     SiteUrl = "http://www.pluralsight.com/author/robert-horvick",
                     UrlName = "robert-horvick"
                  },
                  UrlBackup = "http://www.pluralsight.com/author/modified/robert-horvick",
                  FullNameBackup = "Robert Horvick (old)"
               }
            }.ToList();

            var update = context.UpdateEvents
               .Include(x => x.AuthorsUpdates.Select(a => a.TrainingProviderAuthor.Author))
               .Include(x => x.AuthorsUpdates.Select(a => a.AuthorBackup))
               .Single();

            Assert.Equal(addedAuthors.Count, update.Added.Authors);
            Assert.Equal(deletedAuthors.Count, update.Deleted.Authors);
            Assert.Equal(modifiedAuthorsData.Count, update.Modified.Authors);

            Assert.NotEmpty(update.AuthorsUpdates);

            var updateEvents = update.AuthorsUpdates
               .Where(x => x.OperationType == OperationType.Add);

            foreach (var updateData in updateEvents)
            {
               var expectedTrainingProviderAuthor = addedAuthors.Single(x => x.UrlName == updateData.TrainingProviderAuthor.UrlName);
               Assert.Equal(expectedTrainingProviderAuthor, updateData.TrainingProviderAuthor, TrainingProviderAuthor.PropertiesComparer);

               Assert.Null(updateData.AuthorBackup);
            }


            updateEvents = update.AuthorsUpdates
               .Where(x => x.OperationType == OperationType.Delete);

            foreach (var updateData in updateEvents)
            {
               var expectedTrainingProviderAuthor = deletedAuthors.Single(x => x.UrlName == updateData.TrainingProviderAuthor.UrlName);
               Assert.Equal(expectedTrainingProviderAuthor, updateData.TrainingProviderAuthor, TrainingProviderAuthor.PropertiesComparer);

               Assert.Null(updateData.AuthorBackup);
            }


            updateEvents = update.AuthorsUpdates
               .Where(x => x.OperationType == OperationType.Modify);

            foreach (var updateData in updateEvents)
            {
               var expectedData = modifiedAuthorsData.Single(x => x.Author.UrlName == updateData.TrainingProviderAuthor.UrlName);
               Assert.Equal(expectedData.Author, updateData.TrainingProviderAuthor, TrainingProviderAuthor.PropertiesComparer);

               Assert.Equal(expectedData.FullNameBackup, updateData.AuthorBackup.FullName);
               Assert.Equal(expectedData.UrlBackup, updateData.AuthorBackup.SiteUrl);
            }
         }

         // categories
         using (var context = new UpdateDbContext())
         {
            var addedCategories = new List<Category>
            {
               new Category
               {
                  UrlName = "http",
                  Title = "HTTP",
                  LogoUrl = "http://s.pluralsight.com/mn/img/cs/http-v1.png",
                  LogoFileName = "http-v1.png"
               }
            };

            var deletedCategories = new List<Category>
            {
               new Category
               {
                  UrlName = "android",
                  Title = "Android",
                  LogoUrl = "http://s.pluralsight.com/mn/img/cs/android-v1.png",
                  LogoFileName = "android-v1.png",
                  IsDeleted = true
               }
            };

            var modifiedCategoriesData = new[]
            {
               new
               {
                  Category = new Category
                  {
                     UrlName = "adobe",
                     Title = "Adobe",
                     LogoUrl = "http://s.pluralsight.com/mn/img/cs/adobe-v1.png",
                     LogoFileName = "adobe-v1.png"
                  },
                  TitleBackup = "Adobe old",
                  LogoUrlUBackup = "http://s.pluralsight.com/mn/img/cs/old-adobe-v1.png",
                  LogoFileNameBackup = "old-adobe-v1.png"
               }
            }.ToList();


            var update = context.UpdateEvents
               .Include(x => x.CategoriesUpdates.Select(c => c.Category))
               .Include(x => x.CategoriesUpdates.Select(c => c.CategoryBackup))
               .Single();

            Assert.Equal(addedCategories.Count, update.Added.Categories);
            Assert.Equal(deletedCategories.Count, update.Deleted.Categories);
            Assert.Equal(modifiedCategoriesData.Count, update.Modified.Categories);

            Assert.NotEmpty(update.CategoriesUpdates);

            var updateEvents = update.CategoriesUpdates
               .Where(x => x.OperationType == OperationType.Add);

            foreach (var updateData in updateEvents)
            {
               var expectedCategory = addedCategories.Single(x => x.UrlName == updateData.Category.UrlName);
               Assert.Equal(expectedCategory, updateData.Category, Category.PropertiesComparer);

               Assert.Null(updateData.CategoryBackup);
            }


            updateEvents = update.CategoriesUpdates
               .Where(x => x.OperationType == OperationType.Delete);

            foreach (var updateData in updateEvents)
            {
               var expectedCategory = deletedCategories.Single(x => x.UrlName == updateData.Category.UrlName);
               Assert.Equal(expectedCategory, updateData.Category, Category.PropertiesComparer);

               Assert.Null(updateData.CategoryBackup);
            }


            updateEvents = update.CategoriesUpdates
               .Where(x => x.OperationType == OperationType.Modify);

            foreach (var updateData in updateEvents)
            {
               var expectedData = modifiedCategoriesData.Single(x => x.Category.UrlName == updateData.Category.UrlName);
               Assert.Equal(expectedData.Category, updateData.Category, Category.PropertiesComparer);

               Assert.Equal(expectedData.LogoFileNameBackup, updateData.CategoryBackup.LogoFileName);
               Assert.Equal(expectedData.LogoUrlUBackup, updateData.CategoryBackup.LogoUrl);
               Assert.Equal(expectedData.TitleBackup, updateData.CategoryBackup.Title);
            }
         }

         // courses
         using (var context = new UpdateDbContext())
         {
            context.Configuration.LazyLoadingEnabled = true;

            var addedCourses = new List<Course>
            {
               new Course
               {
                  Category = new Category {UrlName = "dotnet"},
                  Title = "Becoming a .NET Developer",
                  SiteUrl = "http://www.pluralsight.com/courses/becoming-dotnet-developer",
                  UrlName = "becoming-dotnet-developer",
                  Description ="In this course, you will learn all you need to begin your journey to become a .NET developer. This course is for those who are transitioning from being an IT-professional or are moving from other technologies. Filled with practical exercises and real world examples, you will be taken through all the major areas of .NET development. This course is also packed with tips and tricks to ensure that you become as productive as possible, as fast as possible.",
                  ShortDescription = "In this course, you will learn all you need to begin your journey to become a .NET developer. This course is for those who are transitioning from being an IT-professional or are moving from other technologies.",
                  HasClosedCaptions = false,
                  Level = CourseLevel.Beginner,
                  Rating = new CourseRating {Raters = 199, Rating = 4.3m},
                  Duration = TimeSpan.Parse("04:41:15"),
                  ReleaseDate = DateTime.Parse("28 Jan 2015", DateTimeFormatInfo.InvariantInfo),
                  Specializations = Specializations.SoftwareDeveloper,
                  CourseAuthors = new List<CourseAuthor>
                  {
                     new CourseAuthor
                     {
                        IsAuthorCoAuthor = false,
                        Course = new Course
                        {
                           UrlName = "becoming-dotnet-developer"
                        },
                        TrainingProviderAuthor = new TrainingProviderAuthor
                        {
                           UrlName = "janerik-sandberg"
                        }
                     },
                     new CourseAuthor
                     {
                        IsAuthorCoAuthor = false,
                        Course = new Course
                        {
                           UrlName = "becoming-dotnet-developer"
                        },
                        TrainingProviderAuthor = new TrainingProviderAuthor
                        {
                           UrlName = "otherscott-lowe"
                        }
                     }
                  }
               },
               new Course
               {
                  Category = new Category {UrlName = "adobe"},
                  Title = "Beginning After Effects CC",
                  SiteUrl = "http://www.pluralsight.com/courses/beginning-after-effects-cc",
                  UrlName = "beginning-after-effects-cc",
                  Description =
                     "Adobe's After Effects is the industry-leading solution for creating animations, motion graphics and visual effects. Learn the basics and more by being hands-on with real industry animations and following an entertaining scenario story arc. Beginning After Effects CC will walk you through the essential skills you need to animate everything from text and shapes to simple character animations. This course will also help you build a solid foundation in common workflows and easily break down a complex animation into manageable pieces.",
                  ShortDescription = "Learn After Effects the right way by completing real animations and following an entertaining scenario story arc. This course will prepare you to make your own animated promotional video for websites, trade-shows or additions to PowerPoint presentations.",
                  HasClosedCaptions = true,
                  Level = CourseLevel.Beginner,
                  Rating = new CourseRating {Raters = 103, Rating = 4.8m},
                  Duration = TimeSpan.Parse("03:21:09"),
                  ReleaseDate = DateTime.Parse("13 Feb 2014", DateTimeFormatInfo.InvariantInfo),
                  Specializations = Specializations.None,
                  CourseAuthors = new List<CourseAuthor>
                  {
                     new CourseAuthor
                     {
                        IsAuthorCoAuthor = false,
                        Course = new Course {UrlName = "beginning-after-effects-cc"},
                        TrainingProviderAuthor = new TrainingProviderAuthor {UrlName = "jeff-hurd"}
                     }
                  }
               }
            };

            var deletedCourses = new List<Course>
            {
               new Course
               {
                  Category = new Category {UrlName = "dotnet"},
                  Title = "VB.NET Fundamentals",
                  SiteUrl = "http://www.pluralsight.com/courses/vb-fundamentals",
                  UrlName = "vb-fundamentals",
                  Description =
                     "This course is intended for those who are new, or fairly new, to the .NET Framework and programming with Visual Basic. it has a strong focus on the language features and the integration with the Framework Class Libraries (FCL). The course is intended to give you a solid foundation so that you'll be prepared to watch courses on building full applications with technologies like WPF and ASP.NET.",
                  HasClosedCaptions = true,
                  Level = CourseLevel.Beginner,
                  Rating = new CourseRating {Raters = 507, Rating = 4.5m},
                  Duration = TimeSpan.Parse("04:52:36"),
                  ReleaseDate = DateTime.Parse("03 Nov 2011", DateTimeFormatInfo.InvariantInfo),
                  IsDeleted = true,

                  CourseAuthors = new List<CourseAuthor>
                  {
                     new CourseAuthor
                     {
                        IsAuthorCoAuthor = false,
                        IsDeleted = true,
                        Course = new Course {UrlName = "vb-fundamentals"},
                        TrainingProviderAuthor = new TrainingProviderAuthor {UrlName = "rob-windsor"}
                     },
                     new CourseAuthor
                     {
                        IsAuthorCoAuthor = false,
                        IsDeleted = true,
                        Course = new Course {UrlName = "vb-fundamentals"},
                        TrainingProviderAuthor = new TrainingProviderAuthor {UrlName = "robert-horvick"}
                     }
                  }
               }
            };

            var modifiedCoursesData = new[]
            {
               new
               {
                  Course = new Course
                  {
                     Category = new Category {UrlName = "dotnet"},
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
                     CourseAuthors = new List<CourseAuthor>
                     {
                        new CourseAuthor
                        {
                           IsAuthorCoAuthor = false,
                           Course = new Course {UrlName = "ads-part1"},
                           TrainingProviderAuthor = new TrainingProviderAuthor {UrlName = "robert-horvick"}
                        },
                        new CourseAuthor
                        {
                           IsAuthorCoAuthor = true,
                           Course = new Course {UrlName = "ads-part1"},
                           TrainingProviderAuthor = new TrainingProviderAuthor {UrlName = "scott-allen"}
                        },

                        new CourseAuthor
                        {
                           IsAuthorCoAuthor = true,
                           Course = new Course {UrlName = "ads-part1"},
                           TrainingProviderAuthor = new TrainingProviderAuthor {UrlName = "janerik-sandberg"}
                        },
                        new CourseAuthor
                        {
                           IsAuthorCoAuthor = false,
                           IsDeleted = true,
                           Course = new Course {UrlName = "ads-part1"},
                           TrainingProviderAuthor = new TrainingProviderAuthor {UrlName = "scott-lowe"}
                        }
                     }
                  },
                  CategoryUrlNameBkp = (string) null,
                  TitleBkp = "Algorithms and Data Structures - Part 1 (old)",
                  SiteUrlBkp = "http://www.pluralsight.com/courses/old/ads-part1",
                  DescriptionBkp = "Old description",
                  HasClosedCaptionsBkp = (bool?) false,
                  LevelBkp = (CourseLevel?) CourseLevel.Beginner,
                  RatersBkp = (int?) 100,
                  RatingBkp = (decimal?) 4.0m,
                  DurationBkp = (TimeSpan?) TimeSpan.Parse("03:00:00"),
                  ReleaseDateBkp = (DateTime?) DateTime.Parse("01 Aug 2010", DateTimeFormatInfo.InvariantInfo),
                  CourseAuthorsBackupData = new[]
                  {
                     new
                     {
                        AuthorUrlName = "robert-horvick",
                        Backup = new CourseAuthorBackup {IsAuthorCoAuthor = true, OperationType = OperationType.Modify}
                     },
                     new
                     {
                        AuthorUrlName = "scott-lowe",
                        Backup = new CourseAuthorBackup {IsAuthorCoAuthor = null, OperationType = OperationType.Delete}
                     },
                     new
                     {
                        AuthorUrlName = "scott-allen",
                        Backup = new CourseAuthorBackup {IsAuthorCoAuthor = null, OperationType = OperationType.Add}
                     }
                     ,
                     new
                     {
                        AuthorUrlName = "janerik-sandberg",
                        Backup = new CourseAuthorBackup {IsAuthorCoAuthor = null, OperationType = OperationType.Add}
                     }
                  }
               },
               new
               {
                  Course = new Course
                  {
                     Category = new Category {UrlName = "http"},
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
                     CourseAuthors = new List<CourseAuthor>
                     {
                        new CourseAuthor
                        {
                           IsAuthorCoAuthor = false,
                           Course = new Course {UrlName = "xhttp-fund"},
                           TrainingProviderAuthor = new TrainingProviderAuthor {UrlName = "scott-allen"}
                        }
                     }
                  },
                  CategoryUrlNameBkp = "android",
                  TitleBkp = (string) null,
                  SiteUrlBkp = (string) null,
                  DescriptionBkp = (string) null,
                  HasClosedCaptionsBkp = (bool?) null,
                  LevelBkp = (CourseLevel?) null,
                  RatersBkp = (int?) null,
                  RatingBkp = (decimal?) null,
                  DurationBkp = (TimeSpan?) null,
                  ReleaseDateBkp = (DateTime?) null,
                  CourseAuthorsBackupData = new[]
                  {
                     new
                     {
                        AuthorUrlName = "scott-allen",
                        Backup = new CourseAuthorBackup {IsAuthorCoAuthor = null, OperationType = OperationType.Add}
                     }
                  }
               }

            }.ToList();


            var update = context.UpdateEvents
               .Include(x => x.CoursesUpdates.Select(c => c.Course.Category))
               .Include(x => x.CoursesUpdates.Select(c => c.Course.CourseSpecializations))
               .Include(x => x.CoursesUpdates.Select(c => c.Course.CourseAuthors.Select(ca => ca.TrainingProviderAuthor)))
               .Include(x => x.CoursesUpdates.Select(c => c.Course.CourseAuthors.Select(ca => ca.Course)))
               .Include(x => x.CoursesUpdates.Select(c => c.CourseBackup.CourseAuthorBackups))
               .Include(x => x.CoursesUpdates.Select(c => c.CourseBackup.Category))
               .Single();

            Assert.Equal(addedCourses.Count, update.Added.Courses);
            Assert.Equal(deletedCourses.Count, update.Deleted.Courses);
            Assert.Equal(modifiedCoursesData.Count, update.Modified.Courses);

            Assert.NotEmpty(update.CoursesUpdates);

            var updateEvents = update.CoursesUpdates
               .Where(x => x.OperationType == OperationType.Add);

            foreach (var updateData in updateEvents)
            {
               var expectedCourse = addedCourses.Single(x => x.UrlName == updateData.Course.UrlName);
               Assert.Equal(expectedCourse, updateData.Course, Course.PropertiesComparer);
               Assert.Equal(expectedCourse.Category.UrlName, updateData.Course.Category.UrlName);

               foreach (var resultCourseAuthor in updateData.Course.CourseAuthors)
               {
                  var expectedCourseAuthor =
                     expectedCourse.CourseAuthors.Single(x => x.TrainingProviderAuthor.UrlName == resultCourseAuthor.TrainingProviderAuthor.UrlName);

                  Assert.Equal(expectedCourseAuthor, resultCourseAuthor, CourseAuthor.PropertiesComparer);
                  Assert.Equal(expectedCourseAuthor.Course.UrlName, resultCourseAuthor.Course.UrlName);
               }

               Assert.Null(updateData.CourseBackup);
            }


            updateEvents = update.CoursesUpdates
               .Where(x => x.OperationType == OperationType.Delete);

            foreach (var updateData in updateEvents)
            {
               var expectedCourse = deletedCourses.Single(x => x.UrlName == updateData.Course.UrlName);
               Assert.Equal(expectedCourse, updateData.Course, Course.PropertiesComparer);
               Assert.Equal(expectedCourse.Category.UrlName, updateData.Course.Category.UrlName);

               foreach (var resultCourseAuthor in updateData.Course.CourseAuthors)
               {
                  var expectedCourseAuthor =
                     expectedCourse.CourseAuthors.Single(x => x.TrainingProviderAuthor.UrlName == resultCourseAuthor.TrainingProviderAuthor.UrlName);

                  Assert.Equal(expectedCourseAuthor, resultCourseAuthor, CourseAuthor.PropertiesComparer);
                  Assert.Equal(expectedCourseAuthor.Course.UrlName, resultCourseAuthor.Course.UrlName);
               }

               Assert.Null(updateData.CourseBackup);
            }


            updateEvents = update.CoursesUpdates
               .Where(x => x.OperationType == OperationType.Modify);

            foreach (var updateData in updateEvents)
            {
               var expectedData = modifiedCoursesData.Single(x => x.Course.UrlName == updateData.Course.UrlName);
               Assert.Equal(expectedData.Course, updateData.Course, Course.PropertiesComparer);
               Assert.Equal(expectedData.Course.Category.UrlName, updateData.Course.Category.UrlName);

               foreach (var resultCourseAuthor in updateData.Course.CourseAuthors)
               {
                  var expectedCourseAuthor =
                     expectedData.Course.CourseAuthors.Single(x => x.TrainingProviderAuthor.UrlName == resultCourseAuthor.TrainingProviderAuthor.UrlName);

                  Assert.Equal(expectedCourseAuthor, resultCourseAuthor, CourseAuthor.PropertiesComparer);
                  Assert.Equal(expectedCourseAuthor.Course.UrlName, resultCourseAuthor.Course.UrlName);
               }


               var resultCategoryUrlNameBkp = updateData.CourseBackup.CategoryId == null
                  ? null
                  : updateData.CourseBackup.Category.UrlName;

               Assert.Equal(expectedData.CategoryUrlNameBkp, resultCategoryUrlNameBkp);

               Assert.Equal(expectedData.TitleBkp, updateData.CourseBackup.Title);
               Assert.Equal(expectedData.SiteUrlBkp, updateData.CourseBackup.SiteUrl);
               Assert.Equal(expectedData.DescriptionBkp, updateData.CourseBackup.Description);
               Assert.Equal(expectedData.HasClosedCaptionsBkp, updateData.CourseBackup.HasClosedCaptions);
               Assert.Equal(expectedData.LevelBkp, (CourseLevel?)updateData.CourseBackup.CourseLevel);
               Assert.Equal(expectedData.DurationBkp, updateData.CourseBackup.Duration);
               Assert.Equal(expectedData.ReleaseDateBkp, updateData.CourseBackup.ReleaseDate);

               foreach (var resultCourseAuthorBackup in updateData.CourseBackup.CourseAuthorBackups)
               {
                  var expectedCourseAuthorBackup =
                     expectedData.CourseAuthorsBackupData.Single(x => x.AuthorUrlName == resultCourseAuthorBackup.TrainingProviderAuthor.UrlName);

                  Assert.Equal(expectedCourseAuthorBackup.Backup, resultCourseAuthorBackup, CourseAuthorBackup.PropertiesComparer);
               }
            }
         }

         // author resolve
         using (var context = new UpdateDbContext())
         {

            var expectedAuthorResolves = new List<AuthorResolve>
            {
               new AuthorResolve
               {
                  Course = new Course {UrlName = "ads-part1"},
                  AuthorFullName = "Scott Lowe",
                  AuthorUrlName = "otherscott-lowe;scott-lowe",
                  AuthorSiteUrl =
                     "http://www.pluralsight.com/author/otherscott-lowe;http://www.pluralsight.com/author/scott-lowe",
                  IsAuthorCoAuthor = true,
                  ResolveState = ResolveState.Pending,
                  ProblemType = ProblemType.AuthorIsFullnamesake
               },

               new AuthorResolve
               {
                  Course = new Course {UrlName = "ads-part1"},
                  AuthorFullName = "Keith Sparkjoy",
                  AuthorUrlName = null,
                  AuthorSiteUrl = null,
                  IsAuthorCoAuthor = true,
                  ResolveState = ResolveState.Pending,
                  ProblemType = ProblemType.AuthorUrlIsNull
               }
            };

            var authorResolves = context.AuthorsResolves.Include(x => x.Course);

            foreach (var resultAuthorResolve in authorResolves)
            {
               var expectedAuthorResolve = expectedAuthorResolves.Single(
                  x =>
                     x.Course.UrlName == resultAuthorResolve.Course.UrlName &&
                     x.AuthorFullName == resultAuthorResolve.AuthorFullName);

               Assert.Equal(expectedAuthorResolve, resultAuthorResolve, AuthorResolve.PropertiesComparer);
            }
         }
      }



      private class TestInitializer : ApplicationDbInitializer<UpdateDbContext>
      {
         public TestInitializer(IDatabaseInitializer<UpdateDbContext> initializationInitializationStrategy)
            : base(initializationInitializationStrategy)
         {
         }

         protected override void Seed(UpdateDbContext context)
         {
            base.Seed(context);

            var pluralsightProvider = new TrainingProvider
            {
               Id = TrainingProviderId,
               Name = TrainingProviderName,
               SiteUrl = Host,
               LogoFileName = "pluralsight-logo-orange-250x78-v1.png",
               UpdateFrequencyHours = 23,
               AllowedUpdateUtcHoursString = string.Join(";", Enumerable.Range(0, 23)),
               SourceUrl = UpdateContentPath,
               SourceLocation = LocationType.Local,
               AssemblyType =
                  "TM.Data.Pluralsight.PluralsightCatalog, TM.Data.Pluralsight, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            };

            var anotherProvider = new TrainingProvider
            {
               Id = TrainingProviderId + 1,
               Name = "another training provider",
               SiteUrl = "http://somehost.com/",
               LogoFileName = "some-logo.png",
               UpdateFrequencyHours = 23,
               AllowedUpdateUtcHoursString = string.Join(";", Enumerable.Range(0, 23)),
               SourceUrl = "undefined",
               SourceLocation = LocationType.Local,
               AssemblyType = "undefined",
               TrainingProviderAuthors = new List<TrainingProviderAuthor>
               {
                  new TrainingProviderAuthor
                  {
                     FullName = "Jeff Hurd",
                     SiteUrl = "http://www.pluralsight.com/author/jeff-hurd",
                     UrlName = "jeff-hurd",
                     Author = new Author
                     {
                        FirstName = "Jeff",
                        LastName = "Hurd",
                        Social = new Social
                        {
                           TwitterLink = "http://twitter.com/jeffhurd"
                        }
                     }
                  }
               }
            };


            context.TrainingProviders.AddRange(new[]
            {
               pluralsightProvider, anotherProvider
            });

            context.SaveChanges();

            var dotNetCategory = new Category // will be unchanged
            {
               TrainingProviderId = pluralsightProvider.Id,
               UrlName = "dotnet",
               Title = ".NET",
               LogoUrl = "http://s.pluralsight.com/mn/img/cs/dotnet-v1.png",
               LogoFileName = "dotnet-v1.png"
            };

            var adobeCategory = new Category // will be modified
            {
               TrainingProviderId = pluralsightProvider.Id,
               UrlName = "adobe",
               Title = "Adobe old",
               LogoUrl = "http://s.pluralsight.com/mn/img/cs/old-adobe-v1.png",
               LogoFileName = "old-adobe-v1.png"
            };

            var androidCategory = new Category // will be deleted
            {
               TrainingProviderId = pluralsightProvider.Id,
               UrlName = "android",
               Title = "Android",
               LogoUrl = "http://s.pluralsight.com/mn/img/cs/android-v1.png",
               LogoFileName = "android-v1.png"
            };

            context.Categories.AddRange(new[]
            {
               dotNetCategory, adobeCategory, androidCategory
            });

            context.SaveChanges();


            var scottLoweAuthor = new TrainingProviderAuthor // will be unchanged
            {
               TrainingProviderId = pluralsightProvider.Id,
               FullName = "Scott Lowe",
               SiteUrl = "http://www.pluralsight.com/author/scott-lowe",
               UrlName = "scott-lowe",
               Author = new Author { FirstName = "Scott", LastName = "Lowe" }
            };

            var robertHorvickAuthor = new TrainingProviderAuthor // will be modified
            {
               TrainingProviderId = pluralsightProvider.Id,
               FullName = "Robert Horvick (old)",
               SiteUrl = "http://www.pluralsight.com/author/modified/robert-horvick",
               UrlName = "robert-horvick",
               Author = new Author { FirstName = "Robert", LastName = "Horvick" }
            };

            var robWindsorAuthor = new TrainingProviderAuthor // will be deleted
            {
               TrainingProviderId = pluralsightProvider.Id,
               FullName = "Rob Windsor",
               SiteUrl = "http://www.pluralsight.com/author/rob-windsor",
               UrlName = "rob-windsor",
               Author = new Author { FirstName = "Rob", LastName = "Windsor" }
            };

            context.TrainingProviderAuthors.AddRange(new[]
            {
               scottLoweAuthor, robertHorvickAuthor, robWindsorAuthor
            });

            context.SaveChanges();


            var pluralsightCourses = new List<Course>
            {
               new Course // will be unchanged
               {
                  TrainingProviderId = pluralsightProvider.Id,
                  CategoryId = dotNetCategory.Id,
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

                  CourseAuthors = new List<CourseAuthor>
                  {
                     new CourseAuthor
                     {
                        TrainingProviderId = pluralsightProvider.Id,
                        IsAuthorCoAuthor = false,
                        AuthorId = scottLoweAuthor.AuthorId
                     }
                  }
               },

               new Course // will be modified
               {
                  TrainingProviderId = pluralsightProvider.Id,
                  CategoryId = dotNetCategory.Id,
                  Title = "Algorithms and Data Structures - Part 1 (old)",
                  SiteUrl = "http://www.pluralsight.com/courses/old/ads-part1",
                  UrlName = "ads-part1",
                  Description = "Old description",
                  HasClosedCaptions = false,
                  Level = CourseLevel.Beginner,
                  Rating = new CourseRating {Raters = 100, Rating = 4.0m},
                  Duration = TimeSpan.Parse("03:00:00"),
                  ReleaseDate = DateTime.Parse("01 Aug 2010", DateTimeFormatInfo.InvariantInfo),

                  CourseAuthors = new List<CourseAuthor>
                  {
                     new CourseAuthor
                     {
                        TrainingProviderId = pluralsightProvider.Id,
                        IsAuthorCoAuthor = true,
                        AuthorId = robertHorvickAuthor.AuthorId
                     },
                     new CourseAuthor // will be deleted
                     {
                        TrainingProviderId = pluralsightProvider.Id,
                        IsAuthorCoAuthor = false,
                        AuthorId = scottLoweAuthor.AuthorId
                     }
                  }
               },

               new Course // will be deleted
               {
                  TrainingProviderId = pluralsightProvider.Id,
                  CategoryId = dotNetCategory.Id,
                  Title = "VB.NET Fundamentals",
                  SiteUrl = "http://www.pluralsight.com/courses/vb-fundamentals",
                  UrlName = "vb-fundamentals",
                  Description =
                     "This course is intended for those who are new, or fairly new, to the .NET Framework and programming with Visual Basic. it has a strong focus on the language features and the integration with the Framework Class Libraries (FCL). The course is intended to give you a solid foundation so that you'll be prepared to watch courses on building full applications with technologies like WPF and ASP.NET.",
                  HasClosedCaptions = true,
                  Level = CourseLevel.Beginner,
                  Rating = new CourseRating {Raters = 507, Rating = 4.5m},
                  Duration = TimeSpan.Parse("04:52:36"),
                  ReleaseDate = DateTime.Parse("03 Nov 2011", DateTimeFormatInfo.InvariantInfo),

                  CourseAuthors = new List<CourseAuthor>
                  {
                     new CourseAuthor
                     {
                        TrainingProviderId = pluralsightProvider.Id,
                        IsAuthorCoAuthor = false,
                        AuthorId = robWindsorAuthor.AuthorId
                     },
                     new CourseAuthor
                     {
                        TrainingProviderId = pluralsightProvider.Id,
                        IsAuthorCoAuthor = false,
                        AuthorId = robertHorvickAuthor.AuthorId
                     }
                  }
               },

               new Course // will be modified (moved to another category)
               {
                  TrainingProviderId = pluralsightProvider.Id,
                  CategoryId = androidCategory.Id,
                  Title = "HTTP Fundamentals",
                  SiteUrl = "http://www.pluralsight.com/courses/xhttp-fund",
                  UrlName = "xhttp-fund",
                  Description =
                     "HTTP is the protocol of the web, and this course will look at HTTP from a web developer's perspective. We'll cover resources, messages, cookies, and authentication protocols. We'll look at how HTTP clients can use persistent and parallel connections to improve performance, and see how the web scales to meet demand using cache headers and proxy servers. By the end of the course you will have the knowledge to build better web applications and web services.",
                  HasClosedCaptions = true,
                  Level = CourseLevel.Beginner,
                  Rating = new CourseRating {Raters = 1757, Rating = 4.7m},
                  Duration = TimeSpan.Parse("02:50:08"),
                  ReleaseDate = DateTime.Parse("17 Feb 2012", DateTimeFormatInfo.InvariantInfo)
               }
            };

            context.Courses.AddRange(pluralsightCourses);

            context.SaveChanges();
         }
      }
   }
}
