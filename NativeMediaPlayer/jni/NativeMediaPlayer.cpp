/* Bridge File for Unity to interface with plugin. Will expose the relevant
 * methods for using the C++ media player library. Also serves as the tip of
 * the abstraction iceberg that i've got going on.
 */

#include <jni.h>
#include "ffContainer.cpp"

jint JNI_OnLoad(JavaVM* vm, void* reserved) {
	LOGD("JNI attached.");
  JNIEnv* jni_env = 0;
  vm->AttachCurrentThread(&jni_env, 0);
  return (JNI_VERSION_1_6);
}

/*-----ffmpeg methods-----*/

extern "C" int PlayVideo(const char* _fname);

extern "C" void EXPORT_API UnitySetGraphicsDevice (void* _device, int _deviceType, int _eventType);

extern "C" void EXPORT_API UnityRenderEvent(int _eventID);

/*
extern void DestroyPlayer();
extern int Width();
extern int Height();
extern int Duration();
extern int FrameRate();
extern int DecodeFrame();
extern int Init();
*/
