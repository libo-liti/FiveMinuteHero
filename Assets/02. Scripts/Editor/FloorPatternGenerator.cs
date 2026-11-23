using System.IO;
using UnityEditor;
using UnityEngine;

namespace FiveMinuteHero.EditorTools
{
    // Kenney 모래 타일 3종을 섞어 반복용 바닥 이미지(PNG) 한 장을 만든다.
    // 시드·비율을 바꾸고 메뉴를 다시 누르면 같은 경로에 새 무늬로 덮어쓴다.
    // Kenney 원본은 git에서 제외되므로, 다시 만들려면 로컬에 Kenney 팩이 있어야 한다.
    public static class FloorPatternGenerator
    {
        private const string MenuPath = "Tools/FiveMinuteHero/무늬 바닥 만들기";
        private const string TileFolder = "Assets/ThirdParty/kenney_tinyDungeon/Tiles/";
        private const string OutputPath = "Assets/07. Sprites/Floor/floor_sand_pattern.png";

        private const int Seed = 20261123;    // 같은 시드면 항상 같은 무늬가 나온다
        private const int GridSize = 16;      // 한 변의 칸 수: 16칸 × 16px = 256px (128px는 화면에서 반복이 보여 키움)
        private const int TilePixels = 16;    // Kenney 타일 한 칸의 픽셀 수
        private const int PixelsPerUnit = 16; // 기존 도트 기준(PPU 16)과 같게

        // 칸마다 어떤 타일이 나올지의 비율. 합이 100이 아니어도 비율대로 나뉜다.
        private const int PlainWeight = 70;
        private const int SpeckleWeight = 22;
        private const int StoneWeight = 5; // 8이면 256px에서 화면이 바빠 보여 낮춤

        private enum FloorTile { Plain, Speckle, Stone }

        // FloorTile 순서와 같은 순서: 단색 모래, 점박이 모래, 돌 박힌 모래
        private static readonly string[] TileFiles = { "tile_0048.png", "tile_0049.png", "tile_0042.png" };

        [MenuItem(MenuPath)]
        private static void Generate()
        {
            Texture2D[] sources = LoadSourceTiles();
            if (sources == null)
            {
                return;
            }

            FloorTile[,] layout = CreateLayout();
            Texture2D result = Compose(layout, sources);
            SavePng(result);
            ApplyImportSettings();

            // 에디터에서 임시로 만든 텍스처는 씬에 속하지 않아 직접 지워야 메모리에 남지 않는다.
            Object.DestroyImmediate(result);
            foreach (Texture2D source in sources)
            {
                Object.DestroyImmediate(source);
            }

            Debug.Log($"무늬 바닥을 만들었습니다: {OutputPath}");
        }

        // 원본 PNG 파일을 바이트로 직접 읽는다.
        // 에셋으로 불러와 GetPixels를 쓰려면 원본 임포트 설정(Read/Write)을 바꿔야 해서, ThirdParty의 .meta를 건드리지 않으려고 이렇게 한다.
        private static Texture2D[] LoadSourceTiles()
        {
            var sources = new Texture2D[TileFiles.Length];
            for (int i = 0; i < TileFiles.Length; i++)
            {
                string path = TileFolder + TileFiles[i];
                if (!File.Exists(path))
                {
                    Debug.LogError($"원본 타일이 없습니다(Kenney 팩 필요): {path}");
                    return null;
                }

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.LoadImage(File.ReadAllBytes(path)); // 크기는 PNG에 맞춰 바뀐다
                if (texture.width != TilePixels || texture.height != TilePixels)
                {
                    Debug.LogError($"타일 크기가 {TilePixels}px이 아닙니다: {path}");
                    return null;
                }

                sources[i] = texture;
            }

            return sources;
        }

        // 고정 시드 난수로 칸마다 타일을 고르고, 돌 타일끼리는 붙지 않게 한다.
        private static FloorTile[,] CreateLayout()
        {
            var random = new System.Random(Seed);
            var layout = new FloorTile[GridSize, GridSize]; // 아직 안 채운 칸은 기본값 Plain

            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    FloorTile tile = PickByWeight(random);
                    if (tile == FloorTile.Stone && HasStoneNeighbor(layout, x, y))
                    {
                        tile = FloorTile.Speckle; // 돌이 뭉치면 무늬 덩어리가 눈에 띄어서 점박이로 바꾼다
                    }

                    layout[x, y] = tile;
                }
            }

            return layout;
        }

        private static FloorTile PickByWeight(System.Random random)
        {
            int roll = random.Next(PlainWeight + SpeckleWeight + StoneWeight);
            if (roll < PlainWeight)
            {
                return FloorTile.Plain;
            }

            if (roll < PlainWeight + SpeckleWeight)
            {
                return FloorTile.Speckle;
            }

            return FloorTile.Stone;
        }

        // 상하좌우 이웃 중 돌이 있는지 본다. 가장자리는 반대편 끝 칸을 이웃으로 본다.
        // 왜 반대편까지 보나: 이 이미지는 바닥에 반복해서 깔리므로, 오른쪽 끝 칸 바로 옆에는
        // 다음 이미지의 왼쪽 끝 칸이 붙는다(위·아래도 같다). 반대편을 안 보면 이 이음매에서 돌이 붙어
        // 반복 경계가 줄처럼 드러난다.
        private static bool HasStoneNeighbor(FloorTile[,] layout, int x, int y)
        {
            return layout[Wrap(x - 1), y] == FloorTile.Stone
                || layout[Wrap(x + 1), y] == FloorTile.Stone
                || layout[x, Wrap(y - 1)] == FloorTile.Stone
                || layout[x, Wrap(y + 1)] == FloorTile.Stone;
        }

        // -1 → GridSize-1, GridSize → 0 처럼 범위를 넘으면 반대편으로 감는다.
        private static int Wrap(int index)
        {
            return (index + GridSize) % GridSize;
        }

        // 배치표대로 원본 타일의 픽셀을 큰 텍스처의 각 칸에 복사한다.
        private static Texture2D Compose(FloorTile[,] layout, Texture2D[] sources)
        {
            int size = GridSize * TilePixels;
            var result = new Texture2D(size, size, TextureFormat.RGBA32, false);

            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    Texture2D source = sources[(int)layout[x, y]];
                    result.SetPixels(x * TilePixels, y * TilePixels, TilePixels, TilePixels, source.GetPixels());
                }
            }

            result.Apply();
            return result;
        }

        private static void SavePng(Texture2D texture)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            File.WriteAllBytes(OutputPath, texture.EncodeToPNG());
            AssetDatabase.ImportAsset(OutputPath); // 새 파일을 Unity가 알게 하고 .meta를 만든다
        }

        // 기존 도트 기준(tilemap_packed.png)과 같은 임포트 설정을 코드로 맞춘다.
        private static void ApplyImportSettings()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(OutputPath);

            // Mesh Type은 TextureImporterSettings로만 바꿀 수 있다. 이 호출이 다른 값도 덮어쓰므로 가장 먼저 한다.
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; // Tiled 모드는 Full Rect여야 경고 없이 반복된다
            importer.SetTextureSettings(settings);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;                            // 도트가 흐려지지 않게
            importer.textureCompression = TextureImporterCompression.Uncompressed; // 압축 손실로 색이 번지지 않게
            importer.mipmapEnabled = false;                                    // 2D 고정 배율이라 밉맵이 필요 없다
            importer.wrapMode = TextureWrapMode.Clamp;                         // Tiled는 칸마다 사각형을 따로 그려 Repeat가 필요 없다
            importer.alphaIsTransparency = true;

            importer.SaveAndReimport();
        }
    }
}
