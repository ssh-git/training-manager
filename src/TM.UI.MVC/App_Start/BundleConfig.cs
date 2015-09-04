using System.Diagnostics.CodeAnalysis;
using System.Web.Optimization;

namespace TM.UI.MVC
{
   [SuppressMessage("ReSharper", "StringLiteralTypo")]
   [SuppressMessage("ReSharper", "CommentTypo")]
   [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
   public class BundleConfig
   {
      // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
      public static void RegisterBundles(BundleCollection bundles)
      {
         // Use the development version of Modernizr to develop with and learn from. Then, when you're
         // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.

         //BundleTable.EnableOptimizations = true;
         bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            "~/Scripts/modernizr-*"));

         bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            "~/Scripts/jquery.validate*"));

         bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
            "~/Scripts/jquery-{version}.js",
            "~/Scripts/bootstrap.js",
            "~/Scripts/respond.js",
            "~/Scripts/jquery.dataTables.js",
            "~/Scripts/dataTables.bootstrap.js",
            "~/Scripts/dataTables.responsive.js",
            "~/Scripts/dataTables.colReorder.js",
            "~/Scripts/dataTables.colVis.js",
            "~/Scripts/notify-combined.js",
            "~/Scripts/app/app.js",
            "~/Scripts/app/tm-dataTables.js",
            "~/Scripts/app/tm-course-add-controller.js",
            "~/Scripts/app/tm-dom-search.js",
            "~/Scripts/app/layout.js"));

         //Catalog Home
         bundles.Add(new ScriptBundle("~/bundles/home").Include(
            "~/Scripts/app/home.js"));

         //Catalog Training Provider
         bundles.Add(new ScriptBundle("~/bundles/trainingProvider").Include(
            "~/Scripts/jquery-ui.js",
            "~/Scripts/app/tm-training-provider.js"));

         //Catalog Author
         bundles.Add(new ScriptBundle("~/bundles/authors").Include(
            "~/Scripts/app/authors.js"));
         bundles.Add(new ScriptBundle("~/bundles/author").Include(
            "~/Scripts/app/author.js"));

         //Catalog Category
         bundles.Add(new ScriptBundle("~/bundles/categories").Include(
            "~/Scripts/app/categories.js"));
         bundles.Add(new ScriptBundle("~/bundles/category").Include(
            "~/Scripts/app/category.js"));

         //Catalog Course
         bundles.Add(new ScriptBundle("~/bundles/course").Include(
            "~/Scripts/app/tm-course-toc.js",
            "~/Scripts/app/tm-course-ajax-add-to-plan.js"));

         //Catalog Learning Plan
         bundles.Add(new ScriptBundle("~/bundles/learningPlan").Include(
            "~/Scripts/jquery.validate*",
            "~/Scripts/jquery-ui.js",
            "~/Scripts/star-rating.js",
            "~/Scripts/app/tm-learning-plan.js"));

         //Styles
         bundles.Add(new StyleBundle("~/Content/css").Include(
            "~/Content/jquery-ui.css",
            "~/Content/star-rating.css",
            "~/Content/bootstrap.css",
            "~/Content/font-awesome.css",
            "~/Content/dataTables.bootstrap.css",
            "~/Content/dataTables.responsive.css",
            "~/Content/dataTables.colReorder.css",
            "~/Content/dataTables.colVis.css",
            "~/Content/site.css"));
      }
   }
}
