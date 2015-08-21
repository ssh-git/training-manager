using System;
using System.Collections.Generic;
using System.Linq;
using TM.Data;
using TM.Data.Update;
using TM.Shared;
using TM.UI.MVC.Helpers;
using TM.UI.MVC.Models;

namespace TM.UI.MVC.Infrastructure
{
   public abstract class CatalogManagerBase : IDisposable
   {
      private UpdateDbContext _context;

      protected CatalogManagerBase()
         : this(new UpdateDbContext()) { }

      protected CatalogManagerBase(UpdateDbContext context)
      {
         _context = context;
      }

      public CatalogDbContext CatalogContext
      {
         get { return _context; }
      }

      public UpdateDbContext UpdateContext
      {
         get { return _context; }
      }


      public List<Specializations> GetUserSpecializationList(Specializations? userSpecializations)
      {
         var userSpecializationList = userSpecializations.HasValue && userSpecializations != Specializations.AllSpecializations
               ? new List<Specializations>(userSpecializations.Value.GetFlags())
               : new List<Specializations>();

         return userSpecializationList;
      }

      public IQueryable<Course> GetCoursesQueryForSpecializations(Specializations? specializations)
      {
         ThrowIfDisposed();

         var userSpecializationList = GetUserSpecializationList(specializations);

         var coursesQuery = from course in _context.Courses
                            where (!userSpecializationList.Any() ||
                                   course.CourseSpecializations.Any(x => userSpecializationList.Contains(x.Specialization)))
                                   && !course.IsDeleted
                            select course;

         return coursesQuery;
      }

      public List<string> GetTokenCatalog()
      {
         ThrowIfDisposed();

         var tokenCatalog = _context.TrainingProviders
           .Where(x => !x.IsDeleted)
           .Select(x => x.Name)
           .AsEnumerable()
           .Select(x => x.ToTitleCaseInvariant())
           .OrderBy(x => x)
           .ToList();

         tokenCatalog.Insert(0, NavigationViewModel.ALLToken);

         return tokenCatalog;
      }


      /// <exception cref="ObjectDisposedException"></exception>
      protected void ThrowIfDisposed()
      {
         if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
      }


      #region IDisposable Implementation

      private bool _disposed;

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }


      protected virtual void Dispose(bool disposing)
      {
         if (disposing && _context != null)
         {
            _context.Dispose();
            _context = null;

            _disposed = true;
         }
      }

      #endregion

   }
}