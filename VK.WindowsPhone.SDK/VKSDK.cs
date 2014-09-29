using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using VK.WindowsPhone.SDK.API;
using VK.WindowsPhone.SDK.Pages;
using VK.WindowsPhone.SDK.Util;
using System.Net;

namespace VK.WindowsPhone.SDK
{
    public class VKSDK
    {
        public const String SDK_VERSION = "0.9";
        public const String API_VERSION = "5.21";
     
        /// <summary>
        /// SDK instance
        /// </summary>
        internal static VKSDK Instance { get; private set; }

        /// <summary>
        /// Access token for API-requests
        /// </summary>
        protected VKAccessToken AccessToken;

        /// <summary>
        /// Default SDK access token key for storing value in IsolatedStorage.
        /// You shouldn't modify this key or value directly in IsolatedStorage.
        /// </summary>
        private const String VKSDK_ACCESS_TOKEN_ISOLATEDSTORAGE_KEY = "VKSDK_ACCESS_TOKEN_DONTTOUCH";

        /// <summary>
        /// Your VK app ID. 
        /// If you don't have one, create a standalone app here: https://vk.com/editapp?act=create 
        /// </summary>
        internal String CurrentAppID;

        /// <summary>
        /// Initialize SDK
        /// </summary>
        /// <param name="appId">Your VK app ID. 
        /// If you don't have one, create a standalone app here: https://vk.com/editapp?act=create </param>
        public static void Initialize(String appId)
        {
            Logger = new DefaultLogger();

            if (String.IsNullOrEmpty(appId))
            {
                throw new Exception("VKSDK could not initialize. " +
                                    "Application ID cannot be null or empty. " +
                                    "If you don't have one, create a standalone app here: https://vk.com/editapp?act=create");
            }

            if (Instance == null)
                Instance = new VKSDK();


            

            Instance.CurrentAppID = appId;
        }

        /// <summary>
        /// Initialize SDK with custom token key (e.g. saved from other source or for some test reasons)
        /// </summary>
        /// <param name="appId">Your VK app ID. 
        /// If you don't have one, create a standalone app here: https://vk.com/editapp?act=create </param>
        /// <param name="token">Custom-created access token</param>
        public static void Initialize(String appId, VKAccessToken token)
        {
            Initialize(appId);
            Instance.AccessToken = token;
            Instance.PerformTokenCheck(token, true);
        }

        public static Action<VKCaptchaUserRequest, Action<VKCaptchaUserResponse>> CaptchaRequest { private get; set; }

        //    public static Action<ValidationUserRequest, Action<ValidationUserResponse>> ValidationRequest { private get; set; }


    
        /// <summary>
        /// Invokes when existing token has expired
        /// </summary>
        public static event EventHandler<VKAccessTokenExpiredEventArgs> AccessTokenExpired = delegate { };

        /// <summary>
        /// Invokes when user authorization has been canceled
        /// </summary>
        public static event EventHandler<VKAccessDeniedEventArgs> AccessDenied = delegate { };

        /// <summary>
        /// Invokes when new access token has been received
        /// </summary>
        public static event EventHandler<VKAccessTokenReceivedEventArgs> AccessTokenReceived = delegate { };

        /// <summary>
        /// Invokes when predefined token has been received and accepted
        /// </summary>
        public static event EventHandler<VKAccessTokenAcceptedEventArgs> AccessTokenAccepted = delegate { };

        /// <summary>
        /// Invokes when access token has been renewed (e.g. user passed validation)
        /// </summary>
        public static event EventHandler<AccessTokenRenewedEventArgs> AccessTokenRenewed = delegate { };

        public static IVKLogger Logger;

        /// <summary>
        /// Starts authorization process. Opens and requests for access if VK App is installed. 
        /// Otherwise SDK will navigate current app to SDK navigation page and start OAuth in WebBrowser.
        /// </summary>
        /// <param name="scopeList">List of permissions for your app</param>
        /// <param name="revoke">If true user will be allowed to logout and change user</param>
        /// <param name="forceOAuth">SDK will use only OAuth authorization via WebBrowser</param>
        public static void Authorize(List<String> scopeList, bool revoke = false, bool forceOAuth = false)
        {
            try
            {
                CheckConditions();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return;
            }

            if (scopeList == null)
                scopeList = new List<string>();

            // Force OFFLINE scope using
            if (!scopeList.Contains(VKScope.OFFLINE))
                scopeList.Add(VKScope.OFFLINE);


            RootFrame.Navigate(new Uri(string.Format("/VK.WindowsPhone.SDK;component/Pages/VKLoginPage.xaml?Scopes={0}&Revoke={1}", string.Join(",", scopeList), revoke), UriKind.Relative));
        }

        public static void Publish(VKPublishInputData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            VKParametersRepository.SetParameterForId(VKPublishPage.INPUT_PARAM_ID, data);

            RootFrame.Navigate(new Uri(string.Format("/VK.WindowsPhone.SDK;component/Pages/VKPublishPage.xaml"), UriKind.Relative));
        }

        private enum CheckTokenResult
        {
            None,
            Success,
            Error
        };

        private static void CheckConditions()
        {
            if (Instance == null)
                throw new Exception("VK SDK is not initialized. Use VKSDK.Initialize method first");

            if (Application.Current.RootVisual as Frame == null)
                throw new Exception("Application.Current.RootVisual is supposed to be PhoneApplicationFrame");
        }

        internal static PhoneApplicationFrame RootFrame
        {
            get { return (PhoneApplicationFrame)Application.Current.RootVisual; }
        }

        /// <summary>
        /// Check new access token and assign as instance token 
        /// </summary>
        /// <param name="tokenParams">Params of token</param>
        /// <param name="isTokenBeingRenewed">Flag indicating token renewal</param>
        /// <returns>Success if token has been assigned or error</returns>
        private static CheckTokenResult CheckAndSetToken(Dictionary<String, String> tokenParams, bool isTokenBeingRenewed)
        {
            var token = VKAccessToken.TokenFromParameters(tokenParams);
            if (token == null || token.AccessToken == null)
            {
                if (tokenParams.ContainsKey(VKAccessToken.SUCCESS))
                    return CheckTokenResult.Success;

                var error = new VKError { error_code = (int)VKResultCode.UserAuthorizationFailed };

                return CheckTokenResult.Error;                
            }
            else
            {
                SetAccessToken(token, isTokenBeingRenewed);
                return CheckTokenResult.Success;
            }

            
        }

        /// <summary>
        /// Save API access token in IsolatedStorage with default key.
        /// </summary>
        /// <param name="token">Access token to be used for API requests</param>
        /// <param name="renew">Is token being renewed. Raises different event handlers (AccessTokenReceived or AccessTokenRenewed)</param>
        public static void SetAccessToken(VKAccessToken token, bool renew = false)
        {
            Instance.AccessToken = token;

            if (!renew)
                AccessTokenReceived(null, new VKAccessTokenReceivedEventArgs { NewToken = token });
            else
                AccessTokenRenewed(null, new AccessTokenRenewedEventArgs { Token = token });

            Instance.AccessToken.SaveTokenToIsolatedStorage(VKSDK_ACCESS_TOKEN_ISOLATEDSTORAGE_KEY);
        }

        /// <summary>
        /// Get access token to be used for API requests.
        /// </summary>
        /// <returns>Received access token, null if user has not been authorized yet</returns>
        public static VKAccessToken GetAccessToken()
        {
            if (Instance.AccessToken != null)
            {
                if (Instance.AccessToken.IsExpired)
                    AccessTokenExpired(null, new VKAccessTokenExpiredEventArgs { ExpiredToken = Instance.AccessToken });

                return Instance.AccessToken;
            }

            return null;
        }

        /// <summary>
        /// Notify SDK that user denied login
        /// </summary>
        /// <param name="error">Description of error while authorizing user</param>
        public static void SetAccessTokenError(VKError error)
        {
            AccessDenied(null, new VKAccessDeniedEventArgs { AuthorizationError = error });
        }

        private bool PerformTokenCheck(VKAccessToken token, bool isUserDefinedToken = false)
        {
            if (token == null) return false;

            if (token.IsExpired)
            {
                AccessTokenExpired(null, new VKAccessTokenExpiredEventArgs { ExpiredToken = token });
                return false;
            }
            if (token.AccessToken != null)
            {
                if (isUserDefinedToken)
                    AccessTokenAccepted(null, new VKAccessTokenAcceptedEventArgs { Token = token });
                return true;
            }

            var error = new VKError { error_code = (int)VKResultCode.InvalidToken };
            AccessDenied(null, new VKAccessDeniedEventArgs { AuthorizationError = error });
            return false;
        }

        public static bool WakeUpSession()
        {
            var token = VKAccessToken.TokenFromIsolatedStorage(VKSDK_ACCESS_TOKEN_ISOLATEDSTORAGE_KEY);

            if (!Instance.PerformTokenCheck(token)) return false;
            Instance.AccessToken = token;
            return true;
        }

        /// <summary>
        /// Removes active token from memory and IsolatedStorage at default key.
        /// </summary>
        public static void Logout()
        {
            Instance.AccessToken = null;

            VKAccessToken.RemoveTokenInIsolatedStorage(VKSDK_ACCESS_TOKEN_ISOLATEDSTORAGE_KEY);

            VKUtil.ClearCookies();
        }

        public static bool IsLoggedIn
        {
            get { return Instance.AccessToken != null && !Instance.AccessToken.IsExpired; }
        }
      

        internal static void ProcessLoginResult(string result, bool wasValidating, Action<VKValidationResponse> validationCallback)
        {
            bool success = false;

            if (result == null)
            {
                SetAccessTokenError(new VKError { error_code = (int)VKResultCode.UserAuthorizationFailed } );
            }
            else
            {
                var tokenParams = VKUtil.ExplodeQueryString(result);
                if (CheckAndSetToken(tokenParams, wasValidating) == CheckTokenResult.Success)
                {
                    success = true;
                }
            }

            if (validationCallback != null)
            {
                validationCallback(new VKValidationResponse { IsSucceeded = success });
            }

           
        }

        internal static void InvokeCaptchaRequest(VKCaptchaUserRequest request, Action<VKCaptchaUserResponse> callback)
        {
            if (CaptchaRequest == null)
            {
                // no handlers are registered

                callback(
                    new VKCaptchaUserResponse()
                    {
                        Request = request,
                        EnteredString = string.Empty,
                        IsCancelled = true
                    });
            }
            else
            {
                VKExecute.ExecuteOnUIThread(() =>
                {
                    CaptchaRequest(request,
                                   callback);
                });
            }
        }

        internal static void InvokeValidationRequest(VKValidationRequest request, Action<VKValidationResponse> callback)
        {
            VKExecute.ExecuteOnUIThread(() =>
                {
                    VKParametersRepository.SetParameterForId("ValidationCallback", callback);
                    RootFrame.Navigate(new Uri(string.Format("/VK.WindowsPhone.SDK;component/Pages/VKLoginPage.xaml?ValidationUri={0}", HttpUtility.UrlEncode(request.ValidationUri)), UriKind.Relative));
                });
            
        }
    }
}
