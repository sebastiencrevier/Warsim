using System;

using Newtonsoft.Json.Linq;

using PushSharp.Apple;

namespace Warsim.Core.Notifications
{
    public class ApnsService
    {
        public ApnsServiceBroker Broker { get; }

        public ApnsService()
        {
            //var config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, "push-cert.pfx", "push-cert-pwd");

            //this.Broker = new ApnsServiceBroker(config);

            //this.Broker.OnNotificationFailed += (notification, exception) => { Console.WriteLine("Notification Failed!"); };
            //this.Broker.OnNotificationSucceeded += (notification) => { Console.WriteLine("Notification Sent!"); };

            //this.Broker.Start();
        }

        public void SendPushNotification(string deviceToken, object payload)
        {
            this.Broker.QueueNotification(new ApnsNotification
            {
                DeviceToken = deviceToken,
                Payload = JObject.FromObject(payload)
            });
        }
    }
}