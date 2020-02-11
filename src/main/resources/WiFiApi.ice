["java:package:io.github.incplusplus.simplewifijava.generated"]
module WiFiApi
{
    struct AuthRequestStruct
    {
        string password;
        string username;
        string domain;
    };

    interface JAccessPoint
    {
        bool connect();
        bool connectWithAuth(AuthRequestStruct authRequest);
        string getName();
        int getSignalStrength();
        string getInterfaceName();
        bool isConnectable();
        string getWlanNotConnectableReason();
        string getAuthAlgorithm();
        string getCipherAlgorithm();
        string getBssType();
        string getProfileXML();
    };

    sequence<JAccessPoint*> JAccessPointSeq;

    interface WlanInterface
    {
        void scan();
    };

    interface WlanInterfaceI
    {
        void connectOnInterface(WlanInterface* wint, JAccessPoint* accessPoint);
    };

    sequence<WlanInterface*> WlanInterfaceSeq;

    interface ApiHandle
    {
        WlanInterfaceSeq getWlanInterfaces();
        void disconnectAll();
        void terminateApi();
        JAccessPointSeq ListAPsDetail();
    };
};
