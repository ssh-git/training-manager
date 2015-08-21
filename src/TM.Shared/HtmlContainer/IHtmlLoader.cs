using System;
using System.Threading.Tasks;

namespace TM.Shared.HtmlContainer
{
   public interface IHtmlLoader
   {
      /// <exception cref="HtmlLoadException">Error while loading the content.</exception>
      /// <exception cref="ArgumentNullException"><paramref name="contentUrlOrPath"/> is <see langword="null" />.</exception>
      IHtmlContainer Load(string contentUrlOrPath, LocationType location);

      /// <exception cref="HtmlLoadException">Error while loading the content.</exception>
      /// <exception cref="ArgumentNullException"><paramref name="contentUrlOrPath"/> is <see langword="null" />.</exception>
      Task<IHtmlContainer> LoadAsync(string contentUrlOrPath, LocationType location);
   }
}