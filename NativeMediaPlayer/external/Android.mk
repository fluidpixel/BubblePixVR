LOCAL_PATH:= $(call my-dir)
 
include $(CLEAR_VARS)
LOCAL_MODULE:= libavcodec
LOCAL_SRC_FILES:= out/$(TARGET_ARCH_ABI)/lib/libavcodec-55.so
LOCAL_EXPORT_C_INCLUDES := $(LOCAL_PATH)/out/$(TARGET_ARCH_ABI)/include
include $(PREBUILT_SHARED_LIBRARY)
 
include $(CLEAR_VARS)
LOCAL_MODULE:= libavformat
LOCAL_SRC_FILES:= out/$(TARGET_ARCH_ABI)/lib/libavformat-55.so
LOCAL_EXPORT_C_INCLUDES := $(LOCAL_PATH)/out/$(TARGET_ARCH_ABI)/include
include $(PREBUILT_SHARED_LIBRARY)
 
include $(CLEAR_VARS)
LOCAL_MODULE:= libswscale
LOCAL_SRC_FILES:= out/$(TARGET_ARCH_ABI)/lib/libswscale-2.so
LOCAL_EXPORT_C_INCLUDES := $(LOCAL_PATH)/out/$(TARGET_ARCH_ABI)/include
include $(PREBUILT_SHARED_LIBRARY)
 
include $(CLEAR_VARS)
LOCAL_MODULE:= libavutil
LOCAL_SRC_FILES:= out/$(TARGET_ARCH_ABI)/lib/libavutil-52.so
LOCAL_EXPORT_C_INCLUDES := $(LOCAL_PATH)/out/$(TARGET_ARCH_ABI)/include
include $(PREBUILT_SHARED_LIBRARY)
 
include $(CLEAR_VARS)
LOCAL_MODULE:= libavfilter
LOCAL_SRC_FILES:= out/$(TARGET_ARCH_ABI)/lib/libavfilter-3.so
LOCAL_EXPORT_C_INCLUDES := $(LOCAL_PATH)/out/$(TARGET_ARCH_ABI)/include
include $(PREBUILT_SHARED_LIBRARY)
 
include $(CLEAR_VARS)
LOCAL_MODULE:= libwsresample
LOCAL_SRC_FILES:= out/$(TARGET_ARCH_ABI)/lib/libswresample-0.so
LOCAL_EXPORT_C_INCLUDES := $(LOCAL_PATH)/out/$(TARGET_ARCH_ABI)/include
include $(PREBUILT_SHARED_LIBRARY)
