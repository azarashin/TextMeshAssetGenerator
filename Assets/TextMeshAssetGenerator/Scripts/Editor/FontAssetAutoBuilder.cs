using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace TextMeshAssetGenerator.Editor
{
    public static class FontAssetAutoBuilder
    {
        [MenuItem("Tools/SignedOff/Fonts/Generate Font Asset From Selected Settings")]
        public static void GenerateFromSelectedSettings()
        {
            FontAssetBuildSettings settings =
                Selection.activeObject as FontAssetBuildSettings;

            if (settings == null)
            {
                Debug.LogError(
                    "Please select a FontAssetBuildSettings asset before running this command.");
                return;
            }

            Generate(settings);
        }

        public static void Generate(FontAssetBuildSettings settings)
        {
            Validate(settings);

            string characters = CollectCharacters(settings);

            Debug.Log($"FontAsset generation started: {characters.Length} characters");

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                settings.SourceFont,
                settings.SamplingPointSize,
                settings.Padding,
                settings.RenderMode,
                settings.AtlasWidth,
                settings.AtlasHeight,
                TMPro.AtlasPopulationMode.Dynamic,
                settings.EnableMultiAtlasSupport);

            fontAsset.name =
                Path.GetFileNameWithoutExtension(settings.OutputAssetPath);

            bool allAdded = fontAsset.TryAddCharacters(
                characters,
                out string missingCharacters);

            if (settings.FallbackFontAssets != null &&
                settings.FallbackFontAssets.Count > 0)
            {
                fontAsset.fallbackFontAssetTable =
                    settings.FallbackFontAssets
                        .Where(x => x != null)
                        .Distinct()
                        .ToList();
            }

            if (settings.SetStaticAfterGeneration)
            {
                fontAsset.atlasPopulationMode = TMPro.AtlasPopulationMode.Static;
            }

            fontAsset.ReadFontAssetDefinition();

            SaveFontAsset(settings, fontAsset);

            if (!allAdded && settings.WriteMissingCharactersFile)
            {
                WriteMissingCharacters(settings.OutputAssetPath, missingCharacters);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (allAdded)
            {
                Debug.Log(
                    $"FontAsset generation completed: {settings.OutputAssetPath}");
            }
            else
            {
                Debug.LogWarning(
                    $"FontAsset generation completed, but some characters were missing: {missingCharacters}");
            }
        }

        private static void Validate(FontAssetBuildSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (settings.SourceFont == null)
            {
                throw new InvalidOperationException(
                    "SourceFont is not assigned.");
            }

            if (string.IsNullOrWhiteSpace(settings.OutputAssetPath))
            {
                throw new InvalidOperationException(
                    "OutputAssetPath is empty.");
            }

            if (!settings.OutputAssetPath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "OutputAssetPath must start with Assets/.");
            }

            string extension = Path.GetExtension(settings.OutputAssetPath);
            if (!string.Equals(extension, ".asset", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "OutputAssetPath must have the .asset extension.");
            }

            if (settings.AtlasWidth <= 0 || settings.AtlasHeight <= 0)
            {
                throw new InvalidOperationException(
                    "AtlasWidth and AtlasHeight must be greater than zero.");
            }
        }

        private static string CollectCharacters(FontAssetBuildSettings settings)
        {
            HashSet<char> set = new();

            void AddText(string text)
            {
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                foreach (char c in text)
                {
                    if (char.IsControl(c))
                    {
                        continue;
                    }

                    set.Add(c);
                }
            }

            AddText(settings.AdditionalCharacters);

            if (settings.CharacterTextAssets != null)
            {
                foreach (UnityEngine.TextAsset textAsset in settings.CharacterTextAssets)
                {
                    if (textAsset == null)
                    {
                        continue;
                    }

                    AddText(textAsset.text);
                }
            }

            if (settings.ScanFolders != null)
            {
                foreach (string folder in settings.ScanFolders)
                {
                    if (string.IsNullOrWhiteSpace(folder))
                    {
                        continue;
                    }

                    if (!Directory.Exists(folder))
                    {
                        Debug.LogWarning($"Scan target folder does not exist: {folder}");
                        continue;
                    }

                    foreach (string file in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories))
                    {
                        if (!ShouldReadFile(settings, file))
                        {
                            continue;
                        }

                        try
                        {
                            AddText(File.ReadAllText(file));
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning(
                                $"Failed to collect characters from file: {file}\n{ex.Message}");
                        }
                    }
                }
            }

            // Whitespace characters that are often forgotten in Japanese UI.
            set.Add(' ');
            set.Add('　');

            return new string(set.OrderBy(c => c).ToArray());
        }

        private static bool ShouldReadFile(
            FontAssetBuildSettings settings,
            string filePath)
        {
            string extension = Path.GetExtension(filePath);

            if (settings.ScanExtensions == null ||
                settings.ScanExtensions.Count == 0)
            {
                return true;
            }

            return settings.ScanExtensions.Any(x =>
                string.Equals(x, extension, StringComparison.OrdinalIgnoreCase));
        }

        private static void SaveFontAsset(
            FontAssetBuildSettings settings,
            TMP_FontAsset fontAsset)
        {
            string outputPath = settings.OutputAssetPath;
            string outputDir = Path.GetDirectoryName(outputPath);

            if (!string.IsNullOrEmpty(outputDir) &&
                !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outputPath) != null)
            {
                if (!settings.OverwriteExistingAsset)
                {
                    throw new InvalidOperationException(
                        $"A FontAsset already exists at: {outputPath}");
                }

                AssetDatabase.DeleteAsset(outputPath);
            }

            AssetDatabase.CreateAsset(fontAsset, outputPath);

            Material material = fontAsset.material;
            if (material != null && !AssetDatabase.Contains(material))
            {
                material.name = $"{fontAsset.name} Material";
                AssetDatabase.AddObjectToAsset(material, fontAsset);
            }

            if (fontAsset.atlasTextures != null)
            {
                foreach (Texture2D atlasTexture in fontAsset.atlasTextures)
                {
                    if (atlasTexture == null)
                    {
                        continue;
                    }

                    if (!AssetDatabase.Contains(atlasTexture))
                    {
                        atlasTexture.name = $"{fontAsset.name} Atlas";
                        AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
                    }
                }
            }

            EditorUtility.SetDirty(fontAsset);
        }

        private static void WriteMissingCharacters(
            string outputAssetPath,
            string missingCharacters)
        {
            if (string.IsNullOrEmpty(missingCharacters))
            {
                return;
            }

            string path = Path.ChangeExtension(
                outputAssetPath,
                ".missing.txt");

            File.WriteAllText(path, missingCharacters);

            Debug.LogWarning($"Missing character list was written to: {path}");
        }
    }
}
