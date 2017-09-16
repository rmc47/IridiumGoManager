using IridiumGoManager.IridiumGo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IridiumGoManager
{
    class Program
    {
        private sdkClient m_Client = new sdkClient();
        private sdk_user_credentials_t m_Credentials = new sdk_user_credentials_t { userName = "guest", password = "guest" };
        static void Main(string[] args)
        {
            bool result = new Program().ConnectWithRetries(5);
        }

        private bool ConnectWithRetries(int timesToRetry)
        {
            int attempt = 0;
            while (attempt < timesToRetry)
            {
                var currentStatus = GetCallStatus();
                Console.WriteLine("Current status: {0}", currentStatus);
                switch (currentStatus)
                {
                    case InternetCallStatus.Connected:
                        return true;
                    case InternetCallStatus.Disconnected:
                        TryConnect();
                        attempt++;
                        break;
                    default:
                        Thread.Sleep(5000);
                        break;
                }
            }
            return false;
        }

        private void TryConnect()
        {
            const int TASK_INTERNET_CALL = 2;

            var connectOption = new sdk_generic_option_t { name = "set state", value = "1", dataType = "boolean" };
            //var firewallNoAllowAll = new sdk_generic_option_t { name = "Firewall allow all traffic", value = "false", dataType = "boolean" };
            var firewallAllowIridium = new sdk_generic_option_t { name = "Firewall exceptions", value = "51.255.135.163-all", dataType = "string" };
            //var firewallNoAllowDns = new sdk_generic_option_t { name = "Enable DNS forwarding", value = "false", dataType = "boolean" };
            sdk_task_request_t connectRequest = new sdk_task_request_t { taskID = TASK_INTERNET_CALL, options = new sdk_generic_option_t[] { connectOption/*, firewallNoAllowAll, firewallAllowIridium, firewallNoAllowDns */} };
            
            m_Client.performTask(m_Credentials, new sdk_task_request_t[] { connectRequest });
        }

        private InternetCallStatus GetCallStatus()
        {
            sdk_generic_option_t activeCallOption = new sdk_generic_option_t { name = "active call" };
            var response = m_Client.getStatus(m_Credentials, new sdk_generic_option_t[] { activeCallOption });
            foreach (var status in response.status)
            {
                if (status.name == "Internet connection status")
                {
                    return (InternetCallStatus)int.Parse(status.value);
                }
            }
            return InternetCallStatus.Disconnected;
        }
    }

    internal enum InternetCallStatus
    {
        Disconnected = 0,
        Dialling = 1,
        Negotiating = 2,
        Authenticating = 3,
        Connected = 4,
        Disconnecting = 5,
    }
}
