using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TM.UI.MVC.ControllableModel
{
   public abstract class ControllableViewModel<TModel, TSortStateModel> : PageableViewModel<TModel>, IControllableView
   {
      private IEnumerable<SelectListItem> _filterSelectList;
      private TSortStateModel _sortStateModel;

      /// <exception cref="ArgumentNullException"><paramref name="viewModelParams" /> is <see langword="null" />.</exception>
      protected ControllableViewModel(IControllableViewModelParams viewModelParams, string defaultPageSize = "10")
         : base( viewModelParams, defaultPageSize)
      {
      }


      /// <exception cref="ArgumentNullException"><paramref name="viewModelParams" /> or
      /// <paramref name="defaultPageSize" /> or
      /// <paramref name="sizeList" /> is <see langword="null" />.</exception>
      protected ControllableViewModel(IControllableViewModelParams viewModelParams, string defaultPageSize, params string[] sizeList)
         : base(viewModelParams, defaultPageSize, sizeList)
      {
      }

      public string SearchBoxTextPlaceholder { get; protected set; }
      
      public IControllableViewModelParams Params
      {
         get { return (IControllableViewModelParams) ViewModelParams; }
      }

      public IEnumerable<SelectListItem> FilterSelectList
      {
         get {ThrowIfNotInitialized(); return _filterSelectList; }
         protected set { _filterSelectList = value; }
      }

      public TSortStateModel SortState
      {
         get { ThrowIfNotInitialized(); return _sortStateModel; }
         protected set { _sortStateModel = value; }
      }


      public override async Task InitializeAsync(IQueryable<TModel> inputQuery)
      {
         if (InputQuery == null)
         {
            InputQuery = inputQuery;
         }

         var controlParams = ((IControllableViewModelParams) ViewModelParams);

         var chainQuery = inputQuery;

         chainQuery = ApplyFiltering(chainQuery, controlParams);
         chainQuery = ApplySearching(chainQuery, controlParams);
         chainQuery = ApplySorting(chainQuery, controlParams);

         await InitializeFilterSelectList(chainQuery, controlParams);

         SortState = CreateSortStateModel(controlParams);

         await base.InitializeAsync(chainQuery);
      }


      protected abstract IQueryable<TModel> ApplyFiltering(IQueryable<TModel> chainQuery, IFilterParam filterParam);
      protected abstract IQueryable<TModel> ApplySearching(IQueryable<TModel> chainQuery, ISearchParam searchParam);
      protected abstract IQueryable<TModel> ApplySorting(IQueryable<TModel> chainQuery, ISortParam sortParam);

      protected abstract TSortStateModel CreateSortStateModel(IControllableViewModelParams controlParams);

      protected abstract Task InitializeFilterSelectList(IQueryable<TModel> chainQuery, IFilterParam filterParam);
   }
}