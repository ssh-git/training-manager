using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace TM.UI.MVC.Helpers
{
   public static class JsonExtensions
   {
      public static string ToJson<T>(this T obj, bool includeNull = true, int? maxDepth = null)
      {
         var settings = new JsonSerializerSettings
         {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[] { new StringEnumConverter() },
            NullValueHandling = includeNull ? NullValueHandling.Include : NullValueHandling.Ignore
         };

         if (maxDepth != null)
         {
            settings.MaxDepth = maxDepth;
         }

         var serializationResult = JsonConvert.SerializeObject(obj, settings);

         return serializationResult;
      }
   }
}