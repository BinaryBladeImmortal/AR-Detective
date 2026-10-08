using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARCameraDiagnostic : MonoBehaviour
{
    private ARCameraManager cameraManager;
    private ARCameraBackground cameraBackground;
    private ARSession arSession;

    private int frameCount = 0;

    private int lastTextureCount = 0;
    private int lastTextureDescriptorCount = 0;

    private bool lastDisplayMatrixAvailable = false;
    private bool lastProjectionMatrixAvailable = false;
    private bool lastTimestampAvailable = false;

    private bool cpuImageAvailable = false;

    private string materialInfo = "Unknown";
    private string shaderInfo = "Unknown";

    private string configurationInfo = "Unknown";
    private string textureInfo = "Unknown";
    private string errorInfo = "";

    private void Start()
    {
        cameraManager = GetComponent<ARCameraManager>();
        cameraBackground = GetComponent<ARCameraBackground>();
        arSession = FindFirstObjectByType<ARSession>();

        if (cameraManager != null)
        {
            cameraManager.frameReceived += OnCameraFrameReceived;
        }
        else
        {
            errorInfo = "ARCameraManager NOT FOUND";
        }
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        frameCount++;

        // --------------------------------------------------
        // CAMERA FRAME TEXTURES
        // --------------------------------------------------

        if (args.textures != null)
        {
            lastTextureCount = args.textures.Count;

            if (lastTextureCount > 0)
            {
                textureInfo = "";

                for (int i = 0; i < args.textures.Count; i++)
                {
                    Texture2D tex = args.textures[i];

                    if (tex != null)
                    {
                        textureInfo +=
                            "\nTexture " + i +
                            ": " +
                            tex.width + "x" + tex.height +
                            " | " +
                            tex.graphicsFormat;
                    }
                    else
                    {
                        textureInfo +=
                            "\nTexture " + i +
                            ": NULL";
                    }
                }
            }
            else
            {
                textureInfo = "NO FRAME TEXTURES";
            }
        }
        else
        {
            lastTextureCount = 0;
            textureInfo = "TEXTURE LIST NULL";
        }

        // --------------------------------------------------
        // FRAME MATRICES
        // --------------------------------------------------

        lastDisplayMatrixAvailable = args.displayMatrix.HasValue;
        lastProjectionMatrixAvailable = args.projectionMatrix.HasValue;
        lastTimestampAvailable = args.timestampNs.HasValue;

        // --------------------------------------------------
        // CAMERA MATERIAL
        // --------------------------------------------------

        if (cameraManager != null)
        {
            Material mat = cameraManager.cameraMaterial;

            if (mat != null)
            {
                materialInfo = "FOUND";

                if (mat.shader != null)
                    shaderInfo = mat.shader.name;
                else
                    shaderInfo = "NO SHADER";
            }
            else
            {
                materialInfo = "NULL";
                shaderInfo = "NO MATERIAL";
            }
        }

        // --------------------------------------------------
        // CURRENT CAMERA CONFIGURATION
        // --------------------------------------------------

        if (cameraManager != null)
        {
            if (cameraManager.currentConfiguration.HasValue)
            {
                XRCameraConfiguration config =
                    cameraManager.currentConfiguration.Value;

                configurationInfo =
                    "Resolution: " +
                    config.resolution.x +
                    " x " +
                    config.resolution.y;

                if (config.framerate.HasValue)
                {
                    configurationInfo +=
                        "\nFPS: " +
                        config.framerate.Value;
                }
            }
            else
            {
                configurationInfo = "NO CURRENT CONFIGURATION";
            }
        }

        // --------------------------------------------------
        // GPU TEXTURE DESCRIPTORS
        // --------------------------------------------------

        try
        {
            if (cameraManager != null &&
                cameraManager.subsystem != null)
            {
                NativeArray<XRTextureDescriptor> descriptors =
                    cameraManager.subsystem.GetTextureDescriptors(
                        Allocator.Temp
                    );

                lastTextureDescriptorCount = descriptors.Length;

                descriptors.Dispose();
            }
        }
        catch (System.Exception e)
        {
            errorInfo =
                "Texture descriptor error: " +
                e.Message;
        }

        // --------------------------------------------------
        // CPU IMAGE TEST
        // --------------------------------------------------

        cpuImageAvailable = false;

        try
        {
            if (cameraManager != null)
            {
                if (cameraManager.TryAcquireLatestCpuImage(
                    out XRCpuImage cpuImage))
                {
                    cpuImageAvailable = true;

                    Debug.Log(
                        "CPU camera image acquired: " +
                        cpuImage.width +
                        "x" +
                        cpuImage.height
                    );

                    cpuImage.Dispose();
                }
            }
        }
        catch (System.Exception e)
        {
            errorInfo =
                "CPU image error: " +
                e.Message;
        }
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);

        style.fontSize = 30;
        style.normal.textColor = Color.white;

        string output =
            "AR CAMERA GPU DIAGNOSTIC\n\n" +

            "=== CAMERA MANAGER ===\n";

        if (cameraManager == null)
        {
            output +=
                "ARCameraManager: NOT FOUND\n";
        }
        else
        {
            output +=
                "ARCameraManager: FOUND\n" +
                "Enabled: " +
                cameraManager.enabled +
                "\n" +
                "Subsystem: " +
                (cameraManager.subsystem != null
                    ? "FOUND"
                    : "NOT FOUND") +
                "\n" +
                "Running: " +
                (cameraManager.subsystem != null &&
                 cameraManager.subsystem.running) +
                "\n" +
                "Camera Frames: " +
                frameCount +
                "\n";
        }

        output +=
            "\n=== AR CAMERA BACKGROUND ===\n";

        if (cameraBackground != null)
        {
            output +=
                "Component: FOUND\n" +
                "Enabled: " +
                cameraBackground.enabled +
                "\n" +
                "Background Rendering: " +
                cameraBackground.backgroundRenderingEnabled +
                "\n";

            if (cameraManager != null)
            {
                output +=
                    "Requested Mode: " +
                    cameraManager.requestedBackgroundRenderingMode +
                    "\n" +

                    "Current Mode: " +
                    cameraManager.currentRenderingMode +
                    "\n";
            }
        }
        else
        {
            output +=
                "ARCameraBackground: NOT FOUND\n";
        }

        output +=
            "\n=== GPU CAMERA TEXTURES ===\n" +
            "Frame Texture Count: " +
            lastTextureCount +
            "\n" +
            "Texture Descriptors: " +
            lastTextureDescriptorCount +
            "\n" +
            textureInfo +
            "\n";

        output +=
            "\n=== FRAME DATA ===\n" +
            "Display Matrix: " +
            lastDisplayMatrixAvailable +
            "\n" +
            "Projection Matrix: " +
            lastProjectionMatrixAvailable +
            "\n" +
            "Timestamp: " +
            lastTimestampAvailable +
            "\n";

        output +=
            "\n=== CAMERA MATERIAL ===\n" +
            "Material: " +
            materialInfo +
            "\n" +
            "Shader: " +
            shaderInfo +
            "\n";

        output +=
            "\n=== CAMERA CONFIGURATION ===\n" +
            configurationInfo +
            "\n";

        output +=
            "\n=== CPU CAMERA IMAGE ===\n" +
            "CPU Image Available: " +
            cpuImageAvailable +
            "\n";

        output +=
            "\n=== AR SESSION ===\n";

        if (arSession != null)
        {
            output +=
                "Session: FOUND\n" +
                "State: " +
                ARSession.state +
                "\n";
        }
        else
        {
            output +=
                "AR Session: NOT FOUND\n";
        }

        output +=
            "\n=== GRAPHICS ===\n" +
            "Graphics API: " +
            SystemInfo.graphicsDeviceType +
            "\n" +
            "GPU: " +
            SystemInfo.graphicsDeviceName +
            "\n";

        output +=
            "\n=== ERRORS ===\n" +
            (string.IsNullOrEmpty(errorInfo)
                ? "None"
                : errorInfo);

        GUI.Label(
            new Rect(
                20,
                50,
                Screen.width - 40,
                Screen.height - 70
            ),
            output,
            style
        );
    }

    private void OnDestroy()
    {
        if (cameraManager != null)
        {
            cameraManager.frameReceived -=
                OnCameraFrameReceived;
        }
    }
}