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
         bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                     "~/Scripts/modernizr-*"));

         bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                     "~/Scripts/jquery-{version}.js"));

         bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                     "~/Scripts/jquery.validate*"));

         bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
            "~/Scripts/bootstrap.js",
            "~/Scripts/respond.js",
            "~/Scripts/jquery.dataTables.js",
            "~/Scripts/dataTables.bootstrap.js",
            "~/Scripts/dataTables.responsive.js",
            "~/Scripts/dataTables.colReorder.js",
            "~/Scripts/dataTables.colVis.js",
            "~/Scripts/notify-combined.js",
            "~/Scripts/tm-dataTables.js",
            "~/Scripts/tm-course-add-controller.js",
            "~/Scripts/tm-dom-search.js"));

         bundles.Add(new ScriptBundle("~/bundles/learningPlan").Include(
           "~/Scripts/jquery-ui.js",
           "~/Scripts/star-rating.js",
           "~/Scripts/tm-learning-plan.js"));

         bundles.Add(new ScriptBundle("~/bundles/trainingProvider").Include(
          "~/Scripts/jquery-ui.js",
          "~/Scripts/tm-training-provider.js"));

         bundles.Add(new ScriptBundle("~/bundles/course").Include(
           "~/Scripts/tm-course-toc.js",
           "~/Scripts/tm-course-ajax-add-to-plan.js"));


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
