using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TM.Data.Update.Properties;
using TM.Shared;
using TM.Shared.Parse;

namespace TM.Data.Update
{
   public abstract class TrainingCatalogChangesProcessor<TCategoryParseModel, TCourseParseModel, TAuthorParseModel>
      where TCategoryParseModel : ICategoryParseModel
      where TCourseParseModel : ICourseParseModel
      where TAuthorParseModel : IAuthorParseModel
   {
      private readonly IUpdateContentParseResult<TCategoryParseModel, TCourseParseModel, TAuthorParseModel> _updateParseResult;

      public IUpdateContentParseResult<TCategoryParseModel, TCourseParseModel, TAuthorParseModel> UpdateParseResult
      {
         get { return _updateParseResult; }
      }

      /// <exception cref="ArgumentNullException"><paramref name="updateParseResult"/> is <see langword="null" />.</exception>
      protected TrainingCatalogChangesProcessor(IUpdateContentParseResult<TCategoryParseModel, TCourseParseModel, TAuthorParseModel> updateParseResult)
      {
         if (updateParseResult == null)
         {
            throw new ArgumentNullException("updateParseResult");
         }
         _updateParseResult = updateParseResult;
      }


      /// <exception cref="ArgumentNullException">
      /// <paramref name="existingCategories"/> or 
      /// <paramref name="changesDetector"/> is <see langword="null" />.</exception>
      /// <exception cref="ChangesProcessorException"></exception>
      public Task<ChangesResult<Category, TCategoryParseModel>> GetCategoriesChangesAsync(
         int trainingProviderId,
         IEnumerable<Category> existingCategories,
         IChangesDetector<Category, TCategoryParseModel> changesDetector)
      {
         if (existingCategories == null)
            throw new ArgumentNullException("existingCategories");

         if (changesDetector == null)
            throw new ArgumentNullException("changesDetector");

         try
         {
            // ugly copy. Need for generic processing changes
            var processingCategoriesContainer = UpdateParseResult.CategoriesParseResult.CategoryContainer.Values
               .ToDictionary(x => x, x => x, UrlNameNaturalKeyEqualityComparer<IUrlNameNaturalKey>.Instance);

            var task = GetChangesAsync(trainingProviderId, existingCategories, processingCategoriesContainer,
               changesDetector, MapToCategoryAsync, (dbe, pme) => pme.Id = dbe.Id);

            return task;
         }
         catch (Exception ex)
         {
            throw new ChangesProcessorException(Resources.ChangesProcessorException_CategoriesChanges_Message, ex);
         }
      }


      /// <exception cref="ArgumentNullException">
      /// <paramref name="existingCourses"/> or 
      /// <paramref name="changesDetector"/> is <see langword="null" />.</exception>
      /// <exception cref="ChangesProcessorException"></exception>
      public Task<ChangesResult<Course, TCourseParseModel>> GetCoursesChangesAsync(
         int trainingProviderId,
         IEnumerable<Course> existingCourses,
         IChangesDetector<Course, TCourseParseModel> changesDetector)
      {
         if (existingCourses == null)
            throw new ArgumentNullException("existingCourses");

         if (changesDetector == null)
            throw new ArgumentNullException("changesDetector");

         try
         {
            // ugly copy. Need for generic processing changes
            var processingCoursesContainer = UpdateParseResult.CoursesParseResult.CourseContainer.Values
               .ToDictionary(x => x, x => x, UrlNameNaturalKeyEqualityComparer<IUrlNameNaturalKey>.Instance);

            var task = GetChangesAsync(trainingProviderId, existingCourses, processingCoursesContainer, changesDetector,
               MapToCourseAsync, (dbe, pme) => pme.Id = dbe.Id);

            return task;
         }
         catch (Exception ex)
         {
            throw new ChangesProcessorException(Resources.ChangesProcessorException_CoursesChanges_Message, ex);
         }
      }


      /// <exception cref="ArgumentNullException">
      /// <paramref name="existingAuthors"/> or 
      /// <paramref name="changesDetector"/> is <see langword="null" />.</exception>
      /// <exception cref="ChangesProcessorException"></exception>
      public Task<ChangesResult<TrainingProviderAuthor, TAuthorParseModel>> GetAuthorsChangesAsync(
         int trainingProviderId,
         IEnumerable<TrainingProviderAuthor> existingAuthors,
         IChangesDetector<TrainingProviderAuthor, TAuthorParseModel> changesDetector)
      {
         if (existingAuthors == null)
            throw new ArgumentNullException("existingAuthors");

         if (changesDetector == null)
            throw new ArgumentNullException("changesDetector");

         try
         {
            // ugly copy. Need for generic processing changes
            var processingAuthorsContainer = UpdateParseResult.AuthorsParseResult.AuthorsExceptWhoseUrlNullContainer
               .Values
               .ToDictionary(x => x, x => x, UrlNameNaturalKeyEqualityComparer<IUrlNameNaturalKey>.Instance);

            var task = GetChangesAsync(trainingProviderId, existingAuthors, processingAuthorsContainer, changesDetector,
               MapToAuthorAsync, (dbe, pme) => pme.Id = dbe.AuthorId);

            return task;
         }
         catch (Exception ex)
         {
            throw new ChangesProcessorException(Resources.ChangesProcessorException_AuthorsChanges_Message, ex);
         }
      }


      private async Task<ChangesResult<TDbEntity, TParseModelEntity>> GetChangesAsync<TDbEntity, TParseModelEntity>(
         int trainingProviderId,
         IEnumerable<TDbEntity> existingEntities,
         Dictionary<IUrlNameNaturalKey, TParseModelEntity> processingEntitiesContainer,
         IChangesDetector<TDbEntity, TParseModelEntity> changesDetector,
         Func<int, TParseModelEntity, Task<TDbEntity>> entityMapper,
         Action<TDbEntity, TParseModelEntity> keyAssignAction)
         where TParseModelEntity : IUrlNameNaturalKey
         where TDbEntity : IUrlNameNaturalKey
      {
         var unmodifiedEntities = new List<EntityPair<TDbEntity, TParseModelEntity>>();
         var modifiedEntities = new List<EntityPair<TDbEntity, TParseModelEntity>>();
         var deletedEntities = new List<TDbEntity>();

         foreach (var existingEntity in existingEntities)
         {
            TParseModelEntity processingEntity;
            if (processingEntitiesContainer.TryGetValue(existingEntity, out processingEntity))
            {
               // assign identity key of the existing entity to the processing entity
               if (keyAssignAction != null)
               {
                  keyAssignAction.Invoke(existingEntity, processingEntity);
               }

               if (changesDetector.IsDifferent(existingEntity, processingEntity))
               {
                  modifiedEntities.Add(new EntityPair<TDbEntity, TParseModelEntity>(existingEntity, processingEntity));

               } else
               {
                  unmodifiedEntities.Add(new EntityPair<TDbEntity, TParseModelEntity>(existingEntity, processingEntity));
               }

               // need to detect new entities
               processingEntitiesContainer.Remove(existingEntity);
            } else
            {
               deletedEntities.Add(existingEntity);
            }
         }

         var newEntities = new List<EntityPair<TDbEntity, TParseModelEntity>>();
         foreach (var parseModelEntity in processingEntitiesContainer.Values)
         {
            var mappedEntity = await entityMapper.Invoke(trainingProviderId, parseModelEntity);
            newEntities.Add(new EntityPair<TDbEntity, TParseModelEntity>(mappedEntity, parseModelEntity));
         }

         var changesResult = new ChangesResult<TDbEntity, TParseModelEntity>(unmodifiedEntities, modifiedEntities,
            newEntities, deletedEntities);

         return changesResult;
      }


      protected abstract Task<Category> MapToCategoryAsync(int trainingProviderId, TCategoryParseModel processingCategory);
      protected abstract Task<Course> MapToCourseAsync(int trainingProviderId, TCourseParseModel processingCourse);
      protected abstract Task<TrainingProviderAuthor> MapToAuthorAsync(int trainingProviderId, TAuthorParseModel processingAuthor);
   }
}
