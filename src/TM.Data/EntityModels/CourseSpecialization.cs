using System.Diagnostics.CodeAnalysis;
using TM.Shared;

namespace TM.Data
{
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   public class CourseSpecialization
   {
      public int CourseId { get; set; }
      public Specializations Specialization { get; set; }

      public static CourseSpecialization Create(Course course, Specializations specialization)
      {
         return new CourseSpecialization
         {
            CourseId = course.Id,
            Course = course,
            Specialization = specialization
         };
      }

      public virtual Course Course { get; set; }
   }
}
