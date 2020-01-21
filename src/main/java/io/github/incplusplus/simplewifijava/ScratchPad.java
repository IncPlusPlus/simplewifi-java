package io.github.incplusplus.simplewifijava;

import generated.WiFiApi.JAccessPoint;
import io.github.incplusplus.bigtoolbox.io.filesys.TempFile;
import generated.WiFiApi.ApiHandlePrx;

import java.io.IOException;
import java.net.URISyntaxException;
import java.util.Arrays;

public class ScratchPad {
	private static Process dotNetApp;
	private static TempFile interopExe;
	public static void main(String[] args) throws IOException, URISyntaxException, InterruptedException {
		interopExe = new TempFile("JavaInterop", "exe");
		dotNetApp = Runtime.getRuntime().exec(interopExe.getAsFile().getPath());
		try(com.zeroc.Ice.Communicator communicator = com.zeroc.Ice.Util.initialize(args))
		{
			com.zeroc.Ice.ObjectPrx apiBase = communicator.stringToProxy("SimpleWiFi:default -p 10001");
			ApiHandlePrx wifi = ApiHandlePrx.checkedCast(apiBase);
			System.out.println("Disconnecting");
			wifi.ListAPsDetail();
//			System.out.println(jAccessPoints[0].toString());
			//			wifi.disconnectAll();
			System.out.println("Disconnected");
			wifi.terminateApi();
//			dotNetApp.waitFor();
		}
	}
}
