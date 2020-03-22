using System;
using SimpleWifi;
using System.Collections.Generic;
using System.Linq;
using SimpleWifi.Win32.Interop;
using System.Reflection;
using WlanInterface = SimpleWifi.Win32.WlanInterface;
using System.Threading.Tasks;
using Grpc.Core;
using Wifistuff;
using Google.Protobuf.WellKnownTypes;
using Enum = System.Enum;

namespace JavaInterop
{
    class WiFiApiImpl : WiFiApi.WiFiApiBase
    {
        private readonly Wifi _wifi = new Wifi();

        public override Task<JAccessPointSeq> ListAll(Empty request, ServerCallContext context)
        {
            IEnumerable<JAccessPoint> accessPoints = _wifi.GetAccessPoints().OrderByDescending(ap => ap.SignalStrength)
                .Select(Translate);
            JAccessPointSeq seq = new JAccessPointSeq();
            seq.AccessPoints.AddRange(accessPoints);
            return Task.FromResult(seq);
        }

        public override Task<GenericMessage> ConnectWithAuth(ConnectionRequest request, ServerCallContext context)
        {
            AccessPoint accessPoint =
                _wifi.GetAccessPoints().First(ap => ap.Name.Equals(request.AccessPoint.Name));
            SimpleWifi.AuthRequest auth = new SimpleWifi.AuthRequest(accessPoint)
            {
                Password = request.AuthRequest.Password,
                Domain = request.AuthRequest.Domain,
                Username = request.AuthRequest.Username
            };
            return Task.FromResult(new GenericMessage {Result = accessPoint.Connect(auth)});
        }

        public override Task<GenericMessage> EnsureApiAlive(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new GenericMessage());
        }

        public static Wifistuff.WlanInterface Translate(WlanInterface wlanInterface)
        {
            Wifistuff.WlanInterface result = new Wifistuff.WlanInterface
            {
                State = (Wifistuff.WlanInterface.Types.WlanInterfaceState) Enum.Parse(typeof(WlanInterfaceState),
                    wlanInterface.InterfaceState.ToString())
            };
            return result;
        }

        public static JAccessPoint Translate(AccessPoint thisAp)
        {
            WlanInterface wlanInterface = thisAp.GetFieldValue<WlanInterface>("_interface");
            WlanAvailableNetwork network = thisAp.GetFieldValue<WlanAvailableNetwork>("_network");
            JAccessPoint customAp = new JAccessPoint
            {
                //TODO: Figure out why this line is so damn costly (and maybe replace it with interface Guid)
                InterfaceName = wlanInterface.InterfaceName,
                Name = thisAp.Name,
                SignalStrength = (int) thisAp.SignalStrength,
                AuthAlgorithm = Enum.GetName(typeof(Dot11AuthAlgorithm), network.dot11DefaultAuthAlgorithm),
                CipherAlgorithm = Enum.GetName(typeof(Dot11CipherAlgorithm), network.dot11DefaultCipherAlgorithm),
                BssType = Enum.GetName(typeof(Dot11BssType), network.dot11BssType),
                Connectable = network.networkConnectable,
                WlanNotConnectableReason = Enum.GetName(typeof(WlanReasonCode), network.wlanNotConnectableReason)
            };
            return customAp;
        }
    }

    class WlanInterfaceApiImpl : WlanInterfaceApi.WlanInterfaceApiBase
    {
        private readonly Wifi _wifi = new Wifi();

        public override Task<WlanInterfaceSeq> GetWlanInterfaces(Empty request, ServerCallContext context)
        {
            var interfaces = _wifi.Interfaces();
            WlanInterfaceSeq sequence = new WlanInterfaceSeq();
            Wifistuff.WlanInterface[] wlanInterfaces = new Wifistuff.WlanInterface[interfaces.Count()];
            for (int i = 0; i < wlanInterfaces.Length; i++)
            {
                wlanInterfaces[i] = WiFiApiImpl.Translate(interfaces.ElementAt(i));
            }
            sequence.Interfaces.AddRange(wlanInterfaces);
            return Task.FromResult(sequence);
        }

        public override Task<JAccessPointSeq> GetAccessPoints(Wifistuff.WlanInterface request,
            ServerCallContext context)
        {
            IEnumerable<JAccessPoint> accessPoints = GetWlanInterfaceById(request.InterfaceGuid).GetAccessPoints(_wifi)
                .OrderByDescending(ap => ap.SignalStrength)
                .Select(WiFiApiImpl.Translate);
            JAccessPointSeq seq = new JAccessPointSeq();
            seq.AccessPoints.AddRange(accessPoints);
            return Task.FromResult(seq);
        }

        public override Task<GenericMessage> Scan(Wifistuff.WlanInterface request, ServerCallContext context)
        {
            GenericMessage result = new GenericMessage();
            WlanNotifSubscriber subscriber = new WlanNotifSubscriber()
                {InterfaceGuid = Guid.Parse(request.InterfaceGuid)};
            // subscriber._interfaceGuid = Guid.Parse(request.InterfaceGuid);
            //Get the interface matching the one described by the request (Guid should match)
            WlanInterface wlanInterface = GetWlanInterfaceById(request.InterfaceGuid);

            wlanInterface.WlanNotification += subscriber.WlanNotificationChanged;
            try
            {
                wlanInterface.Scan();
                while (subscriber.ScanSuccessful == null)
                {
                }

                result.Result = (bool) subscriber.ScanSuccessful;
                result.Message = subscriber.LastFailReason;
            }
            catch (Exception e)
            {
                subscriber.ScanSuccessful = false;
                subscriber.LastFailReason = e.Message;
            }

            wlanInterface.WlanNotification -= subscriber.WlanNotificationChanged;
            return Task.FromResult(result);
        }

        protected WlanInterface GetWlanInterfaceById(string id)
        {
            return _wifi.Interfaces().First(anInterface =>
                anInterface.GetFieldValue<WlanInterfaceInfo>("info")
                    .interfaceGuid.Equals(Guid.Parse(id)));
        }

        private class WlanNotifSubscriber
        {
            internal bool? ScanSuccessful { get; set; }
            internal string LastFailReason { get; set; }
            internal Guid InterfaceGuid { get; set; }

            public void WlanNotificationChanged(WlanNotificationData e)
            {
                if (e.NotificationCode.Equals(WlanNotificationCodeAcm.ScanComplete))
                {
                    ScanSuccessful = true;
                }
                else if (e.NotificationCode.Equals(WlanNotificationCodeAcm.ScanFail))
                {
                    LastFailReason = e.NotificationCode.ToString();
                    ScanSuccessful = false;
                }
            }
        }
    }

    public static class ReflectionExtensions
    {
        //See https://stackoverflow.com/a/46488844/1687436
        public static T GetFieldValue<T>(this object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T) field?.GetValue(obj);
        }

        //See https://stackoverflow.com/a/39076814/1687436
        public static object CreatePrivateClassInstance(System.Type type, object[] parameters)
        {
            return type.GetConstructors()[0].Invoke(parameters);
        }

        /// <summary>
        /// Grabs the access points seen ONLY by this interface. This is essentially just <see cref="Wifi.Scan"/>
        /// but without the interface iteration.
        /// </summary>
        /// <param name="wlanIface"></param>
        /// <param name="wifiInstance"></param>
        /// <param name="bRescan"></param>
        /// <returns></returns>
        public static List<AccessPoint> GetAccessPoints(this WlanInterface wlanIface, Wifi wifiInstance,
            bool bRescan = true)
        {
            List<AccessPoint> accessPoints = new List<AccessPoint>();
            if (wifiInstance.NoWifiAvailable)
                return accessPoints;

            if (bRescan && (DateTime.Now - wifiInstance.GetFieldValue<DateTime>("_lastScanned") >
                            TimeSpan.FromSeconds(60)))
                wifiInstance.Scan();

            WlanAvailableNetwork[] rawNetworks = wlanIface.GetAvailableNetworkList(0);
            List<WlanAvailableNetwork> networks = new List<WlanAvailableNetwork>();

            // Remove network entries without profile name if one exist with a profile name.
            foreach (WlanAvailableNetwork network in rawNetworks)
            {
                bool hasProfileName = !string.IsNullOrEmpty(network.profileName);
                bool anotherInstanceWithProfileExists =
                    rawNetworks.Where(n => n.Equals(network) && !string.IsNullOrEmpty(n.profileName)).Any();

                if (!anotherInstanceWithProfileExists || hasProfileName)
                    networks.Add(network);
            }

            foreach (WlanAvailableNetwork network in networks)
            {
                //see https://stackoverflow.com/questions/708952/how-to-instantiate-an-object-with-a-private-constructor-in-c/39076814#comment65026579_708976
                // AccessPoint ap = Activator.CreateInstance(typeof(AccessPoint), BindingFlags.Instance | BindingFlags.NonPublic,null,new object[]{wlanIface,network},null) as AccessPoint;
                AccessPoint ap =
                    CreatePrivateClassInstance(typeof(AccessPoint), new object[] {wlanIface, network}) as AccessPoint;
                accessPoints.Add(ap);
            }

            return accessPoints;
        }
    }

    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = {WiFiApi.BindService(new WiFiApiImpl())},
                Ports = {new ServerPort("localhost", Port, ServerCredentials.Insecure)}
            };
            server.Start();

            Console.WriteLine("gRPC server listening on port " + Port);
            Console.WriteLine("Enter a newline to stop the server...");
            Console.ReadLine();

            server.ShutdownAsync().Wait();
        }
    }
}