using System;
using System.Collections.Generic;

namespace Quocbr.Installer
{
    internal enum PluginKind { Upm, GithubRelease, Manual }

    internal sealed class PluginSpec
    {
        internal readonly string Id;
        internal readonly string Name;
        internal readonly PluginKind Kind;
        internal readonly string InstallId;
        internal readonly string Probe;
        internal readonly string Instructions;
        internal readonly string SourceUrl;
        internal readonly string DownloadUrl;
        internal readonly string Sha256;
        internal readonly string[] Dependencies;

        internal PluginSpec(string id, string name, string installId, string probe = null,
            string instructions = null, string sourceUrl = null, string downloadUrl = null,
            string sha256 = null, string[] dependencies = null)
        {
            Id = id;
            Name = name;
            Kind = installId != null ? PluginKind.Upm :
                downloadUrl != null ? PluginKind.GithubRelease : PluginKind.Manual;
            InstallId = installId;
            Probe = probe;
            Instructions = instructions;
            SourceUrl = sourceUrl;
            DownloadUrl = downloadUrl;
            Sha256 = sha256;
            Dependencies = dependencies ?? Array.Empty<string>();
        }
    }

    internal sealed class ModuleSpec
    {
        internal readonly string Id;
        internal readonly string Name;
        internal readonly string Description;
        internal readonly string[] Paths;
        internal readonly string[] Dependencies;
        internal readonly string[] Plugins;

        internal ModuleSpec(string id, string name, string description, string[] paths,
            string[] dependencies = null, string[] plugins = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Paths = paths;
            Dependencies = dependencies ?? Array.Empty<string>();
            Plugins = plugins ?? Array.Empty<string>();
        }
    }

    // A path denotes either one asset or one folder. Source .meta files travel with assets.
    internal static class QuocbrModuleCatalog
    {
        internal static readonly PluginSpec[] Plugins =
        {
            new PluginSpec("ui-module", "Unity UI built-in module", "com.unity.modules.ui@1.0.0"),
            new PluginSpec("imgui-module", "Unity IMGUI built-in module", "com.unity.modules.imgui@1.0.0"),
            new PluginSpec("webrequest-module", "Unity Web Request built-in module",
                "com.unity.modules.unitywebrequest@1.0.0"),
            new PluginSpec("ugui", "Unity UI / TextMeshPro", "com.unity.ugui@2.0.0"),
            new PluginSpec("purchasing", "Unity Purchasing", "com.unity.purchasing@5.5.0"),
            new PluginSpec("edm", "External Dependency Manager", "com.unity.external-dependency-manager@2.1.0"),
            new PluginSpec("adjust", "Adjust SDK",
                "https://github.com/adjust/unity_sdk.git?path=Assets/Adjust",
                sourceUrl: "https://dev.adjust.com/en/sdk/unity/",
                dependencies: new[] { "edm" }),
            new PluginSpec("dotween", "DOTween", null, "Assets/Plugins/Demigiant/DOTween/DOTween.dll",
                "Import DOTween from its official distribution, then run DOTween Setup.",
                sourceUrl: "https://dotween.demigiant.com/"),
            new PluginSpec("odin", "Odin Inspector", null,
                "Assets/Plugins/Sirenix/Assemblies/Sirenix.OdinInspector.Attributes.dll",
                "Import Odin Inspector with a valid license.",
                sourceUrl: "https://odininspector.com/"),
            new PluginSpec("firebase", "Firebase Analytics and Remote Config", null,
                "Assets/Firebase/Plugins/Firebase.App.dll",
                "Import Firebase App, Analytics and Remote Config SDKs; resolve native dependencies.",
                sourceUrl: "https://github.com/firebase/firebase-unity-sdk/releases",
                dependencies: new[] { "edm" }),
            new PluginSpec("admob", "Google Mobile Ads 11.6.0", null,
                "Assets/GoogleMobileAds/GoogleMobileAds.Unity.dll",
                "Configure app IDs after import.",
                sourceUrl: "https://github.com/googleads/googleads-mobile-unity/releases/tag/v11.6.0",
                downloadUrl: "https://github.com/googleads/googleads-mobile-unity/releases/download/v11.6.0/GoogleMobileAds-v11.6.0.unitypackage",
                sha256: "e42c4af9284a6efa3e925ecb58047b722e07c12ac28dceb4547bb743f3db4763",
                dependencies: new[] { "edm" }),
            new PluginSpec("max", "AppLovin MAX 8.6.6", null, "Assets/MaxSdk/Scripts/MaxSdk.cs",
                "Configure ad unit IDs after import.",
                sourceUrl: "https://github.com/AppLovin/AppLovin-MAX-Unity-Plugin/releases/tag/release_8_6_6",
                downloadUrl: "https://github.com/AppLovin/AppLovin-MAX-Unity-Plugin/releases/download/release_8_6_6/AppLovin-MAX-Unity-Plugin-8.6.6-Android-13.6.4-iOS-13.6.4.unitypackage",
                sha256: "62f699916ee06e337e61573efd2255328c99ad2378b12de91c5a1be02f348eba",
                dependencies: new[] { "edm", "ui-module", "ugui", "webrequest-module" }),
            new PluginSpec("playreview", "Google Play In-App Review", null,
                "Assets/GooglePlayPlugins/com.google.play.review/Runtime/Scripts/ReviewManager.cs",
                "Import Google Play Review plugin for Android builds.",
                sourceUrl: "https://developers.google.com/unity/packages")
        };

        internal static readonly ModuleSpec[] Modules =
        {
            new ModuleSpec("singleton", "Singleton", "MonoBehaviour singleton.",
                new[] { "Assets/quocbr/DesignPattern/Singleton.cs" }),
            new ModuleSpec("events", "Event Manager", "Typed in-process events.",
                new[] { "Assets/quocbr/DesignPattern/EventManager.cs" }),
            new ModuleSpec("state", "State Machine", "State interfaces and generic machine; examples excluded.",
                new[] {
                    "Assets/quocbr/DesignPattern/StateMachine/IState.cs",
                    "Assets/quocbr/DesignPattern/StateMachine/BaseState.cs",
                    "Assets/quocbr/DesignPattern/StateMachine/StateMachine.cs"
                }),
            new ModuleSpec("factory", "Factory", "ScriptableObject lookup factory.",
                new[] { "Assets/quocbr/DesignPattern/Factory/BaseFactorySO.cs" }),
            new ModuleSpec("helpers", "Core Helpers", "Color and vector helpers.",
                new[] {
                    "Assets/quocbr/Constants/ColorConst.cs",
                    "Assets/quocbr/Helpers/ColorHelper.cs",
                    "Assets/quocbr/Helpers/Vector2Helper.cs"
                }),
            new ModuleSpec("ui-utilities", "UI Utilities", "Canvas and screen-space helpers.",
                new[] { "Assets/quocbr/Helpers/UtilitiesHelper.cs" },
                plugins: new[] { "ui-module" }),
            new ModuleSpec("image", "Image Helper", "Unity UI image extensions.",
                new[] { "Assets/quocbr/Helpers/ImageExtension.cs" },
                plugins: new[] { "ui-module", "ugui" }),
            new ModuleSpec("text", "Text Helper", "TextMeshPro extensions.",
                new[] { "Assets/quocbr/Helpers/TextExtension.cs" },
                plugins: new[] { "ui-module", "ugui" }),
            new ModuleSpec("tween", "Tween Helper", "DOTween helper methods.",
                new[] { "Assets/quocbr/Helpers/TweenHelper.cs" }, plugins: new[] { "dotween" }),
            new ModuleSpec("debug", "Debug Logger", "Logger and editor menu.",
                new[] { "Assets/quocbr/DebugLogger" }, new[] { "helpers" }),
            new ModuleSpec("mini-pool", "Mini Pool", "Local generic component pool.",
                new[] {
                    "Assets/quocbr/DesignPattern/Pooling/Core/IPoolable.cs",
                    "Assets/quocbr/DesignPattern/Pooling/Core/MiniPool.cs",
                    "Assets/quocbr/DesignPattern/Pooling/Core/PoolEnums.cs"
                }),
            new ModuleSpec("pool", "Object and Particle Pool", "Pool controller, enum editor and prefab; demo excluded.",
                new[] {
                    "Assets/quocbr/DesignPattern/Pooling/Core/GameUnit.cs",
                    "Assets/quocbr/DesignPattern/Pooling/Core/PoolEnums.cs",
                    "Assets/quocbr/DesignPattern/Pooling/Core/SimplePool.cs",
                    "Assets/quocbr/DesignPattern/Pooling/Core/ParticlePool.cs",
                    "Assets/quocbr/DesignPattern/Pooling/Core/PoolController.cs",
                    "Assets/quocbr/DesignPattern/Pooling/Core/Editor",
                    "Assets/quocbr/DesignPattern/Pooling/Pool Controller.prefab"
                }, new[] { "mini-pool" }, new[] { "odin" }),
            new ModuleSpec("popup", "Popup System", "Popup stack, animation and manager prefab.",
                new[] { "Assets/quocbr/PopupSystem" }, plugins: new[] { "ui-module", "ugui" }),
            new ModuleSpec("ui", "UI System", "Canvas manager and prefab.",
                new[] { "Assets/quocbr/UISystem" }, new[] { "singleton" },
                new[] { "ui-module", "ugui", "dotween", "odin" }),
            new ModuleSpec("screen", "Sprite Screen Fitter", "Fit sprites to the screen.",
                new[] { "Assets/quocbr/Tags/SpriteScreenFitter.cs" },
                plugins: new[] { "imgui-module" }),
            new ModuleSpec("mobile", "Mobile SDK Bundle",
                "Ads, IAP, Firebase, remote config, runtime and boosters; tightly coupled.",
                new[] {
                    "Assets/quocbr/SDK",
                    "Assets/quocbr/Booster",
                    "Assets/quocbr/Helpers/Ultils.cs"
                }, new[] { "singleton", "ui" },
                new[] {
                    "ugui", "purchasing", "edm", "adjust", "dotween", "odin",
                    "firebase", "admob", "max", "playreview"
                })
        };

        internal static ModuleSpec FindModule(string id)
        {
            foreach (var module in Modules)
                if (module.Id == id) return module;
            throw new ArgumentException("Unknown module: " + id);
        }

        internal static PluginSpec FindPlugin(string id)
        {
            foreach (var plugin in Plugins)
                if (plugin.Id == id) return plugin;
            throw new ArgumentException("Unknown plugin: " + id);
        }

        internal static List<ModuleSpec> ResolveModules(IEnumerable<string> ids)
        {
            var result = new List<ModuleSpec>();
            var visited = new HashSet<string>();
            foreach (var id in ids) Visit(id, visited, result);
            return result;
        }

        private static void Visit(string id, HashSet<string> visited, List<ModuleSpec> result)
        {
            if (!visited.Add(id)) return;
            var module = FindModule(id);
            foreach (var dependency in module.Dependencies) Visit(dependency, visited, result);
            result.Add(module);
        }

        internal static List<PluginSpec> ResolvePlugins(IEnumerable<ModuleSpec> modules)
        {
            var result = new List<PluginSpec>();
            var visited = new HashSet<string>();
            foreach (var module in modules)
                foreach (var id in module.Plugins)
                    VisitPlugin(id, visited, result);
            return result;
        }

        internal static List<PluginSpec> ResolvePlugins(IEnumerable<string> ids)
        {
            var result = new List<PluginSpec>();
            var visited = new HashSet<string>();
            foreach (var id in ids) VisitPlugin(id, visited, result);
            return result;
        }

        private static void VisitPlugin(string id, HashSet<string> visited, List<PluginSpec> result)
        {
            if (!visited.Add(id)) return;
            var plugin = FindPlugin(id);
            foreach (var dependency in plugin.Dependencies) VisitPlugin(dependency, visited, result);
            result.Add(plugin);
        }
    }
}
