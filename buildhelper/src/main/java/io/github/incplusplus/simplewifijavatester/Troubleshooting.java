package io.github.incplusplus.simplewifijavatester;

import io.github.incplusplus.bigtoolbox.io.filesys.TempFile;
import io.github.incplusplus.simplewifijava.SimpleWifiJavaEntryPoint;

import java.io.IOException;
import java.net.URISyntaxException;

public class Troubleshooting {
	public static void main(String[] args) {
	
	}
	
	void temporarilyExtractExe() throws IOException, URISyntaxException, InterruptedException {
		int waitTimeSecs = 120;
		TempFile interopExe = new TempFile("JavaInterop", "exe", SimpleWifiJavaEntryPoint.class);
		System.out.println(interopExe.getAsFile().getPath());
		System.out.println("Waiting " + waitTimeSecs + " seconds...");
		Thread.sleep(1000*waitTimeSecs);
	}
}

