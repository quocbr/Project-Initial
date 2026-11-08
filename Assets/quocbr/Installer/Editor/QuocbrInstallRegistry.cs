using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace Quocbr.Installer
{
    [Serializable]
    internal sealed class ManagedModule
    {
        public string id;
        public string[] assets;
    }

    [Serializable]
    internal sealed class ManagedAsset
    {
        public string path;
        public string hash;
        public string metaHash;
    }

    [Serializable]
    internal sealed class QuocbrInstallRegistry
    {
        public int version = 1;
        public List<ManagedModule> modules = new List<ManagedModule>();
        public List<ManagedAsset> ownedAssets = new List<ManagedAsset>();
        public List<ManagedAsset> ownedFolders = new List<ManagedAsset>();

        internal static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        private static string RegistryPath => Path.Combine(ProjectRoot, "ProjectSettings", "QuocbrInstallerRegistry.json");

        internal static QuocbrInstallRegistry Load()
        {
            if (!File.Exists(RegistryPath)) return new QuocbrInstallRegistry();
            var registry = JsonUtility.FromJson<QuocbrInstallRegistry>(File.ReadAllText(RegistryPath));
            if (registry == null || registry.version != 1 || registry.modules == null ||
                registry.ownedAssets == null || registry.ownedFolders == null ||
                registry.modules.Any(m => m == null || m.assets == null ||
                    !QuocbrModuleCatalog.Modules.Any(spec => spec.Id == m.id)) ||
                registry.ownedAssets.Any(a => a == null || a.path == null) ||
                registry.ownedFolders.Any(a => a == null || a.path == null))
                throw new InvalidDataException("QuocbrInstallerRegistry.json không hợp lệ.");
            foreach (var asset in registry.ownedAssets) SafePath(asset.path);
            foreach (var folder in registry.ownedFolders) SafePath(folder.path);
            return registry;
        }

        private void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RegistryPath));
            string temporary = RegistryPath + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(this, true));
            if (File.Exists(RegistryPath)) File.Replace(temporary, RegistryPath, null);
            else File.Move(temporary, RegistryPath);
        }

        internal void RecordInstall(Dictionary<string, string[]> moduleAssets,
            IEnumerable<string> copiedAssets, IEnumerable<string> createdFolders)
        {
            foreach (string path in copiedAssets)
            {
                var snapshot = Snapshot(path);
                int index = ownedAssets.FindIndex(a =>
                    string.Equals(a.path, path, StringComparison.OrdinalIgnoreCase));
                if (index < 0) ownedAssets.Add(snapshot);
                else ownedAssets[index] = snapshot;
            }
            foreach (string path in createdFolders)
            {
                var snapshot = Snapshot(path);
                int index = ownedFolders.FindIndex(a =>
                    string.Equals(a.path, path, StringComparison.OrdinalIgnoreCase));
                if (index < 0) ownedFolders.Add(snapshot);
                else ownedFolders[index] = snapshot;
            }

            var ownedPaths = new HashSet<string>(ownedAssets.Select(a => a.path),
                StringComparer.OrdinalIgnoreCase);
            foreach (var pair in moduleAssets)
            {
                // Existing project assets are not claimed by the installer.
                string[] tracked = pair.Value.Where(ownedPaths.Contains).Distinct().ToArray();
                var record = modules.FirstOrDefault(m => m.id == pair.Key);
                if (record == null)
                {
                    if (tracked.Length == 0) continue;
                    modules.Add(new ManagedModule { id = pair.Key, assets = tracked });
                }
                else record.assets = record.assets.Concat(tracked).Distinct().ToArray();
            }
            Save();
        }

        internal List<ManagedAsset> GetRemovalAssets(IEnumerable<string> moduleIds)
        {
            var ids = new HashSet<string>(moduleIds);
            var retained = new HashSet<string>(modules.Where(m => !ids.Contains(m.id))
                .SelectMany(m => m.assets), StringComparer.OrdinalIgnoreCase);
            var removed = new HashSet<string>(modules.Where(m => ids.Contains(m.id))
                .SelectMany(m => m.assets), StringComparer.OrdinalIgnoreCase);
            return ownedAssets.Where(a => removed.Contains(a.path) && !retained.Contains(a.path)).ToList();
        }

        internal int RemoveModules(IEnumerable<string> moduleIds)
        {
            var ids = new HashSet<string>(moduleIds);
            if (ids.Count == 0 || ids.Any(id => modules.All(m => m.id != id)))
                throw new InvalidOperationException("Module cần gỡ không còn trong danh sách quản lý.");
            foreach (var retained in modules.Where(m => !ids.Contains(m.id)))
                if (QuocbrModuleCatalog.ResolveModules(new[] { retained.id })
                    .Any(dependency => ids.Contains(dependency.Id)))
                    throw new InvalidOperationException("Module khác vẫn phụ thuộc vào " + retained.id);
            var assets = GetRemovalAssets(ids);
            foreach (var asset in assets)
            {
                string file = SafePath(asset.path);
                bool hasFile = File.Exists(file);
                bool hasMeta = File.Exists(file + ".meta");
                if (!hasFile && !hasMeta) continue;
                if (!hasMeta || Hash(file + ".meta") != asset.metaHash ||
                    (hasFile && Hash(file) != asset.hash))
                    throw new IOException("Asset đã được chỉnh sửa; hãy xử lý thủ công: " + asset.path);
            }

            int deleted = 0;
            foreach (var asset in assets)
            {
                string file = SafePath(asset.path);
                if (File.Exists(file))
                {
                    File.Delete(file);
                    deleted++;
                }
                if (File.Exists(file + ".meta")) File.Delete(file + ".meta");
            }

            modules.RemoveAll(m => ids.Contains(m.id));
            ownedAssets.RemoveAll(a => assets.Contains(a));
            foreach (var folder in ownedFolders.OrderByDescending(f => f.path.Length).ToArray())
            {
                string directory = SafePath(folder.path);
                if (!Directory.Exists(directory))
                {
                    ownedFolders.Remove(folder);
                    continue;
                }
                if (Directory.EnumerateFileSystemEntries(directory).Any() ||
                    !File.Exists(directory + ".meta") || Hash(directory + ".meta") != folder.metaHash)
                    continue;
                Directory.Delete(directory);
                File.Delete(directory + ".meta");
                ownedFolders.Remove(folder);
            }
            Save();
            return deleted;
        }

        private static ManagedAsset Snapshot(string assetPath)
        {
            string path = SafePath(assetPath);
            return new ManagedAsset { path = assetPath,
                hash = File.Exists(path) ? Hash(path) : null,
                metaHash = Hash(path + ".meta") };
        }

        private static string Hash(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static string SafePath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) ||
                !assetPath.StartsWith("Assets/quocbr/", StringComparison.OrdinalIgnoreCase) ||
                (assetPath.Equals("Assets/quocbr/Installer", StringComparison.OrdinalIgnoreCase) ||
                 assetPath.StartsWith("Assets/quocbr/Installer/", StringComparison.OrdinalIgnoreCase)) ||
                assetPath.Contains("..") || assetPath.Contains('\\'))
                throw new InvalidDataException("Đường dẫn registry không hợp lệ: " + assetPath);
            string root = Path.GetFullPath(Path.Combine(ProjectRoot, "Assets", "quocbr")) +
                Path.DirectorySeparatorChar;
            string full = Path.GetFullPath(Path.Combine(ProjectRoot,
                assetPath.Replace('/', Path.DirectorySeparatorChar)));
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Đường dẫn registry vượt ra ngoài quocbr: " + assetPath);
            return full;
        }
    }
}
