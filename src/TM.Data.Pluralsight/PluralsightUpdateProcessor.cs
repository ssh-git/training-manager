using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TM.Data.Pluralsight.Properties;
using TM.Data.Update;

namespace TM.Data.Pluralsight
{
   internal class PluralsightUpdateProcessor :
      TrainingCatalogUpdateProcessor<PluralsightCategory, PluralsightCourse, PluralsightAuthor>
   {
      /// <exception cref="ArgumentNullException">
      /// <paramref name="context" /> or
      /// <paramref name="catalogBackupProcessor" /> is <see langword="null" />.</exception>
      public PluralsightUpdateProcessor(int trainingProviderId, UpdateDbContext context,
         ICatalogBackupProcessor<PluralsightCategory, PluralsightCourse, PluralsightAuthor> catalogBackupProcessor,
         bool logUpdateToDb)
         : base(trainingProviderId, context, catalogBackupProcessor, logUpdateToDb)
      {
      }


      #region TrainingCatalogUpdateProcessor Overrides

      /// <exception cref="InvalidOperationException"><paramref name="coursePair"/>.ParseModelEntity.Category.Id equal <value>0</value>.</exception>
      protected override bool IsMovedToAnotherCategory(EntityPair<Course, PluralsightCourse> coursePair)
      {
         if (coursePair.ParseModelEntity.Category.Id == 0)
            throw new InvalidOperationException(Resources.InvalidOperation_CategoryIdMustBeSetOnTheCourse);


         var isMovedToAnotherCategory = coursePair.DbEntity.CategoryId != coursePair.ParseModelEntity.Category.Id;

         return isMovedToAnotherCategory;
      }

      /// <exception cref="InvalidOperationException"><paramref name="coursePair"/>.ParseModelEntity.Category.Id equal <value>0</value>.</exception>
      protected override void AssignCategoryIdToDbEntity(EntityPair<Course, PluralsightCourse> coursePair)
      {
         if (coursePair.ParseModelEntity.Category.Id == 0)
            throw new InvalidOperationException(Resources.InvalidOperation_CategoryIdMustBeSetOnTheCourse);


         coursePair.DbEntity.CategoryId = coursePair.ParseModelEntity.Category.Id;
      }


      /// <exception cref="InvalidOperationException">
      /// <paramref name="processingCourse"/>.CourseAuthors.Any(x => x.Course.Id == 0) == <see langword="true" /> or
      /// <paramref name="processingCourse"/>.CourseAuthors.Any(x => x.Author.Id == 0) == <see langword="true" /></exception>
      protected override CourseAuthorsParseResult GetCourseAuthorsParseResult(PluralsightCourse processingCourse, List<AuthorResolve> authorResolves)
      {
         var courseAuthorsParseResult = new CourseAuthorsParseResult();

         foreach (var courseAuthorParseModel in processingCourse.CourseAuthors)
         {
            if (courseAuthorParseModel.Course.Id == 0)
            {
               throw new InvalidOperationException(Resources.InvalidOperation_CategoryIdMustBeSetOnTheCourse);
            }

            if (courseAuthorParseModel.HasFullnamesake)
            {
               var resolvedAuthor =
                  authorResolves.SingleOrDefault(x => x.CourseId == courseAuthorParseModel.Course.Id &&
                                                      x.ProblemType == ProblemType.AuthorIsFullnamesake &&
                                                      x.AuthorSiteUrl == courseAuthorParseModel.Author.SiteUrl);
               if (resolvedAuthor != null)
               {
                  Debug.Assert(resolvedAuthor.ResolvedAuthorId != null, "resolvedAuthor.ResolvedAuthorId != null");

                  courseAuthorsParseResult.ValidAuthors.Add(new CourseAuthor
                  {
                     TrainingProviderId = TrainingProviderId,
                     IsAuthorCoAuthor = courseAuthorParseModel.IsAuthorCoAuthor,
                     CourseId = courseAuthorParseModel.Course.Id,
                     AuthorId = resolvedAuthor.ResolvedAuthorId.Value
                  });
               }
               else
               {
                  courseAuthorsParseResult.FullnamesakeAuthors.Add(new CourseAuthor
                  {
                     TrainingProviderId = TrainingProviderId,
                     IsAuthorCoAuthor = courseAuthorParseModel.IsAuthorCoAuthor,
                     CourseId = courseAuthorParseModel.Course.Id,
                     TrainingProviderAuthor = new TrainingProviderAuthor
                     {
                        FullName = courseAuthorParseModel.Author.FullName,
                        SiteUrl = courseAuthorParseModel.Author.SiteUrl,
                        UrlName = courseAuthorParseModel.Author.UrlName
                     }
                  });
               }

            } else if (courseAuthorParseModel.Author.UrlName == null)
            {
               var resolvedAuthor =
                  authorResolves.SingleOrDefault(x => x.CourseId == courseAuthorParseModel.Course.Id &&
                                                      x.ProblemType == ProblemType.AuthorUrlIsNull &&
                                                      x.AuthorFullName == courseAuthorParseModel.Author.FullName);
               if (resolvedAuthor != null)
               {
                  Debug.Assert(resolvedAuthor.ResolvedAuthorId != null, "resolvedAuthor.ResolvedAuthorId != null");

                  courseAuthorsParseResult.ValidAuthors.Add(new CourseAuthor
                  {
                     TrainingProviderId = TrainingProviderId,
                     IsAuthorCoAuthor = courseAuthorParseModel.IsAuthorCoAuthor,
                     CourseId = courseAuthorParseModel.Course.Id,
                     AuthorId = resolvedAuthor.ResolvedAuthorId.Value
                  });
               }
               else
               {
                  courseAuthorsParseResult.NullUrlAuthors.Add(new CourseAuthor
                  {
                     TrainingProviderId = TrainingProviderId,
                     IsAuthorCoAuthor = courseAuthorParseModel.IsAuthorCoAuthor,
                     CourseId = courseAuthorParseModel.Course.Id,
                     TrainingProviderAuthor = new TrainingProviderAuthor
                     {
                        FullName = courseAuthorParseModel.Author.FullName
                     }
                  });
               }

            } else
            {
               if (courseAuthorParseModel.Author.Id == 0)
               {
                  throw new InvalidOperationException(Resources.InvalidOperation_CategoryIdMustBeSetOnTheCourse);
               }

               courseAuthorsParseResult.ValidAuthors.Add(new CourseAuthor
               {
                  TrainingProviderId = TrainingProviderId,
                  IsAuthorCoAuthor = courseAuthorParseModel.IsAuthorCoAuthor,
                  CourseId = courseAuthorParseModel.Course.Id,
                  AuthorId = courseAuthorParseModel.Author.Id
               });
            }
         }

         return courseAuthorsParseResult;
      }

      #endregion
   }
}