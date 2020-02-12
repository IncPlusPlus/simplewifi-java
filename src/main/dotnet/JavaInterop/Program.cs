using System;
using SimpleWifi;
using System.Collections.Generic;
using System.Linq;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;
using System.Reflection;
using WlanInterface = SimpleWifi.Win32.WlanInterface;
using System.Threading.Tasks;
using Grpc.Core;
using Wifistuff;

namespace JavaInterop
{
    class WiFiApiImpl : WiFiApi.WiFiApiBase
    {
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> Testing(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        }
    }
    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { WiFiApi.BindService(new WiFiApiImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }

    //public class WlanInterfaceImpl : WiFiApi.WlanInterfaceDisp_
    //{
    //    private WlanInterface _wlanInterface;

    //    public WlanInterfaceImpl(WlanInterface wlanInterface)
    //    {
    //        this._wlanInterface = wlanInterface;
    //    }

    //    public override void scan(Current current = null)
    //    {
    //        _wlanInterface.Scan();
    //    }
    //}
    
    //public class WiFiApiI : WiFiApi.ApiHandleDisp_
    //{
    //    private Wifi wifi = new Wifi();
    //    private Communicator communicator;
    //    private bool? _scanSuccessful = null;
    //    public string _lastFailReason="";

    //    public WiFiApiI(Communicator communicator)
    //    {
    //        this.communicator = communicator;
    //    }

    //    public override WlanInterfacePrx[] getWlanInterfaces(Current current = null)
    //    {
    //        var interfaces = wifi.Interfaces();
    //        WlanInterfacePrx[] wlanInterfaces = new WlanInterfacePrx[interfaces.Count()];
    //        // WlanInterfacePrxHelper.uncheckedCast(new WlanInterfaceImpl(null))
    //        for (int i = 0; i < wlanInterfaces.Length; i++)
    //        {
    //            wlanInterfaces[i] =
    //                WlanInterfacePrxHelper.checkedCast(
    //                    current.adapter.addWithUUID(new WlanInterfaceImpl(interfaces.ElementAt(i))));
    //        }
    //        return wlanInterfaces;
    //    }

    //    // public void scanAll(Current current = null)
    //    // {
    //    //     _scanSuccessful = null;
    //    //     foreach (WlanInterface wlanInterface in wifi.Interfaces())
    //    //     {
    //    //         try
    //    //         {
    //    //             wlanInterface.WlanNotification += WlanNotificationChanged;
    //    //             wlanInterface.Scan();
    //    //             while (_scanSuccessful == null)
    //    //             {
    //    //             }
    //    //
    //    //             wlanInterface.WlanNotification -= WlanNotificationChanged;
    //    //             if (_scanSuccessful == false)
    //    //             {
    //    //                 return new MessageBool(false,_lastFailReason);
    //    //             }
    //    //         }
    //    //         catch (Exception e)
    //    //         {
    //    //             _lastFailReason = e.Message;
    //    //             return new MessageBool(false,_lastFailReason);
    //    //         }
    //    //     }
    //    //     return new MessageBool(true,_lastFailReason);
    //    // }

    //    public override void disconnectAll(Current current = null)
    //    {
    //        wifi.Disconnect();
    //    }

    //    public override JAccessPointPrx[] ListAPsDetail(Current current = null)
    //    {
    //        var accessPoints = List();
    //        WiFiApi.JAccessPointPrx[] APList = new WiFiApi.JAccessPointPrx[accessPoints.Count()];

    //        for (int i = 0; i < APList.Length; i++)
    //        {
    //            APList[i] = JAccessPointPrxHelper.checkedCast(
    //                current.adapter.addWithUUID(new JavaInterop.WiFiApiI.JAccessPoint(accessPoints.ElementAt(i))));
    //        }
    //        return APList;
    //    }
        
    //    private void WlanNotificationChanged(WlanNotificationData e)
    //    {
    //        if (e.NotificationCode.Equals(WlanNotificationCodeAcm.ScanComplete))
    //        {
    //            _scanSuccessful = true;
    //        }
    //        else if(e.NotificationCode.Equals(WlanNotificationCodeAcm.ScanFail))
    //        {
    //            _lastFailReason = e.NotificationCode.ToString();
    //            _scanSuccessful = false;
    //        }
    //    }

    //    public override void terminateApi(Current current = null)
    //    {
    //        communicator.shutdown();
    //        //Environment.Exit(0);
    //    }

    //    private IEnumerable<AccessPoint> List()
    //    {
    //        IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints().OrderByDescending(ap => ap.SignalStrength);
    //        return accessPoints;
    //    }

    //    public class JAccessPoint : WiFiApi.JAccessPointDisp_
    //    {
    //        private AccessPoint thisAP;
    //        private string interfaceName;
    //        private string name;
    //        private int signalStrength;
    //        private string authAlgorithm;
    //        private string cipherAlgorithm;
    //        private string bssType;
    //        private bool connectable;
    //        private string wlanNotConnectableReason;

    //        internal JAccessPoint(AccessPoint thisAP)
    //        {
    //            this.thisAP = thisAP;
    //            WlanInterface wlanInterface = thisAP.GetFieldValue<WlanInterface>("_interface");
    //            WlanAvailableNetwork network = thisAP.GetFieldValue<WlanAvailableNetwork>("_network");
    //            this.interfaceName = wlanInterface.InterfaceName;
    //            this.name = thisAP.Name;
    //            this.signalStrength = (int) thisAP.SignalStrength;
    //            this.authAlgorithm = Enum.GetName(typeof(Dot11AuthAlgorithm), network.dot11DefaultAuthAlgorithm);
    //            this.cipherAlgorithm = Enum.GetName(typeof(Dot11CipherAlgorithm), network.dot11DefaultCipherAlgorithm);
    //            this.bssType = Enum.GetName(typeof(Dot11BssType), network.dot11BssType);
    //            this.connectable = network.networkConnectable;
    //            this.wlanNotConnectableReason = Enum.GetName(typeof(WlanReasonCode), network.wlanNotConnectableReason);
    //        }

    //        public override bool connect(Current current = null)
    //        {
    //            //For when the profile already exists or the network is open
    //            AuthRequest auth = new AuthRequest(thisAP);
    //            return this.thisAP.Connect(auth);
    //        }

    //        public override bool connectWithAuth(AuthRequestStruct authRequest, Current current = null)
    //        {
    //            AuthRequest auth = new AuthRequest(thisAP);
    //            auth.Password = authRequest.password;
    //            auth.Username = authRequest.username;
    //            auth.Domain = authRequest.domain;
    //            return this.thisAP.Connect(auth);
    //        }

    //        public override string getName(Current current = null)
    //        {
    //            return this.name;
    //        }

    //        public override int getSignalStrength(Current current = null)
    //        {
    //            return this.signalStrength;
    //        }

    //        public override string getInterfaceName(Current current = null)
    //        {
    //            return this.interfaceName;
    //        }

    //        public override bool isConnectable(Current current = null)
    //        {
    //            return this.connectable;
    //        }

    //        public override string getWlanNotConnectableReason(Current current = null)
    //        {
    //            return this.wlanNotConnectableReason;
    //        }

    //        public override string getAuthAlgorithm(Current current = null)
    //        {
    //            return this.authAlgorithm;
    //        }

    //        public override string getCipherAlgorithm(Current current = null)
    //        {
    //            return this.cipherAlgorithm;
    //        }

    //        public override string getBssType(Current current = null)
    //        {
    //            return this.bssType;
    //        }

    //        public override string getProfileXML(Current current = null)
    //        {
    //            return thisAP.GetProfileXML();
    //        }
    //    }
    //}

    public static class ReflectionExtensions
    {
        //See https://stackoverflow.com/a/46488844/1687436
        public static T GetFieldValue<T>(this object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj);
        }
    }
}