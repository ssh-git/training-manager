using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TM.UI.MVC.Infrastructure
{
   public class JsonNetResult : JsonResult
   {

      private Formatting _formatting;
      private JsonSerializerSettings _jsonSerializerSettings;
      private JsonConverter[] _jsonConverters;

      public JsonSerializerSettings DefaultJsonSerializerSettings
      {
         get
         {
            return new JsonSerializerSettings
            {
               ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
         }
      }

      public JsonNetResult(object data, Formatting formatting = Formatting.None)
      {
         Data = data;
         _formatting = formatting;
      }

      public JsonNetResult(object data, JsonSerializerSettings jsonSerializerSettings, Formatting formatting = Formatting.None)
      {
         Data = data;
         _formatting = formatting;
         _jsonSerializerSettings = jsonSerializerSettings;

      }

      public JsonNetResult(object data, Formatting formatting, params  JsonConverter[] converters)
      {
         Data = data;
         _formatting = formatting;
         _jsonConverters = converters;
      }

      /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null" />.</exception>
      public override void ExecuteResult(ControllerContext context)
      {
        if(context == null) throw new ArgumentNullException("context");

         var response = context.HttpContext.Response;
         response.ContentType = !string.IsNullOrWhiteSpace(ContentType)
            ? this.ContentType
            : "application/json";

         if (this.ContentEncoding != null)
         {
            response.ContentEncoding = ContentEncoding;
         }

         string serializedObject;

         if (_jsonSerializerSettings != null)
         {
            serializedObject = JsonConvert.SerializeObject(Data, _formatting, _jsonSerializerSettings);
         }else if (_jsonConverters != null)
         {
            serializedObject = JsonConvert.SerializeObject(Data, _formatting, _jsonConverters);
         }
         else
         {
            serializedObject = JsonConvert.SerializeObject(Data, _formatting, DefaultJsonSerializerSettings);
         }


         response.Write(serializedObject);
      }
   }
}