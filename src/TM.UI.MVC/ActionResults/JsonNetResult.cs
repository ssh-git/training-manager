using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TM.UI.MVC.Helpers;

namespace TM.UI.MVC
{
   public class JsonNetResult : ActionResult
   {
      public JsonNetResult()
      {
         JsonRequestBehavior = JsonRequestBehavior.DenyGet;
         ErrorMessages = new List<string>();
         IncludeNull = true;
      }

      public IList<string> ErrorMessages { get; private set; }

      public JsonRequestBehavior JsonRequestBehavior { get; set; }

      public Encoding ContentEncoding { get; set; }
      public string ContentType { get; set; }

      public bool IncludeNull { get; set; }
      public int? MaxDepth { get; set; }

      public object Data { get; set; }


      public void AddError(string errorMessage)
      {
         ErrorMessages.Add(errorMessage);
      }

      public void AddErrors(IEnumerable<string> errorMessages)
      {
         foreach (var message in errorMessages)
         {
            ErrorMessages.Add(message);
         }
      }

      /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null" />.</exception>
      /// <exception cref="InvalidOperationException">Get request not allowed.</exception>
      public override void ExecuteResult(ControllerContext context)
      {
         if (context == null)
            throw new ArgumentNullException("context");

         if (JsonRequestBehavior == JsonRequestBehavior.DenyGet &&
             string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(Properties.Resources.JsonRequest_GetNotAllowed);

         ConfigResponse(context.HttpContext.Response);
         SerializeData(context.HttpContext.Response);
      }

      private void ConfigResponse(HttpResponseBase response)
      {
         response.ContentType = string.IsNullOrEmpty(ContentType) ? "application/json" : ContentType;

         if (ContentEncoding != null) response.ContentEncoding = ContentEncoding;
      }

      private void SerializeData(HttpResponseBase response)
      {
         if (ErrorMessages.Any())
         {
            Data = new
            {
               ErrorMessage = string.Join("\n", ErrorMessages),
               ErrorMessages = ErrorMessages.ToArray()
            };

            // Bad Request
            // The request could not be understood by the server due to malformed syntax.
            // The client SHOULD NOT repeat the request without modifications.
            response.StatusCode = 400;
         }

         if (Data == null) return;

         response.Write(Data.ToJson(IncludeNull, MaxDepth));
      }
   }


   public class JsonNetResult<T> : JsonNetResult
   {
      public new T Data
      {
         get { return (T)base.Data; }
         set { base.Data = value; }
      }
   }
}