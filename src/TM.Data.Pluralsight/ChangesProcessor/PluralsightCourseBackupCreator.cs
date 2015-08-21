using System;
using TM.Data.Pluralsight.Properties;
using TM.Data.Update;

namespace TM.Data.Pluralsight
{
   internal class PluralsightCourseBackupCreator : IBackupCreator<Course, PluralsightCourse, CourseBackup>
   {
      public bool IsDifferent(Course existing, PluralsightCourse processing)
      {
         return existing.Title != processing.Title ||
                existing.SiteUrl != processing.SiteUrl ||
                existing.Description != processing.Description ||
                existing.HasClosedCaptions != processing.HasClosedCaptions ||
                existing.Rating != processing.Rating ||
                existing.Level != processing.Level ||
                existing.Duration != processing.Duration ||
                existing.ReleaseDate != processing.ReleaseDate ||
                existing.IsDeleted; 
      }

      /// <exception cref="InvalidOperationException"><see cref="PluralsightCategory.Id"/> doesn't set on the <paramref name="processing"/> course.</exception>
      public CourseBackup ApplayChanges(int updateEventId, Course existing, PluralsightCourse processing)
      {
         var differ = false;
         var backup = new CourseBackup
         {
            UpdateEventId = updateEventId,
            CourseId = existing.Id
         };

         if (processing.Category.Id == 0)
         {
            throw new InvalidOperationException(Resources.InvalidOperation_CategoryIdMustBeSetOnTheCourse);
         }

         if (existing.CategoryId != processing.Category.Id)
         {
            backup.CategoryId = existing.CategoryId;
            existing.CategoryId = processing.Category.Id;
            differ = true;
         }

         if (existing.Title != processing.Title)
         {
            backup.Title = existing.Title;
            existing.Title = processing.Title;
            differ = true;
         }

         if (existing.SiteUrl != processing.SiteUrl)
         {
            backup.SiteUrl = existing.SiteUrl;
            existing.SiteUrl = processing.SiteUrl;
            differ = true;
         }

         if (existing.Description != processing.Description)
         {
            backup.Description = existing.Description;
            existing.Description = processing.Description;
            differ = true;
         }

         if (existing.HasClosedCaptions != processing.HasClosedCaptions)
         {
            backup.HasClosedCaptions = existing.HasClosedCaptions;
            existing.HasClosedCaptions = processing.HasClosedCaptions;
            differ = true;
         }

         // only modify data. don't create backup.
         if (existing.Rating != processing.Rating)
         {
            existing.Rating = processing.Rating;
         }


         if (existing.Level != processing.Level)
         {
            backup.CourseLevel = (byte)existing.Level;
            existing.Level = processing.Level;
            differ = true;
         }

         if (existing.Duration != processing.Duration)
         {
            backup.Duration = existing.Duration;
            existing.Duration = processing.Duration;
            differ = true;
         }

         if (existing.ReleaseDate != processing.ReleaseDate)
         {
            backup.ReleaseDate = existing.ReleaseDate;
            existing.ReleaseDate = processing.ReleaseDate;
            differ = true;
         }

         existing.IsDeleted = false;

         return differ ? backup : null;
      }
   }
}