package com.unity.camera2qr;

import android.app.Activity;
import android.content.Context;
import android.graphics.ImageFormat;
import android.graphics.SurfaceTexture;
import android.hardware.camera2.CameraAccessException;
import android.hardware.camera2.CameraCaptureSession;
import android.hardware.camera2.CameraCharacteristics;
import android.hardware.camera2.CameraDevice;
import android.hardware.camera2.CameraManager;
import android.hardware.camera2.CaptureRequest;
import android.media.Image;
import android.media.ImageReader;
import android.os.Build;
import android.os.Handler;
import android.os.HandlerThread;
import android.util.Log;
import android.util.Size;
import android.view.Surface;

import java.nio.ByteBuffer;
import java.util.Arrays;

public class Camera2QRPlugin {
    private static final String TAG = "Camera2QRPlugin";
    
    private static CameraManager cameraManager;
    private static String backCameraId;
    private static CameraDevice cameraDevice;
    private static CameraCaptureSession captureSession;
    private static ImageReader imageReader;
    private static Handler backgroundHandler;
    private static HandlerThread backgroundThread;
    private static boolean isTorchOn = false;
    private static SurfaceTexture surfaceTexture;
    private static int textureId;
    private static boolean isInitialized = false;

    public static boolean initialize(Activity activity, int unityTextureId) {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.LOLLIPOP) {
            Log.e(TAG, "Camera2 requires Android 5.0 (API 21) or higher");
            return false;
        }

        textureId = unityTextureId;
        
        try {
            cameraManager = (CameraManager) activity.getSystemService(Context.CAMERA_SERVICE);
            
            if (cameraManager == null) {
                Log.e(TAG, "CameraManager is null");
                return false;
            }

            String[] cameraIdList = cameraManager.getCameraIdList();
            
            for (String cameraId : cameraIdList) {
                CameraCharacteristics characteristics = cameraManager.getCameraCharacteristics(cameraId);
                Integer facing = characteristics.get(CameraCharacteristics.LENS_FACING);
                
                if (facing != null && facing == CameraCharacteristics.LENS_FACING_BACK) {
                    backCameraId = cameraId;
                    isInitialized = true;
                    Log.d(TAG, "Found back camera: " + cameraId);
                    return true;
                }
            }
            
            Log.e(TAG, "No back camera found");
            return false;
            
        } catch (Exception e) {
            Log.e(TAG, "Initialization error: " + e.getMessage());
            e.printStackTrace();
            return false;
        }
    }

    public static void startBackgroundThread() {
        backgroundThread = new HandlerThread("CameraBackground");
        backgroundThread.start();
        backgroundHandler = new Handler(backgroundThread.getLooper());
        Log.d(TAG, "Background thread started");
    }

    public static void stopBackgroundThread() {
        if (backgroundThread != null) {
            backgroundThread.quitSafely();
            try {
                backgroundThread.join();
                backgroundThread = null;
                backgroundHandler = null;
            } catch (InterruptedException e) {
                Log.e(TAG, "Error stopping background thread: " + e.getMessage());
            }
        }
    }

    public static boolean setTorchMode(boolean enabled) {
        if (!isInitialized || cameraManager == null || backCameraId == null) {
            Log.e(TAG, "Cannot set torch - not initialized");
            return false;
        }

        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M) {
            Log.e(TAG, "Torch mode requires Android 6.0 (API 23) or higher");
            return false;
        }

        try {
            cameraManager.setTorchMode(backCameraId, enabled);
            isTorchOn = enabled;
            Log.d(TAG, "Torch set to: " + enabled);
            return true;
        } catch (CameraAccessException e) {
            Log.e(TAG, "Failed to set torch mode: " + e.getMessage());
            return false;
        }
    }

    public static boolean isTorchOn() {
        return isTorchOn;
    }

    public static void cleanup() {
        setTorchMode(false);
        
        if (captureSession != null) {
            captureSession.close();
            captureSession = null;
        }
        
        if (cameraDevice != null) {
            cameraDevice.close();
            cameraDevice = null;
        }
        
        if (imageReader != null) {
            imageReader.close();
            imageReader = null;
        }
        
        stopBackgroundThread();
        isInitialized = false;
        Log.d(TAG, "Cleaned up");
    }
}
