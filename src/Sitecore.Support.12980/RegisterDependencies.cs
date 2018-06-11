using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.XA.Foundation.Multisite.SiteResolvers;

namespace Sitecore.Support
{
  public class RegisterDependencies : IServicesConfigurator
  {
    public void Configure(IServiceCollection serviceCollection)
    {
      serviceCollection.AddTransient<IEnvironmentSitesResolver, Sitecore.Support.XA.Foundation.Multisite.SiteResolvers.EnvironmentSitesResolver>();
    }
  }
}