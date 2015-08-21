using System;
using System.Collections.Generic;
using TM.Shared.HtmlContainer;

namespace TM.Data.Pluralsight
{
   internal interface INodeSelector
   {
      void SetContext(IHtmlContainer catalog);
      INode SelectNode(INode context, string xpath);
      IEnumerable<INode> SelectAuthorNodes();
      IEnumerable<INode> SelectCategoryNodes(out int nodesCount);
      IEnumerable<INode> SelectCourseNodes(string categoryUrlName, out int nodesCount);
      INode SelectSketchNode(string categoryUrlName);
      INode SelectInfoNode(INode courseNode);
      INode SelectClosedCaptionsNode(INode courseNode);
      IEnumerable<INode> SelectAuthorNodes(INode courseNode);
      INode SelectLevelNode(INode courseNode);
      INode SelectRatingNode(INode courseNode);
      INode SelectDurationNode(INode courseNode);
      INode SelectReleaseDateNode(INode courseNode);
   }

   internal class PluralsightNodeSelector : INodeSelector
   {
      private IHtmlContainer _catalog;

      public PluralsightNodeSelector()
      {
      }

      /// <exception cref="ArgumentNullException"><paramref name="catalog"/> is <see langword="null" />.</exception>
      public PluralsightNodeSelector(IHtmlContainer catalog)
      {
         if(catalog == null)
            throw new ArgumentNullException("catalog");

         _catalog = catalog;
      }

      /// <exception cref="ArgumentNullException"><paramref name="catalog"/> is <see langword="null" />.</exception>
      public void SetContext(IHtmlContainer catalog)
      {
         if (catalog == null)
            throw new ArgumentNullException("catalog");

         _catalog = catalog;
      }


      public  IEnumerable<INode> SelectNodes(string xpath)
      {
         var nodes = _catalog.RootNode.SelectNodes(xpath);
         return nodes;
      }

      public  IEnumerable<INode> SelectNodes(string xpath, out int nodesCount)
      {
         var nodes = _catalog.RootNode.SelectNodes(xpath, out nodesCount);
         return nodes;
      }

      public  INode SelectNode(string xpath)
      {
         var node = _catalog.RootNode.SelectSingleNode(xpath);
         return node;
      }
      

      public  INode SelectNode(INode context, string xpath)
      {
         var node = ((IQueryableNode) context).SelectSingleNode(xpath);
         return node;
      }


      public  IEnumerable<INode> SelectAuthorNodes()
      {
         return SelectNodes(Constants.AuthorNodesXPath);
      }
     

      public  IEnumerable<INode> SelectCategoryNodes(out int nodesCount)
      {
         return SelectNodes(Constants.CategoryNodesXPath, out nodesCount);
      }

      
      public  IEnumerable<INode> SelectCourseNodes(string categoryUrlName, out int nodesCount)
      {
         var xPath = string.Format(Constants.CourseNodesXPathTemplate, categoryUrlName);
         return SelectNodes(xPath, out nodesCount);
      }


      public  INode SelectSketchNode(string categoryUrlName)
      {
         var xPath = string.Format(Constants.SketchNodeXPathTemplate, categoryUrlName);
         return SelectNode(xPath);
      }


      public  INode SelectInfoNode(INode courseNode)
      {
         
         return ((IQueryableNode) courseNode).SelectSingleNode(Constants.CourseInfoXPath);
      }


      public  INode SelectClosedCaptionsNode(INode courseNode)
      {
         return ((IQueryableNode) courseNode).SelectSingleNode(Constants.ClosedCaptionsFeatureXPath);
      }


      public  IEnumerable<INode> SelectAuthorNodes(INode courseNode)
      {
         var authorNodes = ((IQueryableNode) courseNode).SelectNodes(Constants.CourseAuthorsXPath);
         return authorNodes;
      }


      public  INode SelectLevelNode(INode courseNode)
      {
         return ((IQueryableNode) courseNode).SelectSingleNode(Constants.CourseLevelXPath);
      }


      public  INode SelectRatingNode(INode courseNode)
      {
         return ((IQueryableNode) courseNode).SelectSingleNode(Constants.CourseRatingXPath);
      }


      public  INode SelectDurationNode(INode courseNode)
      {
         return ((IQueryableNode) courseNode).SelectSingleNode(Constants.CourseDurationXPath);
      }


      public  INode SelectReleaseDateNode(INode courseNode)
      {
         return ((IQueryableNode) courseNode).SelectSingleNode(Constants.CourseReleaseDateXPath);
      }


      internal static class Constants
      {
         public const string AuthorNodesXPath = @"//td[@class='author']/a";
         public const string CategoryNodesXPath = @"./div[@class='categoryHeader']";
         public const string CourseNodesXPathTemplate = @"./div[div/img[@class='{0}']]/div/table//tr";
         public const string SketchNodeXPathTemplate = @"./div/div[@class='sketch']/img[@class='{0}']";

         public const string CourseInfoXPath = @"./td[@class='title']/a";
         public const string ClosedCaptionsFeatureXPath = @"./td[@class='title']/img[@class='cc']";
         public const string CourseAuthorsXPath = @"./td[@class='author']//a";
         public const string CourseLevelXPath = @"./td[@class='level']";
         public const string CourseRatingXPath = @"./td[@class='rating']/span";
         public const string CourseDurationXPath = @"./td[@class='duration']";
         public const string CourseReleaseDateXPath = @"./td[@class='releaseDate']";
      }
   }
}
