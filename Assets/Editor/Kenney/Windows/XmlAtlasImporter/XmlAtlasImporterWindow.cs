namespace Cozy.Editor.Kenney.Assets.Editor.Kenney.Windows
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using UnityEditor;
    using UnityEngine;

    public class XmlAtlasImporter : EditorWindow
    {
        private Texture2D atlasTexture;
        private TextAsset xmlFile;

        private Sprite[] importedSprites;
        private Vector2 scrollPos;

        private static readonly Color RowLight = new(0.22f, 0.22f, 0.22f);
        private static readonly Color RowDark  = new(0.18f, 0.18f, 0.18f);

        private List<SpriteEntry> allEntries = new();
        private List<SpriteEntry> filteredEntries = new();

        private string searchQuery = "";

        [MenuItem("Tools/Kenney/Import XML Atlas")]
        private static void ShowWindow()
        {
            var window = GetWindow<XmlAtlasImporter>(
                utility: true,
                title: "XML Atlas Importer",
                focus: true
            );

            window.minSize = new(700, 500);
            window.Show();
        }

        [MenuItem("Assets/Open in XML Atlas Importer", true)]
        private static bool ValidateOpenXml()
        {
            return Selection.activeObject is TextAsset;
        }

        [MenuItem("Assets/Open in XML Atlas Importer")]
        private static void OpenXml()
        {
            var window = GetWindow<XmlAtlasImporter>("XML Atlas Importer");
            window.xmlFile = Selection.activeObject as TextAsset;
            window.Show();
        }

        [MenuItem("Assets/Open Atlas in XML Atlas Importer", true)]
        private static bool ValidateOpenTexture()
        {
            return Selection.activeObject is Texture2D;
        }

        [MenuItem("Assets/Open Atlas in XML Atlas Importer")]
        private static void OpenTexture()
        {
            var window = GetWindow<XmlAtlasImporter>("XML Atlas Importer");
            window.atlasTexture = Selection.activeObject as Texture2D;
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Sprite Atlas Importer", EditorStyles.boldLabel);

            atlasTexture = (Texture2D)EditorGUILayout.ObjectField("Atlas Texture", atlasTexture, typeof(Texture2D), false);
            xmlFile      = (TextAsset)EditorGUILayout.ObjectField("XML File", xmlFile, typeof(TextAsset), false);

            if (atlasTexture != null)
            {
                var info = LoadImportMetadata(atlasTexture);
                if (info != null)
                {
                    GUILayout.Space(10);
                    GUILayout.Label("Stored Import Metadata", EditorStyles.boldLabel);
                    GUILayout.Label($"XML: {info.xmlPath}", EditorStyles.miniLabel);
                    GUILayout.Label($"Imported: {info.importDate}", EditorStyles.miniLabel);

                    if (GUILayout.Button("Reimport Using Stored XML"))
                    {
                        xmlFile = AssetDatabase.LoadAssetAtPath<TextAsset>(info.xmlPath);
                        if (xmlFile != null)
                            ImportAtlas();
                        else
                            Debug.LogWarning("Stored XML file not found.");
                    }
                }
            }

            GUILayout.Space(10);

            GUI.enabled = atlasTexture != null && xmlFile != null;

            if (GUILayout.Button("Import Atlas", GUILayout.Height(30)))
                ImportAtlas();

            GUI.enabled = true;

            GUILayout.Space(20);

            DrawSpriteList();
        }

        // ---------------------------------------------------------
        // Import Logic
        // ---------------------------------------------------------

        private void ImportAtlas()
        {
            string texturePath = AssetDatabase.GetAssetPath(atlasTexture);
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;

            if (importer == null)
            {
                Debug.LogError("Could not load TextureImporter for atlas.");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var sprites = new List<SpriteMetaData>();
            XDocument xml = XDocument.Parse(xmlFile.text);

            foreach (var sub in xml.Root.Elements("SubTexture"))
            {
                string name = Path.GetFileNameWithoutExtension((string)sub.Attribute("name"));

                float x = (float)sub.Attribute("x");
                float y = (float)sub.Attribute("y");
                float w = (float)sub.Attribute("width");
                float h = (float)sub.Attribute("height");

                Rect rect = new(
                    x,
                    atlasTexture.height - y - h,
                    w,
                    h
                );

                var meta = new SpriteMetaData
                {
                    name = name,
                    rect = rect,
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new(0.5f, 0.5f),
                    border = Vector4.zero
                };

                sprites.Add(meta);
            }

#pragma warning disable CS0618
            importer.spritesheet = sprites.ToArray();
#pragma warning restore CS0618

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();

            importedSprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>()
                .OrderBy(s => s.name)
                .ToArray();

            allEntries = importedSprites.Select(s => new SpriteEntry(s)).ToList();
            filteredEntries = new List<SpriteEntry>(allEntries);

            SaveImportMetadata(atlasTexture, AssetDatabase.GetAssetPath(xmlFile));

            Debug.Log($"Imported {sprites.Count} sprites from XML atlas.");
        }

        // ---------------------------------------------------------
        // Sprite List UI (two columns)
        // ---------------------------------------------------------

        private void DrawSpriteList()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search:", GUILayout.Width(50));
            string newSearch = GUILayout.TextField(searchQuery);
            GUILayout.EndHorizontal();

            if (newSearch != searchQuery)
            {
                searchQuery = newSearch;
                ApplySearchFilter();
            }

            if (importedSprites == null || importedSprites.Length == 0)
                return;

            GUILayout.Label("Imported Sprites", EditorStyles.boldLabel);

            GUILayout.BeginVertical("box");
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            const float rowHeight = 120f;

            for (int i = 0; i < filteredEntries.Count; i += 2)
            {
                Rect rowRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(rowHeight));

                if (Event.current.type == EventType.Repaint)
                    EditorGUI.DrawRect(rowRect, (i / 2) % 2 == 0 ? RowLight : RowDark);

                GUILayout.Space(6);

                DrawSpriteItem(filteredEntries[i]);

                Rect divider = GUILayoutUtility.GetRect(1, rowHeight, GUILayout.Width(1));
                EditorGUI.DrawRect(divider, new(0f, 0f, 0f, 0.35f));

                GUILayout.Space(6);

                if (i + 1 < filteredEntries.Count)
                    DrawSpriteItem(filteredEntries[i + 1]);
                else
                    GUILayout.FlexibleSpace();

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(2);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawSpriteItem(SpriteEntry entry)
        {
            const float frameSize = 50f;
            const int maxNameLength = 22;

            GUILayout.BeginVertical(GUILayout.Width(320));
            GUILayout.BeginHorizontal();

            GUILayout.Box("", GUILayout.Width(frameSize), GUILayout.Height(frameSize));
            Rect frameRect = GUILayoutUtility.GetLastRect();

            Texture2D tex = entry.sprite.texture;
            Rect r = entry.sprite.textureRect;

            Rect uv = new(
                r.x / tex.width,
                r.y / tex.height,
                r.width / tex.width,
                r.height / tex.height
            );

            float spriteW = entry.sprite.rect.width;
            float spriteH = entry.sprite.rect.height;
            float spriteAspect = spriteW / spriteH;

            float drawW = frameSize;
            float drawH = frameSize;

            if (spriteAspect > 1f)
                drawH = frameSize / spriteAspect;
            else
                drawW = frameSize * spriteAspect;

            Rect drawRect = new(
                frameRect.x + (frameSize - drawW) * 0.5f,
                frameRect.y + (frameSize - drawH) * 0.5f,
                drawW,
                drawH
            );

            GUI.DrawTextureWithTexCoords(drawRect, tex, uv);

            GUILayout.Space(10);

            GUILayout.BeginVertical(GUILayout.Width(180));

            string displayName = TruncateName(entry.name, maxNameLength);
            GUILayout.Label(displayName, EditorStyles.whiteLabel);

            GUILayout.Label($"Size: {entry.size.x}×{entry.size.y}", EditorStyles.miniLabel);
            GUILayout.Label($"Rect: ({(int)entry.rect.x}, {(int)entry.rect.y}, {(int)entry.rect.width}, {(int)entry.rect.height})", EditorStyles.miniLabel);
            GUILayout.Label($"Pivot: {entry.sprite.pivot}", EditorStyles.miniLabel);
            GUILayout.Label($"Border: {entry.sprite.border}", EditorStyles.miniLabel);

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Width(90));

            if (GUILayout.Button("Sprite Editor"))
            {
                Selection.activeObject = entry.sprite;
                EditorApplication.ExecuteMenuItem("Window/2D/Sprite Editor");
            }

            if (GUILayout.Button("Select"))
            {
                Selection.activeObject = entry.sprite;
                EditorGUIUtility.PingObject(entry.sprite);
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private string TruncateName(string name, int maxLength)
        {
            if (name.Length <= maxLength)
                return name;

            return name[..(maxLength - 1)] + "…";
        }

        private void SaveImportMetadata(Texture2D atlas, string xmlPath)
        {
            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(atlas));

            var info = new AtlasImportInfo
            {
                xmlPath = xmlPath,
                importDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            importer.userData = JsonUtility.ToJson(info);
            importer.SaveAndReimport();
        }

        private AtlasImportInfo LoadImportMetadata(Texture2D atlas)
        {
            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(atlas));
            if (string.IsNullOrEmpty(importer.userData))
                return null;

            return JsonUtility.FromJson<AtlasImportInfo>(importer.userData);
        }
        
        private void ApplySearchFilter()
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                filteredEntries = new List<SpriteEntry>(allEntries);
                return;
            }

            string q = searchQuery.ToLowerInvariant();
            filteredEntries = allEntries
                .Where(e => e.lowerName.Contains(q))
                .ToList();
        }
    }
}
