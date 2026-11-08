using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Quocbr.Installer
{
    public sealed class QuocbrModuleInstallerWindow : EditorWindow
    {
        private const string SourceKey = "QuocbrInstaller.SourceRoot";
        private const string SelectionKey = "QuocbrInstaller.Selection.";
        private const string LastResultKey = "QuocbrInstaller.LastResult";
        private const string PendingKey = "QuocbrInstaller.Pending.";
        private const string InstallerAssetPath = "Assets/quocbr/Installer";

        [Serializable]
        private sealed class PendingInstall
        {
            public string sourceRoot;
            public string[] moduleIds;
            public string[] pluginIds;
            public string importingPluginId;
        }

        private static AddAndRemoveRequest packageRequest;
        private static Task<string> downloadTask;
        private static PluginSpec downloadingPlugin;
        private static PendingInstall pending;
        private static string pendingKey;
        private static double waitUntil;

        private readonly HashSet<string> selected = new HashSet<string>();
        private readonly HashSet<string> selectedForRemoval = new HashSet<string>();
        private string sourceRoot;
        private int activeTab;
        private string search = "";
        private bool onlySelected;
        private bool showPlugins = true;
        private int categoryIndex;
        private Vector2 scroll;
        private static readonly string[] Categories = { "Tất cả", "Lõi", "Giao diện", "Tiện ích", "Mobile SDK" };

        [MenuItem("Tools/Quocbr/Module Installer")]
        public static void ShowWindow()
        {
            var window = GetWindow<QuocbrModuleInstallerWindow>("Quocbr Modules");
            window.minSize = new Vector2(600, 620);
        }

        [MenuItem("Tools/Quocbr/Export Installer Package")]
        public static void ExportInstaller()
        {
            if (!AssetDatabase.IsValidFolder(InstallerAssetPath))
            {
                EditorUtility.DisplayDialog("Quocbr Installer", "Installer source folder was not found.", "OK");
                return;
            }

            string output = EditorUtility.SaveFilePanel("Export installer", "", "QuocbrInstaller", "unitypackage");
            if (string.IsNullOrEmpty(output)) return;
            AssetDatabase.ExportPackage(InstallerAssetPath, output, ExportPackageOptions.Recurse);
            Debug.Log("[Quocbr Installer] Exported " + output);
        }

        [MenuItem("Tools/Quocbr/Export Installer Package", true)]
        private static bool CanExportInstaller()
        {
            return AssetDatabase.IsValidFolder(InstallerAssetPath);
        }

        [InitializeOnLoadMethod]
        private static void ResumeAfterReload()
        {
            pendingKey = PendingKey + Application.dataPath;
            string json = EditorPrefs.GetString(pendingKey, "");
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                pending = JsonUtility.FromJson<PendingInstall>(json);
                if (pending == null) throw new InvalidDataException("Trạng thái cài đặt trống.");
                pending.moduleIds = pending.moduleIds ?? Array.Empty<string>();
                pending.pluginIds = pending.pluginIds ?? Array.Empty<string>();
                if (pending.moduleIds.Any(id => !QuocbrModuleCatalog.Modules.Any(m => m.Id == id)) ||
                    pending.pluginIds.Any(id => !QuocbrModuleCatalog.Plugins.Any(p => p.Id == id)) ||
                    (!string.IsNullOrEmpty(pending.importingPluginId) &&
                     !QuocbrModuleCatalog.Plugins.Any(p => p.Id == pending.importingPluginId)))
                    throw new InvalidDataException("Trạng thái cài đặt chứa module hoặc plugin không hợp lệ.");
            }
            catch (Exception ex)
            {
                pending = null;
                EditorPrefs.DeleteKey(pendingKey);
                Debug.LogError("[Quocbr Installer] Invalid pending operation: " + ex.Message);
                return;
            }

            waitUntil = EditorApplication.timeSinceStartup + 120;
            EditorApplication.update -= PollInstall;
            EditorApplication.update += PollInstall;
        }

        private void OnEnable()
        {
            selected.Clear();
            foreach (string id in EditorPrefs.GetString(SelectionKey + Application.dataPath, "")
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                if (QuocbrModuleCatalog.Modules.Any(m => m.Id == id)) selected.Add(id);
            sourceRoot = EditorPrefs.GetString(SourceKey, "");
            if (string.IsNullOrEmpty(sourceRoot))
            {
                string currentProject = Directory.GetParent(Application.dataPath).FullName;
                if (Directory.Exists(Path.Combine(currentProject, "Assets", "quocbr", "DesignPattern")))
                    sourceRoot = currentProject;
            }
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(SelectionKey + Application.dataPath,
                string.Join(",", selected.OrderBy(id => id)));
        }

        private void OnGUI()
        {
            DrawHeader();
            activeTab = GUILayout.Toolbar(activeTab, new[] { "Tích hợp module", "Gỡ module" },
                GUILayout.Height(32));
            scroll = EditorGUILayout.BeginScrollView(scroll);
            GUILayout.Space(12);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(16);
                using (new EditorGUILayout.VerticalScope())
                {
                    if (activeTab == 0)
                    {
                        DrawSourceSection();
                        GUILayout.Space(18);
                        DrawModuleSection();
                        GUILayout.Space(18);
                        DrawPluginSection();
                    }
                    else DrawRemovalSection();
                    GUILayout.Space(12);
                }
                GUILayout.Space(16);
            }
            EditorGUILayout.EndScrollView();
            if (activeTab == 0) DrawInstallBar();
            else DrawRemovalBar();
        }

        private void DrawHeader()
        {
            var rect = GUILayoutUtility.GetRect(0, 76, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
                ? new Color(0.13f, 0.21f, 0.28f) : new Color(0.86f, 0.93f, 0.97f));
            var title = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18 };
            var subtitle = new GUIStyle(EditorStyles.label) { fontSize = 11 };
            GUI.Label(new Rect(rect.x + 18, rect.y + 12, rect.width - 36, 27),
                "Quocbr  /  Module Installer", title);
            GUI.Label(new Rect(rect.x + 18, rect.y + 43, rect.width - 36, 20),
                "Tích hợp hoặc gỡ các module Quocbr trong project này.", subtitle);
        }

        private void DrawSourceSection()
        {
            DrawSectionTitle("01", "Project nguồn", "Nơi lấy module để tích hợp");
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                sourceRoot = EditorGUILayout.TextField(sourceRoot ?? "", GUILayout.Height(26));
                if (EditorGUI.EndChangeCheck()) EditorPrefs.SetString(SourceKey, sourceRoot);
                if (GUILayout.Button("Chọn thư mục...", GUILayout.Width(126), GUILayout.Height(26)))
                {
                    string chosen = EditorUtility.OpenFolderPanel("Chọn project nguồn", sourceRoot, "");
                    if (!string.IsNullOrEmpty(chosen))
                    {
                        sourceRoot = chosen;
                        EditorPrefs.SetString(SourceKey, sourceRoot);
                    }
                }
            }
            bool valid = IsSourceValid(sourceRoot);
            EditorGUILayout.LabelField(valid
                ? "✓ Đã tìm thấy Assets/quocbr trong project nguồn."
                : IsCurrentProject(sourceRoot)
                    ? "Project nguồn phải khác project đang mở trong Unity."
                    : "Chọn thư mục project chứa Assets/quocbr và ProjectSettings.",
                EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawModuleSection()
        {
            DrawSectionTitle("02", "Chọn module", "Bấm vào bất kỳ vị trí nào trên thẻ để chọn hoặc bỏ chọn");
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Tìm kiếm", EditorStyles.miniLabel, GUILayout.Width(58));
                search = EditorGUILayout.TextField(search, GUILayout.Height(25));
                onlySelected = GUILayout.Toggle(onlySelected, "Chỉ xem đã chọn",
                    EditorStyles.miniButton, GUILayout.Width(128), GUILayout.Height(25));
            }
            GUILayout.Space(6);
            categoryIndex = GUILayout.Toolbar(categoryIndex, Categories, GUILayout.Height(27));
            GUILayout.Space(7);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(selected.Count + " / " + QuocbrModuleCatalog.Modules.Length + " module đã chọn",
                    EditorStyles.miniLabel);
                if (GUILayout.Button("Chọn mục hiển thị", EditorStyles.miniButton, GUILayout.Width(132)))
                    foreach (var module in QuocbrModuleCatalog.Modules)
                        if (IsVisible(module)) selected.Add(module.Id);
                using (new EditorGUI.DisabledScope(selected.Count == 0))
                    if (GUILayout.Button("Bỏ chọn tất cả", EditorStyles.miniButton, GUILayout.Width(96)))
                        selected.Clear();
            }
            GUILayout.Space(3);
            int visible = 0;
            foreach (var module in QuocbrModuleCatalog.Modules)
            {
                if (!IsVisible(module)) continue;
                visible++;
                DrawModuleCard(module);
                GUILayout.Space(5);
            }
            if (visible == 0)
                EditorGUILayout.HelpBox("Không tìm thấy module phù hợp. Hãy đổi từ khóa hoặc bộ lọc.",
                    MessageType.Info);
        }

        private void DrawRemovalSection()
        {
            DrawSectionTitle("01", "Module đã tích hợp", "Chỉ hiển thị module do panel này quản lý");
            QuocbrInstallRegistry registry;
            try { registry = QuocbrInstallRegistry.Load(); }
            catch (Exception ex)
            {
                EditorGUILayout.HelpBox(ex.Message, MessageType.Error);
                return;
            }
            var installedIds = new HashSet<string>(registry.modules.Select(m => m.id));
            selectedForRemoval.RemoveWhere(id => !installedIds.Contains(id));
            if (registry.modules.Count == 0)
            {
                EditorGUILayout.HelpBox("Chưa có module nào được panel ghi nhận. Asset có sẵn hoặc được " +
                    "cài trước khi có tính năng gỡ cần được xử lý thủ công.", MessageType.Info);
                return;
            }
            EditorGUILayout.LabelField(registry.modules.Count + " module đang được quản lý",
                EditorStyles.miniLabel);
            GUILayout.Space(5);
            foreach (var module in QuocbrModuleCatalog.Modules)
            {
                var record = registry.modules.FirstOrDefault(m => m.id == module.Id);
                if (record == null) continue;
                DrawModuleCard(module, true, record.assets.Length);
                GUILayout.Space(5);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Chọn tất cả", EditorStyles.miniButton, GUILayout.Width(100)))
                    foreach (var record in registry.modules) selectedForRemoval.Add(record.id);
                using (new EditorGUI.DisabledScope(selectedForRemoval.Count == 0))
                    if (GUILayout.Button("Bỏ chọn", EditorStyles.miniButton, GUILayout.Width(90)))
                        selectedForRemoval.Clear();
            }
            EditorGUILayout.HelpBox("Plugin bên ngoài được giữ lại để tránh ảnh hưởng các phần khác " +
                "trong project. Asset đã sửa sau khi tích hợp sẽ không bị xóa tự động.", MessageType.Info);
        }

        private void DrawRemovalBar()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("02   Xem trước và gỡ", EditorStyles.boldLabel);
                try
                {
                    var registry = QuocbrInstallRegistry.Load();
                    var installedIds = new HashSet<string>(registry.modules.Select(m => m.id));
                    var ids = selectedForRemoval.Where(installedIds.Contains).ToArray();
                    var retained = registry.modules.Where(m => !ids.Contains(m.id)).ToArray();
                    var blockers = retained.Where(m => QuocbrModuleCatalog.ResolveModules(new[] { m.id })
                        .Any(dependency => ids.Contains(dependency.Id))).Select(m =>
                            QuocbrModuleCatalog.FindModule(m.id).Name).ToArray();
                    int assetCount = registry.GetRemovalAssets(ids).Count;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(ids.Length + " module · " + assetCount +
                            " asset có thể gỡ", EditorStyles.miniLabel);
                        using (new EditorGUI.DisabledScope(ids.Length == 0 || blockers.Length > 0 || pending != null))
                            if (GUILayout.Button("Gỡ " + ids.Length + " module",
                                GUILayout.Width(168), GUILayout.Height(42)))
                                BeginRemoval(registry, ids, assetCount);
                    }
                    if (blockers.Length > 0)
                        EditorGUILayout.HelpBox("Cần gỡ thêm module phụ thuộc: " +
                            string.Join(", ", blockers), MessageType.Warning);
                }
                catch (Exception ex) { EditorGUILayout.HelpBox(ex.Message, MessageType.Error); }
                string lastResult = EditorPrefs.GetString(LastResultKey + Application.dataPath, "");
                if (!string.IsNullOrEmpty(lastResult)) EditorGUILayout.HelpBox(lastResult, MessageType.None);
            }
        }

        private void BeginRemoval(QuocbrInstallRegistry registry, string[] ids, int assetCount)
        {
            string names = Names(ids.Select(QuocbrModuleCatalog.FindModule));
            if (!EditorUtility.DisplayDialog("Gỡ module Quocbr",
                "Gỡ " + names + " và xóa " + assetCount +
                " asset do panel đã sao chép? Plugin bên ngoài sẽ được giữ lại.",
                "Gỡ module", "Hủy")) return;
            try
            {
                int deleted = registry.RemoveModules(ids);
                selectedForRemoval.Clear();
                EditorPrefs.SetString(LastResultKey + Application.dataPath,
                    "Đã gỡ " + ids.Length + " module, xóa " + deleted + " asset.");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                EditorPrefs.SetString(LastResultKey + Application.dataPath,
                    "Không thể gỡ: " + ex.Message);
                Debug.LogError("[Quocbr Installer] Remove failed: " + ex);
            }
        }

        private bool IsVisible(ModuleSpec module)
        {
            return (categoryIndex == 0 || GetCategory(module) == categoryIndex) &&
                (!onlySelected || selected.Contains(module.Id)) &&
                (string.IsNullOrWhiteSpace(search) ||
                 module.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 module.Description.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void DrawModuleCard(ModuleSpec module, bool removal = false, int managedAssets = 0)
        {
            var selection = removal ? selectedForRemoval : selected;
            bool chosen = selection.Contains(module.Id);
            bool manual = !removal && module.Plugins.Select(QuocbrModuleCatalog.FindPlugin)
                .Any(p => p.Kind == PluginKind.Manual && !IsPluginInstalled(p));
            string dependency = removal ? managedAssets + " asset được panel quản lý" :
                module.Dependencies.Length == 0 ? null : "Tự thêm: " +
                string.Join(", ", module.Dependencies.Select(id => QuocbrModuleCatalog.FindModule(id).Name));
            float width = Mathf.Max(280, EditorGUIUtility.currentViewWidth - 58);
            var descriptionStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel);
            float descriptionHeight = descriptionStyle.CalcHeight(new GUIContent(module.Description), width - 58);
            float height = 48 + descriptionHeight + (dependency == null && !manual ? 8 : 25);
            var rect = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                Color background = chosen
                    ? (EditorGUIUtility.isProSkin ? new Color(0.16f, 0.29f, 0.39f) : new Color(0.87f, 0.94f, 0.99f))
                    : (EditorGUIUtility.isProSkin ? new Color(0.22f, 0.23f, 0.25f) : new Color(0.96f, 0.96f, 0.96f));
                EditorGUI.DrawRect(rect, background);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, chosen ? 4 : 1, rect.height),
                    chosen ? new Color(0.22f, 0.62f, 0.94f) : new Color(0.45f, 0.48f, 0.5f, 0.3f));
            }
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (GUI.Button(rect, new GUIContent("", "Bấm để " + (chosen ? "bỏ chọn " : "chọn ") + module.Name),
                GUIStyle.none))
            {
                if (chosen) selection.Remove(module.Id);
                else selection.Add(module.Id);
                Repaint();
            }
            var check = new Rect(rect.x + 15, rect.y + 13, 25, 25);
            EditorGUI.DrawRect(check, chosen ? new Color(0.22f, 0.62f, 0.94f) :
                (EditorGUIUtility.isProSkin ? new Color(0.36f, 0.38f, 0.41f) : new Color(0.79f, 0.81f, 0.83f)));
            GUI.Label(check, chosen ? "✓" : "+", new GUIStyle(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter, fontSize = 16,
                normal = { textColor = Color.white }
            });
            var nameStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 };
            GUI.Label(new Rect(rect.x + 51, rect.y + 10, rect.width - 190, 23), module.Name, nameStyle);
            GUI.Label(new Rect(rect.xMax - 137, rect.y + 13, 123, 19),
                chosen ? (removal ? "SẼ GỠ" : "ĐÃ CHỌN") :
                    (removal ? "BẤM ĐỂ GỠ" : "BẤM ĐỂ CHỌN"), new GUIStyle(EditorStyles.miniLabel) {
                    alignment = TextAnchor.MiddleRight,
                    normal = { textColor = chosen ? new Color(0.24f, 0.7f, 1f) :
                        (EditorGUIUtility.isProSkin ? new Color(0.75f, 0.77f, 0.8f) : new Color(0.36f, 0.4f, 0.44f)) }
                });
            GUI.Label(new Rect(rect.x + 51, rect.y + 36, rect.width - 66, descriptionHeight),
                module.Description, descriptionStyle);
            if (dependency != null || manual)
                GUI.Label(new Rect(rect.x + 51, rect.yMax - 22, rect.width - 66, 18),
                    (dependency == null ? "" : dependency + (manual ? "  ·  " : "")) +
                    (manual ? "Cần cài plugin thủ công" : ""), EditorStyles.miniLabel);
        }

        private void DrawPluginSection()
        {
            var requested = QuocbrModuleCatalog.ResolveModules(selected);
            var plugins = QuocbrModuleCatalog.ResolvePlugins(requested);
            showPlugins = EditorGUILayout.Foldout(showPlugins,
                "03   Plugin cần thiết  (" + plugins.Count + ")", true, EditorStyles.foldoutHeader);
            if (showPlugins)
            {
                if (plugins.Count == 0)
                    EditorGUILayout.LabelField("Chọn module để xem plugin cần cài.", EditorStyles.miniLabel);
                foreach (var plugin in plugins) DrawPlugin(plugin);
            }
        }

        private void DrawInstallBar()
        {
            var requested = QuocbrModuleCatalog.ResolveModules(selected);
            var ready = GetReadyModules(requested, out var blocked, includeMissingAuto: true);
            bool sourceValid = IsSourceValid(sourceRoot);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField("04   Xem trước và cài đặt", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField(ready.Count + " sẵn sàng  ·  " + blocked.Count +
                            " cần xử lý", EditorStyles.miniLabel);
                    }
                    using (new EditorGUI.DisabledScope(!sourceValid || ready.Count == 0 || pending != null))
                        if (GUILayout.Button("Cài " + ready.Count + " module  →",
                            GUILayout.Width(168), GUILayout.Height(42)))
                            BeginInstall(ready);
                }
                if (blocked.Count > 0)
                    EditorGUILayout.LabelField("Cần plugin thủ công: " + Names(blocked),
                        EditorStyles.wordWrappedMiniLabel);
                else if (!sourceValid)
                    EditorGUILayout.LabelField("Chọn project nguồn hợp lệ để bắt đầu.", EditorStyles.miniLabel);
                if (pending != null)
                    EditorGUILayout.HelpBox(downloadingPlugin != null
                        ? "Đang tải " + downloadingPlugin.Name + " từ GitHub..."
                        : !string.IsNullOrEmpty(pending.importingPluginId)
                            ? "Unity đang import " + QuocbrModuleCatalog.FindPlugin(pending.importingPluginId).Name
                            : "Đang xử lý package. Panel sẽ tiếp tục sau khi Unity reload.", MessageType.Info);
                string lastResult = EditorPrefs.GetString(LastResultKey + Application.dataPath, "");
                if (!string.IsNullOrEmpty(lastResult))
                    EditorGUILayout.HelpBox(lastResult, MessageType.None);
            }
        }

        private static void DrawSectionTitle(string number, string name, string hint)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(number, new GUIStyle(EditorStyles.boldLabel) {
                    fontSize = 15, normal = { textColor = new Color(0.22f, 0.62f, 0.94f) }
                }, GUILayout.Width(30));
                GUILayout.Label(name, new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 });
            }
            EditorGUILayout.LabelField(hint, EditorStyles.miniLabel);
            GUILayout.Space(6);
        }

        private static int GetCategory(ModuleSpec module)
        {
            if (module.Id == "mobile") return 4;
            if (module.Id == "ui" || module.Id == "popup" || module.Id == "screen" ||
                module.Id == "image" || module.Id == "text" || module.Id == "ui-utilities") return 2;
            if (module.Id == "debug" || module.Id == "helpers" || module.Id == "tween") return 3;
            return 1;
        }

        private static void DrawPlugin(PluginSpec plugin)
        {
            bool installed = IsPluginInstalled(plugin);
            string status = installed ? "Đã cài" : plugin.Kind == PluginKind.Manual
                ? "Cần cài thủ công" : plugin.Kind == PluginKind.GithubRelease
                    ? "Tự tải GitHub" : "Tự cài UPM";
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(plugin.Name, EditorStyles.boldLabel);
                    GUILayout.Label(status, EditorStyles.miniLabel, GUILayout.Width(108));
                    if (!installed && plugin.Kind != PluginKind.Manual)
                    {
                        using (new EditorGUI.DisabledScope(pending != null))
                        {
                            if (GUILayout.Button("Cài ngay", GUILayout.Width(74), GUILayout.Height(25)))
                                BeginPluginInstall(plugin);
                        }
                    }
                    if (!string.IsNullOrEmpty(plugin.SourceUrl) &&
                        GUILayout.Button("Nguồn ↗", GUILayout.Width(74), GUILayout.Height(25)))
                        Application.OpenURL(plugin.SourceUrl);
                }
                if (!installed && plugin.Kind == PluginKind.Manual)
                    EditorGUILayout.LabelField(plugin.Instructions, EditorStyles.wordWrappedMiniLabel);
                if (!installed && plugin.Kind == PluginKind.GithubRelease)
                    EditorGUILayout.LabelField("Gói phát hành đã ghim; kiểm tra SHA-256 trước khi import.",
                        EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static string Names(IEnumerable<ModuleSpec> modules)
        {
            string value = string.Join(", ", modules.Select(m => m.Name));
            return string.IsNullOrEmpty(value) ? "(không có)" : value;
        }

        private static bool IsSourceValid(string root)
        {
            if (string.IsNullOrWhiteSpace(root)) return false;
            try
            {
                return File.Exists(Path.Combine(root, "ProjectSettings", "ProjectVersion.txt")) &&
                    Directory.Exists(Path.Combine(root, "Assets", "quocbr", "DesignPattern")) &&
                    !IsCurrentProject(root);
            }
            catch (ArgumentException) { return false; }
        }

        private static bool IsCurrentProject(string root)
        {
            if (string.IsNullOrWhiteSpace(root)) return false;
            try { return string.Equals(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar),
                QuocbrInstallRegistry.ProjectRoot.TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase); }
            catch (Exception) { return false; }
        }

        private static List<ModuleSpec> GetReadyModules(List<ModuleSpec> requested,
            out List<ModuleSpec> blocked, bool includeMissingAuto)
        {
            var ready = new List<ModuleSpec>();
            blocked = new List<ModuleSpec>();
            var blockedIds = new HashSet<string>();
            foreach (var module in requested)
            {
                bool missingDependency = module.Dependencies.Any(blockedIds.Contains);
                bool missingPlugin = false;
                foreach (var pluginId in module.Plugins)
                {
                    var plugin = QuocbrModuleCatalog.FindPlugin(pluginId);
                    if (IsPluginInstalled(plugin)) continue;
                    if (plugin.Kind == PluginKind.Manual || !includeMissingAuto)
                        missingPlugin = true;
                }
                if (missingDependency || missingPlugin)
                {
                    blockedIds.Add(module.Id);
                    blocked.Add(module);
                }
                else ready.Add(module);
            }
            return ready;
        }

        private static bool IsPluginInstalled(PluginSpec plugin)
        {
            if (plugin.Kind != PluginKind.Upm)
                return File.Exists(ToProjectPath(plugin.Probe));
            string manifest = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "Packages", "manifest.json");
            if (!File.Exists(manifest)) return false;
            string content = File.ReadAllText(manifest);
            return Regex.IsMatch(content, "\"" + Regex.Escape(plugin.Id == "adjust" ? "com.adjust.sdk" :
                plugin.InstallId.Split('@')[0]) + "\"\\s*:");
        }

        private static string ToProjectPath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static List<PluginSpec> PendingPlugins(List<ModuleSpec> modules)
        {
            var ids = QuocbrModuleCatalog.ResolvePlugins(modules).Select(p => p.Id);
            return QuocbrModuleCatalog.ResolvePlugins(ids.Concat(pending.pluginIds ?? Array.Empty<string>()));
        }

        private static void BeginPluginInstall(PluginSpec plugin)
        {
            if (pending != null) return;
            try
            {
                pendingKey = PendingKey + Application.dataPath;
                pending = new PendingInstall {
                    sourceRoot = "",
                    moduleIds = Array.Empty<string>(),
                    pluginIds = new[] { plugin.Id }
                };
                EditorPrefs.SetString(pendingKey, JsonUtility.ToJson(pending));
                EditorPrefs.DeleteKey(LastResultKey + Application.dataPath);
                var missingUpm = PendingPlugins(new List<ModuleSpec>())
                    .Where(p => p.Kind == PluginKind.Upm && !IsPluginInstalled(p))
                    .Select(p => p.InstallId).Distinct().ToArray();
                waitUntil = EditorApplication.timeSinceStartup + 120;
                EditorApplication.update -= PollInstall;
                EditorApplication.update += PollInstall;
                if (missingUpm.Length > 0)
                {
                    packageRequest = Client.AddAndRemove(missingUpm, Array.Empty<string>());
                }
                else PollInstall();
            }
            catch (Exception ex) { FailInstall(ex.Message); }
        }

        private void BeginInstall(List<ModuleSpec> modules)
        {
            try
            {
                string root = Path.GetFullPath(sourceRoot);
                if (IsCurrentProject(root))
                    throw new InvalidOperationException("Project nguồn phải khác project đích.");
                var files = CollectFiles(root, modules);
                ValidateFiles(root, files);
                var missingUpm = QuocbrModuleCatalog.ResolvePlugins(modules)
                    .Where(p => p.Kind == PluginKind.Upm && !IsPluginInstalled(p))
                    .Select(p => p.InstallId).Distinct().ToArray();

                pendingKey = PendingKey + Application.dataPath;
                pending = new PendingInstall {
                    sourceRoot = root, moduleIds = modules.Select(m => m.Id).ToArray(),
                    pluginIds = Array.Empty<string>()
                };
                EditorPrefs.SetString(pendingKey, JsonUtility.ToJson(pending));
                EditorPrefs.DeleteKey(LastResultKey + Application.dataPath);
                waitUntil = EditorApplication.timeSinceStartup + 120;
                EditorApplication.update -= PollInstall;
                EditorApplication.update += PollInstall;
                if (missingUpm.Length == 0)
                {
                    PollInstall();
                    return;
                }

                packageRequest = Client.AddAndRemove(missingUpm, Array.Empty<string>());
            }
            catch (Exception ex)
            {
                FailInstall(ex.Message);
            }
        }

        private static void PollInstall()
        {
            try { PollInstallCore(); }
            catch (Exception ex) { FailInstall(ex.Message); }
        }

        private static void PollInstallCore()
        {
            if (pending == null)
            {
                EditorApplication.update -= PollInstall;
                return;
            }
            if (packageRequest != null)
            {
                if (!packageRequest.IsCompleted) return;
                if (packageRequest.Status == StatusCode.Failure)
                {
                    string error = packageRequest.Error != null
                        ? packageRequest.Error.message : "Unknown package error";
                    packageRequest = null;
                    InstallAvailableModules(error);
                    return;
                }
                packageRequest = null;
            }

            var modules = QuocbrModuleCatalog.ResolveModules(pending.moduleIds);
            var missing = PendingPlugins(modules)
                .Where(p => p.Kind == PluginKind.Upm && !IsPluginInstalled(p)).ToList();
            if (missing.Count > 0)
            {
                if (EditorApplication.timeSinceStartup < waitUntil) return;
                InstallAvailableModules("UPM chưa cài xong: " +
                    string.Join(", ", missing.Select(p => p.Name)));
                return;
            }

            if (!string.IsNullOrEmpty(pending.importingPluginId))
            {
                var importing = QuocbrModuleCatalog.FindPlugin(pending.importingPluginId);
                if (IsPluginInstalled(importing) &&
                    !EditorApplication.isCompiling && !EditorApplication.isUpdating)
                {
                    pending.importingPluginId = null;
                    EditorPrefs.SetString(pendingKey, JsonUtility.ToJson(pending));
                }
                else if (EditorApplication.timeSinceStartup >= waitUntil)
                {
                    InstallAvailableModules("Import chưa hoàn tất: " + importing.Name);
                    return;
                }
                else return;
            }

            if (downloadTask != null)
            {
                if (!downloadTask.IsCompleted) return;
                PluginSpec plugin = downloadingPlugin;
                Task<string> completed = downloadTask;
                downloadTask = null;
                downloadingPlugin = null;
                try
                {
                    string packagePath = completed.GetAwaiter().GetResult();
                    pending.importingPluginId = plugin.Id;
                    EditorPrefs.SetString(pendingKey, JsonUtility.ToJson(pending));
                    waitUntil = EditorApplication.timeSinceStartup + 120;
                    AssetDatabase.ImportPackage(packagePath, false);
                }
                catch (Exception ex)
                {
                    pending.importingPluginId = null;
                    InstallAvailableModules("Không cài được " + plugin.Name + ": " + ex.Message);
                }
                return;
            }

            var nextRelease = PendingPlugins(modules)
                .FirstOrDefault(p => p.Kind == PluginKind.GithubRelease && !IsPluginInstalled(p));
            if (nextRelease != null)
            {
                downloadingPlugin = nextRelease;
                downloadTask = Task.Run(() => DownloadRelease(nextRelease));
                return;
            }

            EditorApplication.update -= PollInstall;
            try { FinishInstall(); }
            catch (Exception ex) { FailInstall(ex.Message); }
        }

        private static void InstallAvailableModules(string reason)
        {
            try
            {
                var requested = QuocbrModuleCatalog.ResolveModules(pending.moduleIds);
                var ready = GetReadyModules(requested, out var blocked, includeMissingAuto: false);
                if (ready.Count == 0)
                {
                    FailInstall(reason + ". Không có module độc lập để cài.");
                    return;
                }
                pending.moduleIds = ready.Select(m => m.Id).ToArray();
                EditorPrefs.SetString(pendingKey, JsonUtility.ToJson(pending));
                EditorApplication.update -= PollInstall;
                FinishInstall("Đã bỏ qua " + Names(blocked) + " vì " + reason);
            }
            catch (Exception ex) { FailInstall(ex.Message); }
        }

        private static async Task<string> DownloadRelease(PluginSpec plugin)
        {
            if (plugin.Kind != PluginKind.GithubRelease ||
                !Uri.TryCreate(plugin.DownloadUrl, UriKind.Absolute, out var uri) ||
                uri.Scheme != Uri.UriSchemeHttps || uri.Host != "github.com" ||
                string.IsNullOrEmpty(plugin.Sha256))
                throw new InvalidOperationException("GitHub release source is not pinned: " + plugin.Name);

            string cache = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "Library", "QuocbrInstaller", "Downloads");
            Directory.CreateDirectory(cache);
            string path = Path.Combine(cache, plugin.Id + "-" + Path.GetFileName(uri.LocalPath));
            if (File.Exists(path) && VerifySha256(path, plugin.Sha256)) return path;
            if (File.Exists(path)) File.Delete(path);

            string temporary = path + ".download";
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Quocbr-Unity-Installer/1.0");
                    using (var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var input = await response.Content.ReadAsStreamAsync())
                        using (var output = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                            await input.CopyToAsync(output);
                    }
                }
                if (!VerifySha256(temporary, plugin.Sha256))
                    throw new IOException("SHA-256 không khớp với release chính thức");
                File.Move(temporary, path);
                return path;
            }
            finally
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        private static bool VerifySha256(string path, string expected)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
            {
                string actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
                return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
            }
        }

        private static void FinishInstall(string note = null)
        {
            var modules = QuocbrModuleCatalog.ResolveModules(pending.moduleIds);
            string installedNames = modules.Count == 0
                ? string.Join(", ", (pending.pluginIds ?? Array.Empty<string>())
                    .Select(id => QuocbrModuleCatalog.FindPlugin(id).Name))
                : Names(modules);
            var files = CollectFiles(pending.sourceRoot, modules);
            ValidateFiles(pending.sourceRoot, files);
            var registry = QuocbrInstallRegistry.Load();
            var moduleAssets = modules.ToDictionary(m => m.Id,
                m => CollectFiles(pending.sourceRoot, new[] { m }).ToArray());
            var copiedAssets = new List<string>();
            var createdFolders = new List<string>();
            try
            {
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach (string assetPath in files)
                    {
                        string from = FromSource(pending.sourceRoot, assetPath);
                        string to = ToProjectPath(assetPath);
                        EnsureDirectoryWithMeta(pending.sourceRoot,
                            Path.GetDirectoryName(assetPath).Replace('\\', '/'), createdFolders);
                        if (File.Exists(to)) continue;
                        copiedAssets.Add(assetPath);
                        File.Copy(from, to);
                        File.Copy(from + ".meta", to + ".meta");
                    }
                }
                finally { AssetDatabase.StopAssetEditing(); }
                registry.RecordInstall(moduleAssets, copiedAssets, createdFolders);
            }
            catch
            {
                foreach (string assetPath in copiedAssets)
                {
                    string target = ToProjectPath(assetPath);
                    if (File.Exists(target)) File.Delete(target);
                    if (File.Exists(target + ".meta")) File.Delete(target + ".meta");
                }
                foreach (string folder in createdFolders.OrderByDescending(path => path.Length))
                {
                    string target = ToProjectPath(folder);
                    if (Directory.Exists(target) && !Directory.EnumerateFileSystemEntries(target).Any())
                    {
                        Directory.Delete(target);
                        if (File.Exists(target + ".meta")) File.Delete(target + ".meta");
                    }
                }
                AssetDatabase.Refresh();
                throw;
            }

            EditorPrefs.SetString(LastResultKey + Application.dataPath,
                (modules.Count == 0 ? "Đã cài plugin được chọn." :
                "Đã cài " + modules.Count + " module, thêm " + copiedAssets.Count +
                " asset. Unity đang import.") +
                (string.IsNullOrEmpty(note) ? "" : "\n" + note));
            EditorPrefs.DeleteKey(pendingKey);
            pending = null;
            Debug.Log("[Quocbr Installer] Installed: " + installedNames);
            AssetDatabase.Refresh();
        }

        private static void FailInstall(string message)
        {
            EditorApplication.update -= PollInstall;
            packageRequest = null;
            downloadTask = null;
            downloadingPlugin = null;
            pending = null;
            if (!string.IsNullOrEmpty(pendingKey)) EditorPrefs.DeleteKey(pendingKey);
            EditorPrefs.SetString(LastResultKey + Application.dataPath, "Cài đặt dừng: " + message);
            Debug.LogError("[Quocbr Installer] " + message);
        }

        private static SortedSet<string> CollectFiles(string source, IEnumerable<ModuleSpec> modules)
        {
            var files = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var module in modules)
            {
                foreach (string assetPath in module.Paths)
                {
                    if (!assetPath.StartsWith("Assets/quocbr/", StringComparison.Ordinal))
                        throw new InvalidOperationException("Unsafe catalog path: " + assetPath);
                    string fullPath = FromSource(source, assetPath);
                    if (File.Exists(fullPath)) files.Add(assetPath);
                    else if (Directory.Exists(fullPath))
                    {
                        foreach (string file in Directory.GetFiles(fullPath, "*", SearchOption.AllDirectories))
                        {
                            if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                            files.Add(file.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar,
                                Path.AltDirectorySeparatorChar).Replace('\\', '/'));
                        }
                    }
                    else throw new FileNotFoundException("Source asset missing", fullPath);
                }
            }
            return files;
        }

        private static string FromSource(string source, string assetPath)
        {
            string full = Path.GetFullPath(Path.Combine(source,
                assetPath.Replace('/', Path.DirectorySeparatorChar)));
            string prefix = Path.GetFullPath(source).TrimEnd(Path.DirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Path escapes source project: " + assetPath);
            return full;
        }

        private static void ValidateFiles(string source, IEnumerable<string> files)
        {
            foreach (string assetPath in files)
            {
                string from = FromSource(source, assetPath);
                string to = ToProjectPath(assetPath);
                if (!File.Exists(from + ".meta"))
                    throw new FileNotFoundException("Source .meta missing", from + ".meta");
                if (File.Exists(to))
                {
                    if (!File.Exists(to + ".meta") || !SameFile(from, to) ||
                        !SameFile(from + ".meta", to + ".meta"))
                        throw new IOException("Asset already exists with different content or GUID: " + assetPath);
                }
                else if (File.Exists(to + ".meta"))
                    throw new IOException("Orphan .meta in target project: " + assetPath);

                string guid = ExtractGuid(from + ".meta");
                if (string.IsNullOrEmpty(guid))
                    throw new IOException("Invalid .meta GUID: " + assetPath);
                string otherPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(otherPath) &&
                    !string.Equals(otherPath, assetPath, StringComparison.OrdinalIgnoreCase))
                    throw new IOException("GUID already belongs to " + otherPath + ": " + assetPath);
            }
        }

        private static string ExtractGuid(string metaPath)
        {
            foreach (string line in File.ReadLines(metaPath))
                if (line.StartsWith("guid: ", StringComparison.Ordinal)) return line.Substring(6).Trim();
            return null;
        }

        private static bool SameFile(string left, string right)
        {
            if (new FileInfo(left).Length != new FileInfo(right).Length) return false;
            using (var sha = SHA256.Create())
            using (var a = File.OpenRead(left))
            using (var b = File.OpenRead(right))
                return sha.ComputeHash(a).SequenceEqual(sha.ComputeHash(b));
        }

        private static void EnsureDirectoryWithMeta(string source, string assetDirectory,
            List<string> createdFolders)
        {
            if (assetDirectory == "Assets" || string.IsNullOrEmpty(assetDirectory)) return;
            string parent = Path.GetDirectoryName(assetDirectory).Replace('\\', '/');
            EnsureDirectoryWithMeta(source, parent, createdFolders);
            string target = ToProjectPath(assetDirectory);
            if (Directory.Exists(target)) return;
            Directory.CreateDirectory(target);
            if (assetDirectory != "Assets/quocbr") createdFolders.Add(assetDirectory);
            string sourceMeta = FromSource(source, assetDirectory) + ".meta";
            if (!File.Exists(sourceMeta)) throw new FileNotFoundException("Folder .meta missing", sourceMeta);
            File.Copy(sourceMeta, target + ".meta");
        }
    }
}
