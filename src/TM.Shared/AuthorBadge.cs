using System.ComponentModel.DataAnnotations;

namespace TM.Shared
{
   public class AuthorBadge
   {
      [StringLength(400)]
      public string ImageSiteUrl { get; set; }

      [StringLength(100)]
      public string ImageName { get; set; }

      [StringLength(400)]
      public string Link { get; set; }

      [StringLength(100)]
      public string HoverText { get; set; }

      public bool IsEmpty
      {
         get { return string.IsNullOrWhiteSpace(Link); }
      }
   }
}