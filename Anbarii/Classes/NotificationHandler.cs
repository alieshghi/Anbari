using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace Anbarii.Classes
{
    public static class NotificationHandler
    {
        public static void SendToMulti(List<string> tokens, string title, string body, string link = "")
        {
            try
            {
                var defaultApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(HostingEnvironment.ApplicationPhysicalPath + @"\assets\anbarii-firebase-adminsdk-rh9du-ab014d04e2.json"),
                });
                Console.WriteLine(defaultApp.Name); // "[DEFAULT]"

                // Retrieve services by passing the defaultApp variable...
                var defaultAuth = FirebaseAuth.GetAuth(defaultApp);

                // ... or use the equivalent shorthand notation
                defaultAuth = FirebaseAuth.DefaultInstance;
            }
            catch { }
            var message = new MulticastMessage()
            {
                Notification = new Notification()
                {
                    Title = title,
                    Body = body,
                },
                Webpush = new WebpushConfig()
                {
                    Headers = new Dictionary<string, string>() {
                        { "Urgency", "high" },
                    },
                    Notification = new WebpushNotification()
                    {
                        Title = title,
                        Body = body,

                    },
                    FcmOptions = new WebpushFcmOptions() { Link = link },
                },
                Tokens = tokens,
            };
            FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
            // Response is a message ID string.
        }
        public static void Send(string token, string title, string body, string link = "")
        {
            try
            {
                var defaultApp = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(HostingEnvironment.ApplicationPhysicalPath + @"\assets\anbarii-firebase-adminsdk-rh9du-ab014d04e2.json"),
                });
                Console.WriteLine(defaultApp.Name); // "[DEFAULT]"

                // Retrieve services by passing the defaultApp variable...
                var defaultAuth = FirebaseAuth.GetAuth(defaultApp);

                // ... or use the equivalent shorthand notation
                defaultAuth = FirebaseAuth.DefaultInstance;
            }
            catch { }
            var message = new Message()
            {
                Notification = new Notification()
                {
                    Title = title,
                    Body = body,
                },
                Webpush = new WebpushConfig()
                {
                    Headers = new Dictionary<string, string>() {
                        { "Urgency", "high" },
                    },
                    Notification = new WebpushNotification()
                    {
                        Title = title,
                        Body = body,

                    },
                    FcmOptions = new WebpushFcmOptions() { Link = link },
                },
                Token = token,
            };
            FirebaseMessaging.DefaultInstance.SendAsync(message);
            // Response is a message ID string.
        }

        public static bool BroadCast(string token, string title, string body, string link = "")
        {
            Send(token: token, title: title, body: body, link: link);
            return true;
        }
        public static bool MultiCast(Dictionary<string, string> tokenlinks, string title, string body)
        {
            foreach (var item in tokenlinks ?? new Dictionary<string, string>())
            {
                Send(token: item.Key, title: title, body: body,link: item.Value);
            }
            return true;

        }
    }
}