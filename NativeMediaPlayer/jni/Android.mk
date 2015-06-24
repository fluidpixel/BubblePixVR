LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE    := NativeMediaPlayer
LOCAL_SRC_FILES := NativeMediaPlayer.c
LOCAL_LDLIBS := -llog -lz -lGLESv2
LOCAL_SHARED_LIBRARIES := libavformat libavcodec libavutil liblog libswscale libNativeRenderPlugin

include $(BUILD_SHARED_LIBRARY)

$(call import-add-path, ../)
$(call import-module, NativeMediaPlayer/external)

#$(call import-add-path, jni)
#$(call import-module, prebuilt)