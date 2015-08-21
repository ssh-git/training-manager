using System.Collections.Generic;

namespace TM.Shared.HtmlContainer
{
   public interface IQueryableNode : INode
   {
      IEnumerable<IQueryableNode> SelectNodes(string xpath);
      IEnumerable<IQueryableNode> SelectNodes(string xpath, out int nodeCount);
      IQueryableNode SelectSingleNode(string xpath);
   }
}