using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TM.Data.Update;
using TM.Shared;
using TM.UI.MVC.Infrastructure;

namespace TM.UI.MVC.Models
{
   public class TrainingProviderViewModel
   {
      public string Name { get; set; }
      public string SiteUrl { get; set; }
      public string LogoFileName { get; set; }

      public object TrainingProviderRouteValuesObject
      {
         get { return new { trainingProviderName = Name }; }
      }

      public string LogoUrl
      {
         get { return VirtualPathUtility.Combine(AppConstants.VirtualPaths.TrainingProvidersContent, LogoFileName).Substring(1); }
      }
   }

   [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
   public class TrainingProviderViewModels
   {

      public class CatalogEntryViewModel : TrainingProviderViewModel
      {
         public int CategoryCount { get; set; }
         public int CourseCount { get; set; }
         public int AuthorsCount { get; set; }
      }


      public class InfoViewModel : CatalogEntryViewModel
      {
         public List<DateTime> UpdateDates { get; set; }

         [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
         public DateTime SelectedUpdateDate { get; set; }
         public string Description { get; set; }

         public object TrainingProviderRouteValueObject
         {
            get
            {
               return new
               {
                  trainingProviderName = Name
               };
            }
         }

         public object CoursesForDateRouteValueObject
         {
            get
            {
               return new
               {
                  trainingProviderName = Name,
                  addDate = SelectedUpdateDate.ToString("yyyy-MM-dd")
               };
            }
         }
      }




      public class TrainingProvidersManager : CatalogManagerBase
      {
         public TrainingProvidersManager()
         {
         }

         public TrainingProvidersManager(UpdateDbContext context)
            : base(context)
         {
         }

         public async Task<List<CatalogEntryViewModel>> GetTrainingProviderCatalogAsync(Specializations? specializations)
         {
            ThrowIfDisposed();

            var coursesQuery = GetCoursesQueryForSpecializations(specializations);

            var catalog = await CatalogContext.TrainingProviders
               .Where(x => !x.IsDeleted)
               .Select(x => new CatalogEntryViewModel
               {
                  Name = x.Name,
                  SiteUrl = x.SiteUrl,
                  LogoFileName = x.LogoFileName,
                  CategoryCount = x.Categories
                     .Count(category => coursesQuery.Any(q => q.CategoryId == category.Id) && !category.IsDeleted),
                  CourseCount = x.Courses
                     .Count(course => coursesQuery.Any(q => q.Id == course.Id)),
                  AuthorsCount = CatalogContext.CourseAuthors
                     .Where(ca => x.Id == ca.TrainingProviderId && !ca.IsDeleted && coursesQuery.Any(q => q.Id == ca.CourseId))
                     .GroupBy(ca => ca.AuthorId).Count()
               }).ToListAsync();

            return catalog;
         }



         public async Task<InfoViewModel> GetTrainingProviderInfoAsync(string trainingProviderName, Specializations? specializations)
         {
            var coursesQuery = GetCoursesQueryForSpecializations(specializations);

            var trainingProviderInfo = await CatalogContext.TrainingProviders
               .Where(tp => tp.Name == trainingProviderName && !tp.IsDeleted)
               .Select(tp => new InfoViewModel
               {
                  Name = tp.Name,
                  Description = tp.Description,
                  SiteUrl = tp.SiteUrl,
                  LogoFileName = tp.LogoFileName,

                  CategoryCount = tp.Categories
                     .Count(cat => coursesQuery.Any(cq => cq.CategoryId == cat.Id) && !cat.IsDeleted),
                  CourseCount = tp.Courses
                     .Count(course => coursesQuery.Any(cq => cq.Id == course.Id)),
                  AuthorsCount = UpdateContext.CourseAuthors
                     .Where(
                        ca =>
                           tp.Id == ca.TrainingProviderId && !ca.IsDeleted &&
                           coursesQuery.Any(cq => cq.Id == ca.CourseId))
                     .GroupBy(ca => ca.AuthorId).Count(),

                  UpdateDates = UpdateContext.UpdateEvents
                     .Where(ue => ue.TrainingProviderId == tp.Id &&
                                  (ue.UpdateResult == UpdateResult.Success ||
                                   ue.UpdateResult == UpdateResult.Resolved) &&
                                  ue.CoursesUpdates.Any(
                                     c =>
                                        c.OperationType == OperationType.Add &&
                                        coursesQuery.Any(cq => cq.Id == c.CourseId)))

                     .Select(ue => DbFunctions.TruncateTime(ue.StartedOn).Value)
                     .Distinct()
                     .OrderBy(ue => ue)
                     .ToList()
               }).SingleOrDefaultAsync();

            return trainingProviderInfo;
         }
      }
   }
}