using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TM.Data;
using TM.Data.Update;
using TM.Shared;
using TM.Shared.Parse;
using TM.UI.MVC.Infrastructure;
using TM.UI.MVC.Properties;

namespace TM.UI.MVC.Models
{
   public class LearningPlanViewModels
   {
      public class LearningPlanEntryViewModel : CourseCatalogEntryViewModel
      {
         public SubscriptionViewModel Subscription { get; set; }
      }

      public class LearningPlanViewModel
      {
         public LearningPlanEntryViewModel Metadata;
         public IEnumerable<LearningPlanEntryViewModel> Courses { get; set; }
      }

     

      public class SubscriptionViewModel :IValidatableObject
      {
         public int CourseId { get; set; }

         [JsonConverter(typeof(StringEnumConverter))]
         public LearningState State { get; set; }

         [JsonConverter(typeof(CustomJsonNetDateConverter), "yyyy-MM-dd")]
         [Display(Name = "Started On")]
         [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
         public DateTime? StartDate { get; set; }

         public long StartDateOrdinal
         {
            get
            {
               return StartDate.HasValue
                  ? StartDate.Value.Date.Ticks
                  : 0L;
            }
         }


         [JsonConverter(typeof(CustomJsonNetDateConverter), "yyyy-MM-dd")]
         [Display(Name = "Finished On")]
         [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
         public DateTime? FinishDate { get; set; }

         public long FinishDateOrdinal
         {
            get
            {
               return FinishDate.HasValue
                  ? FinishDate.Value.Date.Ticks
                  : 0L;
            }
         }


         [Display(Name = "Own Rating")]
         public byte? OwnRating { get; set; }

         [Display(Name = "Comment")]
         [StringLength(1000)]
         public string Comment { get; set; }

         public bool IsModelValid { get; set; }

         public string ErrorMessage { get; set; }


         public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
         {
            var errors = new List<ValidationResult>();

            IsModelValid = true;
            var tenYearsAgo = DateTime.Today.AddYears(-10);

            if (CourseId <= 0)
            {
               errors.Add(new ValidationResult("Hack attempt detected. Course Id is not valid"));
            }

            if (OwnRating == 0)
            {
               OwnRating = null;
            }

            switch (State)
            {
               case LearningState.Planned:
                  if (StartDate.HasValue || FinishDate.HasValue || OwnRating.HasValue || !string.IsNullOrWhiteSpace(Comment))
                  {
                     errors.Add(new ValidationResult("For 'Planned' state 'Start Date', 'Finish Date', 'Own Rating' and 'Comments' must be empty"));
                  }
                  break;
               case LearningState.InProgress:

                  if (!(tenYearsAgo <= StartDate))
                  {
                     errors.Add(new ValidationResult(string.Format("Invalid 'Start Date'. Start date must be greater than {0:yyyy-MM-dd}", tenYearsAgo)));
                  }
                  if (FinishDate.HasValue || OwnRating.HasValue  || !string.IsNullOrWhiteSpace(Comment))
                  {
                     errors.Add(new ValidationResult("For 'In Progress' state 'Finish Date', 'Own Rating' and 'Comments' must be empty"));
                  }
                  break;
               case LearningState.Finished:

                  if (!((tenYearsAgo <= StartDate) && (StartDate <= FinishDate)))
                  {
                     errors.Add(new ValidationResult(string.Format("Invalid 'Start Date' or 'Finish Date'.\r\n" +
                                                                   "'Start Date' must be less or equal 'Finish Date'.\r\n" +
                                                                   " and greater than {0:yyyy-MM-dd}",
                        tenYearsAgo)));
                  }

                  if (OwnRating < 0 ||  5 < OwnRating)
                  {
                     errors.Add(new ValidationResult("Own Rating must be from 0 to 5"));
                  }

                  break;
               default:
                  errors.Add(new ValidationResult("Hack attempt detected. Invalid course state."));
                  break;
            }

            return errors;
         }
      }

      public class CustomJsonNetDateConverter : IsoDateTimeConverter
      {
         public CustomJsonNetDateConverter(string formatString)
         {
            DateTimeFormat = formatString;
         }
      }


      public struct SubscriptionChange
      {
         public SubscriptionChange(DateTime changeDate)
            : this()
         {
            ChangeDate = changeDate.ToString("yyyy-MM-dd");
            SortOrdinal = changeDate.Date.Ticks;
         }

         public string ChangeDate { get; set; }
         public long SortOrdinal { get; set; }
      }


      public class LearningPlanManager : CatalogManagerBase
      {
         public LearningPlanManager()
         {
         }

         public LearningPlanManager(UpdateDbContext context)
            : base(context)
         {
         }


         public async Task<LearningPlanViewModel> GetLearningPlanAsync(string userId, Specializations? specializations)
         {
            ThrowIfDisposed();

            var userSpecializationList = GetUserSpecializationList(specializations);

            var courses = await CatalogContext.Subscriptions.Where(x => x.UserId == userId &&
                                             (!userSpecializationList.Any() ||
                                              x.Course.CourseSpecializations.Any(
                                                 cs => userSpecializationList.Contains(cs.Specialization))))
               .Select(x => new LearningPlanEntryViewModel
               {
                  TrainingProvider = new TrainingProviderViewModel
                  {
                     Name = x.Course.TrainingProvider.Name,
                     SiteUrl = x.Course.TrainingProvider.SiteUrl,
                     LogoFileName = x.Course.TrainingProvider.LogoFileName
                  },
                  Category = new CategoryViewModel
                  {
                     Title = x.Course.Category.Title,
                     UrlName = x.Course.Category.UrlName,
                     LogoFileName = x.Course.Category.LogoFileName
                  },
                  Course = new CourseViewModel
                  {
                     Id = x.Course.Id,
                     Title = x.Course.Title,
                     SiteUrl = x.Course.SiteUrl,
                     UrlName = x.Course.UrlName,
                     HasClosedCaptions = x.Course.HasClosedCaptions,
                     Level = x.Course.Level,
                     Rating = x.Course.Rating,
                     Duration = x.Course.Duration,
                     ReleaseDate = x.Course.ReleaseDate,
                     LearningState = x.Course.Subscriptions
                        .Where(s => s.UserId == userId)
                        .Select(s => s.State)
                        .FirstOrDefault(),
                     Authors = x.Course.CourseAuthors
                     .Where(a => (!a.IsDeleted))
                     .Select(a => new CourseAuthorViewModel
                     {
                        FullName = a.TrainingProviderAuthor.FullName,
                        UrlName = a.TrainingProviderAuthor.UrlName,
                        IsCoAuthor = a.IsAuthorCoAuthor
                     }).ToList()
                  },
                  Subscription = new SubscriptionViewModel
                  {
                     StartDate = x.StartDate,
                     FinishDate = x.FinishDate
                  }
               }).ToListAsync();

            if (courses.Any())
            {
               return new LearningPlanViewModel
               {
                  Courses = courses
               };
            }

            return null;
         }


         public async Task<CallResult> AddCourseToLearningPlanAsync(string userId, int courseId)
         {
            ThrowIfDisposed();

            if (CatalogContext.Courses.Any(x => x.Id == courseId))
            {
               if (CatalogContext.Subscriptions.Any(x => x.CourseId == courseId && x.UserId == userId))
               {
                  // subscription already exists
                  return CallResult.Success;
               }

               CatalogContext.Subscriptions.Add(new Subscription
               {
                  UserId = userId,
                  CourseId = courseId,
                  AddDate = DateTime.UtcNow,
                  State = LearningState.Planned
               });

               await CatalogContext.SaveChangesAsync();

               return CallResult.Success;
            }

            var message = string.Format(Resources.EntityNotFound_Course, courseId);

            return CallResult.Failed(message);
         }



         public async Task<CallResult<SubscriptionChange>> SetStartStateAsync(int courseId, string userId)
         {
            ThrowIfDisposed();

            var subscription = await CatalogContext.Subscriptions.FindAsync(userId, courseId);
            if (subscription == null)
            {
               var message = string.Format(Resources.EntityNotFound_Subscription, userId, courseId);
               return CallResult<SubscriptionChange>.Failed(message);
            }

            subscription.StartDate = DateTime.UtcNow.Date;
            subscription.State = LearningState.InProgress;

            await CatalogContext.SaveChangesAsync();

            var subscriptionChange = new SubscriptionChange(subscription.StartDate.Value);

            return CallResult<SubscriptionChange>.Success(subscriptionChange);
         }


         public async Task<CallResult<SubscriptionChange>> SetFinishStateAsync(int courseId, string userId)
         {
            ThrowIfDisposed();

            var subscription = await CatalogContext.Subscriptions.FindAsync(userId, courseId);
            if (subscription == null)
            {
               var message = string.Format(Resources.EntityNotFound_Subscription, userId, courseId);
               return CallResult<SubscriptionChange>.Failed(message);
            }

            subscription.FinishDate = DateTime.UtcNow.Date;
            subscription.State = LearningState.Finished;

            await CatalogContext.SaveChangesAsync();

            var subscriptionChange = new SubscriptionChange(subscription.FinishDate.Value);

            return CallResult<SubscriptionChange>.Success(subscriptionChange);
         }


         public async Task<CallResult> DeleteCourseFromLearningPlanAsync(string userId, int courseId)
         {
            ThrowIfDisposed();

            var currentSubscription = await CatalogContext.Subscriptions.FindAsync(userId, courseId);
            if (currentSubscription != null)
            {
               CatalogContext.Subscriptions.Remove(currentSubscription);

               await CatalogContext.SaveChangesAsync();

               return CallResult.Success;
            }

            var message = string.Format(Resources.EntityNotFound_Subscription, userId, courseId);
            return CallResult.Failed(message);
         }


         public async Task<CourseRouteParam> GetCourseRouteParamAsync(int courseId)
         {
            ThrowIfDisposed();

            var courseRouteParam = await CatalogContext.Courses.Where(x => x.Id == courseId)
               .Select(x => new CourseRouteParam
               {
                  TrainingProviderName = x.TrainingProvider.Name,
                  CategoryUrlName = x.Category.UrlName,
                  CourseUrlName = x.UrlName
               }).SingleAsync();

            return courseRouteParam;
         }


         public async Task<CallResult<SubscriptionViewModel>> GetCourseSubscriptionInfoAsync(string userId, int courseId)
         {
            var subscriptionInfo = await CatalogContext.Subscriptions.Where(x => x.UserId == userId && x.CourseId == courseId)
               .Select(x => new SubscriptionViewModel
               {
                  CourseId = x.CourseId,
                  State = x.State,
                  StartDate = x.StartDate,
                  FinishDate = x.FinishDate,
                  Comment = x.Comment,
                  OwnRating = x.OwnRating
               }).SingleOrDefaultAsync();

            if (subscriptionInfo == null)
            {
               var message = string.Format(Resources.EntityNotFound_Subscription, userId, courseId);
               return CallResult<SubscriptionViewModel>.Failed(message);
            }

            return CallResult<SubscriptionViewModel>.Success(subscriptionInfo);
         }


         public async Task<CallResult> UpdateCourseSubscriptionInfoAsync(string userId, SubscriptionViewModel subscriptionInfo)
         {
            var currentSubscriptionInfo = await CatalogContext.Subscriptions
               .Where(x => x.UserId == userId && x.CourseId == subscriptionInfo.CourseId)
               .SingleOrDefaultAsync();

            if (currentSubscriptionInfo == null)
            {
               var message = string.Format(Resources.EntityNotFound_Subscription, userId, subscriptionInfo.CourseId);
               return CallResult.Failed(message);
            }

            currentSubscriptionInfo.State = subscriptionInfo.State;
            currentSubscriptionInfo.StartDate = subscriptionInfo.StartDate;
            currentSubscriptionInfo.FinishDate = subscriptionInfo.FinishDate;
            currentSubscriptionInfo.Comment = subscriptionInfo.Comment;
            currentSubscriptionInfo.OwnRating = subscriptionInfo.OwnRating;

            try
            {
               await CatalogContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
               return CallResult.Failed(ex.Message);
            }

            return CallResult.Success;
         }
      }
   }

}