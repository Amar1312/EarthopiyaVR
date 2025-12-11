package com.unity.torchonly;

import android.app.Activity;
import android.hardware.camera2.CameraManager;
import android.util.Log;

public class FlashTorchOnly {

    private static final String TAG = "FlashTorchOnly";
    private Activity activity;
    private CameraManager cameraManager;
    private String backCameraId;

    public FlashTorchOnly(Activity activity) {
        this.activity = activity;
        cameraManager = (CameraManager) activity.getSystemService(Activity.CAMERA_SERVICE);

        try {
            for (String id : cameraManager.getCameraIdList()) {
                Integer lens = cameraManager.getCameraCharacteristics(id)
                        .get(android.hardware.camera2.CameraCharacteristics.LENS_FACING);

                Boolean flash = cameraManager.getCameraCharacteristics(id)
                        .get(android.hardware.camera2.CameraCharacteristics.FLASH_INFO_AVAILABLE);

                if (lens != null && lens == android.hardware.camera2.CameraCharacteristics.LENS_FACING_BACK && flash != null && flash) {
                    backCameraId = id;
                    break;
                }
            }
        } catch (Exception e) {
            Log.e(TAG, "Error getting camera: " + e.getMessage());
        }
    }

    public void setTorch(boolean state) {
        try {
            cameraManager.setTorchMode(backCameraId, state);
            Log.d(TAG, "Torch set to: " + state);
        } catch (Exception e) {
            Log.e(TAG, "Torch error: " + e.getMessage());
        }
    }
}
