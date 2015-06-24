LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := libNativeRenderPlugin
LOCAL_SRC_FILES := $(TARGET_ARCH_ABI)/lib/libNativeRenderPlugin.so
LOCAL_EXPORT_C_INCLUDES := $(TARGET_ARCH_ABI)/include
include $(PREBUILT_SHARED_LIBRARY)