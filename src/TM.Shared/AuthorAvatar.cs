using System.ComponentModel.DataAnnotations;

namespace TM.Shared
{
   public class AuthorAvatar
   {
      [StringLength(400)]
      public string SiteUrl { get; set; }

      [StringLength(100)]
      public string Name { get; set; }
   }
}