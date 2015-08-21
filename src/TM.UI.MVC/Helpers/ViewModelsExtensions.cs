using System.Web;
using TM.Shared;

namespace TM.UI.MVC.Helpers
{
   public static class ViewModelsExtensions
   {
      public static string GetImageLocalUrl(this AuthorAvatar avatar)
      {
         if (string.IsNullOrWhiteSpace(avatar.Name))
         {
            return AppConstants.VirtualPaths.ImagePlaceholder.Substring(1);
         }

         var url = VirtualPathUtility.Combine(AppConstants.VirtualPaths.AuthorsContent, avatar.Name).Substring(1);
         return url;
      }

      public static string GetBadgeLocalUrl(this AuthorBadge badge)
      {
         var url = VirtualPathUtility.Combine(AppConstants.VirtualPaths.BadgesContent, badge.ImageName).Substring(1);
         return url;
      }
   }
}