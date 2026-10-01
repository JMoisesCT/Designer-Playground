using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Configura una cámara para pixel art: ortográfica, sin HDR/MSAA y con Pixel Perfect Camera de URP
/// usando la resolución de referencia y el PPU de GameConstants.
/// </summary>
public static class PixelPerfectCameraSetup
{
    [MenuItem("Designer Playground/Cámara/Configurar cámara Pixel Perfect", priority = 100)]
    private static void ConfigureFromMenu()
    {
        Camera camera = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<Camera>()
            : null;

        if (camera == null)
        {
            camera = Camera.main;
        }

        if (camera == null)
        {
            EditorUtility.DisplayDialog(
                "Pixel Perfect Camera",
                "No hay cámara seleccionada ni ninguna con el tag MainCamera en la escena.",
                "OK");
            return;
        }

        Configure(camera);
        Debug.Log($"[Setup] Cámara '{camera.name}' configurada como Pixel Perfect " +
                  $"({GameConstants.ReferenceResolutionX}x{GameConstants.ReferenceResolutionY}, PPU {GameConstants.PixelsPerUnit}).",
                  camera);
    }

    public static PixelPerfectCamera Configure(Camera camera)
    {
        Undo.RecordObject(camera, "Configurar cámara Pixel Perfect");
        camera.orthographic = true;
        camera.orthographicSize = GameConstants.ReferenceResolutionY / 2f / GameConstants.PixelsPerUnit;
        camera.allowHDR = false;
        camera.allowMSAA = false;
        camera.allowDynamicResolution = false;

        var pixelPerfect = camera.GetComponent<PixelPerfectCamera>();
        if (pixelPerfect == null)
        {
            pixelPerfect = Undo.AddComponent<PixelPerfectCamera>(camera.gameObject);
        }
        else
        {
            Undo.RecordObject(pixelPerfect, "Configurar cámara Pixel Perfect");
        }

        pixelPerfect.assetsPPU = GameConstants.PixelsPerUnit;
        pixelPerfect.refResolutionX = GameConstants.ReferenceResolutionX;
        pixelPerfect.refResolutionY = GameConstants.ReferenceResolutionY;
        pixelPerfect.gridSnapping = PixelPerfectCamera.GridSnapping.PixelSnapping;
        pixelPerfect.cropFrame = PixelPerfectCamera.CropFrame.Windowbox;

        EditorUtility.SetDirty(camera);
        EditorUtility.SetDirty(pixelPerfect);
        if (camera.gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(camera.gameObject.scene);
        }

        return pixelPerfect;
    }
}
