using System;
using System.Runtime.Serialization;

namespace TM.Shared.HtmlContainer
{
   [Serializable]
   public class NodeParseException : Exception
   {
      public NodeParseException()
      {
      }

      public NodeParseException(string message, string htmlNodeData)
         : base(message)
      {
         NodeData = htmlNodeData;
      }

      public NodeParseException(string message, string htmlNodeData, Exception inner)
         : base(message, inner)
      {
         NodeData = htmlNodeData;
      }

      /// <exception cref="SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0). </exception>
      /// <exception cref="ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
      protected NodeParseException(SerializationInfo info,StreamingContext context)
         : base(info, context)
      {
      }
      
      public string NodeData { get; private set; }
   }
}