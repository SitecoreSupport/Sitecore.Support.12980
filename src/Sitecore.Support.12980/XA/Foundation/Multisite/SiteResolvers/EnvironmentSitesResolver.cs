namespace Sitecore.Support.XA.Foundation.Multisite.SiteResolvers
{
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Events;
  using Sitecore.Data.Fields;
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
      #region Added code
      AddMissingSites(obj, database);
      #endregion
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
    #region Added code
    public void AddMissingSites(List<Item> sites, Database database)
    {
      Item item = database.GetItem(new ID("{F78EC6BE-D9BA-4595-B740-E801ACDB0459}"));
      if (item == null)
      {
        return;
      }
      MultilistField multilistField = item.Fields[Templates.SiteManagement.Fields.Order];
      if (multilistField != null)
      {
        foreach (var site in sites)
        {
          if (site.TemplateID == new ID("{EDA823FC-BC7E-4EF6-B498-CD09EC6FDAEF}"))
          {
            if (!multilistField.TargetIDs.Contains(site.ID))
            {
              using (new EventDisabler())
              {
                using (new EditContext(item))
                {
                  multilistField.Add(site.ID.ToString());
                }
              }
              string[] cacheKeys = Sitecore.Caching.CacheManager.GetItemCache(database).InnerCache.GetCacheKeys();
              foreach (var cacheKey in cacheKeys)
              {
                if (cacheKey.StartsWith(item.ID.ToString()))
                {
                  Sitecore.Caching.CacheManager.GetItemCache(database).Remove(cacheKey);
                }
              }
            }
          }
        }
      }
    }
    #endregion
  }
}