using Ice;
using System;
using SimpleWifi;
using System.Collections.Generic;
using System.Linq;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;
using System.Reflection;

namespace JavaInterop
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                using (Ice.Communicator communicator = Ice.Util.initialize(ref args))
                {
                    var wifiApiAdapter =
                        communicator.createObjectAdapterWithEndpoints("SimpleWiFiAdapter", "default -h localhost -p 10001");
                    wifiApiAdapter.add(new WiFiApiI(communicator), Ice.Util.stringToIdentity("SimpleWiFi"));
                    wifiApiAdapter.activate();
                    communicator.waitForShutdown();
                }
            }
            catch (System.Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }
            return 0;
        }
    }
    public class WiFiApiI : WiFiApi.ApiHandleDisp_
    {
        private Wifi wifi = new Wifi();
        private Communicator communicator;

        public WiFiApiI(Communicator communicator)
        {
            this.communicator = communicator;
        }

        public override void disconnectAll(Current current = null)
        {
            wifi.Disconnect();
        }

        public override WiFiApi.JAccessPoint[] ListAPsDetail(Current current = null)
        {
            var accessPoints = List();
            WiFiApi.JAccessPoint[] APList = new WiFiApi.JAccessPoint[accessPoints.Count()];

            for (int i = 0; i < APList.Length; i++)
            {
                APList[i] = new JavaInterop.WiFiApiI.JAccessPoint(accessPoints.ElementAt(i));
            }
            return APList;
        }

        public override void terminateApi(Current current = null)
        {
            communicator.shutdown();
            //Environment.Exit(0);
        }

        private IEnumerable<AccessPoint> List()
        {
            IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints().OrderByDescending(ap => ap.SignalStrength);
            return accessPoints;
        }

        public class JAccessPoint : WiFiApi.JAccessPoint
        {
            internal JAccessPoint(AccessPoint thisAP)
            {
                WlanInterface wlanInterface = thisAP.GetFieldValue<WlanInterface>("_interface");
                WlanAvailableNetwork network = thisAP.GetFieldValue<WlanAvailableNetwork>("_network");
                InterfaceName = wlanInterface.InterfaceName;
                name = thisAP.Name;
                signalStrength = (int)thisAP.SignalStrength;
                AuthAlgorithm = Enum.GetName(typeof(Dot11AuthAlgorithm), network.dot11DefaultAuthAlgorithm);
                CipherAlgorithm = Enum.GetName(typeof(Dot11CipherAlgorithm), network.dot11DefaultCipherAlgorithm);
                BssType = Enum.GetName(typeof(Dot11BssType), network.dot11BssType);
                connectable = network.networkConnectable;
                wlanNotConnectableReason = Enum.GetName(typeof(WlanReasonCode), network.wlanNotConnectableReason);

            }
        }
    }

    //See https://stackoverflow.com/a/46488844/1687436
    public static class ReflectionExtensions
    {
        public static T GetFieldValue<T>(this object obj, string name)
        {
            // Set the flags so that private and public fields from instances will be found
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var field = obj.GetType().GetField(name, bindingFlags);
            return (T)field?.GetValue(obj);
        }
    }
}