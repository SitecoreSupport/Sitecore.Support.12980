namespace Sitecore.Support.XA.Foundation.Multisite.SiteResolvers
{
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.XA.Foundation.Multisite;
  using Sitecore.XA.Foundation.Multisite.Comparers;
  using Sitecore.XA.Foundation.Multisite.SiteResolvers;
  using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class EnvironmentSitesResolver : IEnvironmentSitesResolver
  {
    public const string AnyEnvironment = "*";

    public List<Item> ResolveAllSites(Database database)
    {
      List<Item> obj = database?.GetContentItemsOfTemplate(Templates.SiteDefinition.ID).ToList() ?? new List<Item>();
      obj.Sort(new TreeOrderComparer());
      return obj;
    }

    public List<Item> ResolveEnvironmentSites(List<Item> sites, string environment)
    {
      if (!string.IsNullOrEmpty(environment) && !string.Equals(environment, "*", StringComparison.OrdinalIgnoreCase))
      {
        sites = sites.Where(delegate (Item site)
        {
          string text = ((BaseItem)site)[Templates.SiteDefinition.Fields.Environment].Trim();
          if (!string.IsNullOrWhiteSpace(text) && !string.Equals(text, environment, StringComparison.OrdinalIgnoreCase))
          {
            return string.Equals(text, "*", StringComparison.OrdinalIgnoreCase);
          }
          return true;
        }).ToList();
        sites.Sort(new TreeOrderComparer());
        return sites;
      }
      sites.Sort(new TreeOrderComparer());
      return sites;
    }

    public string GetActiveEnvironment()
    {
      return Settings.GetSetting("XA.Foundation.Multisite.Environment", "*");
    }

    public List<string> ResolveEnvironments(List<Item> sites)
    {
      return (from s in sites
              select ((BaseItem)s)[Templates.SiteDefinition.Fields.Environment].Trim()).Distinct().Where(delegate (string i)
              {
                if (string.Equals(i, "*", StringComparison.OrdinalIgnoreCase))
                {
                  return string.IsNullOrWhiteSpace(i);
                }
                return false;
              }).ToList();
    }
  }
}