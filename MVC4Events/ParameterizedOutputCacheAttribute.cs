/*
 * Coded by Alexander Kouznetsov
 * www.unconnected.info / inbox@unconnected.info
 * Saint-Petersburg. Russia.
 * 2015-07-10
 * 
 * You can remove this whole commetnt if it's annoying :)
 */

using System.Web.Mvc;
using System.Web.Configuration;

namespace Unconnected.Mvc.Outputcache
{
    public class ParameterizedOutputCacheAttribute:OutputCacheAttribute
    {
        private const string sectionPath = "system.web/caching/outputCacheSettings";

        protected static OutputCacheProfileCollection profiles
        {
            get
            {
                var section = (OutputCacheSettingsSection)WebConfigurationManager.GetSection(sectionPath);
                if (section != null)
                {
                    return section.OutputCacheProfiles;
                }
                return null;
            }
        }

        public new string CacheProfile
        {
            get
            {
                return base.CacheProfile;
            }
            set
            {
                base.CacheProfile = value;

                if (profiles != null)
                {
                    var profile = profiles[value];
                    if (!(profile==null || string.IsNullOrWhiteSpace(profile.VaryByParam)))
                    {
                        this.VaryByParam = profile.VaryByParam;
                    }
                }
            }
        }
    }
}
