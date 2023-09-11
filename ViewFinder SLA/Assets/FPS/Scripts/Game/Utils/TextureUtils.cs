using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TextureUtils
{
    public static Texture2D GetScreenshot(Camera camera)
    {
        if (camera == null) return null;

        var renderTex = camera.targetTexture;
        if (renderTex == null)
        {
            renderTex = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = renderTex;
            camera.Render();
        }

        var texture = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGBA32, false);
        Graphics.CopyTexture(renderTex, texture);
        return texture;
    }
}