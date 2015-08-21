using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using TM.Data;
using TM.Shared;
using TM.UI.MVC.Helpers;

namespace TM.UI.MVC.Models
{
   public class SpecializationsListViewModel
   {
      private Specializations? _selectedSpecializations;
      private IEnumerable<SelectListItem> _specializationsSelectList;

      public SpecializationsListViewModel():this(Specializations.None)
      {
      }

      public SpecializationsListViewModel(Specializations selectedSpecializations)
      {
         SelectedSpecializationTitles = selectedSpecializations.GetFlags().Select(x => x.ToString());
      }


      public Specializations SelectedSpecializations
      {
         get
         {
            if (_selectedSpecializations != null)
            {
               return _selectedSpecializations.Value;
            }

            if (SelectedSpecializationTitles == null)
            {
               _selectedSpecializations = Specializations.AllSpecializations;

               return _selectedSpecializations.Value;
            }

            var enumText = string.Join(",", SelectedSpecializationTitles);
            if (enumText == string.Empty)
            {
               _selectedSpecializations = Specializations.None;
            } else
            {
               _selectedSpecializations = (Specializations)Enum.Parse(typeof(Specializations), enumText);
            }

            return _selectedSpecializations.Value;
         }

      }


      [Display(Name = "Select Specializations")]
      public IEnumerable<string> SelectedSpecializationTitles { get; set; }


      public IEnumerable<SelectListItem> SpecializationsSelectList
      {
         get
         {
            if (_specializationsSelectList != null)
            {
               return _specializationsSelectList;
            }

            _specializationsSelectList = Specializations.AllSpecializations.GetFlags().Select(x => new SelectListItem
            {
               Text = x.GetDisplayName().ToString(),
               Value = x.ToString()
            }).ToList(); 

            return _specializationsSelectList;
         }
      }
   }
}