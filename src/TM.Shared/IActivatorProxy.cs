using System;

namespace TM.Shared
{
   public interface IActivatorProxy
   {
      TActivatedType CreateInstance<TActivatedType>(string assemblyQualifiedTypeString);
   }

   public class ActivatorProxy : IActivatorProxy
   {
      private static readonly ActivatorProxy ActivatorInstance = new ActivatorProxy();

      private ActivatorProxy() { }

      public static ActivatorProxy Instance
      {
         get { return ActivatorInstance; }
      }

      public TActivatedType CreateInstance<TActivatedType>(string assemblyQualifiedTypeString)
      {
         return (TActivatedType)Activator.CreateInstance(Type.GetType(assemblyQualifiedTypeString));
      }
   }
}