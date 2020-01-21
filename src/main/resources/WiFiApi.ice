module WiFiApi
{
    interface ApiHandle
    {
        void disconnectAll();
        void terminateApi();
    };

    class JAccessPoint
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
};
