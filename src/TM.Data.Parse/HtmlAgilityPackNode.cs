using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;
using TM.Shared.HtmlContainer;

namespace TM.Data.Parse
{
   internal class HtmlAgilityPackNode : IQueryableNode
   {
      private readonly HtmlNode _node;

      /// <exception cref="ArgumentNullException"><paramref name="node"/> is <see langword="null" />.</exception>
      public HtmlAgilityPackNode(HtmlNode node)
      {
         if(node == null) throw new ArgumentNullException("node");

         _node = node;
      }

      public string InnerText
      {
         get { return HttpUtility.HtmlDecode(_node.InnerText.Trim()); }
      }

      public string OuterText
      {
         get { return _node.OuterHtml; }
      }


      public string GetAttributeValue(string attributeName)
      {
         return HttpUtility.HtmlDecode(_node.Attributes[attributeName].Value);
      }

      public IEnumerable<IQueryableNode> SelectNodes(string xpath)
      {
         var selectionResult = _node.SelectNodes(xpath);

         var selectedNodes = selectionResult != null
            ? selectionResult.Select(n => new HtmlAgilityPackNode(n))
            : null;
         return selectedNodes;
      }

      public IEnumerable<IQueryableNode> SelectNodes(string xpath, out int nodeCount)
      {
         var selectionResult = _node.SelectNodes(xpath);

         if (selectionResult == null)
         {
            nodeCount = 0;
            return null;
         }
         nodeCount = selectionResult.Count;
         return selectionResult.Select(n => new HtmlAgilityPackNode(n));
      }

      public IQueryableNode SelectSingleNode(string xpath)
      {
         var selectionResult = _node.SelectSingleNode(xpath);
         var selectedNode = selectionResult != null
            ? new HtmlAgilityPackNode(selectionResult)
            : null;
         return selectedNode;
      }
   }
}