using SimpleWifi;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaInterop
{
	public class JAccessPoint
	{
		public string InterfaceName;
		public string name;
		public uint signalStrength;
		public bool connectable;
		public string wlanNotConnectableReason;
		public string AuthAlgorithm;
		public string CipherAlgorithm;
		public string BssType;

		internal JAccessPoint(AccessPoint thisAP)
		{
			WlanInterface wlanInterface = (WlanInterface)thisAP.GetType().GetProperty("_interface").GetValue(thisAP, null);
			WlanAvailableNetwork network = (WlanAvailableNetwork)thisAP.GetType().GetProperty("_network").GetValue(thisAP, null);
			InterfaceName = wlanInterface.InterfaceName;
			name = thisAP.Name;
			signalStrength = thisAP.SignalStrength;
			AuthAlgorithm = Enum.GetName(typeof(Dot11AuthAlgorithm), network.dot11DefaultAuthAlgorithm);
			CipherAlgorithm = Enum.GetName(typeof(Dot11CipherAlgorithm), network.dot11DefaultCipherAlgorithm);
			BssType = Enum.GetName(typeof(Dot11BssType), network.dot11BssType);
			connectable = network.networkConnectable;
			wlanNotConnectableReason = Enum.GetName(typeof(WlanReasonCode), network.wlanNotConnectableReason);

		}
	}
}
