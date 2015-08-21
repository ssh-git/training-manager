using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.SqlServer;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TM.UI.MVC.ControllableModel;

namespace TM.UI.MVC.Areas.Admin.ViewModels
{
   [SuppressMessage("ReSharper", "LocalizableElement")]
   [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
   [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
   public class ApplicationUserViewModels
   {
      #region Index Model

      public class IndexViewModel
      {
         [Required(AllowEmptyStrings = false)]
         [DisplayName("Nickname")]
         public string UserName { get; set; }

         [DisplayName("Registration Date")]
         [DataType(DataType.Date)]
         [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
         public DateTime RegisteredOnUtc { get; set; }

         [DisplayName("Last Login Date")]
         [DataType(DataType.DateTime)]
         [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:mm:ss}")]
         public DateTime LastLoginOnUtc { get; set; }

         [DisplayName("Lockout")]
         [UIHint("EnabledDisabled")]
         public bool LockoutEnabled { get; set; }

         [DisplayName("Locked")]
         [UIHint("YesNo")]
         public bool IsLocked
         {
            get
            {
               var utcNow = DateTime.UtcNow;
               if (LockoutEndDateUtc.HasValue && utcNow < LockoutEndDateUtc.Value)
               {
                  return true;
               }

               return false;
            }
         }

         [DisplayName("Unlock Date")]
         [DataType(DataType.DateTime)]
         [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:mm:ss}")]
         public DateTime? LockoutEndDateUtc { get; set; }

         [DisplayName("Failed Logins")]
         public int AccessFailedCount { get; set; }

         public bool IsAdmin { get; set; }


         public static async Task<IndexControllableViewModel> ToControlableViewModelAsync(
            IQueryable<IndexViewModel> query, ControllableViewModelParams controlParams)
         {
            var controllableViewModel = new IndexControllableViewModel(controlParams);

            await controllableViewModel.InitializeAsync(query);

            return controllableViewModel;
         }
      }

      public class UserSortStateModel
      {
         public enum State
         {
            None = 0,
            NicknameDesc,

            RegDate,
            RegDateDesc,

            LastLoginDate,
            LastLoginDateDesc
         }

         public static UserSortStateModel Create(string sortKey)
         {
            State sortState;
            Enum.TryParse(sortKey, true, out sortState);

            return new UserSortStateModel
            {
               NicknameSortState = sortState == State.None ? State.NicknameDesc : State.None,
               RegistrationDateSortState = sortState == State.RegDate ? State.RegDateDesc : State.RegDate,
               LastLoginDateSortState = sortState == State.LastLoginDate ? State.LastLoginDateDesc : State.LastLoginDate
            };
         }


         public State NicknameSortState { get; private set; }
         public State RegistrationDateSortState { get; private set; }
         public State LastLoginDateSortState { get; private set; }
      }

      public class IndexControllableViewModel : ControllableViewModel<IndexViewModel, UserSortStateModel>
      {
         public IndexControllableViewModel(IControllableViewModelParams viewModelParams, string defaultPageSize = "10")
            : base(viewModelParams, defaultPageSize)
         {
            SearchBoxTextPlaceholder = "Search by Nickname";
         }

         public IndexControllableViewModel(IControllableViewModelParams viewModelParams, string defaultPageSize,
            params string[] sizeList)
            : base(viewModelParams, defaultPageSize, sizeList)
         {
            SearchBoxTextPlaceholder = "Search by Nickname";
         }

         protected override IQueryable<IndexViewModel> ApplyFiltering(
            IQueryable<IndexViewModel> chainQuery, IFilterParam filterParam)
         {
            UserFilter filterKey;
            Enum.TryParse(filterParam.FilterKey, true, out filterKey);

            switch (filterKey)
            {
               case UserFilter.None:
                  break;
               case UserFilter.InAdministratorRole:
                  chainQuery = chainQuery.Where(x => x.IsAdmin);
                  break;
               case UserFilter.LockoutEnabled:
                  chainQuery = chainQuery.Where(x => x.LockoutEnabled);
                  break;
               case UserFilter.LockoutDisabled:
                  chainQuery = chainQuery.Where(x => !x.LockoutEnabled);
                  break;
               case UserFilter.LockedUsers:
                  chainQuery =
                     chainQuery.Where(
                        x =>
                           x.LockoutEndDateUtc.HasValue &&
                           SqlFunctions.DateDiff("second", SqlFunctions.GetUtcDate(), x.LockoutEndDateUtc) > 0);
                  break;
               default:
                  throw new ArgumentOutOfRangeException();
            }

            return chainQuery;
         }


         protected override IQueryable<IndexViewModel> ApplySearching(
            IQueryable<IndexViewModel> chainQuery, ISearchParam searchParam)
         {
            if (!string.IsNullOrWhiteSpace(searchParam.SearchKey))
            {
               chainQuery = chainQuery.Where(x => x.UserName.Contains(searchParam.SearchKey));
            }
            return chainQuery;
         }


         protected override IQueryable<IndexViewModel> ApplySorting(
            IQueryable<IndexViewModel> chainQuery, ISortParam sortParam)
         {
            UserSortStateModel.State sortState;
            Enum.TryParse(sortParam.SortKey, true, out sortState);

            switch (sortState)
            {
               case UserSortStateModel.State.NicknameDesc:
                  chainQuery = chainQuery.OrderByDescending(x => x.UserName);
                  break;
               case UserSortStateModel.State.RegDate:
                  chainQuery = chainQuery.OrderBy(x => x.RegisteredOnUtc);
                  break;
               case UserSortStateModel.State.RegDateDesc:
                  chainQuery = chainQuery.OrderByDescending(x => x.RegisteredOnUtc);
                  break;
               case UserSortStateModel.State.LastLoginDate:
                  chainQuery = chainQuery.OrderBy(x => x.LastLoginOnUtc);
                  break;
               case UserSortStateModel.State.LastLoginDateDesc:
                  chainQuery = chainQuery.OrderByDescending(x => x.LastLoginOnUtc);
                  break;
               default:
                  chainQuery = chainQuery.OrderBy(x => x.UserName);
                  break;
            }

            return chainQuery;
         }

         protected override UserSortStateModel CreateSortStateModel(IControllableViewModelParams controlParams)
         {
            return UserSortStateModel.Create(controlParams.SortKey);
         }

         protected override Task InitializeFilterSelectList(IQueryable<IndexViewModel> chainQuery,
            IFilterParam filterParam)
         {
            UserFilter filterKey;
            Enum.TryParse(filterParam.FilterKey, true, out filterKey);

            FilterSelectList = new[]
            {
               new SelectListItem
               {
                  Value = UserFilter.None.ToString(),
                  Text = "Filter not applyed",
                  Selected = filterKey == UserFilter.None
               },
               new SelectListItem
               {
                  Value = UserFilter.InAdministratorRole.ToString(),
                  Text = "Display Administrators",
                  Selected = filterKey == UserFilter.InAdministratorRole
               },
               new SelectListItem
               {
                  Value = UserFilter.LockoutEnabled.ToString(),
                  Text = "Display Users With Lockout Enabled",
                  Selected = filterKey == UserFilter.LockoutEnabled
               },
               new SelectListItem
               {
                  Value = UserFilter.LockoutDisabled.ToString(),
                  Text = "Display Users With Lockout Disabled",
                  Selected = filterKey == UserFilter.LockoutDisabled
               },
               new SelectListItem
               {
                  Value = UserFilter.LockedUsers.ToString(),
                  Text = "Display Locked Users",
                  Selected = filterKey == UserFilter.LockedUsers
               }
            };

            return Task.FromResult(true);
         }


         private enum UserFilter
         {
            None = 0,
            InAdministratorRole,
            LockoutEnabled,
            LockoutDisabled,
            LockedUsers
         }
      }

      #endregion


      #region Edit Model

      public class EditViewModel
      {
         public EditViewModel()
         {
            RoleList = new List<SelectListItem>();
            SelectedRoles = new List<string>();
         }

         [Required]
         public string UserId { get; set; }

         [Required]
         [DisplayName("Nickname")]
         public string UserName { get; set; }

         [Required]
         [DisplayName("Lockout")]
         public bool LockoutEnabled { get; set; }

         [Display(Name = "Is Locked")]
         [UIHint("YesNo")]
         public bool IsLocked
         {
            get
            {
               var utcNow = DateTime.UtcNow;
               if (LockoutEndDateUtc.HasValue && utcNow < LockoutEndDateUtc.Value)
               {
                  return true;
               }

               return false;
            }
         }

         [DisplayFormat(DataFormatString = "Automatic unlock will be on {0:R}")]
         public DateTime? LockoutEndDateUtc { get; set; }

         [DisplayName("Select Roles")]
         public List<string> SelectedRoles { get; set; }

         public IEnumerable<SelectListItem> LockoutSelectList
         {
            get
            {
               yield return new SelectListItem {Text = "Enabled", Value = bool.TrueString, Selected = LockoutEnabled};

               yield return new SelectListItem {Text = "Disabled", Value = bool.FalseString, Selected = LockoutEnabled};
            }
         }

         public List<SelectListItem> RoleList { get; set; }

      }

      #endregion
   }
}