LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE    := NDKBridge
LOCAL_SRC_FILES := NDKBridge.h

include $(BUILD_SHARED_LIBRARY)
