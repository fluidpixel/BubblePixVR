package com.sherif.cardboard3d.bitmaphandler;

/**
 * Mostly a tester class
 *
 */

public class VideoTester {

	public int Test() {
		return 123;
	}

	public String TestString() {
		return stringFromJNI();
	}

	public VideoTester() {

	}

	public native String stringFromJNI();

	static {
		System.loadLibrary("NDKBridge");
	}
}
