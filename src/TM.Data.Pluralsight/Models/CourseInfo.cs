using System.Diagnostics;

namespace TM.Data.Pluralsight
{
   [DebuggerDisplay("Name = {Name}; Url = {SiteUrl}; Description = {Description}")]
   internal class CourseInfo
   {
      public string Name { get; set; }
      public string Description { get; set; }
      public string SiteUrl { get; set; }
      public string UrlName { get; set; }
   }
}