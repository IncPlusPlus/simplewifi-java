["java:package:io.github.incplusplus.simplewifijava.generated"]
module WiFiApi
{
    class JAccessPoint
    {
        ["protected"] string name;
        ["protected"] int signalStrength;
        ["protected"] string interfaceName;
        ["protected"] bool connectable;
        ["protected"] string wlanNotConnectableReason;
        ["protected"] string authAlgorithm;
        ["protected"] string cipherAlgorithm;
        ["protected"] string bssType;
    };

    sequence<JAccessPoint> JAccessPointSeq;

    interface ApiHandle
    {
        void disconnectAll();
        void terminateApi();
        JAccessPointSeq ListAPsDetail();
    };
};
