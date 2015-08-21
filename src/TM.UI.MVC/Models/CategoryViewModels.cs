using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TM.Data.Update;
using TM.Shared;
using TM.UI.MVC.Helpers;
using TM.UI.MVC.Infrastructure;

namespace TM.UI.MVC.Models
{
   public class CategoryViewModel
   {
      [Display(Name = "Category")]
      public string Title { get; set; }

      public string UrlName { get; set; }
      public string LogoFileName { get; set; }

      public TrainingProviderViewModel TrainingProvider { get; set; }

      public string LogoUrl
      {
         get
         {
            if (string.IsNullOrWhiteSpace(LogoFileName))
            {
               return AppConstants.VirtualPaths.ImagePlaceholder.Substring(1);
            }

            var relativePath = Path.Combine(TrainingProvider.Name.ToLowerInvariant(), LogoFileName);

            var url = VirtualPathUtility.Combine(AppConstants.VirtualPaths.CategoryContent, relativePath).Substring(1);
            return url;
         }
      }

      public object CategoryRouteValuesObject
      {
         get
         {
            return new
            {
               trainingProviderName = TrainingProvider.Name,
               categoryUrlName = UrlName
            };
         }
      }
   }


   [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
   public class CategoryViewModels
   {
      public class CatalogEntryViewModel : CategoryViewModel
      {
         public int CourseCount { get; set; }

         [DataType(DataType.Date)]
         [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
         public DateTime LastCourseDate { get; set; }

      }

      public class CatalogViewModel
      {
         public NavigationViewModel Navigation { get; set; }
         public IEnumerable<CatalogEntryViewModel> Categories { get; set; }
      }



      public class CategoriesManager : CatalogManagerBase
      {
         public CategoriesManager()
         {
         }

         public CategoriesManager(UpdateDbContext context)
            : base(context)
         {
         }

         public async Task<CatalogViewModel> GetCategoriesCatalogAsync(Specializations? specializations, string trainingProviderName)
         {
            var coursesQuery = GetCoursesQueryForSpecializations(specializations);

            var categoriesQuery = string.IsNullOrWhiteSpace(trainingProviderName)
               ? CatalogContext.Categories
                  .Where(x => coursesQuery.Any(q => q.CategoryId == x.Id) && !x.IsDeleted)
               : CatalogContext.Categories
                  .Where(x => x.TrainingProvider.Name == trainingProviderName &&
                              coursesQuery.Any(q => q.CategoryId == x.Id) && !x.IsDeleted);


            var catalog = await categoriesQuery
               .Select(x => new CatalogEntryViewModel
               {
                  TrainingProvider = new TrainingProviderViewModel
                  {
                     Name = x.TrainingProvider.Name,
                     LogoFileName = x.TrainingProvider.LogoFileName,
                     SiteUrl = x.TrainingProvider.SiteUrl
                  },

                  Title = x.Title,
                  UrlName = x.UrlName,
                  LogoFileName = x.LogoFileName,
                  CourseCount = coursesQuery.Count(q => q.CategoryId == x.Id),

                  LastCourseDate = coursesQuery
                     .Where(q => q.CategoryId == x.Id)
                     .OrderByDescending(d => d.ReleaseDate)
                     .FirstOrDefault().ReleaseDate
               })
               .OrderBy(x => x.Title)
               .ToListAsync();

            if (!catalog.Any()) return null;

            var tokenCatalog = GetTokenCatalog();

            var categoryCatalogViewModel = new CatalogViewModel
            {
               Navigation = new NavigationViewModel
               {
                  SelectedToken = string.IsNullOrWhiteSpace(trainingProviderName)
                     ? NavigationViewModel.ALLToken
                     : trainingProviderName.ToTitleCaseInvariant(),
                  TokenCatalog = tokenCatalog
               },

               Categories = catalog
            };

            return categoryCatalogViewModel;
         }

         public async Task<CategoryViewModel> GetCategoryInfoAsync(string trainingProviderName, string categoryUrlName)
         {
            var category = await CatalogContext.Categories
               .Where(x => x.UrlName == categoryUrlName && x.TrainingProvider.Name == trainingProviderName && !x.IsDeleted)
               .Select(x => new CategoryViewModel
               {
                  TrainingProvider = new TrainingProviderViewModel
                  {
                     Name = x.TrainingProvider.Name,
                     LogoFileName = x.TrainingProvider.LogoFileName,
                     SiteUrl = x.TrainingProvider.SiteUrl
                  },

                  Title = x.Title,
                  UrlName = x.UrlName,
                  LogoFileName = x.LogoFileName
               }).SingleOrDefaultAsync();

            return category;
         }
      }
   }
}