using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK.WindowsPhone.SDK;
using VK.WindowsPhone.SDK.Util;
using Windows.ApplicationModel.Activation;

namespace VK.WindowsPhone.SDK_XAML
{
    public static class VKProtocolActivationHelper
    {
         public static void HandleProtocolLaunch(ProtocolActivatedEventArgs protocolArgs)
         {
                 var innerQueryParamsString = VKUtil.GetParamsOfQueryString(protocolArgs.Uri.ToString());
                            
                 VKSDK.ProcessLoginResult(innerQueryParamsString, false, null);
         }
    }
}
