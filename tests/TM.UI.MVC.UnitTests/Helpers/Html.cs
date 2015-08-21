using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace TM.UI.MVC.UnitTests.Helpers
{
   public static class Html
   {
      public static HtmlHelper<T> CreateHtmlHelper<T>(ViewDataDictionary viewData)
      {
         var cc = new Mock<ControllerContext>(
            new Mock<HttpContextBase>().Object,
            new RouteData(),
            new Mock<ControllerBase>().Object);

         var mockViewContext = new Mock<ViewContext>(
            cc.Object,
            new Mock<IView>().Object,
            viewData,
            new TempDataDictionary(),
            TextWriter.Null);

         var mockViewDataContainer = new Mock<IViewDataContainer>();

         mockViewDataContainer.Setup(v => v.ViewData).Returns(viewData);

         return new HtmlHelper<T>(
            mockViewContext.Object, mockViewDataContainer.Object);
      }
   }
}
