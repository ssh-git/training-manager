using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using TM.Shared;
using TM.UI.MVC.Models;
using WebGrease.Css.Extensions;

namespace TM.UI.MVC.Helpers
{
   public static class HtmlHelperExtensions
   {
      /// <summary>
      /// A helper for performing conditional IF logic using Razor
      /// </summary>
      public static IHtmlString If(this HtmlHelper html, bool condition, string trueString)
      {
         if (html == null) throw new ArgumentNullException("html");

         return html.IfElse(condition, h => MvcHtmlString.Create(trueString), h => MvcHtmlString.Empty);
      }

      /// <summary>
      /// A helper for performing conditional IF,ELSE logic using Razor
      /// </summary>
      public static IHtmlString IfElse(this HtmlHelper html, bool condition, string trueString, string falseString)
      {
         if (html == null) throw new ArgumentNullException("html");

         return html.IfElse(condition, h => MvcHtmlString.Create(trueString), h => MvcHtmlString.Create(falseString));
      }

      /// <summary>
      /// A helper for performing conditional IF logic using Razor
      /// </summary>
      public static IHtmlString If(this HtmlHelper html, bool condition, Func<HtmlHelper, IHtmlString> action)
      {
         if (html == null) throw new ArgumentNullException("html");
         if (action == null) throw new ArgumentNullException("action");

         return html.IfElse(condition, action, h => MvcHtmlString.Empty);
      }

      /// <summary>
      /// A helper for performing conditional IF,ELSE logic using Razor
      /// </summary>
      public static IHtmlString IfElse(this HtmlHelper html, bool condition, Func<HtmlHelper, IHtmlString> trueAction, Func<HtmlHelper, IHtmlString> falseAction)
      {
         if (html == null) throw new ArgumentNullException("html");
         if (trueAction == null) throw new ArgumentNullException("trueAction");
         if (falseAction == null) throw new ArgumentNullException("falseAction");

        return (condition ? trueAction : falseAction).Invoke(html);
      }

      /// <summary>
      /// A helper for performing conditional IF logic within Razor
      /// </summary>
      public static TResult If<TResult>(this HtmlHelper html, bool condition, Func<TResult> trueFunc)
      {
         if(html == null) throw new ArgumentNullException("html");
         if (trueFunc == null) throw new ArgumentNullException("trueFunc");

         return html.IfElse(condition, trueFunc, () => default(TResult));
      }

      /// <summary>
      /// A helper for performing conditional IF ELSE logic within Razor
      /// </summary>
      public static TResult IfElse<TResult>(this HtmlHelper html, bool criteria, Func<TResult> trueFunc, Func<TResult> falseFunc)
      {
         if (trueFunc == null) throw new ArgumentNullException("trueFunc");
         if (falseFunc == null) throw new ArgumentNullException("falseFunc");

         return criteria ? trueFunc() : falseFunc();
      }


      /// <summary>
      /// Returns a checkbox for each of the provided <paramref name="items"/>.
      /// </summary>
      public static IHtmlString CheckBoxListFor<TModel, TValue>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TValue>> expression, IEnumerable<SelectListItem> items, object htmlAttributes = null)
      {
         var templateInfo = htmlHelper.ViewData.TemplateInfo;
         var expressionText = ExpressionHelper.GetExpressionText(expression);

         var listId = templateInfo.GetFullHtmlFieldId(expressionText);
         var listName = templateInfo.GetFullHtmlFieldName(expressionText);
        
         var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

         items = GetCheckboxListWithDefaultValues(metaData.Model, items);
         var checkBoxList = htmlHelper.CheckBoxList(listName, listId, items, htmlAttributes);

         return checkBoxList;
      }

      /// <summary>
      /// Returns a checkbox for each of the provided <paramref name="items"/>.
      /// </summary>
      public static IHtmlString CheckBoxList(this HtmlHelper htmlHelper, string listName, string listId, IEnumerable<SelectListItem> items, object htmlAttributes = null)
      {
         var sb = new StringBuilder();

         var container = new TagBuilder("div");
         container.GenerateId(listId);
         sb.Append(container.ToString(TagRenderMode.StartTag));

         foreach (var item in items)
         {
            var checkboxContainer = new TagBuilder("div");
            checkboxContainer.AddCssClass("checkbox");
            checkboxContainer.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);

            var label = new TagBuilder("label");
           
            var checkbox = new TagBuilder("input");
            checkbox.MergeAttribute("type", "checkbox");
            checkbox.MergeAttribute("name", listName);
            checkbox.MergeAttribute("value", item.Value ?? item.Text);
            if (item.Selected)
               checkbox.MergeAttribute("checked", "checked");

            label.InnerHtml = checkbox.ToString(TagRenderMode.SelfClosing) + item.Text;

            sb.Append(checkboxContainer.ToString(TagRenderMode.StartTag));
            sb.Append(label.ToString(TagRenderMode.StartTag));
            sb.Append(checkbox.ToString(TagRenderMode.SelfClosing));
            sb.Append(item.Text);
            sb.Append(label.ToString(TagRenderMode.EndTag));
            sb.Append(checkboxContainer.ToString(TagRenderMode.EndTag));
         }

         sb.Append(container.ToString(TagRenderMode.EndTag));

         var checkBoxList =new MvcHtmlString(sb.ToString());

         return checkBoxList;
      }


      private static IEnumerable<SelectListItem> GetCheckboxListWithDefaultValues(object defaultValues, IEnumerable<SelectListItem> selectList)
      {
         var defaultValuesList = defaultValues as IEnumerable;

         if (defaultValuesList == null)
            return selectList;

         IEnumerable<string> values = from object value in defaultValuesList
                                      select Convert.ToString(value, CultureInfo.CurrentCulture);

         var selectedValues = new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
         var newSelectList = new List<SelectListItem>();

         selectList.ForEach(item =>
         {
            item.Selected = (item.Value != null) ? selectedValues.Contains(item.Value) : selectedValues.Contains(item.Text);
            newSelectList.Add(item);
         });

         return newSelectList;
      }
     
      public static IHtmlString DisplayAuthorParticipationDropDown(this HtmlHelper<AuthorViewModels.AuthorInfoViewModel> html, string text)
      {
         var participations = html.ViewData.Model.Participations.ToList();
         if (!participations.Any())
         {
            return null;
         }

         var startTag = new TagBuilder("span");
         startTag.MergeAttribute("class", "dropdown");

         const string navStartTemplate = "{0}" +
                                         "<button class='btn btn-lg btn-link dropdown-toggle' data-toggle='dropdown' type='button'>" +
                                         "{1} <span class='caret'></span>" +
                                         "</button>";

         var navStartTag = string.Format(navStartTemplate, startTag.ToString(TagRenderMode.StartTag), text);


         const string menuStartTag = "<ul class='dropdown-menu' role='menu'>";
         const string menuContentTemplate = "<li><a target='_blank' href='{0}'><img height='30' src='{1}' alt='{2}'/></a></li>";
         const string menuEndTag = "</ul>";

         var urlHelper = new UrlHelper(html.ViewContext.RequestContext);
         var sb = new StringBuilder();

         sb.Append(navStartTag);
         sb.Append(menuStartTag);

         foreach (var participation in participations)
         {
            sb.Append(string.Format(menuContentTemplate,
               urlHelper.RouteUrl(AppConstants.RouteNames.Author, participation.TrainingProviderAuthorRouteObject),
               participation.TrainingProvider.LogoUrl,
               participation.TrainingProvider.Name));
         }

         sb.Append(menuEndTag);
         sb.Append(startTag.ToString(TagRenderMode.EndTag));


         var linksHtml = MvcHtmlString.Create(sb.ToString());

         return linksHtml;
      }

      public static IHtmlString DisplayCourseSubscriptionMarker(this HtmlHelper html, LearningState learningState)
      {
         return GetCourseSubscriptionMarker(learningState);
      }

      public static IHtmlString GetCourseSubscriptionMarker(LearningState learningState)
      {
         if (!HttpContext.Current.User.Identity.IsAuthenticated)
         {
            return MvcHtmlString.Create(string.Empty);
         }

         string cssClass;
         string markerText;
         string toolTip;
         switch (learningState)
         {
            case LearningState.Planned:
               markerText = "P";
               toolTip = "Planned";
               cssClass = "label-warning";
               break;
            case LearningState.InProgress:
               markerText = "S";
               toolTip = "Started";
               cssClass = "label-primary";
               break;
            case LearningState.Finished:
               markerText = "F";
               toolTip = "Finished";
               cssClass = "label-success";
               break;
            default:
               markerText = "A";
               toolTip = "Available";
               cssClass = "label-info";
               break;
         }

         var marker = string.Format("<span class='tm-sb-marker label {0}' data-toggle='tooltip' title={1}>{2}</span>", cssClass, toolTip, markerText);
         return MvcHtmlString.Create(marker);
      }


      public static IHtmlString DisplayCourseSubscriptionToken(this HtmlHelper html, LearningState learningState)
      {
         string token;
         switch (learningState)
         {
            case LearningState.Planned:
               token = "plan";
               break;
            case LearningState.InProgress:
               token = "start";
               break;
            case LearningState.Finished:
               token = "finish";
               break;
            default:
                token = "available";
               break;
         }

         return MvcHtmlString.Create(token);
      }


      public static IHtmlString DisplayCourseRating(this HtmlHelper html, CourseRating courseRating)
      {
         return GetCourseRating(courseRating);
      }

      public static IHtmlString GetCourseRating(CourseRating courseRating)
      {
         int fullStars = (int)courseRating.Rating;

         int halfStar = courseRating.Rating - fullStars >= 0.5m
             ? 1
             : 0;

         int zeroStar = 5 - fullStars - halfStar;

         var sb = new StringBuilder();

         while (fullStars-- > 0)
         {
            sb.Append("<i class='tm-star fa fa-star'></i>");
         }
         while (halfStar-- > 0)
         {
            sb.Append("<i class='tm-star fa fa-star-half-o'></i>");
         }
         while (zeroStar-- > 0)
         {
            sb.Append("<i class='tm-star fa fa-star-o'></i>");
         }

         var courseRatingHtmlString = string.Format("<span class='text-nowrap' style='cursor: default;' title='{0:N1} points from {1} user(s)'>{2}</span>",
            courseRating.Rating, courseRating.Raters, sb.ToString());

         var courseRatingMvcString = MvcHtmlString.Create(courseRatingHtmlString);
         return courseRatingMvcString;
      }


      /// <exception cref="ArgumentException">Training provider name cannot be null or whitespace</exception>
      /// <exception cref="ArgumentNullException"><paramref name="courseAuthors"/> is <see langword="null" />.</exception>
      public static IHtmlString DisplayCourseAuthors(this HtmlHelper html, string trainingProviderName, IList<CourseAuthorViewModel> courseAuthors)
      {
         if (string.IsNullOrWhiteSpace(trainingProviderName))
            throw new ArgumentException("Training provider name cannot be null or whitespace", "trainingProviderName");

         if(courseAuthors == null)
            throw new ArgumentNullException("courseAuthors");
         
         var courseAuthorsHtml = GetCourseAuthors(trainingProviderName, courseAuthors);
         return courseAuthorsHtml;
      }

      public static IHtmlString GetCourseAuthors(string trainingProviderName, IList<CourseAuthorViewModel> authors)
      {
         var sb = new StringBuilder();

         var creator = authors.FirstOrDefault(x => !x.IsCoAuthor);
         if (creator != null)
         {
            sb.Append(GetCourseAuthorLink(trainingProviderName, creator.UrlName, creator.FullName));
         }

         var coAuthors = authors.Where(x => x.UrlName != creator.UrlName).ToList();
         if (coAuthors.Count != 0)
         {
            sb.Append(", ");
            var otherAuthorList =
               HttpUtility.HtmlAttributeEncode(GetOtherCourseAuthorsList(trainingProviderName, coAuthors));
            var popoverElementString =
               string.Format(
                  "<a tabindex='0' role='button' data-trigger='focus' data-toggle='popover' title='Other authors' data-content='{0}' data-html='true'>et&nbsp;al.</a>",
                  otherAuthorList);
            sb.Append(popoverElementString);
         }

         var courseAuthorMvcString = MvcHtmlString.Create(sb.ToString());
         return courseAuthorMvcString;
      }

      private static string GetCourseAuthorLink(string trainingProviderName, string authorUrlName, string linkText)
      {
         var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
         var linkString = string.Format("<a href='{0}'>{1}</a>",
            urlHelper.RouteUrl(AppConstants.RouteNames.Author, new { trainingProviderName, authorUrlName }), linkText);
         return linkString;
      }

      private static string GetOtherCourseAuthorsList(string trainingProviderName, IList<CourseAuthorViewModel> otherAuthors)
      {
         var sb = new StringBuilder();
         sb.Append("<div>");
         foreach (var author in otherAuthors)
         {
            var link = GetCourseAuthorLink(trainingProviderName, author.UrlName, author.FullName);
            var listItem = string.Concat(link, "<br/>");
            sb.Append(listItem);
         }
         sb.Append("</div>");

         var authorListString = sb.ToString();
         return authorListString;
      }
   }
}
