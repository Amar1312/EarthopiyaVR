package com.unity.flashlight;

import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.hardware.camera2.CameraAccessException;
import android.hardware.camera2.CameraCharacteristics;
import android.hardware.camera2.CameraManager;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

public class CameraFlashlightPlugin {
    private static final String TAG = "CameraFlashPlugin";
    private static CameraManager cameraManager;
    private static String flashCameraId = null;
    private static boolean isFlashOn = false;
    private static Activity activity;
    private static boolean isInitialized = false;
    
    public static boolean initialize(Activity act) {
        activity = act;
        
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M) {
            Log.e(TAG, "Requires Android 6.0+ (API 23+)");
            return false;
        }
        
        try {
            if (!activity.getPackageManager().hasSystemFeature(PackageManager.FEATURE_CAMERA_FLASH)) {
                Log.e(TAG, "Device does not have flash");
                return false;
            }
            
            cameraManager = (CameraManager) activity.getSystemService(Context.CAMERA_SERVICE);
            
            if (cameraManager == null) {
                Log.e(TAG, "CameraManager is null");
                return false;
            }
            
            String[] cameraIds = cameraManager.getCameraIdList();
            Log.d(TAG, "Found " + cameraIds.length + " cameras");
            
            for (String cameraId : cameraIds) {
                CameraCharacteristics characteristics = cameraManager.getCameraCharacteristics(cameraId);
                Boolean hasFlash = characteristics.get(CameraCharacteristics.FLASH_INFO_AVAILABLE);
                Integer facing = characteristics.get(CameraCharacteristics.LENS_FACING);
                
                Log.d(TAG, "Camera " + cameraId + ": hasFlash=" + hasFlash + ", facing=" + facing);
                
                if (hasFlash != null && hasFlash) {
                    if (facing != null && facing == CameraCharacteristics.LENS_FACING_BACK) {
                        flashCameraId = cameraId;
                        Log.i(TAG, "Selected back camera " + cameraId + " for flash");
                        break;
                    } else if (flashCameraId == null) {
                        flashCameraId = cameraId;
                        Log.i(TAG, "Selected camera " + cameraId + " for flash (fallback)");
                    }
                }
            }
            
            if (flashCameraId == null) {
                Log.e(TAG, "No camera with flash available");
                return false;
            }
            
            isInitialized = true;
            Log.i(TAG, "Flashlight initialized successfully");
            return true;
            
        } catch (Exception e) {
            Log.e(TAG, "Initialization error: " + e.getMessage());
            e.printStackTrace();
            return false;
        }
    }
    
    public static boolean toggleFlashlight() {
        if (!isInitialized) {
            Log.e(TAG, "Plugin not initialized");
            return false;
        }
        
        isFlashOn = !isFlashOn;
        return setFlashlightState(isFlashOn);
    }
    
    public static boolean setFlashlightState(final boolean enabled) {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M) {
            Log.e(TAG, "Requires Android 6.0+");
            return false;
        }
        
        if (cameraManager == null || flashCameraId == null) {
            Log.e(TAG, "Camera manager not initialized");
            return false;
        }
        
        final Handler mainHandler = new Handler(Looper.getMainLooper());
        final boolean[] result = {false};
        
        mainHandler.post(new Runnable() {
            @Override
            public void run() {
                try {
                    cameraManager.setTorchMode(flashCameraId, enabled);
                    isFlashOn = enabled;
                    result[0] = true;
                    Log.i(TAG, "Torch set to: " + enabled);
                } catch (CameraAccessException e) {
                    Log.e(TAG, "CameraAccessException: " + e.getMessage());
                    
                    if (e.getReason() == CameraAccessException.CAMERA_IN_USE) {
                        Log.w(TAG, "Camera in use - trying workaround");
                        tryWorkaround(enabled);
                    } else {
                        Log.e(TAG, "Camera access error code: " + e.getReason());
                        isFlashOn = !enabled;
                    }
                    result[0] = false;
                } catch (Exception e) {
                    Log.e(TAG, "Unexpected error: " + e.getMessage());
                    e.printStackTrace();
                    result[0] = false;
                }
            }
        });
        
        try {
            Thread.sleep(100);
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
        
        return result[0];
    }
    
    private static void tryWorkaround(final boolean enabled) {
        new Handler(Looper.getMainLooper()).postDelayed(new Runnable() {
            @Override
            public void run() {
                try {
                    String[] cameraIds = cameraManager.getCameraIdList();
                    for (String id : cameraIds) {
                        try {
                            CameraCharacteristics characteristics = cameraManager.getCameraCharacteristics(id);
                            Boolean hasFlash = characteristics.get(CameraCharacteristics.FLASH_INFO_AVAILABLE);
                            
                            if (hasFlash != null && hasFlash && !id.equals(flashCameraId)) {
                                Log.d(TAG, "Trying alternate camera: " + id);
                                cameraManager.setTorchMode(id, enabled);
                                isFlashOn = enabled;
                                Log.i(TAG, "Success with alternate camera!");
                                return;
                            }
                        } catch (CameraAccessException e) {
                            Log.d(TAG, "Camera " + id + " also in use");
                        }
                    }
                    Log.e(TAG, "All cameras in use - cannot control torch");
                } catch (Exception e) {
                    Log.e(TAG, "Workaround failed: " + e.getMessage());
                }
            }
        }, 200);
    }
    
    public static boolean isFlashOn() {
        return isFlashOn;
    }
    
    public static void turnOff() {
        if (isFlashOn) {
            setFlashlightState(false);
        }
    }
    
    public static void cleanup() {
        turnOff();
        isInitialized = false;
    }
}
