#include <string.h>
#include <jni.h>

/*
* Simple test to start off with, just return a string to the 
* java code section of the plugin.
*/

extern "C" {
	JNIEXPORT jstring JNICALL Java_com_sherif_cardboard3d_VideoTester_stringFromJNI(JNIEnv *env, jobject thiz) {

		return env->NewStringUTF("Oh god, it works!");
	}

	int intFromJNI() {
		return 123;
	}
}