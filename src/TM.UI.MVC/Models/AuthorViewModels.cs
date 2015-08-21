using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TM.Data;
using TM.Data.Update;
using TM.Shared;
using TM.UI.MVC.Helpers;
using TM.UI.MVC.Infrastructure;

namespace TM.UI.MVC.Models
{
   public class AuthorViewModel
   {
      public string FullName { get { return string.Concat(FirstName, " ", LastName); } }
      public string FirstName { get; set; }
      public string LastName { get; set; }

      public string UrlName { get; set; }
      public string SiteUrl { get; set; }

      public string Bio { get; set; }

      [UIHint("Social")]
      public Social Social { get; set; }

      [UIHint("AuthorBadge")]
      public AuthorBadge Badge { get; set; }

      [UIHint("AuthorAvatar")]
      public AuthorAvatar Avatar { get; set; }
      
   }


   public class TrainingProviderAuthorViewModel
   {
      public string FullName { get; set; }

      public string FirstName
      {
         get
         {
            var firstName = Author.GetFirstName(FullName);
            return firstName;
         }
      }

      public string LastName
      {
         get
         {
            var lastName = Author.GetLastName(FullName);
            return lastName;
         }
      }

      public string UrlName { get; set; }
   }

   public class CourseAuthorViewModel
   {
      public string FullName { get; set; }
      public string UrlName { get; set; }
      public bool IsCoAuthor { get; set; }
   }



   public class AuthorViewModels
   {

      public class AuthorInfoViewModel : AuthorViewModel
      {
         public TrainingProviderViewModel TrainingProvider { get; set; }

         public IEnumerable<ParticipationViewModel> Participations { get; set; }

         public object TrainingProviderAuthorRouteObject
         {
            get
            {
               return new
               {
                  trainingProviderName = TrainingProvider.Name,
                  authorUrlName = UrlName
               };
            }
         }
      }

      public class ParticipationViewModel
      {
         public TrainingProviderViewModel TrainingProvider { get; set; }
         public string UrlName { get; set; }

         public object TrainingProviderAuthorRouteObject
         {
            get
            {
            
                return new
                {
                   trainingProviderName=TrainingProvider.Name, 
                   authorUrlName=UrlName
                }; 
            }
         }
      }


      public class CatalogEntryViewModel
      {
         public TrainingProviderViewModel TrainingProvider { get; set; }
         public TrainingProviderAuthorViewModel Author { get; set; }

         public object TrainingProviderAuthorRouteObject
         {
            get { return new {trainingProviderName=TrainingProvider.Name, authorUrlName=Author.UrlName}; }
         }
      }

      public class AuthorCatalogViewModel
      {
         public NavigationViewModel Navigation { get; set; }
         public IEnumerable<IGrouping<char, CatalogEntryViewModel>> Catalog { get; set; }
      }



      public class AuthorsManager : CatalogManagerBase
      {
         public AuthorsManager()
         {
         }

         public AuthorsManager(UpdateDbContext context)
            : base(context)
         {
         }
         

         public async Task<AuthorCatalogViewModel> GetAuthorsCatalogAsync( string trainingProviderName, Specializations? specializations)
         {
            var coursesQuery = GetCoursesQueryForSpecializations(specializations);

            var authorsQuery = trainingProviderName != null
               ? CatalogContext.TrainingProviderAuthors
                  .Where(x => x.TrainingProvider.Name == trainingProviderName &&
                              x.AuthorCourses.Any(ca => coursesQuery.Any(q => q.Id == ca.CourseId)) && !x.IsDeleted)
               : CatalogContext.TrainingProviderAuthors
                  .Where(x => x.AuthorCourses.Any(ca => coursesQuery.Any(q => q.Id == ca.CourseId)) && !x.IsDeleted);

            var catalogModel = await authorsQuery
              .Select(x => new CatalogEntryViewModel
              {
                 TrainingProvider = new TrainingProviderViewModel
                 {
                    Name = x.TrainingProvider.Name,
                    LogoFileName = x.TrainingProvider.LogoFileName,
                    SiteUrl = x.TrainingProvider.SiteUrl
                 },
                 Author = new TrainingProviderAuthorViewModel
                 {
                    FullName = x.FullName,
                    UrlName = x.UrlName
                 }
              }).ToListAsync();

            if (!catalogModel.Any())
            {
               return null;
            }

            var catalog = catalogModel
               .GroupBy(x => char.ToUpperInvariant(x.Author.LastName[0]), x => x)
               .OrderBy(group => group.Key)
               .AsEnumerable();

            var tokenCatalog = GetTokenCatalog();

            var authorCatalogViewModel = new AuthorCatalogViewModel
            {
               Navigation = new NavigationViewModel
               {
                  SelectedToken = trainingProviderName != null
                     ? trainingProviderName.ToTitleCaseInvariant()
                     : NavigationViewModel.ALLToken,
                  TokenCatalog = tokenCatalog
               },
               
               Catalog = catalog
            };

            return authorCatalogViewModel;
         }


         public async Task<AuthorInfoViewModel> GetAuthorInfoAsync(string authorUrlName, string trainingProviderName, Specializations? specializations)
         {
            var authorIdentity = await CatalogContext.TrainingProviderAuthors
               .Where(x => x.UrlName == authorUrlName &&
                           x.TrainingProvider.Name == trainingProviderName &&
                           !x.IsDeleted)
               .Select(x => new { x.AuthorId, x.TrainingProviderId })
               .SingleOrDefaultAsync();

            if (authorIdentity == null)
            {
               return null;
            }

            var coursesQuery = GetCoursesQueryForSpecializations(specializations);

            var authorInfoViewModel = await CatalogContext.TrainingProviderAuthors
               .Where(x => x.AuthorId == authorIdentity.AuthorId &&
                           x.TrainingProviderId == authorIdentity.TrainingProviderId &&
                           !x.IsDeleted)
               .Select(x => new AuthorInfoViewModel
               {
                  TrainingProvider = new TrainingProviderViewModel
                  {
                     Name = x.TrainingProvider.Name,
                     SiteUrl = x.TrainingProvider.SiteUrl,
                     LogoFileName = x.TrainingProvider.LogoFileName
                  },

                  FirstName = x.Author.FirstName,
                  LastName = x.Author.LastName,
                  Bio = x.Author.Bio,
                  Social = x.Author.Social,
                  Badge = x.Author.Badge,
                  Avatar = x.Author.Avatar,

                  SiteUrl = x.SiteUrl,
                  UrlName = x.UrlName,

                  Participations = x.Author.AuthorTrainingProviders
                     .Where(tpa => tpa.TrainingProviderId != authorIdentity.TrainingProviderId &&
                                   coursesQuery.Any(q => q.TrainingProviderId == tpa.TrainingProviderId))
                     .Select(tpa => new ParticipationViewModel
                     {
                        TrainingProvider = new TrainingProviderViewModel
                        {
                           Name = tpa.TrainingProvider.Name,
                           SiteUrl = tpa.TrainingProvider.SiteUrl,
                           LogoFileName = tpa.TrainingProvider.LogoFileName
                        },
                        UrlName = tpa.UrlName
                     })
               }).SingleOrDefaultAsync();
            

            return authorInfoViewModel;
         }
      }
   }
}