/*
Author: quocbr
Location: Phải đặt trong thư mục tên là "Editor"
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace quocbr.DebugLogger
{
    [InitializeOnLoad] // <--- Đây là chìa khóa: Chạy ngay khi Unity load xong (sau khi import)
    public class DebugLoggerInstaller
    {
        private const string SYMBOL = "ENABLE_DEBUG_LOGGER";

        // Static Constructor sẽ chạy ngay khi script được compile xong
        static DebugLoggerInstaller()
        {
            EnsureDefineSymbol();
        }

        private static void EnsureDefineSymbol()
        {
            // Lấy target hiện tại (PC, Android, iOS...)
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            
            // Lấy danh sách symbols hiện tại
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            // Nếu chưa có symbol thì thêm vào
            if (!defines.Contains(SYMBOL))
            {
                if (string.IsNullOrEmpty(defines))
                    defines = SYMBOL;
                else
                    defines += ";" + SYMBOL;

                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
                
                // In ra log để người dùng biết gói đã được cài đặt thành công
                Debug.Log($"<color=green><b>[DebugLogger]</b></color> Package Imported! Auto-added define symbol: <b>{SYMBOL}</b>");
            }
        }
        
        [MenuItem("Tools/Quocbr/Toggle Debug Logger")]
        public static void ToggleDebugLogger()
        {
            BuildTargetGroup currentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentGroup);
            
            if (definesString.Contains(SYMBOL))
            {
                // Đang bật -> Tắt (Xóa đi)
                if (definesString.Contains(";" + SYMBOL)) 
                    definesString = definesString.Replace(";" + SYMBOL, "");
                else if (definesString.Contains(SYMBOL + ";")) 
                    definesString = definesString.Replace(SYMBOL + ";", "");
                else 
                    definesString = definesString.Replace(SYMBOL, "");
                
                Debug.Log($"[DebugLogger] <color=red>DISABLED</color>");
            }
            else
            {
                // Đang tắt -> Bật (Thêm vào)
                if (definesString.Length > 0 && !definesString.EndsWith(";")) definesString += ";";
                definesString += SYMBOL;
                
                Debug.Log($"[DebugLogger] <color=green>ENABLED</color>");
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentGroup, definesString);
        }
    }
}
#endif