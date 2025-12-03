using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.InputSystem.XR;

public class CameraSetupManager : MonoBehaviour
{
    private static CameraSetupManager instance;

    public static CameraSetupManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("CameraSetupManager");
                instance = go.AddComponent<CameraSetupManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SetupVRCamera()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in scene!");
            return;
        }

        GameObject cameraObject = mainCamera.gameObject;

        if (cameraObject.GetComponent<TrackedPoseDriver>() == null)
        {
            TrackedPoseDriver poseDriver = cameraObject.AddComponent<TrackedPoseDriver>();
            poseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            poseDriver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
            Debug.Log("Added TrackedPoseDriver to VR camera");
        }

        mainCamera.nearClipPlane = 0.1f;
        mainCamera.farClipPlane = 1000f;

        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.identity;

        Debug.Log("VR Camera setup completed");
    }

    public void SetupARCamera()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in scene!");
            return;
        }

        GameObject cameraObject = mainCamera.gameObject;

        if (cameraObject.GetComponent<TrackedPoseDriver>() == null)
        {
            TrackedPoseDriver poseDriver = cameraObject.AddComponent<TrackedPoseDriver>();
            poseDriver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            poseDriver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
            Debug.Log("Added TrackedPoseDriver to AR camera");
        }

        if (cameraObject.GetComponent<ARCameraManager>() == null)
        {
            cameraObject.AddComponent<ARCameraManager>();
            Debug.Log("Added ARCameraManager");
        }

        if (cameraObject.GetComponent<ARCameraBackground>() == null)
        {
            cameraObject.AddComponent<ARCameraBackground>();
            Debug.Log("Added ARCameraBackground");
        }

        ARSession arSession = FindFirstObjectByType<ARSession>();
        if (arSession == null)
        {
            GameObject sessionObject = new GameObject("AR Session");
            sessionObject.AddComponent<ARSession>();
            Debug.Log("Created AR Session");
        }

        mainCamera.nearClipPlane = 0.1f;
        mainCamera.farClipPlane = 1000f;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = Color.black;

        Debug.Log("AR Camera setup completed");
    }

    public void SetupNormalCamera()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in scene!");
            return;
        }

        GameObject cameraObject = mainCamera.gameObject;

        TrackedPoseDriver poseDriver = cameraObject.GetComponent<TrackedPoseDriver>();
        if (poseDriver != null)
        {
            Destroy(poseDriver);
            Debug.Log("Removed TrackedPoseDriver from normal camera");
        }

        ARCameraManager arCameraManager = cameraObject.GetComponent<ARCameraManager>();
        if (arCameraManager != null)
        {
            Destroy(arCameraManager);
        }

        ARCameraBackground arBackground = cameraObject.GetComponent<ARCameraBackground>();
        if (arBackground != null)
        {
            Destroy(arBackground);
        }

        Debug.Log("Normal Camera setup completed");
    }
}
