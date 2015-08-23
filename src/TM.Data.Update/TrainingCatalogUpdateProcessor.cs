using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using TM.Data.Update.Properties;
using TM.Shared.Parse;

namespace TM.Data.Update
{
   public abstract class TrainingCatalogUpdateProcessor<TCategoryParseModel, TCourseParseModel, TAuthorParseModel>
      where TCategoryParseModel : ICategoryParseModel
      where TCourseParseModel : ICourseParseModel
      where TAuthorParseModel : IAuthorParseModel
   {
      protected readonly int TrainingProviderId;
      protected UpdateEvent UpdateEvent;
      protected readonly UpdateDbContext Context;
      protected readonly ICatalogBackupProcessor<TCategoryParseModel, TCourseParseModel, TAuthorParseModel> CatalogBackupProcessor;
      protected readonly bool LogUpdateToDb;

      /// <exception cref="ArgumentNullException">
      /// <paramref name="context"/> or
      /// <paramref name="catalogBackupProcessor"/> is <see langword="null" />.</exception>
      protected TrainingCatalogUpdateProcessor(int trainingProviderId, UpdateDbContext context,
         ICatalogBackupProcessor<TCategoryParseModel, TCourseParseModel, TAuthorParseModel> catalogBackupProcessor,
         bool logUpdateToDb)
      {
         if (context == null)
            throw new ArgumentNullException("context");

         if (catalogBackupProcessor == null)
            throw new ArgumentNullException("catalogBackupProcessor");

         TrainingProviderId = trainingProviderId;
         Context = context;
         CatalogBackupProcessor = catalogBackupProcessor;
         LogUpdateToDb = logUpdateToDb;
      }


      /// <exception cref="ArgumentNullException">
      /// <paramref name="updateEvent"/> or
      /// <paramref name="changesProcessor"/> is <see langword="null" />.</exception>
      /// <exception cref="ChangesProcessorException"></exception>
      /// <exception cref="UpdateProcessorException"></exception>
      public async Task ProcessUpdateAsync(UpdateEvent updateEvent,
         TrainingCatalogChangesProcessor<TCategoryParseModel, TCourseParseModel, TAuthorParseModel> changesProcessor)
      {
         if (updateEvent == null)
            throw new ArgumentNullException("updateEvent");

         if (changesProcessor == null)
            throw new ArgumentNullException("changesProcessor");


         UpdateEvent = updateEvent;

         // order of the calls is important: (categories or authors) -> courses !!!
         var existingCategories = await Context.Categories
            .Where(x => x.TrainingProviderId == TrainingProviderId)
            .AsNoTracking()
            .ToListAsync();

         var categoriesChanges =
            await
               changesProcessor.GetCategoriesChangesAsync(TrainingProviderId, existingCategories,
                  CatalogBackupProcessor.CategoryBackupCreator);


         var existingAuthors = await Context.TrainingProviderAuthors
            .Where(x => x.TrainingProviderId == TrainingProviderId)
            .AsNoTracking()
            .ToListAsync();

         var authorsChanges =
            await
               changesProcessor.GetAuthorsChangesAsync(TrainingProviderId, existingAuthors,
                  CatalogBackupProcessor.AuthorBackupCreator);


         var existingCourses = await Context.Courses
            .Include(x => x.CourseAuthors)
            .Where(x => x.TrainingProviderId == TrainingProviderId)
            .ToListAsync();

         var coursesChanges =
            await
               changesProcessor.GetCoursesChangesAsync(TrainingProviderId, existingCourses,
                  CatalogBackupProcessor.CourseBackupCreator);

         var authorResolves = await Context.AuthorsResolves
            .Where(x => x.ResolveState == ResolveState.Resolved && x.TrainingProviderId == TrainingProviderId)
            .AsNoTracking().ToListAsync();

         try
         {
            TrainingManagerDbConfiguration.SuspendExecutionStrategy = true;

            using (var transaction = Context.Database.BeginTransaction())
            {
               await UpdateCategoriesAsync(categoriesChanges);
               await UpdateAuthorsAsync(authorsChanges, authorResolves);
               await UpdateCoursesAsync(coursesChanges, authorResolves);

               transaction.Commit();
            }
         }
         finally
         {
            TrainingManagerDbConfiguration.SuspendExecutionStrategy = false;
         }
      }
      

      #region Categories Processing

      /// <exception cref="UpdateProcessorException"></exception>
      protected virtual async Task UpdateCategoriesAsync(ChangesResult<Category, TCategoryParseModel> categoriesChanges)
      {
         try
         {
            ProcessModifiedCategories(categoriesChanges.ModifiedEntities);
            ProcessDeletedCategories(categoriesChanges.DeletedEntities);
            await ProcessNewCategories(categoriesChanges.NewEntities);
         }
         catch (Exception ex)
         {
            throw new UpdateProcessorException(Resources.UpdateProcessorException_CategoriesUpdate_Message, ex);
         }
      }


      protected virtual void ProcessModifiedCategories(
         ICollection<EntityPair<Category, TCategoryParseModel>> modifiedCategories)
      {
         try
         {
            Context.Configuration.AutoDetectChangesEnabled = false;

            foreach (var categoryPair in modifiedCategories)
            {
               Context.Entry(categoryPair.DbEntity).State = EntityState.Unchanged;

               var categoryBackup = CatalogBackupProcessor.CategoryBackupCreator.ApplayChanges(UpdateEvent.Id,
                  categoryPair.DbEntity, categoryPair.ParseModelEntity);

               AddUpdates(EntityType.Category, categoryPair.DbEntity.Id, OperationType.Modify, categoryBackup);
            }

            Context.ChangeTracker.DetectChanges();
         }
         finally
         {
            Context.Configuration.AutoDetectChangesEnabled = true;
         }
      }


      protected virtual void ProcessDeletedCategories(ICollection<Category> deletedCategories)
      {
         try
         {
            Context.Configuration.AutoDetectChangesEnabled = false;

            foreach (var category in deletedCategories)
            {
               Context.Entry(category).State = EntityState.Unchanged;
               category.IsDeleted = true;
               AddUpdates(EntityType.Category, category.Id, OperationType.Delete);
            }

            Context.ChangeTracker.DetectChanges();
         }
         finally
         {
            Context.Configuration.AutoDetectChangesEnabled = true;
         }
      }


      [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
      protected virtual async Task ProcessNewCategories(
         ICollection<EntityPair<Category, TCategoryParseModel>> newCategories)
      {
         Context.Categories.AddRange(newCategories.Select(x => x.DbEntity));

         await Context.SaveChangesAsync();

         foreach (var categoryPair in newCategories)
         {
            categoryPair.ParseModelEntity.Id = categoryPair.DbEntity.Id;

            AddUpdates(EntityType.Category, categoryPair.DbEntity.Id, OperationType.Add);
         }

         await Context.SaveChangesAsync();
      }


      #endregion


      #region Authors Processing

      /// <exception cref="UpdateProcessorException"></exception>
      protected virtual async Task UpdateAuthorsAsync(ChangesResult<TrainingProviderAuthor, TAuthorParseModel> authorsChanges, List<AuthorResolve> authorResolves )
      {
         try
         {
            ProcessModifiedAuthors(authorsChanges.ModifiedEntities);
            ProcessDeletedAuthors(authorsChanges.DeletedEntities, authorResolves);

            await ProcessNewAuthors(authorsChanges.NewEntities);
         }
         catch (Exception ex)
         {
            throw new UpdateProcessorException(Resources.UpdateProcessorException_AuthorsUpdate_Message, ex);
         }
      }

      protected virtual void ProcessModifiedAuthors(
         ICollection<EntityPair<TrainingProviderAuthor, TAuthorParseModel>> modifiedAuthors)
      {

         try
         {
            Context.Configuration.AutoDetectChangesEnabled = false;

            foreach (var authorPair in modifiedAuthors)
            {
               Context.Entry(authorPair.DbEntity).State = EntityState.Unchanged;

               var authorBackup = CatalogBackupProcessor.AuthorBackupCreator.ApplayChanges(UpdateEvent.Id,
                  authorPair.DbEntity, authorPair.ParseModelEntity);

               AddUpdates(EntityType.Author, authorPair.DbEntity.AuthorId, OperationType.Modify, authorBackup);
            }

            Context.ChangeTracker.DetectChanges();
         }
         finally
         {
            Context.Configuration.AutoDetectChangesEnabled = true;
         }
      }

      protected virtual void ProcessDeletedAuthors(ICollection<TrainingProviderAuthor> deletedAuthors, List<AuthorResolve> authorResolves)
      {
         try
         {
            Context.Configuration.AutoDetectChangesEnabled = false;

            foreach (var author in deletedAuthors)
            {
               // resolved author. No need to delete.
               if (authorResolves.Any(x => x.AuthorSiteUrl == author.SiteUrl))
               {
                  continue;
               }

               Context.Entry(author).State = EntityState.Unchanged;

               author.IsDeleted = true;
               AddUpdates(EntityType.Author, author.AuthorId, OperationType.Delete);
            }

            Context.ChangeTracker.DetectChanges();
         }
         finally
         {
            Context.Configuration.AutoDetectChangesEnabled = true;
         }
      }

      [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
      protected virtual async Task ProcessNewAuthors(
         ICollection<EntityPair<TrainingProviderAuthor, TAuthorParseModel>> newAuthors)
      {
         var authorsInfoForAnotherTrainingProviders = await Context.Authors
           .Where(x => !x.IsDeleted && x.AuthorTrainingProviders.All(tpa => tpa.TrainingProviderId != TrainingProviderId))
           .Select(x => new
           {
              AuthorId = x.Id,
              x.FirstName,
              x.LastName,
              x.Social
           }).AsNoTracking()
           .ToDictionaryAsync(x => Author.GetFullName(x.FirstName, x.LastName), x => x);

         try
         {
            Context.Configuration.AutoDetectChangesEnabled = false;

            // check if an author has content for another training providers
            foreach (var authorPair in newAuthors)
            {
               if (authorsInfoForAnotherTrainingProviders.ContainsKey(authorPair.ParseModelEntity.FullName))
               {
                  var existingAuthor = authorsInfoForAnotherTrainingProviders[authorPair.ParseModelEntity.FullName];
                  if (existingAuthor.Social.IsPartialEquals(authorPair.DbEntity.Author.Social))
                  {
                     authorPair.DbEntity.Author = null;
                     authorPair.DbEntity.AuthorId = existingAuthor.AuthorId;
                  }
               }

               Context.TrainingProviderAuthors.Add(authorPair.DbEntity);
            }
         }
         finally
         {
            Context.Configuration.AutoDetectChangesEnabled = true;
         }

         await Context.SaveChangesAsync();

         foreach (var authorPair in newAuthors)
         {
            authorPair.ParseModelEntity.Id = authorPair.DbEntity.AuthorId;

            AddUpdates(EntityType.Author, authorPair.DbEntity.AuthorId, OperationType.Add);
         }

         await Context.SaveChangesAsync();
      }

      #endregion


      #region Courses Processing

      /// <exception cref="UpdateProcessorException"></exception>
      protected virtual async Task UpdateCoursesAsync(ChangesResult<Course, TCourseParseModel> coursesChanges, List<AuthorResolve> authorResolves)
      {
         try
         {
            ProcessCategoryAndAuthorsChangesForUnmodifiedCourses(coursesChanges.UnmodifiedEntities, authorResolves);
            ProcessModifiedCourses(coursesChanges.ModifiedEntities, authorResolves);
            ProcessDeletedCourses(coursesChanges.DeletedEntities);
            await ProcessNewCourses(coursesChanges.NewEntities, authorResolves);
         }
         catch (Exception ex)
         {
            throw new UpdateProcessorException(Resources.UpdateProcessorException_CoursesUpdate_Message, ex);
         }
      }


      protected virtual void ProcessCategoryAndAuthorsChangesForUnmodifiedCourses(
         ICollection<EntityPair<Course, TCourseParseModel>> unmodifiedCourses, List<AuthorResolve> authorResolves)
      {
         foreach (var coursePair in unmodifiedCourses)
         {
            CourseBackup courseBackup = null;

            // detect category changes
            if (IsMovedToAnotherCategory(coursePair))
            {
               courseBackup = new CourseBackup
               {
                  UpdateEventId = UpdateEvent.Id,
                  CategoryId = coursePair.DbEntity.CategoryId
               };

               AssignCategoryIdToDbEntity(coursePair);
            }

            // detect authors changes
            var courseAuthorBackups = DetectCourseAuthorChanges(coursePair, authorResolves);

            if (courseAuthorBackups.Any())
            {
               if (courseBackup == null)
               {
                  courseBackup = new CourseBackup
                  {
                     UpdateEventId = UpdateEvent.Id,
                     CourseId = coursePair.DbEntity.Id,
                     CourseAuthorBackups = courseAuthorBackups
                  };
               } else
               {
                  courseBackup.CourseAuthorBackups = courseAuthorBackups;
               }
            }

            // log if any changes
            if (courseBackup != null)
            {
               AddUpdates(EntityType.Course, coursePair.DbEntity.Id, OperationType.Modify, courseBackup);
            }
         }
      }


      protected virtual void ProcessModifiedCourses(
         ICollection<EntityPair<Course, TCourseParseModel>> modifiedCourses, List<AuthorResolve> authorResolves)
      {
         foreach (var coursePair in modifiedCourses)
         {
            var courseBackup = CatalogBackupProcessor.CourseBackupCreator.ApplayChanges(UpdateEvent.Id,
               coursePair.DbEntity, coursePair.ParseModelEntity);

            // detect authors changes
            var courseAuthorBackups = DetectCourseAuthorChanges(coursePair, authorResolves);
            if (courseAuthorBackups.Any())
            {
               if (courseBackup == null)
               {
                  courseBackup = new CourseBackup
                  {
                     UpdateEventId = UpdateEvent.Id,
                     CourseId = coursePair.DbEntity.Id,
                     CourseAuthorBackups = courseAuthorBackups
                  };
               }
               else
               {
                  courseBackup.CourseAuthorBackups = courseAuthorBackups;
               }
            }

            AddUpdates(EntityType.Course, coursePair.DbEntity.Id, OperationType.Modify, courseBackup);
         }
      }


      protected virtual void ProcessDeletedCourses(ICollection<Course> deletedCourses)
      {
         foreach (var course in deletedCourses)
         {
            course.IsDeleted = true;

            foreach (var courseAuthor in course.CourseAuthors.Where(x => !x.IsDeleted))
            {
               courseAuthor.IsDeleted = true;
            }

            AddUpdates(EntityType.Course, course.Id, OperationType.Delete);
         }
      }


      [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
      protected virtual async Task ProcessNewCourses(
         ICollection<EntityPair<Course, TCourseParseModel>> newCourses, List<AuthorResolve> authorResolves)
      {

         try
         {
            Context.Configuration.AutoDetectChangesEnabled = false;
            Context.ChangeTracker.DetectChanges();

            foreach (var coursePair in newCourses)
            {
               AssignCategoryIdToDbEntity(coursePair);
               Context.Courses.Add(coursePair.DbEntity);
            }

            await Context.SaveChangesAsync();

            foreach (var coursePair in newCourses)
            {
               coursePair.ParseModelEntity.Id = coursePair.DbEntity.Id;

               var courseAuthorsParseResult = GetCourseAuthorsParseResult(coursePair.ParseModelEntity, authorResolves);

               AddFullnamesakeAuthorsToResolveList(courseAuthorsParseResult.FullnamesakeAuthors);
               AddNullUrlAuthorsToResolveList(courseAuthorsParseResult.NullUrlAuthors);

               Context.CourseAuthors.AddRange(courseAuthorsParseResult.ValidAuthors);

               AddUpdates(EntityType.Course, coursePair.DbEntity.Id, OperationType.Add);
            }

            Context.ChangeTracker.DetectChanges();

            await Context.SaveChangesAsync();
         }
         finally
         {
            Context.Configuration.AutoDetectChangesEnabled = true;
         }
      }

      protected virtual ICollection<CourseAuthorBackup> DetectCourseAuthorChanges(EntityPair<Course, TCourseParseModel> coursePair, List<AuthorResolve> authorResolves)
      {
         var existingCourseAuthors = coursePair.DbEntity.CourseAuthors;

         var courseAuthorsParseResult = GetCourseAuthorsParseResult(coursePair.ParseModelEntity, authorResolves);

         AddFullnamesakeAuthorsToResolveList(courseAuthorsParseResult.FullnamesakeAuthors);
         AddNullUrlAuthorsToResolveList(courseAuthorsParseResult.NullUrlAuthors);

         var processingCourseAuthorsDictionary = courseAuthorsParseResult.ValidAuthors
            .ToDictionary(x => x, x => x, CourseAuthor.IdentityComparer);

         var courseAuthorsBackup = new List<CourseAuthorBackup>();

         foreach (var existingCourseAuthor in existingCourseAuthors)
         {
            CourseAuthor processingCourseAuthor;
            if (processingCourseAuthorsDictionary.TryGetValue(existingCourseAuthor, out processingCourseAuthor))
            {
               if (existingCourseAuthor.IsAuthorCoAuthor != processingCourseAuthor.IsAuthorCoAuthor)
               {
                  // modify
                  courseAuthorsBackup.Add(new CourseAuthorBackup
                  {
                     UpdateEventId = UpdateEvent.Id,
                     TrainingProviderId = TrainingProviderId,
                     CourseId = existingCourseAuthor.CourseId,
                     AuthorId = existingCourseAuthor.AuthorId,
                     IsAuthorCoAuthor = existingCourseAuthor.IsAuthorCoAuthor,
                     OperationType = OperationType.Modify
                  });
                 
                  existingCourseAuthor.IsAuthorCoAuthor = processingCourseAuthor.IsAuthorCoAuthor;
               }

               // restore author
               existingCourseAuthor.IsDeleted = false;

               processingCourseAuthorsDictionary.Remove(existingCourseAuthor);
            } else
            {
               // remove
               courseAuthorsBackup.Add(new CourseAuthorBackup
               {
                  UpdateEventId = UpdateEvent.Id,
                  TrainingProviderId = TrainingProviderId,
                  CourseId = existingCourseAuthor.CourseId,
                  AuthorId = existingCourseAuthor.AuthorId,
                  OperationType = OperationType.Delete
               });

               existingCourseAuthor.IsDeleted = true;
            }
         }

         foreach (var newCourseAuthor in processingCourseAuthorsDictionary.Values)
         {
            // add
            courseAuthorsBackup.Add(new CourseAuthorBackup
            {
               UpdateEventId = UpdateEvent.Id,
               TrainingProviderId = TrainingProviderId,
               CourseId = newCourseAuthor.CourseId,
               AuthorId = newCourseAuthor.AuthorId,
               OperationType = OperationType.Add
            });

            existingCourseAuthors.Add(newCourseAuthor);
         }


         return courseAuthorsBackup;
      }

      protected virtual void AddFullnamesakeAuthorsToResolveList(IEnumerable<CourseAuthor> courseAuthors)
      {
         // ReSharper disable once LoopCanBePartlyConvertedToQuery
         foreach (var courseAuthor in courseAuthors)
         {
            var authorResolve = new AuthorResolve
            {
               UpdateEventId = UpdateEvent.Id,
               TrainingProviderId = TrainingProviderId,
               CourseId = courseAuthor.CourseId,
               AuthorFullName = courseAuthor.TrainingProviderAuthor.FullName,
               AuthorSiteUrl = courseAuthor.TrainingProviderAuthor.SiteUrl,
               AuthorUrlName = courseAuthor.TrainingProviderAuthor.UrlName,
               IsAuthorCoAuthor = courseAuthor.IsAuthorCoAuthor,
               ResolveState = ResolveState.Pending,
               ProblemType = ProblemType.AuthorIsFullnamesake
            };

            Context.AuthorsResolves.Add(authorResolve);
         }
      }

      protected virtual void AddNullUrlAuthorsToResolveList(IEnumerable<CourseAuthor> courseAuthors)
      {
         // ReSharper disable once LoopCanBePartlyConvertedToQuery
         foreach (var courseAuthor in courseAuthors)
         {
            var authorResolve = new AuthorResolve
            {
               UpdateEventId = UpdateEvent.Id,
               TrainingProviderId = TrainingProviderId,
               CourseId = courseAuthor.CourseId,
               AuthorFullName = courseAuthor.TrainingProviderAuthor.FullName,
               IsAuthorCoAuthor = courseAuthor.IsAuthorCoAuthor,
               ResolveState = ResolveState.Pending,
               ProblemType = ProblemType.AuthorUrlIsNull
            };

            Context.AuthorsResolves.Add(authorResolve);
         }
      }


      #endregion


      #region Log Updated

      /// <exception cref="ArgumentOutOfRangeException">'entityType' or 'operationType' not defined in enumeration</exception>
      protected virtual void AddUpdates(EntityType entityType, int entityId,
         OperationType operationType, object backup = null)
      {
         switch (entityType)
         {
            case EntityType.Category:
               if (LogUpdateToDb)
               {
                  UpdateEvent.CategoriesUpdates.Add(new CategoryUpdate
                  {
                     UpdateEventId = UpdateEvent.Id,
                     CategoryId = entityId,
                     OperationType = operationType,
                     CategoryBackup = (CategoryBackup)backup
                  });
               }
               UpdateStatistic(entityType, operationType);
               break;
            case EntityType.Course:
               if (LogUpdateToDb)
               {
                  UpdateEvent.CoursesUpdates.Add(new CourseUpdate
                  {
                     UpdateEventId = UpdateEvent.Id,
                     CourseId = entityId,
                     OperationType = operationType,
                     CourseBackup = (CourseBackup)backup
                  });
               }

               UpdateStatistic(entityType, operationType);
               break;
            case EntityType.Author:
               if (LogUpdateToDb)
               {
                  UpdateEvent.AuthorsUpdates.Add(new AuthorUpdate
                  {
                     UpdateEventId = UpdateEvent.Id,
                     TrainingProviderId = TrainingProviderId,
                     AuthorId = entityId,
                     OperationType = operationType,
                     AuthorBackup = (AuthorBackup)backup
                  });
               }

               UpdateStatistic(entityType, operationType);
               break;
            default:
               throw new ArgumentOutOfRangeException("entityType");
         }
      }


      /// <exception cref="ArgumentOutOfRangeException">'entityType' or 'operationType' not defined in enumeration</exception>
      protected virtual void UpdateStatistic(EntityType entityType, OperationType operationType)
      {
         Statistic statistic;
         switch (operationType)
         {
            case OperationType.Add:
               switch (entityType)
               {
                  case EntityType.Category:
                     statistic = UpdateEvent.Added;
                     statistic.Categories++;
                     break;
                  case EntityType.Course:
                     statistic = UpdateEvent.Added;
                     statistic.Courses++;
                     break;
                  case EntityType.Author:
                     statistic = UpdateEvent.Added;
                     statistic.Authors++;
                     break;
                  default:
                     throw new ArgumentOutOfRangeException("entityType");
               }
               break;
            case OperationType.Modify:
               switch (entityType)
               {
                  case EntityType.Category:
                     statistic = UpdateEvent.Modified;
                     statistic.Categories++;
                     break;
                  case EntityType.Course:
                     statistic = UpdateEvent.Modified;
                     statistic.Courses++;
                     break;
                  case EntityType.Author:
                     statistic = UpdateEvent.Modified;
                     statistic.Authors++;
                     break;
                  default:
                     throw new ArgumentOutOfRangeException("entityType");
               }
               break;
            case OperationType.Delete:
               switch (entityType)
               {
                  case EntityType.Category:
                     statistic = UpdateEvent.Deleted;
                     statistic.Categories++;
                     break;
                  case EntityType.Course:
                     statistic = UpdateEvent.Deleted;
                     statistic.Courses++;
                     break;
                  case EntityType.Author:
                     statistic = UpdateEvent.Deleted;
                     statistic.Authors++;
                     break;
                  default:
                     throw new ArgumentOutOfRangeException("entityType");
               }
               break;
            default:
               throw new ArgumentOutOfRangeException("operationType");
         }
      }

      #endregion


      #region Abstract Methods

      protected abstract bool IsMovedToAnotherCategory(EntityPair<Course, TCourseParseModel> coursePair);

      protected abstract void AssignCategoryIdToDbEntity(EntityPair<Course, TCourseParseModel> coursePair);

      protected abstract CourseAuthorsParseResult GetCourseAuthorsParseResult(TCourseParseModel processingCourse, List<AuthorResolve> authorResolves);

      #endregion


      #region Nested Types

      protected class CourseAuthorsParseResult
      {
         public CourseAuthorsParseResult()
         {
            FullnamesakeAuthors = new List<CourseAuthor>();
            NullUrlAuthors = new List<CourseAuthor>();
            ValidAuthors = new List<CourseAuthor>();
         }

         public List<CourseAuthor> FullnamesakeAuthors { get; set; }
         public List<CourseAuthor> NullUrlAuthors { get; set; }
         public List<CourseAuthor> ValidAuthors { get; set; }
      }

      #endregion
   }
}