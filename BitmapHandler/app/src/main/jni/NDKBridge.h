#define LOG_TAG "NDKTester"
#include <android/log.h>
#include <string.h>
#include <jni.h>

static JavaVM* jvm;

jint JNI_OnLoad(JavaVM* vm, void* reserved) {
	JNIEnv* env = 0;
	if (vm->AttachCurrentThread(&env, jvm) != 0) {
		__android_log_print(ANDROID_LOG_ERROR, LOG_TAG, "Problem attaching to jvm thread");
	}
	jvm = vm;
	__android_log_print(ANDROID_LOG_ERROR, LOG_TAG, "jvm attached?");
	return JNI_VERSION_1_6;
}

extern "C" {
	JNIEXPORT jstring JNICALL Java_com_sherif_cardboard3d_VideoTester_stringFromJNI(JNIEnv *env, jobject thiz) {
		return env->NewStringUTF("Oh god, it works!");
	}

	jint intFromJNI() {
		JNIEnv* env;
		int res = jvm->GetEnv((void**) &env, JNI_VERSION_1_6);
		if (res == JNI_EDETACHED) {
			jvm->AttachCurrentThread(&env, 0);
			__android_log_print(ANDROID_LOG_ERROR, LOG_TAG, "Attaching to current thread");
		}

		jclass cls_JavaClass = env->FindClass("com/sherif/cardboard3d/bitmaphandler/VideoTester");
		jmethodID constructor = env->GetMethodID(cls_JavaClass, "<init>", "()V");
		jobject obj_JavaClass = env->NewObject(cls_JavaClass, constructor);
		jmethodID method = env->GetMethodID(cls_JavaClass, "Test", "()V");
		jint ret = env->CallIntMethod(obj_JavaClass, method);

		return ret;
	}
}
