using Ice;
using System;
using SimpleWifi;
using SimpleWifi.Win32.Interop;
using System.Collections.Generic;
using System.Linq;

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

        public override void terminateApi(Current current = null)
        {
            communicator.shutdown();
        }

        private void ListAPsDetail()
        {
            var accessPoints = List();
            JAccessPointPack pack = new JAccessPointPack();

            foreach (AccessPoint accessPoint in accessPoints)
            {
                pack.Add(new JAccessPoint(accessPoint));
            }
            Console.WriteLine(JsonConvert.SerializeObject(pack));

        }

        private IEnumerable<AccessPoint> List()
        {
            IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints().OrderByDescending(ap => ap.SignalStrength);
            return accessPoints;
        }
    }
}