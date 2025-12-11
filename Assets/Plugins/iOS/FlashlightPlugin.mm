#import <AVFoundation/AVFoundation.h>

extern "C" {
    void _SetTorchEnabled(bool enabled) {
        AVCaptureDevice *device = [AVCaptureDevice defaultDeviceWithMediaType:AVMediaTypeVideo];
        
        if ([device hasTorch] && [device hasFlash]) {
            NSError *error = nil;
            [device lockForConfiguration:&error];
            
            if (!error) {
                if (enabled) {
                    [device setTorchMode:AVCaptureTorchModeOn];
                } else {
                    [device setTorchMode:AVCaptureTorchModeOff];
                }
                [device unlockForConfiguration];
            }
        }
    }
    
    bool _IsTorchAvailable() {
        AVCaptureDevice *device = [AVCaptureDevice defaultDeviceWithMediaType:AVMediaTypeVideo];
        return [device hasTorch] && [device hasFlash];
    }
}
