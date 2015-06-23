/* Bridge File for Unity to interface with plugin. Will expose the relevant
 * methods for using the C++ media player library. Also serves as the tip of
 * the abstraction iceberg that i've got going on.
 */
#include "ffContainer.c"

#define LOG_TAG "FFPlayer"
#define LOGD(...)  __android_log_print(ANDROID_LOG_DEBUG, LOG_TAG, __VA_ARGS__)
#define LOGE(...)  __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

static JavaVM* jvm;

jint JNI_OnLoad(JavaVM* vm, void* reserved) {
	JNIEnv* env = NULL;

	(*vm)->AttachCurrentThread(vm, &env, NULL);

	jvm = vm;
	LOGD("jvm attached");
	Test();
	return (JNI_VERSION_1_6);
}

/*-----ffmpeg methods-----*/

extern int InitNativeVideo(char _fname[]) {
	JNIEnv *env;
	(*jvm)->GetEnv(jvm, (void **)&env, JNI_VERSION_1_6);

	LOGD("Attaching to current thread.");
	(*jvm)->AttachCurrentThread(jvm, &env, NULL);
	if (init == 0) {
		LOGD("Initialising player.");
		const char* str = _fname;
		return (initPlayer(str));
	}
	else {
		LOGE("Player already initialised. Destroy current player before creating a new one.");
		return (-1);
	}
	return (0);
}

extern void DestroyPlayer();
extern int Width();
extern int Height();
extern int Duration();
extern int FrameRate();
extern int DecodeFrame();
