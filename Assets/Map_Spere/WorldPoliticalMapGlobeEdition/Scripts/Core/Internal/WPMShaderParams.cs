using UnityEngine;

namespace WPM {

    static class ShaderParams {
        public static int GlobalGlobePos = Shader.PropertyToID("_WPM_GlobePos");
        public static int GlobalObject2WorldMatrix = Shader.PropertyToID("_CustomObjectToWorld");
        public static int Alpha = Shader.PropertyToID("_Alpha");
        public static int Alpha1 = Shader.PropertyToID("_Alpha1");
        public static int Alpha2 = Shader.PropertyToID("_Alpha2");
        public static int Alpha3 = Shader.PropertyToID("_Alpha3");
        public static int OutlineThickness = Shader.PropertyToID("_Thickness");
        public static int InvertedGlobe = Shader.PropertyToID("_Inverted");
        public static int Brightness = Shader.PropertyToID("_Brightness");
        public static int Contrast = Shader.PropertyToID("_Contrast");
        public static int AmbientLight = Shader.PropertyToID("_AmbientLight");
        public static int CityLightBrightness = Shader.PropertyToID("_CityLightsBrightness");
        public static int GlowIntensity = Shader.PropertyToID("_GlowIntensity");
        public static int GlowColor = Shader.PropertyToID("_GlowColor");
        public static int GlowFallOff = Shader.PropertyToID("_GlowFallOff");
        public static int GlowGrow = Shader.PropertyToID("_GlowGrow");
        public static int CloudSpeed = Shader.PropertyToID("_CloudSpeed");
        public static int CloudAlpha = Shader.PropertyToID("_CloudAlpha");
        public static int CloudShadowStrength = Shader.PropertyToID("_CloudShadowStrength");
        public static int CloudElevation = Shader.PropertyToID("_CloudElevation");
        public static int BumpAmount = Shader.PropertyToID("_BumpAmount");
        public static int SpecularPower = Shader.PropertyToID("_SpecularPower");
        public static int SpecularIntensity = Shader.PropertyToID("_SpecularIntensity");
        public static int ScatterOuterRadius = Shader.PropertyToID("fOuterRadius");
        public static int ScatterOuterRadiusSqr = Shader.PropertyToID("fOuterRadius2");
        public static int ScatterInnerRadius = Shader.PropertyToID("fInnerRadius");
        public static int ScatterInnerRadiusSqr = Shader.PropertyToID("fInnerRadius2");
        public static int ScatterKRSun = Shader.PropertyToID("fKrESun");
        public static int ScatterKMSun = Shader.PropertyToID("fKmESun");
        public static int ScatterKR4PI = Shader.PropertyToID("fKr4PI");
        public static int ScatterKM4PI = Shader.PropertyToID("fKm4PI");
        public static int ScatterScale = Shader.PropertyToID("fScale");
        public static int ScatterScaleDepth = Shader.PropertyToID("fScaleDepth");
        public static int ScatterScaleOverScaleDepth = Shader.PropertyToID("fScaleOverScaleDepth");
        public static int ScatterG = Shader.PropertyToID("g");
        public static int ScatterInvWaveLength = Shader.PropertyToID("v3InvWavelength");
        public static int SunLightDirection = Shader.PropertyToID("_SunLightDirection");
        public static int AtmosColor = Shader.PropertyToID("_AtmosphereColor");
        public static int AtmosAlpha = Shader.PropertyToID("_AtmosphereAlpha");
        public static int AtmosFallOff = Shader.PropertyToID("_AtmosphereFallOff");
        public static int ScenicAtmosIntensity = Shader.PropertyToID("_ScenicIntensity");
        public static int Color = Shader.PropertyToID("_Color");
        public static int Color2 = Shader.PropertyToID("_Color2");
        public static int FoWElevation = Shader.PropertyToID("_Elevation");
        public static int FoWNoise = Shader.PropertyToID("_Noise");
        public static int FoWPaintData = Shader.PropertyToID("_PaintData");
        public static int FoWPaintStrength = Shader.PropertyToID("_PaintStrength");
        public static int FoWMaskTex = Shader.PropertyToID("_MaskTex");
        public static int ColorShift = Shader.PropertyToID("_ColorShift");
        public static int SpaceToGround = Shader.PropertyToID("_SpaceToGround");
        public static int ParentCoords = Shader.PropertyToID("_ParentCoords");
        public static int ParentCoords1 = Shader.PropertyToID("_ParentCoords1");
        public static int ParentCoords2 = Shader.PropertyToID("_ParentCoords2");
        public static int ParentCoords3 = Shader.PropertyToID("_ParentCoords3");
        public static int CameraRot = Shader.PropertyToID("_CameraRot");
        public static int CountryHighlightData = Shader.PropertyToID("_CountryHighlightData");
        public static int ProvinceHighlightData = Shader.PropertyToID("_ProvinceHighlightData");

        public static int TextMeshProUnderlayColor = Shader.PropertyToID("_UnderlayColor");
        public static int TextMeshProUnderlayOffsetX = Shader.PropertyToID("_UnderlayOffsetX");
        public static int TextMeshProUnderlayOffsetY = Shader.PropertyToID("_UnderlayOffsetY");
    }


}
