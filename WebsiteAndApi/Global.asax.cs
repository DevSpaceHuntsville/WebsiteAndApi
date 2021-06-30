using System.Web.Http;

namespace DevSpace {
	public class WebApiApplication : System.Web.HttpApplication {
		protected void Application_Start() {
			System.Net.ServicePointManager.SecurityProtocol =
				System.Net.SecurityProtocolType.Tls12;

			GlobalConfiguration.Configure( WebApiConfig.Register );
		}
	}
}