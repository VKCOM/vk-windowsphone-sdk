using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VK.WindowsPhone.SDK.Util
{
    public class VKExecute
    {
        public static void ExecuteOnUIThread(Action action)
        {
            if (Deployment.Current.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(action);
            }
        }
    }
}
