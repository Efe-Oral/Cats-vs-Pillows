using System.IO;
using UnityEditor;
using UnityEngine;

namespace CatsVsPillows.Editor.Art
{
    /// <summary>
    /// Paints the small set of stylized textures the game uses (striped wallpapers per room theme, wooden floor planks,
    /// the pillow icon for the queue UI and the glow sprite) so the look can be tweaked here and regenerated from the menu.
    /// </summary>
    internal static class StylizedTextureGenerator
    {
        private const string Folder = "Assets/GAME ASSETS/Textures";
        public const string WallpaperPath = Folder + "/Wallpaper.png";
        public const string FloorPath = Folder + "/FloorPlanks.png";
        public const string PillowIconPath = Folder + "/PillowIcon.png";
        public const string GlowPath = Folder + "/Glow.png";

        private readonly struct WallpaperPalette
        {
            public WallpaperPalette(string fileSuffix, string stripeA, string stripeB, string dot)
            {
                FileSuffix = fileSuffix;
                StripeA = Hex(stripeA);
                StripeB = Hex(stripeB);
                Dot = Hex(dot);
            }

            public string FileSuffix { get; }
            public Color StripeA { get; }
            public Color StripeB { get; }
            public Color Dot { get; }
        }

        // One wallpaper per room theme. The mint one keeps the original file name ("Wallpaper.png").
        private static readonly WallpaperPalette[] Wallpapers =
        {
            new WallpaperPalette("", "#CDEFE2", "#B6E5D3", "#F2FFF9"),          // Mint
            new WallpaperPalette("_Rose", "#FFDDE4", "#FBC9D4", "#FFF4F6"),     // Rose
            new WallpaperPalette("_Butter", "#FFF1C4", "#FFE59E", "#FFFBEA"),   // Butter
            new WallpaperPalette("_Lavender", "#E3DBFF", "#D3C7FA", "#F7F4FF"), // Lavender
            new WallpaperPalette("_Midnight", "#33416E", "#2B375E", "#F8E9A8"), // Midnight with little gold "stars"
        };

        [MenuItem("Cats vs Pillows/Art/Generate Stylized Textures")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(Folder);
            foreach (WallpaperPalette palette in Wallpapers)
                Save(PaintWallpaper(palette), $"{Folder}/Wallpaper{palette.FileSuffix}.png", isSprite: false);

            Save(PaintFloorPlanks(new[] { "#E9C08F", "#E3B583", "#EDC899", "#E0B07C" }, "#B98458"), FloorPath, isSprite: false);
            Save(PaintFloorPlanks(new[] { "#A86F4C", "#9C6544", "#B07853", "#946043" }, "#5E3A28"),
                $"{Folder}/FloorPlanks_Walnut.png", isSprite: false);
            Save(PaintPillowIcon(), PillowIconPath, isSprite: true);
            Save(PaintGlow(), GlowPath, isSprite: true, pixelsPerUnit: 128f);
        }

        /// <summary>White radial falloff (alpha only matters) used for lamp halos, fairy bulbs and dust motes.</summary>
        private static Texture2D PaintGlow()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size * 0.5f, size * 0.5f)) / (size * 0.5f);
                    float core = Mathf.Clamp01(1f - distance / 0.25f);
                    float halo = Mathf.Pow(Mathf.Clamp01(1f - distance), 2.2f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(halo * 0.75f + core * 0.5f)));
                }
            }
            texture.Apply();
            return texture;
        }

        private static Texture2D PaintWallpaper(WallpaperPalette palette)
        {
            const int size = 256;
            Color stripeA = palette.StripeA;
            Color stripeB = palette.StripeB;
            Color dot = palette.Dot;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int stripe = x / 32;
                    Color color = stripe % 2 == 0 ? stripeA : stripeB;

                    // Little polka dots centered in the light stripes.
                    if (stripe % 2 == 0)
                    {
                        float cx = stripe * 32 + 16;
                        float cy = (y / 32) * 32 + 16 + ((stripe / 2) % 2) * 16;
                        float dy = Mathf.Repeat(y - cy + 16, 32) - 16;
                        if (new Vector2(x - cx, dy).sqrMagnitude < 16f)
                            color = dot;
                    }
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            return texture;
        }

        private static Texture2D PaintFloorPlanks(string[] woodHexes, string gapHex)
        {
            const int size = 256;
            const int plankHeight = 64;
            Color[] woods = System.Array.ConvertAll(woodHexes, Hex);
            Color gap = Hex(gapHex);
            var random = new System.Random(7);
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

            for (int row = 0; row < size / plankHeight; row++)
            {
                int joint = random.Next(40, 216);
                Color left = woods[random.Next(woods.Length)];
                Color right = woods[random.Next(woods.Length)];

                for (int y = row * plankHeight; y < (row + 1) * plankHeight; y++)
                {
                    int localY = y - row * plankHeight;
                    for (int x = 0; x < size; x++)
                    {
                        bool isGap = localY < 3 || Mathf.Abs(x - joint) < 2;
                        Color wood = x < joint ? left : right;
                        // Soft lengthwise grain.
                        float grain = 0.04f * Mathf.Sin(x * 0.09f + localY * 0.6f + row * 3f);
                        texture.SetPixel(x, y, isGap ? gap : wood * (1f + grain));
                    }
                }
            }
            texture.Apply();
            return texture;
        }

        /// <summary>White pillow silhouette (tinted by the UI Image color) with baked soft shading and an ink outline.</summary>
        private static Texture2D PaintPillowIcon()
        {
            const int width = 256;
            const int height = 208;
            const float pinch = 0.12f;
            Color ink = Hex("#4A2C28");
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int py = 0; py < height; py++)
            {
                for (int px = 0; px < width; px++)
                {
                    float nx = (px + 0.5f) / width * 2f - 1f;
                    float ny = (py + 0.5f) / height * 2f - 1f;

                    // Inverse of the inward-bowing sides used by the 3D pillow.
                    float ux = nx / (0.92f * (1f - pinch * (1f - ny * ny)));
                    float uy = ny / (0.88f * (1f - pinch * (1f - nx * nx)));
                    float inside = Mathf.Max(Mathf.Abs(ux), Mathf.Abs(uy));

                    Color color;
                    if (inside > 1.06f)
                    {
                        color = Color.clear;
                    }
                    else if (inside > 0.96f)
                    {
                        color = ink;
                    }
                    else
                    {
                        float puff = Mathf.Pow(Mathf.Max(0f, (1f - ux * ux) * (1f - uy * uy)), 0.45f);
                        float light = Mathf.Lerp(0.72f, 1f, puff) + 0.12f * Mathf.Clamp01(uy - ux * 0.5f);
                        float tuft = Mathf.Exp(-(ux * ux + uy * uy) / 0.012f);
                        light -= 0.25f * tuft;
                        float highlight = Mathf.Exp(-((ux + 0.45f) * (ux + 0.45f) + (uy - 0.45f) * (uy - 0.45f)) / 0.02f);
                        color = Color.Lerp(new Color(light, light, light, 1f), Color.white, highlight * 0.8f);
                    }

                    float edgeAlpha = Mathf.Clamp01((1.06f - inside) / 0.02f);
                    color.a *= inside > 1.04f ? edgeAlpha : 1f;
                    texture.SetPixel(px, py, color);
                }
            }
            texture.Apply();
            return texture;
        }

        private static void Save(Texture2D texture, string path, bool isSprite, float pixelsPerUnit = 100f)
        {
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = isSprite ? TextureImporterType.Sprite : TextureImporterType.Default;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.wrapMode = isSprite ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
            importer.alphaIsTransparency = isSprite;
            importer.mipmapEnabled = !isSprite;
            importer.SaveAndReimport();
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }
    }
}
