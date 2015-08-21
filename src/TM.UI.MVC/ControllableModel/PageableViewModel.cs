using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using PagedList;

namespace TM.UI.MVC.ControllableModel
{
   public class PageableViewModel<TModel>
   {
      protected IQueryable<TModel> InputQuery;
      protected readonly IPageParam ViewModelParams;

      private readonly int _selectedPageSize;
      private readonly int _selectedPageNumber;

      private IPagedList<TModel> _pagedList;
      private TModel _metadata;
      private IEnumerable<SelectListItem> _pageSizeSelectList;

      /// <exception cref="ArgumentNullException"><paramref name="viewModelParams"/> is <see langword="null" />.</exception>
      public PageableViewModel(IPageParam viewModelParams, string defaultPageSize = "10")
         : this(viewModelParams, defaultPageSize, "5", "10", "25", "50", "100")
      {
      }


      /// <exception cref="ArgumentNullException"><paramref name="viewModelParams"/> or
      /// <paramref name="defaultPageSize"/> or
      /// <paramref name="sizeList"/> is <see langword="null" />.</exception>
      public PageableViewModel(IPageParam viewModelParams, string defaultPageSize, params string[] sizeList)
      {
         if (viewModelParams == null)
            throw new ArgumentNullException("viewModelParams");

         if (viewModelParams == null)
            throw new ArgumentNullException("defaultPageSize");

         if (viewModelParams == null)
            throw new ArgumentNullException("sizeList");
        
         ViewModelParams = viewModelParams;

         var pageSizeList = sizeList.OrderBy(x => x.Length).ThenBy(x => x).ToList();

         var pageSizeString = pageSizeList.Contains(viewModelParams.PageSize ?? string.Empty)
            ? viewModelParams.PageSize
            : defaultPageSize;
         
         _selectedPageSize = Convert.ToInt32(pageSizeString);

         var positiveIntegerNumberStyle = NumberStyles.Integer ^ NumberStyles.AllowLeadingSign;
         var parseSuccess = int.TryParse(viewModelParams.Page, positiveIntegerNumberStyle, NumberFormatInfo.InvariantInfo,
            out _selectedPageNumber);

         if (!parseSuccess)
         {
            _selectedPageNumber = 1;
         }

         FixPageNumberAndPageSize();

         PageSizeSelectList = pageSizeList.Select(pageSize => new SelectListItem
         {
            Text = pageSize,
            Value = pageSize,
            Selected = pageSize == pageSizeString
         }).ToList();
      }


      protected bool Initialized { get; private set; }


      public TModel Metadata
      {
         get { ThrowIfNotInitialized(); return _metadata; }
         protected set { _metadata = value; }
      }

      public IPagedList<TModel> PagedList
      {
         get { ThrowIfNotInitialized(); return _pagedList; }
         private set { _pagedList = value; }
      }

      public IEnumerable<SelectListItem> PageSizeSelectList
      {
         get { ThrowIfNotInitialized(); return _pageSizeSelectList; }
         private set { _pageSizeSelectList = value; }
      }


      public virtual async Task InitializeAsync(IQueryable<TModel> inputQuery)
      {
         if (InputQuery == null)
         {
            InputQuery = inputQuery;
         }

         PagedList = await Task.Run(() => inputQuery.ToPagedList(_selectedPageNumber, _selectedPageSize));

         Initialized = true;
      }


      private void FixPageNumberAndPageSize()
      {
         ViewModelParams.Page = _selectedPageNumber.ToString();
         ViewModelParams.PageSize = _selectedPageSize.ToString();
      }

      protected void ThrowIfNotInitialized()
      {
         if (!Initialized)
         {
            throw new InvalidOperationException("Method InitializeAsync() must be invoked first.");
         }
      }
   }
}