using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VK.WindowsPhone.SDK;
using VK.WindowsPhone.SDK.API;
using VK.WindowsPhone.SDK.API.Model;
using VK.WindowsPhone.SDK.Pages;
using VK.WindowsPhone.SDK.Util;
using VK.WindowsPhone.SDK_XAML.Pages;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace SDKSample_XAML
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<String> _scope = new List<string> { VKScope.FRIENDS, VKScope.WALL, VKScope.PHOTOS, VKScope.AUDIO };

        public MainPage()
        {
            this.InitializeComponent();
            AuthorizeButton.Click += AuthorizeButtonOnClick;
            AuthorizeButton2.Click += AuthorizeButton2_Click;
            VKSDK.Initialize("4460217");

            VKSDK.AccessTokenReceived += (sender, args) =>
            {
                UpdateUIState();
            };

            VKSDK.WakeUpSession();

            VKSDK.CaptchaRequest = CaptchaRequest;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            UpdateUIState();
        }    

        private void CaptchaRequest(VKCaptchaUserRequest captchaUserRequest, Action<VKCaptchaUserResponse> action)
        {       
            new VKCaptchaRequestUserControl().ShowCaptchaRequest(captchaUserRequest, action);
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            HookupBackKeyPress(true);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            HookupBackKeyPress(false);
        }

        private void HookupBackKeyPress(bool subscribe)
        {
            if (subscribe)
            {
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            }
            else
            {
                HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            }
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (VKPopupControlBase.CurrentlyShownInstances.Count > 0)
            {
                VKLoginUserControl.CurrentlyShownInstances.Last().IsShown = false;
                e.Handled = true;
            }
        }

        private void AuthorizeButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            VKSDK.Authorize(_scope, false, false);
        }

        void AuthorizeButton2_Click(object sender, RoutedEventArgs e)
        {
            VKSDK.Authorize(_scope, false, false, LoginType.VKApp);
        }


        public void UpdateUIState()
        {
            bool isLoggedIn = VKSDK.IsLoggedIn;

            NotAuthorizedContent.Visibility = isLoggedIn ? Visibility.Collapsed : Visibility.Visible;

            AuthorizedContent.Visibility = isLoggedIn ? Visibility.Visible : Visibility.Collapsed;

            if (!isLoggedIn)
            {
                userImage.Source = null;
                userInfo.Text = "";
            }

        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            VKSDK.Logout();
            UpdateUIState();
        }

        private void TriggerCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            var request = new VKRequest(new VKRequestParameters("captcha.force"));

            request.Dispatch<Object>((res) => { }, (json) => new Object());
        }

        private void GetUserInfoButton_Click(object sender, RoutedEventArgs e)
        {
            VKRequest.Dispatch<List<VKUser>>(
               new VKRequestParameters(
                   "users.get",
                   "fields", "photo_200, city, country"),
               (res) =>
               {
                   if (res.ResultCode == VKResultCode.Succeeded)
                   {
                       VKExecute.ExecuteOnUIThread(() =>
                       {
                           var user = res.Data[0];

                           userImage.Source = new BitmapImage(new Uri(user.photo_200, UriKind.Absolute));

                           userInfo.Text = user.first_name + " " + user.last_name;
                       });
                   }
               });            
        }

        private void GetFriends_Click(object sender, RoutedEventArgs e)
        {
            VKRequest.Dispatch<VKList<VKUser>>(new VKRequestParameters("friends.get", "fields", "photo_200"),
               (res) =>
               {
                   VKExecute.ExecuteOnUIThread(() =>
                   {
                       if (res.ResultCode == VKResultCode.Succeeded && res.Data.count > 0)
                       {
                           friends.Text = "Example Friend name: " + res.Data.items[0].first_name + " " + res.Data.items[0].last_name;
                       }
                   });

               });
        }

        private void Publish_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            var proxyFolder = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            var proxyFile = await proxyFolder.GetFileAsync("WindowsStoreProxy.xml");
            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);


            try
            {
                var listing = await CurrentAppSimulator.LoadListingInformationAsync();

                var product = listing.ProductListings["product1"];                

                var results = await CurrentAppSimulator.RequestProductPurchaseAsync("product1");

                if (results.Status == ProductPurchaseStatus.Succeeded)
                {
                    VKAppPlatform.Instance.ReportInAppPurchase(new VKAppPlatform.InAppPurchaseData(results.ReceiptXml, product.FormattedPrice));
                }
            }
            catch (Exception exc)
            {
                /* Handle exception */
            }
        }
    }
}
