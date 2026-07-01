using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace TextMeshAssetGenerator.Editor
{
    [CreateAssetMenu(
        fileName = "FontAssetBuildSettings",
        menuName = "SignedOff/Fonts/Font Asset Build Settings")]
    public sealed class FontAssetBuildSettings : ScriptableObject
    {
        [Header("Source")]
        public Font SourceFont;

        [Header("Output")]
        public string OutputAssetPath =
            "Assets/TextMeshAssetGenerator/FontAssets/JapaneseSubset SDF.asset";

        [Header("Atlas")]
        public int SamplingPointSize = 90;
        public int Padding = 9;
        public int AtlasWidth = 4096;
        public int AtlasHeight = 4096;
        public GlyphRenderMode RenderMode = GlyphRenderMode.SDFAA;
        public bool EnableMultiAtlasSupport = true;

        [Header("Characters")]
        [TextArea(3, 10)]
        public string AdditionalCharacters =
            "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "。、，．・：；！？ー〜（）「」『』【】[]{}+-*/=%&<>#@";

        public List<TextAsset> CharacterTextAssets = new();

        [Tooltip("Folder paths starting with Assets/")]
        public List<string> ScanFolders = new()
        {
        };

        public List<string> ScanExtensions = new()
        {
            ".json",
            ".txt",
            ".uxml",
            ".uss",
            ".cs",
        };

        [Header("Fallback")]
        public List<TMP_FontAsset> FallbackFontAssets = new();

        [Header("Build Behavior")]
        public bool SetStaticAfterGeneration = true;
        public bool OverwriteExistingAsset = true;
        public bool WriteMissingCharactersFile = true;
    }
}
