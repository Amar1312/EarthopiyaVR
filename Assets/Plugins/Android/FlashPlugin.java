package com.flash;

import android.app.Activity;
import android.content.Context;
import android.hardware.camera2.CameraManager;
import android.hardware.camera2.CameraAccessException;

public class FlashPlugin {

    private static String fixedCameraId = null;

    public static void setCameraID(String id) {
        fixedCameraId = id;
    }

    public static void turnFlashOn(Activity activity) {
        setFlash(activity, true);
    }

    public static void turnFlashOff(Activity activity) {
        setFlash(activity, false);
    }

    private static void setFlash(Activity activity, boolean state) {
        try {
            CameraManager cameraManager =
                (CameraManager) activity.getSystemService(Context.CAMERA_SERVICE);

            String cameraId = (fixedCameraId != null) ? fixedCameraId : cameraManager.getCameraIdList()[0];

            cameraManager.setTorchMode(cameraId, state);

        } catch (Exception e) { }
    }
}
