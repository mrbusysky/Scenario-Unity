using UnityEditor;
using UnityEngine;

public class UpscaleEditor : EditorWindow
{
    private static readonly float MinimumWidth = 1650f;
    private static readonly UpscaleEditorUI UpscaleEditorUI = new();

    [MenuItem("Window/Scenario/Upscale Editor")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow(typeof(UpscaleEditor), false, "Upscale Editor") as UpscaleEditor;
        window.minSize = new Vector2(MinimumWidth, window.minSize.y);
    }

    public static void ShowWindow(Texture2D selectedTexture, ImageDataStorage.ImageData imageData)
    {
        UpscaleEditorUI.currentImage = selectedTexture;
        UpscaleEditorUI.imageData = imageData;
        ShowWindow();
    }

    private void OnGUI()
    {
        UpscaleEditorUI.OnGUI(this.position);
    }

    private void OnDestroy()
    {
        UpscaleEditorUI.currentImage = null;
    }
}