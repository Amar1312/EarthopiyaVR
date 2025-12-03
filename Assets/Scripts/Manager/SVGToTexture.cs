using UnityEngine;
using Unity.VectorGraphics;
using System.IO;

public static class SVGToTexture
{
    public static Texture2D ConvertSVGToTexture(byte[] svgBytes, int width = 512, int height = 512)
    {
        // Convert bytes  UTF8 SVG text
        string svgText = System.Text.Encoding.UTF8.GetString(svgBytes);

        // Parse SVG using TextReader
        using (TextReader reader = new StringReader(svgText))
        {
            var sceneInfo = SVGParser.ImportSVG(reader);

            // Tessellate the SVG into geometry
            var tessOptions = new VectorUtils.TessellationOptions()
            {
                StepDistance = 0.1f,
                MaxCordDeviation = 0.5f,
                MaxTanAngleDeviation = 0.1f,
                SamplingStepSize = 0.01f
            };

            var geoms = VectorUtils.TessellateScene(sceneInfo.Scene, tessOptions);

            // Build a sprite from geometry
            Sprite svgSprite = VectorUtils.BuildSprite(
                geoms,
                1.0f,                                   // pixelsPerUnit
                VectorUtils.Alignment.Center,
                Vector2.zero,
                128,
                true
            );

            // Convert sprite to texture
            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            RenderTexture rt = new RenderTexture(width, height, 32);
            RenderTexture.active = rt;

            // Draw sprite texture into RenderTexture
            Graphics.DrawTexture(new Rect(0, 0, width, height), svgSprite.texture);

            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            return tex;
        }
    }
}
