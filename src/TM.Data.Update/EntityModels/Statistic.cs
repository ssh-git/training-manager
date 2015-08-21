namespace TM.Data.Update
{
   public class Statistic
   {
      public int Categories { get; set; }
      public int Courses { get; set; }
      public int Authors { get; set; }

      public override string ToString()
      {
         var stringRepresentation = string.Format("categories: {0};\t courses: {1};\t authors: {2}\n", Categories, Courses, Authors);
         return stringRepresentation;
      }
   }
}