using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VK.WindowsPhone.SDK.Util;
using Windows.System;

namespace VK.WindowsPhone.SDK
{
    public static class VKAppLaunchAuthorizationHelper
    {
        private static readonly string _launchUriStrFrm = @"vkconnect://authorize?State={0}&ClientId={1}&Scope={2}&Revoke={3}&RedirectUri={4}";

        public static async Task AuthorizeVKApp(
            string state,
            string clientId, 
            List<string> scopeList,
            bool revoke)
        {
            string redirectUri = await GetRedirectUri();

            var uriString = string.Format(_launchUriStrFrm,
                HttpUtility.UrlEncode(state == null ? string.Empty : state),
                clientId,
                StrUtil.GetCommaSeparated(scopeList),
                revoke,
                redirectUri);

            await Launcher.LaunchUriAsync(new Uri(uriString));
        }

        private static async Task<string> GetRedirectUri()
        {
            return await GetVKLoginCallbackSchemeName() + "://authorize";
        }

        async private static Task<string> GetVKLoginCallbackSchemeName()
        {
            string result = await GetFilteredManifestAppAttributeValue("Protocol", "Name", "vkc");
            return result;
        }

        internal async static Task<string> GetFilteredManifestAppAttributeValue(string node, string attribute, string prefix)
        {
#if WINDOWS_UNIVERSAL
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///FacebookConfig.xml"));
            using (Stream strm = await file.OpenStreamForReadAsync())
#endif

#if WINDOWS_PHONE
            using (System.IO.Stream strm = Microsoft.Xna.Framework.TitleContainer.OpenStream("WMAppManifest.xml"))
#endif
            {
                var xml = XElement.Load(strm);
                var filteredAttributeValue = (from app in xml.Descendants(node)
                                              let xAttribute = app.Attribute(attribute)
                                              where xAttribute != null
                                              select xAttribute.Value).FirstOrDefault(a => a.StartsWith(prefix));

                if (string.IsNullOrWhiteSpace(filteredAttributeValue))
                {
                    return string.Empty;
                }

                return filteredAttributeValue;
            }
        }
    }
}
