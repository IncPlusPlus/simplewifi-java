package io.github.incplusplus.simplewifijava;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.IOException;

import WiFiApi.ApiHandlePrx;
import io.github.incplusplus.bigtoolbox.io.filesys.TempFile;

public class ScratchPad {
	private static Process dotNetApp;
	private static TempFile interopExe;
	public static void main(String[] args) throws IOException {
		interopExe = new TempFile("JavaInterop", "exe");
		dotNetApp = Runtime.getRuntime().exec(interopExe.getAsFile().getPath());
		try(com.zeroc.Ice.Communicator communicator = com.zeroc.Ice.Util.initialize(args))
		{
			com.zeroc.Ice.ObjectPrx apiBase = communicator.stringToProxy("SimpleWiFi:default -p 10001");
			ApiHandlePrx wifi = ApiHandlePrx.checkedCast(apiBase);
			System.out.println("Disconnecting");
			wifi.disconnectAll();
			System.out.println("Disconnected");
		}
	}
}
