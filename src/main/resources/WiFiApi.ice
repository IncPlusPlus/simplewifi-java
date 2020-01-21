["java:package:generated"]
module WiFiApi
{
    struct JAccessPoint
    {
        string InterfaceName;
        string name;
        int signalStrength;
        bool connectable;
        string wlanNotConnectableReason;
        string AuthAlgorithm;
        string CipherAlgorithm;
        string BssType;
    };

    sequence<JAccessPoint> JAccessPointSeq;

    interface ApiHandle
    {
        void disconnectAll();
        void terminateApi();
        JAccessPointSeq ListAPsDetail();
    };
};
