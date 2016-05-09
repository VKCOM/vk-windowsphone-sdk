using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK.WindowsPhone.SDK
{
    public enum LoginType
    {
        WebView,
        VKApp,

#if !SILVERLIGHT
		/// <summary>
		/// Use WebAuthenticationBroker to authenticate VK user by OAuth provider.
		/// </summary>
		WebAuthBroker
#endif
	}
}
