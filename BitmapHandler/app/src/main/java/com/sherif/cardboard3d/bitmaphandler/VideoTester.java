package com.sherif.cardboard3d.bitmaphandler;

/**
 * Mostly a tester class
 *
 */

public class VideoTester {

	public int Test() {
		return intFromJNI();
	}

	public VideoTester() {

	}

	//private native String stringFromJNI();
	private native int intFromJNI();

	static {
		System.loadLibrary("NDKBridge");
	}
}
