using System;
using System.IO;
using System.Text;

namespace TM.UI.MVC.Helpers
{
   public class StringWriterWithEncoding : StringWriter
   {
      private readonly Encoding _encoding;
      public StringWriterWithEncoding()
      {
      }

      public StringWriterWithEncoding(Encoding encoding)
      {
         _encoding = encoding;
      }

      public StringWriterWithEncoding(IFormatProvider formatProvider)
         : base(formatProvider)
      {
      }

      public StringWriterWithEncoding(Encoding encoding, IFormatProvider formatProvider)
         : base(formatProvider)
      {
         _encoding = encoding;
      }



      public StringWriterWithEncoding(StringBuilder sb)
         : base(sb)
      {
      }

      public StringWriterWithEncoding(StringBuilder sb, Encoding encoding)
         : base(sb)
      {
         _encoding = encoding;
      }

      public StringWriterWithEncoding(StringBuilder sb, IFormatProvider formatProvider)
         : base(sb, formatProvider)
      {
      }

      public StringWriterWithEncoding(StringBuilder sb, Encoding encoding, IFormatProvider formatProvider)
         : base(sb, formatProvider)
      {
         _encoding = encoding;
      }

      public override Encoding Encoding
      {
         get
         {
            return _encoding ?? base.Encoding;
         }
      }
   }
}