using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TM.Data.Update;
using TM.Shared.Parse;

namespace TM.Data.Pluralsight
{
   internal class PluralsightChangesProcessor :
      TrainingCatalogChangesProcessor<PluralsightCategory, PluralsightCourse, PluralsightAuthor>
   {
      private readonly IPluralsightDataService _dataService;


      #region Constructors

      /// <exception cref="ArgumentNullException">
      /// <paramref name="updateParseResult" /> or
      /// <paramref name="dataService" /> is <see langword="null" />.</exception>
      public PluralsightChangesProcessor(
         IUpdateContentParseResult<PluralsightCategory, PluralsightCourse, PluralsightAuthor> updateParseResult,
         IPluralsightDataService dataService)
         : base(updateParseResult)
      {
         if (dataService == null)
            throw new ArgumentNullException("dataService");

         _dataService = dataService;
      }

      #endregion


      #region TrainingCatalogChangesProcessor Overrides

      protected override Task<Category> MapToCategoryAsync(int trainingProviderId, PluralsightCategory processingCategory)
      {
         var category = new Category
         {
            TrainingProviderId = trainingProviderId,
            Title = processingCategory.Title,
            UrlName = processingCategory.UrlName,
            LogoFileName = processingCategory.LogoFileName,
            LogoUrl = processingCategory.LogoUrl
         };

         return Task.FromResult(category);
      }

      protected override async Task<Course> MapToCourseAsync(int trainingProviderId, PluralsightCourse processingCourse)
      {
         var specializations = await _dataService.GetCourseSpecializationsAsync(processingCourse.UrlName);

         var shortDescription = await _dataService.GetCourseShortDescriptionAsync(processingCourse.UrlName);
         var modules = await _dataService.GetCourseToCAsync(processingCourse.UrlName);

         var course = new Course
         {
            TrainingProviderId = trainingProviderId,
            Title = processingCourse.Title,
            SiteUrl = processingCourse.SiteUrl,
            UrlName = processingCourse.UrlName,
            Description = processingCourse.Description,
            ShortDescription = shortDescription,
            HasClosedCaptions = processingCourse.HasClosedCaptions,
            Level = processingCourse.Level,
            Rating = processingCourse.Rating,
            Duration = processingCourse.Duration,
            ReleaseDate = processingCourse.ReleaseDate,

            Specializations = specializations,

            Modules = modules
         };

         return course;
      }

      protected override async Task<TrainingProviderAuthor> MapToAuthorAsync(int trainingProviderId, PluralsightAuthor processingAuthor)
      {
         var author = await _dataService.GetAuthorAsync(processingAuthor.UrlName);

         var trainingProviderAuthor = new TrainingProviderAuthor
         {
            TrainingProviderId = trainingProviderId,
            FullName = processingAuthor.FullName,
            SiteUrl = processingAuthor.SiteUrl,
            UrlName = processingAuthor.UrlName,
            Author = author
         };

         author.AuthorTrainingProviders = new List<TrainingProviderAuthor>
         {
            trainingProviderAuthor
         };

         return trainingProviderAuthor;
      }

      #endregion
   }
}