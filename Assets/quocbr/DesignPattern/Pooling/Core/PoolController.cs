/*
Author: quocbr
github: https://github.com/quocbr
Date:
*/

using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.Utilities.Editor;
#endif

[HideMonoScript]
public class PoolController : MonoBehaviour
{
    // ============================================================================================================
    // CONFIGURATION
    // ============================================================================================================

    private const string GROUP_POOLS = "Object Pools";
    private const string GROUP_PARTICLES = "Particle Pools";
    private const string GROUP_TOOLS = "Tools & Stats";

    // ===== OBJECT POOLS =====
    [TabGroup("Groups", GROUP_POOLS)]
    [ListDrawerSettings(
        ShowIndexLabels = true,
        ListElementLabelName = "InspectorLabel",
        DraggableItems = true,
        ShowPaging = true,
        NumberOfItemsPerPage = 5,
        CustomAddFunction = "AddNewPool",
        CustomRemoveElementFunction = "RemovePool"
    )]
    [LabelText("Object Pool List")]
    [InfoBox("⚠️ No pools configured! Click '+' to add.", InfoMessageType.Warning, "@Pool.Count == 0")]
    public List<PoolAmount> Pool = new List<PoolAmount>();

    // ===== PARTICLE POOLS =====
    [TabGroup("Groups", GROUP_PARTICLES)]
    [ListDrawerSettings(
        ShowIndexLabels = true,
        ListElementLabelName = "InspectorLabel",
        DraggableItems = true,
        ShowPaging = true,
        NumberOfItemsPerPage = 5
    )]
    [LabelText("Particle Pool List")]
    [InfoBox("⚠️ No particle pools configured!", InfoMessageType.Warning, "@Particle.Count == 0")]
    public List<ParticleAmount> Particle = new List<ParticleAmount>();

    // ============================================================================================================
    // ⚙️ RUNTIME LOGIC
    // ============================================================================================================
    private void Awake()
    {
        // 1. Init Object Pools
        foreach (var p in Pool)
        {
            if (p == null || p.prefab == null) continue;
            if (p.enableLazyPreloading) StartLazyPreload(p);
            else SimplePool.Preload(p.prefab, p.amount, p.root, p.collect);
        }

        // 2. Init Particle Pools
        foreach (var p in Particle)
        {
            if (p == null || p.prefab == null) continue;
            ParticlePool.Preload(p.prefab, p.amount, p.root);
            ParticlePool.Shortcut(p.particleType, p.prefab);
        }
    }

#if UNITY_EDITOR
    [OnInspectorGUI]
    [PropertyOrder(-10)]
    private void DrawHeader()
    {
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            richText = true,
            fontSize = 25,
            fontStyle = FontStyle.Bold
        };

        // Vẽ chữ
        GUILayout.Space(10);
        GUILayout.Label("🎮 Pool Controller", titleStyle);

        // Vẽ subtitle nhỏ bên dưới
        var subStyle = new GUIStyle(GUI.skin.label)
            { alignment = TextAnchor.MiddleCenter, fontSize = 11, normal = { textColor = Color.gray } };
        GUILayout.Label("Quản lý tất cả Object Pools và Particle Pools trong scene", subStyle);

        GUILayout.Space(5);

        // 2. Vẽ InfoBox
        string message = "📖 Instructions:\n" +
                         "• Pool: Quản lý game objects (bullets, enemies, items)\n" +
                         "• Particle: Quản lý VFX effects (explosions, hits, blood)\n" +
                         "• Preload: Khởi tạo pool khi Awake()\n" +
                         "• Lazy Preload (Beta): Tạo objects dần dần theo thời gian";

        // Sử dụng hàm vẽ Box của Odin để đẹp hơn Unity HelpBox mặc định
        SirenixEditorGUI.MessageBox(message, MessageType.Info);

        GUILayout.Space(10);
    }
#endif

    private void StartLazyPreload(PoolAmount poolConfig)
    {
        // Tạo pool ngay nhưng chưa spawn objects
        SimplePool.LazyPreload(poolConfig.prefab, poolConfig.root, poolConfig.collect, poolConfig.amount);

        // Tạo instances on awake nếu có
        if (poolConfig.instancesCreatedOnAwake > 0)
        {
            SimplePool.Preload(
                poolConfig.prefab,
                poolConfig.instancesCreatedOnAwake,
                poolConfig.root,
                poolConfig.collect
            );
        }

        // Tạo dần objects còn lại
        int remainingInstances = poolConfig.amount - poolConfig.instancesCreatedOnAwake;
        if (remainingInstances > 0)
        {
            StartCoroutine(LazyPreloadCoroutine(poolConfig, remainingInstances));
        }
    }

    private IEnumerator LazyPreloadCoroutine(PoolAmount poolConfig, int totalToCreate)
    {
        // Wait initial delay
        if (poolConfig.initialDelay > 0)
        {
            yield return new WaitForSeconds(poolConfig.initialDelay);
        }

        int created = 0;
        while (created < totalToCreate)
        {
            // Tính số lượng cần tạo trong pass này
            int toCreateThisPass = Mathf.Min(poolConfig.instancesToCreatePerPass, totalToCreate - created);

            // Tạo objects
            for (int i = 0; i < toCreateThisPass; i++)
            {
                var obj = SimplePool.Spawn(poolConfig.prefab);
                if (obj == null) continue;
                if (poolConfig.collect)
                {
                    obj.gameObject.SetActive(false);
                }
                else
                {
                    SimplePool.Despawn(obj);
                }
            }

            created += toCreateThisPass;
            // Wait before next pass
            if (created < totalToCreate && poolConfig.delayBetweenPasses > 0)
            {
                yield return new WaitForSeconds(poolConfig.delayBetweenPasses);
            }
        }

        if (poolConfig.collect)
        {
            SimplePool.Collect(poolConfig.prefab.PoolType);
        }

        Debug.Log($"Lazy Preload Complete [{poolConfig.prefab.PoolType}]: {totalToCreate} objects ready");
    }

    private PoolAmount AddNewPool()
    {
        return new PoolAmount(null, null, 10, true);
    }

    private void RemovePool(PoolAmount pool)
    {
        if (pool.root != null)
        {
#if UNITY_EDITOR
            if (pool.root != transform) Undo.DestroyObjectImmediate(pool.root.gameObject);
#else
            Destroy(pool.root.gameObject);
#endif
        }

        Pool.Remove(pool);
    }

    // ============================================================================================================
    // 🔧 EDITOR TOOLS & STATS
    // ============================================================================================================
#if UNITY_EDITOR

    // SUMMARY INFO
    [TabGroup("Groups", GROUP_TOOLS)]
    [Title("Information")]
    [ShowInInspector]
    [ReadOnly]
    [HideLabel]
    [DisplayAsString]
    [GUIColor(0.7f, 1f, 0.7f)]
    private string SummaryInfo => $"📊 Overview: {Pool.Count} Object Pools | {Particle.Count} Particle Pools";

    // EDITOR ACTIONS
    [TabGroup("Groups", GROUP_TOOLS)]
    [Title("Editor Actions")]
    [Button("🏗️ Create All Roots", ButtonSizes.Large)]
    [GUIColor(0.4f, 0.8f, 1f)]
    [TabGroup("Groups", GROUP_TOOLS)]
    private void CreateAllRoots()
    {
        int created = 0;
        foreach (var p in Pool)
        {
            if (p.root == null && p.prefab != null)
            {
                p.root = CreateRoot($"Pool_{p.prefab.PoolType}");
                created++;
            }
        }

        foreach (var p in Particle)
        {
            if (p.root == null)
            {
                p.root = CreateRoot($"Particle_{p.particleType}");
                created++;
            }
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"✅ Created {created} root transforms!");
    }

    [Button("📦 Load Prefabs from Resources", ButtonSizes.Large)]
    [GUIColor(0.4f, 1f, 0.4f)]
    [TabGroup("Groups", GROUP_TOOLS)]
    private void LoadPrefabsFromResources()
    {
        GameUnit[] resources = Resources.LoadAll<GameUnit>("Pool");
        if (resources.Length == 0)
        {
            Debug.LogWarning("⚠️ No prefabs found!");
            return;
        }

        int added = 0;
        foreach (var res in resources)
        {
            bool exists = false;
            foreach (var p in Pool)
            {
                if (p.prefab != null && p.prefab.PoolType == res.PoolType)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                Pool.Add(new PoolAmount(CreateRoot($"Pool_{res.PoolType}"), res, 10, true));
                added++;
            }
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"✅ Added {added} new pools!");
    }

    [Button("🧹 Clean Null Pools", ButtonSizes.Medium)]
    [GUIColor(1f, 0.7f, 0.4f)]
    [TabGroup("Groups", GROUP_TOOLS)]
    private void CleanNullPools()
    {
        Pool.RemoveAll(p => p.prefab == null);
        EditorUtility.SetDirty(this);
    }


    // RUNTIME STATISTICS
    [TabGroup("Groups", GROUP_TOOLS)]
    [Title("Runtime Statistics", "Only available during Play Mode")]
    [Button("📊 Log Pool Statistics", ButtonSizes.Large)]
    [GUIColor(0.8f, 0.4f, 1f)]
    [EnableIf("@UnityEngine.Application.isPlaying")]
    [TabGroup("Groups", GROUP_TOOLS)]
    private void LogPoolStats()
    {
        if (Application.isPlaying) SimplePool.LogAllPoolStats();
    }

    [Button("🔄 Collect All Objects", ButtonSizes.Medium)]
    [GUIColor(0.4f, 0.8f, 0.8f)]
    [EnableIf("@UnityEngine.Application.isPlaying")]
    [TabGroup("Groups", GROUP_TOOLS)]
    private void CollectAllObjects()
    {
        if (Application.isPlaying) SimplePool.CollectAll();
    }

    private Transform CreateRoot(string name)
    {
        Transform tf = new GameObject(name).transform;
        tf.SetParent(transform);
        tf.localPosition = Vector3.zero;
        return tf;
    }
#endif
}

// ============================================================================================================
// 📦 DATA CLASSES
// ============================================================================================================

[Serializable]
public class PoolAmount
{
    [FoldoutGroup("Pool Settings", true)] [BoxGroup("Pool Settings/Basic")] [LabelText("Root")] [Required]
    public Transform root;

    [BoxGroup("Pool Settings/Basic")] [LabelText("Prefab")] [Required] [AssetsOnly]
    public GameUnit prefab;

    [BoxGroup("Pool Settings/Basic")] [LabelText("Size")] [MinValue(1)]
    public int amount;

    [BoxGroup("Pool Settings/Basic")] [LabelText("Collect")]
    public bool collect;

    [FoldoutGroup("Lazy Preloading (Beta)", false)]
    [BoxGroup("Lazy Preloading (Beta)/Toggle")]
    [LabelText("Enable Lazy")]
    [ToggleLeft]
    public bool enableLazyPreloading;

    [FoldoutGroup("Lazy Preloading (Beta)")]
    [BoxGroup("Lazy Preloading (Beta)/Settings")]
    [ShowIf("enableLazyPreloading")]
    [LabelText("On Awake")]
    public int instancesCreatedOnAwake = 1;

    [FoldoutGroup("Lazy Preloading (Beta)")]
    [BoxGroup("Lazy Preloading (Beta)/Settings")]
    [ShowIf("enableLazyPreloading")]
    [LabelText("Delay")]
    public float initialDelay = 1f;

    [FoldoutGroup("Lazy Preloading (Beta)")]
    [BoxGroup("Lazy Preloading (Beta)/Settings")]
    [ShowIf("enableLazyPreloading")]
    [LabelText("Per Pass")]
    public int instancesToCreatePerPass = 1;

    [FoldoutGroup("Lazy Preloading (Beta)")]
    [BoxGroup("Lazy Preloading (Beta)/Settings")]
    [ShowIf("enableLazyPreloading")]
    [LabelText("Interval")]
    public float delayBetweenPasses = 0.2f;

    public PoolAmount(Transform root, GameUnit prefab, int amount, bool collect)
    {
        this.root = root;
        this.prefab = prefab;
        this.amount = amount;
        this.collect = collect;
        enableLazyPreloading = false;
    }

    public string InspectorLabel
    {
        get
        {
            if (prefab == null) return "⚠️ Not Configured";
            string icon = enableLazyPreloading ? "⚡" : "📦";
            return $"{icon} {prefab.PoolType} (x{amount})";
        }
    }
}

[Serializable]
public class ParticleAmount
{
    [FoldoutGroup("$InspectorLabel", true)] [BoxGroup("$InspectorLabel/Settings")] [LabelText("Root")]
    public Transform root;

    [BoxGroup("$InspectorLabel/Settings")] [LabelText("Type")]
    public ParticleType particleType;

    [BoxGroup("$InspectorLabel/Settings")] [LabelText("Prefab")] [Required] [AssetsOnly]
    public ParticleSystem prefab;

    [BoxGroup("$InspectorLabel/Settings")] [LabelText("Size")] [MinValue(1)]
    public int amount;

    public string InspectorLabel => prefab == null ? "⚠️ Not Configured" : $"🎆 {particleType} (x{amount})";
}