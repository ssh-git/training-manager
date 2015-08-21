using System.ComponentModel.DataAnnotations;

namespace TM.Shared
{
   public class Social
   {
      [StringLength(400)]
      public string FacebookLink { get; set; }

      [StringLength(400)]
      public string LinkedInLink { get; set; }

      [StringLength(400)]
      public string RssLink { get; set; }

      [StringLength(400)]
      public string TwitterLink { get; set; }

      public bool IsEmpty
      {
         get { return string.IsNullOrWhiteSpace(FacebookLink) &&
                      string.IsNullOrWhiteSpace(LinkedInLink) && 
                      string.IsNullOrWhiteSpace(RssLink) && 
                      string.IsNullOrWhiteSpace(TwitterLink); }
      }

      public bool IsPartialEquals(Social other)
      {
         
         if (!IsEmpty && !other.IsEmpty)
         {
            return FacebookLink == other.FacebookLink ||
                   LinkedInLink == other.LinkedInLink ||
                   RssLink == other.RssLink ||
                   TwitterLink == other.TwitterLink;
         }

         return false;
      }
   }
}