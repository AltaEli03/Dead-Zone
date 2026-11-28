using DeadZone.Level;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace DeadZone.EditorTools
{
    // Menu "Dead Zone > Poner fondos en los niveles": imports Art/Backgrounds/BG_LevelXX.png
    // and places each one behind its level with ParallaxBackground fitted to the CameraBounds.
    // Menu "Dead Zone > Ocultar piso / Mostrar piso": toggles the floor row so the player walks on the background.
    public static class BackgroundSetup
    {
        const string BackgroundFolder = "Assets/_Project/Art/Backgrounds";
        const string SceneFolder = "Assets/_Project/Scenes";
        const string UnlitMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
        const string BackgroundName = "Background";
        const float BackgroundHeight = 12.5f; // World units. The camera shows 9.2 and moves inside CameraBounds (y -3 to 8).
        const float FloorTopY = 0f;

        // streetLine: where the image's ground is, from 0 (top of the image) to 1 (bottom).
        static readonly (string scene, string image, float streetLine, Color tint)[] Levels =
        {
            ("Level01", "BG_Level01", 0.74f, Color.white),
            ("Level02", "BG_Level02", 0.75f, Color.white),
            ("Level03", "BG_Level03", 0.76f, Color.white),
            ("Level04", "BG_Level04", 0.68f, new Color(0.75f, 0.75f, 0.8f)),
            ("Level05", "BG_Level05", 0.72f, Color.white),
        };

        const string GroundName = "Ground";
        const string PlatformsName = "Platforms";

        // Hides the floor row so the player looks like walking on the background's ground.
        // The floor keeps its collider, and the floating platforms move to a visible "Platforms" tilemap.
        [MenuItem("Dead Zone/Ocultar piso (caminar sobre el fondo)")]
        static void HideFloorInAllLevels() => SetFloorVisibleInAllLevels(false);

        [MenuItem("Dead Zone/Mostrar piso")]
        static void ShowFloorInAllLevels() => SetFloorVisibleInAllLevels(true);

        static void SetFloorVisibleInAllLevels(bool visible)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string previousScene = SceneManager.GetActiveScene().path;
            int changed = 0;

            foreach (var level in Levels)
            {
                Scene scene = EditorSceneManager.OpenScene($"{SceneFolder}/{level.scene}.unity", OpenSceneMode.Single);
                GameObject ground = GameObject.Find(GroundName);
                if (ground == null || ground.GetComponent<Tilemap>() == null)
                {
                    Debug.LogWarning($"[Piso] No se encontró el Tilemap '{GroundName}' en {level.scene}.");
                    continue;
                }

                if (!visible) SplitPlatforms(ground);
                ground.GetComponent<TilemapRenderer>().enabled = visible;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                changed++;
            }

            if (!string.IsNullOrEmpty(previousScene)) EditorSceneManager.OpenScene(previousScene);
            Debug.Log($"[Piso] Piso {(visible ? "visible" : "oculto")} en {changed} de {Levels.Length} niveles.");
        }

        static void SplitPlatforms(GameObject ground)
        {
            Transform parent = ground.transform.parent;
            if (parent != null && parent.Find(PlatformsName) != null) return;

            Tilemap floor = ground.GetComponent<Tilemap>();
            floor.CompressBounds();
            int floorRow = floor.cellBounds.yMin;

            GameObject platforms = Object.Instantiate(ground, parent);
            platforms.name = PlatformsName;
            platforms.GetComponent<TilemapRenderer>().enabled = true;
            Tilemap platformTiles = platforms.GetComponent<Tilemap>();

            foreach (Vector3Int cell in floor.cellBounds.allPositionsWithin)
            {
                if (!floor.HasTile(cell)) continue;
                if (cell.y == floorRow) platformTiles.SetTile(cell, null);
                else floor.SetTile(cell, null);
            }

            floor.CompressBounds();
            platformTiles.CompressBounds();
        }

        [MenuItem("Dead Zone/Poner fondos en los niveles")]
        static void SetupAllLevels()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string previousScene = SceneManager.GetActiveScene().path;
            Material unlit = AssetDatabase.LoadAssetAtPath<Material>(UnlitMaterialPath);
            int placed = 0;

            foreach (var level in Levels)
            {
                string imagePath = $"{BackgroundFolder}/{level.image}.png";
                Sprite sprite = ImportAsBackground(imagePath);
                if (sprite == null)
                {
                    Debug.LogWarning($"[Fondos] No se encontró {imagePath}, se omite {level.scene}.");
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene($"{SceneFolder}/{level.scene}.unity", OpenSceneMode.Single);
                PlaceBackground(sprite, unlit, level.streetLine, level.tint);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                placed++;
            }

            if (!string.IsNullOrEmpty(previousScene)) EditorSceneManager.OpenScene(previousScene);
            Debug.Log($"[Fondos] Fondos colocados en {placed} de {Levels.Length} niveles.");
        }

        static Sprite ImportAsBackground(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return null;

            importer.GetSourceTextureWidthAndHeight(out _, out int height);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = height / BackgroundHeight;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static void PlaceBackground(Sprite sprite, Material unlit, float streetLine, Color tint)
        {
            GameObject background = GameObject.Find(BackgroundName);
            if (background == null) background = new GameObject(BackgroundName);

            Transform t = background.transform;
            t.SetParent(null);
            t.SetAsFirstSibling();
            float topY = FloorTopY + streetLine * BackgroundHeight;
            t.position = new Vector3(0f, topY - BackgroundHeight / 2f, 0f);
            t.localScale = Vector3.one;

            SpriteRenderer spriteRenderer = GetOrAdd<SpriteRenderer>(background);
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = tint;
            spriteRenderer.sortingOrder = -100;
            if (unlit != null) spriteRenderer.sharedMaterial = unlit;

            ParallaxBackground parallax = GetOrAdd<ParallaxBackground>(background);
            GameObject cameraBounds = GameObject.Find("CameraBounds");
            var serialized = new SerializedObject(parallax);
            serialized.FindProperty("followY").floatValue = 0f;
            serialized.FindProperty("fitToBounds").objectReferenceValue =
                cameraBounds != null ? cameraBounds.GetComponent<Collider2D>() : null;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }
    }
}
