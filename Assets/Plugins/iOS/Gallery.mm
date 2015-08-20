//
//  Gallery_iOS.m
//  Gallery_iOS
//
//  Created by Paul on 30/07/2015.
//  Copyright (c) 2015 Fluid Pixel. All rights reserved.
//

#if ! __has_feature(objc_arc)
#error This file must be compiled with ARC. Use the -fobjc-arc compiler option
#endif

// Need to add the Photos framework, the CoreText framework and libc++ to the Xcode project

#import "Gallery.h"

#import <Photos/Photos.h>
#import <CoreLocation/CoreLocation.h>
#import <OpenGLES/ES3/gl.h>

#define REVERSE_GEOCODE_TIMEOUT (NSEC_PER_SEC * 1)

static Gallery * s_instance;
static dispatch_once_t s_token;

@implementation Gallery {
    PHFetchResult * __nullable m_panoramas;
    CLGeocoder * __nonnull m_sharedGeocoder;
    NSDictionary * __nonnull m_locations;
    NSDateFormatter * __nonnull m_dateFormatter;
}

#pragma mark - Singleton Shared Instance
+ (Gallery* __nonnull) sharedInstance;
{
    dispatch_once(&s_token, ^{
        s_instance = [Gallery new];
    });
    return s_instance;
}


#pragma mark - init / dealloc
- (instancetype) init
{
    self = [super init];
    if (self) {
        m_panoramas = NULL;
        m_sharedGeocoder = [[CLGeocoder alloc] init];
        m_locations = [NSDictionary dictionary];
        
        m_dateFormatter = [[NSDateFormatter alloc] init];
        [m_dateFormatter setDateFormat:@"dd/MM/YY HH:mm:ss"];
        
        dispatch_group_t group = dispatch_group_create();
        dispatch_group_enter(group);
        
        [PHPhotoLibrary requestAuthorization:^(PHAuthorizationStatus status) {
            if (status == PHAuthorizationStatusAuthorized) {
                PHFetchResult * panos = [PHAssetCollection fetchAssetCollectionsWithType:PHAssetCollectionTypeSmartAlbum
                                                                                 subtype:PHAssetCollectionSubtypeSmartAlbumPanoramas
                                                                                 options:nil];
                
                m_panoramas = [PHAsset fetchAssetsInAssetCollection:[panos firstObject] options:nil];
                
                if (m_panoramas) {
                    // TODO: Fill dictionary
                }
            }
            else {
                m_panoramas = NULL;
            }
            
            dispatch_group_leave(group);

        }];
        
        // messy but enures object is fully initialised on init
        dispatch_group_wait(group, DISPATCH_TIME_FOREVER);
        
        [[PHPhotoLibrary sharedPhotoLibrary] registerChangeObserver:self];
        
    }
    return self;
}


- (void) dealloc
{
    [[PHPhotoLibrary sharedPhotoLibrary] unregisterChangeObserver:self];
}


#pragma mark - PHPhotoLibraryChangeObserver
- (void) photoLibraryDidChange:(PHChange*)changeInstance;
{
    if (m_panoramas) {
        PHFetchResultChangeDetails * changes = [changeInstance changeDetailsForFetchResult:m_panoramas];
        if (changes) {
            m_panoramas = [changes fetchResultAfterChanges];
            // TODO: Fill dictionary
        }
    }
}


#pragma mark - Photo Library Interaction
- (NSUInteger) getAssetCount;
{
    return [m_panoramas count];
}


- (NSString* __nullable) getLocalIdentifierForIndex: (NSUInteger) index;
{
    if (index >= [m_panoramas count]) {
        return NULL;
    }
    PHAsset* asset = [m_panoramas objectAtIndex:index];
    return [asset localIdentifier];
}


- (PHAsset* __nullable) getAssetForLocalIdentifier: (NSString* __nonnull) localIdentifier;
{
    __block PHAsset * rv = NULL;
    [m_panoramas enumerateObjectsUsingBlock:^(id obj, NSUInteger idx, BOOL *stop) {
        PHAsset * asset = (PHAsset *)obj;
        if ([[asset localIdentifier] isEqualToString:localIdentifier]) {
            rv = asset;
            *stop = YES;
        }
    }];
    return rv;
}

- (NSString * __nonnull) getFormattedDate: (PHAsset* __nonnull) asset;
{
    return [m_dateFormatter stringFromDate:[asset creationDate]];
}

- (NSString * __nonnull) getCountryForAssetLocation: (PHAsset* __nonnull) asset;
{
    CLLocation * location = [asset location];
    NSString * key = [location description];
    
    __block NSString * country = [m_locations objectForKey:key];
    if (country) {
        return country;
    }
    
    dispatch_group_t timeout = dispatch_group_create();
    
    dispatch_group_enter(timeout);
    
    [m_sharedGeocoder reverseGeocodeLocation:location
                           completionHandler:^(NSArray *placemarks, NSError *error)
     {
         CLPlacemark * place = [placemarks firstObject];
         
         if ([place country]) {
             country = [place country];
         }
         else {
             country = @"Unknown";
         }
         dispatch_group_leave(timeout);
     }];
    
    if ( dispatch_group_wait(timeout, dispatch_time(DISPATCH_TIME_NOW, REVERSE_GEOCODE_TIMEOUT)) == 0) {
        return country;
    }
    else {
        return @"Unknown";
    }
}


@end



#pragma mark - C Utility Functions
static PHAsset* getAsset(const char* localID) {
    NSString * convLocalID = [NSString stringWithUTF8String:localID];
    return [[Gallery sharedInstance] getAssetForLocalIdentifier:convLocalID];
}

#pragma mark - C Interface
extern "C" {
    int _iOS_Gallery__GetPanoramaCount() {
        return (int)[[Gallery sharedInstance] getAssetCount];
    }
    const char* _iOS_Gallery__GetLocalID(int index) {
        return [[[Gallery sharedInstance] getLocalIdentifierForIndex:index] UTF8String];
    }
    int _iOS_Gallery__GetPanoramaWidth (const char* localID) {
        return (int)[getAsset(localID) pixelWidth];
    }
    int _iOS_Gallery__GetPanoramaHeight (const char* localID) {
        return (int)[getAsset(localID) pixelHeight];
    }
    const char* _iOS_Gallery__GetPanoramaDateTaken (const char* localID) {
        return [[[Gallery sharedInstance] getFormattedDate:getAsset(localID)] UTF8String];
    }
    const char* _iOS_Gallery__GetPanoramaCountry (const char* localID) {
        return [[[Gallery sharedInstance] getCountryForAssetLocation:getAsset(localID)] UTF8String];
    }
    void _iOS_Gallery__PanoramaToTexture (const char* localID, int gl_tex_id, int texWidth, int texHeight) {
        //PHAsset * asset = getAsset(localID);
        
        CGRect targetRectangle = CGRectMake(0.0, 0.0, (CGFloat)texWidth, (CGFloat)texHeight);
        
        PHImageRequestOptions * options = [PHImageRequestOptions new];
        [options setSynchronous:YES];
        [options setVersion:PHImageRequestOptionsVersionCurrent];
        [options setDeliveryMode:PHImageRequestOptionsDeliveryModeHighQualityFormat];
        [options setResizeMode:PHImageRequestOptionsResizeModeNone];
        [options setNetworkAccessAllowed:NO];
        
        [[PHImageManager defaultManager] requestImageForAsset: getAsset(localID)
                                                   targetSize: targetRectangle.size
                                                  contentMode: PHImageContentModeDefault
                                                      options: options
                                                resultHandler: ^(UIImage *result, NSDictionary *info)
         {
             if (result) {
                 
                 CGColorSpaceRef cs = CGColorSpaceCreateDeviceRGB();
                 CGContextRef context = CGBitmapContextCreate(NULL, texWidth, texHeight, 8, 0, cs, kCGImageAlphaPremultipliedLast);
                 CGColorSpaceRelease(cs);
                 
                 CGContextDrawImage(context, targetRectangle, result.CGImage);
                 
                 glPixelStorei(GL_UNPACK_ALIGNMENT, 1);
                 
                 glBindTexture(GL_TEXTURE_2D, (GLuint)gl_tex_id);
                 
                 glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, (GLsizei)texWidth, (GLsizei)texHeight, 0, GL_RGBA, GL_UNSIGNED_BYTE, CGBitmapContextGetData(context));
                 
                 glBindTexture(GL_TEXTURE_2D, 0);
                 
                 CGContextRelease(context);
             }
         }];
        
    }
}


