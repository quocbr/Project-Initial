/*
Author: quocbr
Github: https://github.com/quocbr
Created: 2026-02-05
Description: Editor Window để thêm enum values mới vào PoolType hoặc ParticleType
*/

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor Window để quản lý và thêm enum values cho Pool Pattern
/// Menu: Tools/Pool Pattern/Enum Manager
/// </summary>
public class EnumManagerWindow : EditorWindow
{
    private const string POOL_ENUMS_PATH = "Assets/quocbr/DesignPattern/Pool Parttern/Core/PoolEnums.cs";
    private bool autoGenerateValue = true;

    // Colors
    private Color headerColor = new Color(0.3f, 0.5f, 0.8f);
    private string newEnumName = "";
    private string newEnumValue = "";
    private Color particleTypeColor = new Color(1f, 0.8f, 0.3f);
    private Color poolTypeColor = new Color(0.5f, 1f, 0.5f);
    private Vector2 scrollPosition;
    private EnumType selectedEnumType = EnumType.PoolType;

    private void OnGUI()
    {
        // Header
        DrawHeader();

        GUILayout.Space(10);

        // Enum Type Selector
        DrawEnumTypeSelector();

        GUILayout.Space(10);

        // Current Enums Display
        DrawCurrentEnums();

        GUILayout.Space(10);

        // Add New Enum Section
        DrawAddNewEnum();
    }

    [MenuItem("Tools/Pool Pattern/Enum Manager")]
    public static void ShowWindow()
    {
        var window = GetWindow<EnumManagerWindow>("Enum Manager");
        window.minSize = new Vector2(500, 610);
        window.Show();
    }

    private void DrawHeader()
    {
        // Background color
        var rect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(new Rect(0, 0, position.width, 80), headerColor);

        GUILayout.Space(10);

        // Title
        var titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        EditorGUILayout.LabelField("🔧 Pool Pattern Enum Manager", titleStyle);

        // Subtitle
        var subtitleStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.9f, 0.9f, 0.9f) }
        };
        EditorGUILayout.LabelField("Add new PoolType or ParticleType values", subtitleStyle);

        GUILayout.Space(10);
        EditorGUILayout.EndVertical();
    }

    private void DrawEnumTypeSelector()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // PoolType button
        Color oldBgColor = GUI.backgroundColor;
        GUI.backgroundColor = selectedEnumType == EnumType.PoolType ? poolTypeColor : Color.gray;
        if (GUILayout.Button("📦 PoolType", GUILayout.Width(200), GUILayout.Height(40)))
        {
            selectedEnumType = EnumType.PoolType;
            newEnumName = "";
        }

        GUILayout.Space(10);

        // ParticleType button
        GUI.backgroundColor = selectedEnumType == EnumType.ParticleType ? particleTypeColor : Color.gray;
        if (GUILayout.Button("🎆 ParticleType", GUILayout.Width(200), GUILayout.Height(40)))
        {
            selectedEnumType = EnumType.ParticleType;
            newEnumName = "";
        }

        GUI.backgroundColor = oldBgColor;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawCurrentEnums()
    {
        EditorGUILayout.LabelField($"Current {selectedEnumType} Values:", EditorStyles.boldLabel);

        // Get current enum values
        var enumValues = GetCurrentEnumValues(selectedEnumType.ToString());

        // Scroll view
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

        if (enumValues.Count == 0)
        {
            EditorGUILayout.HelpBox("No enum values found!", MessageType.Warning);
        }
        else
        {
            foreach (var kvp in enumValues.OrderBy(x => x.Value))
            {
                EditorGUILayout.BeginHorizontal("box");

                // Value
                EditorGUILayout.LabelField(kvp.Value.ToString(), GUILayout.Width(50));

                // Name
                EditorGUILayout.LabelField(kvp.Key, EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();

                // Delete button (không cho xóa None)
                if (kvp.Key != "None")
                {
                    GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
                    if (GUILayout.Button("🗑️ Delete", GUILayout.Width(80)))
                    {
                        if (EditorUtility.DisplayDialog("Confirm Delete",
                                $"Are you sure you want to delete '{kvp.Key}'?",
                                "Yes", "No"))
                        {
                            DeleteEnumValue(selectedEnumType.ToString(), kvp.Key);
                            Repaint();
                        }
                    }

                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();

        // Count
        EditorGUILayout.LabelField($"Total: {enumValues.Count} values", EditorStyles.miniLabel);
    }

    private void DrawAddNewEnum()
    {
        EditorGUILayout.LabelField("Add New Value:", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        // Name
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Name:", GUILayout.Width(100));
        newEnumName = EditorGUILayout.TextField(newEnumName);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Auto generate toggle
        autoGenerateValue = EditorGUILayout.Toggle("Auto Generate Value", autoGenerateValue);

        GUILayout.Space(5);

        // Value (if not auto)
        EditorGUI.BeginDisabledGroup(autoGenerateValue);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Value:", GUILayout.Width(100));
        newEnumValue = EditorGUILayout.TextField(newEnumValue);
        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();

        // Help box
        if (autoGenerateValue)
        {
            EditorGUILayout.HelpBox("✨ Value will be auto-generated (max + 1)", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Enter a custom integer value", MessageType.Info);
        }

        GUILayout.Space(10);

        // Preview
        if (!string.IsNullOrEmpty(newEnumName))
        {
            string previewValue = autoGenerateValue ? "Auto" : string.IsNullOrEmpty(newEnumValue) ? "0" : newEnumValue;
            EditorGUILayout.HelpBox($"Preview: {newEnumName} = {previewValue}", MessageType.None);
        }

        GUILayout.Space(10);

        // Buttons
        EditorGUILayout.BeginHorizontal();

        // Add button
        GUI.backgroundColor = selectedEnumType == EnumType.PoolType ? poolTypeColor : particleTypeColor;
        if (GUILayout.Button($"➕ Add to {selectedEnumType}", GUILayout.Height(40)))
        {
            AddNewEnumValue();
        }

        GUI.backgroundColor = Color.white;

        // Clear button
        if (GUILayout.Button("🧹 Clear", GUILayout.Width(100), GUILayout.Height(40)))
        {
            newEnumName = "";
            newEnumValue = "";
            autoGenerateValue = true;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // Bottom info
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.HelpBox($"📄 File: {POOL_ENUMS_PATH}", MessageType.None);
        if (GUILayout.Button("📝 Open File", GUILayout.Width(100)))
        {
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<TextAsset>(POOL_ENUMS_PATH));
        }

        EditorGUILayout.EndHorizontal();
    }

    private Dictionary<string, int> GetCurrentEnumValues(string enumTypeName)
    {
        var result = new Dictionary<string, int>();

        if (!File.Exists(POOL_ENUMS_PATH))
            return result;

        string fileContent = File.ReadAllText(POOL_ENUMS_PATH);
        string pattern = $@"public enum {enumTypeName}\s*{{\s*(?<content>[\s\S]*?)\s*}}";
        Match match = Regex.Match(fileContent, pattern);

        if (!match.Success)
            return result;

        string enumContent = match.Groups["content"].Value;

        // Parse enum values
        var matches = Regex.Matches(enumContent, @"(\w+)\s*=\s*(\d+)");
        foreach (Match m in matches)
        {
            string name = m.Groups[1].Value;
            int value = int.Parse(m.Groups[2].Value);
            result[name] = value;
        }

        return result;
    }

    private void AddNewEnumValue()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(newEnumName))
        {
            EditorUtility.DisplayDialog("Error", "Name cannot be empty!", "OK");
            return;
        }

        if (!Regex.IsMatch(newEnumName, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
        {
            EditorUtility.DisplayDialog("Error", "Invalid name! Use only letters, numbers, and underscores.", "OK");
            return;
        }

        int value = 0;
        if (!autoGenerateValue)
        {
            if (string.IsNullOrWhiteSpace(newEnumValue) || !int.TryParse(newEnumValue, out value))
            {
                EditorUtility.DisplayDialog("Error", "Value must be an integer!", "OK");
                return;
            }
        }

        // Read file
        if (!File.Exists(POOL_ENUMS_PATH))
        {
            EditorUtility.DisplayDialog("Error", "PoolEnums.cs not found!", "OK");
            return;
        }

        string fileContent = File.ReadAllText(POOL_ENUMS_PATH);
        string enumTypeName = selectedEnumType.ToString();
        string pattern = $@"public enum {enumTypeName}\s*{{\s*(?<content>[\s\S]*?)\s*}}";
        Match match = Regex.Match(fileContent, pattern);

        if (!match.Success)
        {
            EditorUtility.DisplayDialog("Error", $"Enum {enumTypeName} not found!", "OK");
            return;
        }

        string enumContent = match.Groups["content"].Value;

        // Check duplicate
        if (Regex.IsMatch(enumContent, $@"\b{newEnumName}\b"))
        {
            EditorUtility.DisplayDialog("Error", $"'{newEnumName}' already exists!", "OK");
            return;
        }

        // Auto generate value
        if (autoGenerateValue)
        {
            var matches = Regex.Matches(enumContent, @"=\s*(\d+)");
            int maxValue = 0;
            foreach (Match m in matches)
            {
                if (int.TryParse(m.Groups[1].Value, out int val))
                {
                    if (val > maxValue)
                        maxValue = val;
                }
            }

            value = maxValue + 1;
        }

        // ✅ FIX: Thêm dấu phẩy vào enum entry cuối nếu chưa có
        // (Vì enum mới sẽ không có dấu phẩy, enum trước đó cần có)
        var allEnumMatches = Regex.Matches(enumContent, @"(\w+)\s*=\s*(\d+)");

        if (allEnumMatches.Count > 0)
        {
            // Lấy match cuối cùng
            Match lastEnum = allEnumMatches[allEnumMatches.Count - 1];
            int lastEnumEndPos = lastEnum.Index + lastEnum.Length;

            // Check xem sau enum có dấu phẩy không (bỏ qua whitespace và comment)
            string afterLastEnum = enumContent.Substring(lastEnumEndPos);
            var commaCheck = Regex.Match(afterLastEnum, @"^\s*,");

            if (!commaCheck.Success)
            {
                // Chưa có dấu phẩy - cần thêm vào
                // Tính vị trí trong full file content
                int enumContentStartInFile = match.Index + match.Value.IndexOf(enumContent);
                int commaInsertPosInFile = enumContentStartInFile + lastEnumEndPos;

                // Insert dấu phẩy ngay sau giá trị enum
                fileContent = fileContent.Insert(commaInsertPosInFile, ",");

                Debug.Log($"✅ Added comma after: {lastEnum.Groups[1].Value}");

                // Parse lại match vì content đã thay đổi
                match = Regex.Match(fileContent, pattern);
            }
        }

        // Insert position (before last })
        int insertPos = match.Index + match.Length - 1;

        // Create entry - Không có dấu phẩy ở enum cuối cùng
        // ✅ Format: \n\n + comment + \n + enum + \n (để } xuống dòng)
        string newEntry = $"\n  {newEnumName} = {value}\n";

        // Insert
        string newContent = fileContent.Insert(insertPos, newEntry);

        // Write file
        File.WriteAllText(POOL_ENUMS_PATH, newContent);

        // Refresh
        AssetDatabase.Refresh();

        Debug.Log($"✅ Added: {newEnumName} = {value} to {enumTypeName}");
        EditorUtility.DisplayDialog("Success!", $"Added successfully!\n\n{newEnumName} = {value}", "OK");

        // Clear fields
        newEnumName = "";
        newEnumValue = "";
        autoGenerateValue = true;

        Repaint();
    }

    private void DeleteEnumValue(string enumTypeName, string enumName)
    {
        if (!File.Exists(POOL_ENUMS_PATH))
            return;

        string fileContent = File.ReadAllText(POOL_ENUMS_PATH);

        // Tìm enum block
        string enumPattern = $@"public enum {enumTypeName}\s*{{\s*(?<content>[\s\S]*?)\s*}}";
        Match enumMatch = Regex.Match(fileContent, enumPattern);

        if (!enumMatch.Success)
        {
            Debug.LogError($"Cannot find enum {enumTypeName}");
            return;
        }

        string enumContent = enumMatch.Groups["content"].Value;

        // Tìm tất cả enum entries
        var allEnums = new List<(string name, int value, int startIndex, int length)>();
        foreach (Match m in Regex.Matches(enumContent, @"(\w+)\s*=\s*(\d+)"))
        {
            allEnums.Add((m.Groups[1].Value, int.Parse(m.Groups[2].Value), m.Index, m.Length));
        }

        // Tìm index của enum cần xóa
        int deleteIndex = -1;
        for (int i = 0; i < allEnums.Count; i++)
        {
            if (allEnums[i].name == enumName)
            {
                deleteIndex = i;
                break;
            }
        }

        if (deleteIndex == -1)
        {
            Debug.LogError($"Cannot find enum {enumName}");
            return;
        }

        bool isLastEnum = deleteIndex == allEnums.Count - 1;

        // ✅ Xóa enum line (bao gồm comment nếu có)
        // Pattern tìm: comment (optional) + enum line + comma (optional) + trailing comment (optional)
        string deletePattern = $@"(\s*//[^\r\n]*[\r\n]+)?\s*{enumName}\s*=\s*\d+\s*,?\s*(//[^\r\n]*)?[\r\n]+";
        string newContent = Regex.Replace(fileContent, deletePattern, "");

        // ✅ Nếu xóa enum cuối, cần xóa dấu phẩy của enum TRƯỚC ĐÓ
        if (isLastEnum && deleteIndex > 0)
        {
            string prevEnumName = allEnums[deleteIndex - 1].name;

            // Pattern: tìm enum trước và xóa dấu phẩy sau nó
            string removeCommaPattern = $@"({prevEnumName}\s*=\s*\d+)\s*,";
            newContent = Regex.Replace(newContent, removeCommaPattern, "$1");

            Debug.Log($"✅ Removed trailing comma from: {prevEnumName}");
        }

        // ✅ Đảm bảo dấu } luôn ở dòng riêng (không dính với enum cuối)
        // Pattern: tìm (number)(optional whitespace)} và thay bằng (number)\n}
        newContent = Regex.Replace(newContent,
            $@"(public enum {enumTypeName}\s*{{[^}}]*?)(\d+)(\s*)}}",
            "$1$2\n}");

        // Write file
        File.WriteAllText(POOL_ENUMS_PATH, newContent);

        // Refresh
        AssetDatabase.Refresh();

        Debug.Log($"🗑️ Deleted: {enumName} from {enumTypeName}");
    }
}
#endif