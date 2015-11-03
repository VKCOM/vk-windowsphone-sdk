using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Phone.Controls;
using VK.WindowsPhone.SDK;
using VK.WindowsPhone.SDK.API;
using VK.WindowsPhone.SDK.API.Model;
using VK.WindowsPhone.SDK.Util;
using System.Windows.Media.Imaging;
using VK.WindowsPhone.SDK.Pages;
using System.IO;

namespace SDK_Sample
{
    public partial class MainPage : PhoneApplicationPage
    {
        private List<String> _scope = new List<string> { VKScope.FRIENDS, VKScope.WALL, VKScope.PHOTOS, VKScope.AUDIO };

        public MainPage()
        {
            InitializeComponent();
            AuthorizeButton.Click += AuthorizeButtonOnClick;
            AuthorizeButton2.Click += AuthorizeButton2OnClick;

            // VKSDK initialization is in App.xaml.cs

            VKSDK.AccessTokenReceived += (sender, args) =>
            {
                UpdateUIState();
            };

            VKSDK.CaptchaRequest = CaptchaRequest;

            UpdateUIState();
        }

        private void CaptchaRequest(VKCaptchaUserRequest captchaUserRequest, Action<VKCaptchaUserResponse> action)
        {
            this.Focus();

            if (captchaRequestControl != null)
            {
                captchaRequestControl.ShowCaptchaRequest(captchaUserRequest, action);
            }
        }

        private void AuthorizeButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            VKSDK.Authorize(_scope, false, false);
        }


        private void AuthorizeButton2OnClick(object sender, RoutedEventArgs e)
        {
            VKSDK.Authorize(_scope, false, false, LoginType.VKApp);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            UpdateUIState();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            VKSDK.Logout();

            UpdateUIState();
        }

        private void UpdateUIState()
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

        private void TriggerCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            var request = new VKRequest(new VKRequestParameters("captcha.force"));

            request.Dispatch<Object>((res) => { }, (json) => new Object());

            VKRequest.Dispatch<object>(
                new VKRequestParameters("captcha.force"),
                (res) =>
                { },
                (json) => new object());
        }

        private void Publish_Click(object sender, RoutedEventArgs e)
        {
            var rs = Application.GetResourceStream(new Uri("TestImage.jpg", UriKind.Relative));
            Stream imageStream = rs.Stream;

            var inputData = new VKPublishInputData
            {
                Text = "В Доме Зингера",
                Image = imageStream,
                ExternalLink = new VKPublishInputData.VKLink
                {
                    Title = "VK",
                    Subtitle = "VKontakte",
                    Uri = "http://VK.com"
                }
            };

            VKSDK.Publish(inputData);
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

            VKRequest.Dispatch<VKList<VKAudio>>(new VKRequestParameters("audio.get"), (res) => { });
        }
    }
}