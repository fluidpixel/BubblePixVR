LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE    := NativeRenderPlugin
LOCAL_SRC_FILES := NativeRenderPlugin.cpp
LOCAL_SHARED_LIBRARIES := liblog
LOCAL_LDLIBS := -llog -lz -lGLESv2

include $(BUILD_SHARED_LIBRARY)
