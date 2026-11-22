using System;
using UnityEditor;
using UnityEngine;

namespace XadrezRpg.Editor
{
    public sealed class PieceSpritePostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!PieceSpriteImportSettings.IsPiecePng(assetPath))
            {
                return;
            }

            PieceSpriteImportSettings.Apply((TextureImporter)assetImporter);
        }
    }

    public static class PieceSpriteImportSettings
    {
        private const string PieceRoot = "Assets/_Game/Sprites/Pieces/";

        public static bool IsPiecePng(string path)
        {
            return path.StartsWith(PieceRoot, StringComparison.Ordinal)
                && path.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
        }

        public static void Apply(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            // The source artwork is much larger than its on-board display size.
            // Bilinear filtering keeps diagonal and curved edges smooth when Unity downsamples it.
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;
            importer.maxTextureSize = 2048;
        }

        [MenuItem("Xadrez RPG/Configure Piece Sprites")]
        public static void ConfigureAll()
        {
            string[] spriteGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { PieceRoot });
            foreach (string guid in spriteGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (IsPiecePng(path))
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}
