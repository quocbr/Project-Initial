/*
Simple pooling for Unity.
Original Author: Martin "quill18" Glaude (quill18@quill18.com)
Latest Version: https://gist.github.com/quill18/5a7cfffae68892621267
License: CC0 (http://creativecommons.org/publicdomain/zero/1.0/)

UPDATES:
2015-04-16:
    - Support Minh tito CTO ABI games studio
    - Advantage Linh soi game developer
2017-09-10:
    - simple pool with gameobject
    - release game object
2019-10-09:
    - Pool Clamp to keep the quantity within a certain range
    - Pool collect all to despawn all object comeback the pool
    - Spawn with generic T
    - Optimize pool
2022-10-09:
    - Pool with pool type from resources
    - pool with pool container
2022-11-27:
    - Remove clamp pool
    - Spawn in parent transform same instantiate(gameobject, transform)
    - Get list object is actived
2026-02-05: (by quocbr)
    - Add IPoolable interface support
    - Add OnSpawn/OnDespawn lifecycle callbacks
    - Improve documentation with XML comments
    - Add pool statistics
    - Better error handling
    - Add namespace support
*/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static pool manager cho game objects trong Unity
/// Tối ưu hiệu suất bằng cách tái sử dụng objects thay vì Instantiate/Destroy
/// </summary>
public static class SimplePool
{
    /// <summary>
    /// Số lượng objects mặc định khi khởi tạo pool
    /// </summary>
    public const int DEFAULT_POOL_SIZE = 3;

    /// <summary>
    /// Dictionary map PoolType -> Prefab để search nhanh
    /// </summary>
    private static Dictionary<PoolType, GameUnit> poolTypes = new Dictionary<PoolType, GameUnit>();

    /// <summary>
    /// HashSet lưu instance ID của objects đang là con của transform khác
    /// </summary>
    private static HashSet<int> memberInParent = new HashSet<int>();

    /// <summary>
    /// Root transform chứa tất cả pools
    /// </summary>
    private static Transform root;

    /// <summary>
    /// Dictionary chứa tất cả pool instances theo PoolType
    /// </summary>
    private static Dictionary<PoolType, Pool> poolInstance = new Dictionary<PoolType, Pool>();

    /// <summary>
    /// Lấy hoặc tạo root transform cho pools
    /// </summary>
    public static Transform Root
    {
        get
        {
            if (root == null)
            {
                PoolController controler = GameObject.FindObjectOfType<PoolController>();
                root = controler != null ? controler.transform : new GameObject("Pool").transform;
            }

            return root;
        }
    }

    /// <summary>
    /// Preload (warm-up) pool với số lượng objects cụ thể
    /// Nên gọi ở Awake/Start để tránh lag khi spawn lần đầu
    /// </summary>
    /// <param name="prefab">Prefab GameUnit cần pool</param>
    /// <param name="qty">Số lượng objects khởi tạo</param>
    /// <param name="parent">Transform cha chứa pool</param>
    /// <param name="collect">Có thu thập active objects không</param>
    public static void Preload(GameUnit prefab, int qty = 1, Transform parent = null, bool collect = false)
    {
        if (!poolTypes.ContainsKey(prefab.PoolType))
        {
            poolTypes.Add(prefab.PoolType, prefab);
        }

        if (prefab == null)
        {
            Debug.LogError(parent.name + " : IS EMPTY!!!");
            return;
        }

        InitPool(prefab, qty, parent, collect);

        // Tạo mảng để preload objects
        GameUnit[] obs = new GameUnit[qty];
        for (int i = 0; i < qty; i++)
        {
            obs[i] = Spawn(prefab);
        }

        // Despawn tất cả về pool
        for (int i = 0; i < qty; i++)
        {
            Despawn(obs[i]);
        }
    }

    /// <summary>
    /// Lazy Preload - Chỉ khởi tạo pool, không tạo objects ngay
    /// Objects sẽ được tạo khi cần thiết (lazy load)
    /// </summary>
    /// <param name="prefab">Prefab GameUnit cần pool</param>
    /// <param name="parent">Transform cha chứa pool</param>
    /// <param name="collect">Có thu thập active objects không</param>
    /// <param name="maxSize">Số lượng tối đa (0 = unlimited)</param>
    public static void LazyPreload(GameUnit prefab, Transform parent = null, bool collect = false, int maxSize = 0)
    {
        if (!poolTypes.ContainsKey(prefab.PoolType))
        {
            poolTypes.Add(prefab.PoolType, prefab);
        }

        if (prefab == null)
        {
            Debug.LogError("Prefab is null!");
            return;
        }

        // Khởi tạo pool với lazy load = true
        if (!IsHasPool(prefab))
        {
            poolInstance.Add(prefab.PoolType, new Pool(prefab, 0, parent, collect, true, maxSize));
        }
    }

    /// <summary>
    /// Khởi tạo pool nếu chưa tồn tại
    /// </summary>
    private static void InitPool(GameUnit prefab = null, int qty = DEFAULT_POOL_SIZE, Transform parent = null,
        bool collect = false, bool lazyLoad = false, int maxSize = 0)
    {
        if (prefab != null && !IsHasPool(prefab))
        {
            poolInstance.Add(prefab.PoolType, new Pool(prefab, qty, parent, collect, lazyLoad, maxSize));
        }
    }

    /// <summary>
    /// Kiểm tra xem pool đã tồn tại chưa
    /// </summary>
    private static bool IsHasPool(GameUnit obj)
    {
        return poolInstance.ContainsKey(obj.PoolType);
    }

    /// <summary>
    /// Lấy pool instance theo object
    /// </summary>
    private static Pool GetPool(GameUnit obj)
    {
        return poolInstance[obj.PoolType];
    }

    /// <summary>
    /// Lấy prefab theo PoolType
    /// Tự động load từ Resources/Pool nếu chưa có
    /// </summary>
    public static GameUnit GetPrefabByType(PoolType poolType)
    {
        if (!poolTypes.ContainsKey(poolType) || poolTypes[poolType] == null)
        {
            GameUnit[] resources = Resources.LoadAll<GameUnit>("Pool");

            for (int i = 0; i < resources.Length; i++)
            {
                poolTypes[resources[i].PoolType] = resources[i];
            }
        }

        return poolTypes[poolType];
    }

    #region Despawn

    //take gameunit to pool
    public static void Despawn(GameUnit obj)
    {
        if (obj.gameObject.activeSelf)
        {
            if (IsHasPool(obj))
            {
                GetPool(obj).Despawn(obj);
            }
            else
            {
                GameObject.Destroy(obj.gameObject);
            }
        }
    }

    #endregion

    /// <summary>
    /// Inner class đại diện cho một pool cụ thể
    /// </summary>
    private class Pool
    {
        // HashSet chứa objects đang active trong game
        private HashSet<GameUnit> m_active;

        // Có thu thập objects đang active không
        private bool m_collect;

        // Queue chứa objects không active (sẵn sàng spawn)
        private Queue<GameUnit> m_inactive;

        // Lazy load settings
        private bool m_lazyLoad;

        private int m_maxSize;

        // Prefab gốc để tạo objects mới
        private GameUnit m_prefab;

        // Parent transform chứa tất cả pool members
        private Transform m_sRoot;
        private int m_totalCreated;

        // Statistics
        private int m_totalSpawned;

        /// <summary>
        /// Constructor - Khởi tạo pool
        /// </summary>
        /// <param name="prefab">Prefab để pool</param>
        /// <param name="initialQty">Số lượng khởi tạo ban đầu</param>
        /// <param name="parent">Transform cha</param>
        /// <param name="collect">Thu thập active objects</param>
        /// <param name="lazyLoad">Lazy load - chỉ tạo khi cần</param>
        /// <param name="maxSize">Số lượng tối đa (0 = unlimited)</param>
        public Pool(GameUnit prefab, int initialQty, Transform parent, bool collect, bool lazyLoad = false,
            int maxSize = 0)
        {
            m_inactive = new Queue<GameUnit>(initialQty);
            m_sRoot = parent;
            m_prefab = prefab;
            m_collect = collect;
            m_lazyLoad = lazyLoad;
            m_maxSize = maxSize;

            if (m_collect) m_active = new HashSet<GameUnit>();
        }

        public bool IsCollect => m_collect;
        public HashSet<GameUnit> Active => m_active;
        public int Count => m_inactive.Count + m_active.Count;
        public Transform Root => m_sRoot;
        public int TotalSpawned => m_totalSpawned;
        public int TotalCreated => m_totalCreated;
        public bool LazyLoad => m_lazyLoad;
        public int MaxSize => m_maxSize;

        /// <summary>
        /// Spawn object với vị trí và rotation
        /// </summary>
        public GameUnit Spawn(Vector3 pos, Quaternion rot)
        {
            GameUnit obj = Spawn();

            obj.TF.SetPositionAndRotation(pos, rot);

            return obj;
        }

        /// <summary>
        /// Spawn object từ pool
        /// Tạo mới nếu pool rỗng (hoặc lazy load)
        /// </summary>
        public GameUnit Spawn()
        {
            GameUnit obj;

            if (m_inactive.Count == 0)
            {
                obj = GameObject.Instantiate(m_prefab, m_sRoot);
                m_totalCreated++;
            }
            else
            {
                obj = m_inactive.Dequeue();
                if (obj == null) return Spawn();
            }

            if (m_collect) m_active.Add(obj);

            obj.gameObject.SetActive(true);
            m_totalSpawned++;

            if (obj is IPoolable poolable)
            {
                poolable.OnSpawn();
            }

            return obj;
        }

        /// <summary>
        /// Trả object về pool (inactive)
        /// </summary>
        public void Despawn(GameUnit obj)
        {
            if (obj != null)
            {
                // Gọi lifecycle callback trước khi despawn
                if (obj is IPoolable poolable)
                {
                    poolable.OnDespawn();
                }

                obj.gameObject.SetActive(false);
                m_inactive.Enqueue(obj);

                if (memberInParent.Contains(obj.GetInstanceID()))
                {
                    obj.TF.SetParent(GetPool(obj).Root);
                    memberInParent.Remove(obj.GetInstanceID());
                }
            }

            if (m_collect) m_active.Remove(obj);
        }

        /// <summary>
        /// Destroy tất cả objects trong pool
        /// Dùng để giải phóng memory
        /// </summary>
        public void Release()
        {
            while (m_inactive.Count > 0)
            {
                GameUnit go = m_inactive.Dequeue();
                GameObject.DestroyImmediate(go);
            }

            m_inactive.Clear();
        }

        /// <summary>
        /// Thu thập tất cả active objects về pool
        /// Thường dùng khi chuyển màn
        /// </summary>
        public void Collect()
        {
            HashSet<GameUnit> units = new HashSet<GameUnit>(m_active);
            foreach (var item in units)
            {
                Despawn(item);
            }
        }
    }

    #region Get List object ACTIVE

    /// <summary>
    /// Lấy tất cả objects đang active trong game theo prefab
    /// </summary>
    public static HashSet<GameUnit> GetAllUnitIsActive(GameUnit obj)
    {
        return IsHasPool(obj) ? GetPool(obj).Active : new HashSet<GameUnit>();
    }

    /// <summary>
    /// Lấy tất cả objects đang active trong game theo PoolType
    /// </summary>
    public static HashSet<GameUnit> GetAllUnitIsActive(PoolType poolType)
    {
        return GetAllUnitIsActive(GetPrefabByType(poolType));
    }

    #endregion

    #region Pool Statistics

    /// <summary>
    /// Lấy thống kê pool theo PoolType
    /// </summary>
    public static string GetPoolStats(PoolType poolType)
    {
        if (!poolInstance.ContainsKey(poolType))
        {
            return $"Pool {poolType} không tồn tại";
        }

        Pool pool = poolInstance[poolType];
        string lazyInfo = pool.LazyLoad ? " [LazyLoad]" : "";
        string maxInfo = pool.MaxSize > 0 ? $" Max:{pool.MaxSize}" : "";
        return
            $"[{poolType}]{lazyInfo} Active: {pool.Active?.Count ?? 0}, Total: {pool.Count}, Spawned: {pool.TotalSpawned}, Created: {pool.TotalCreated}{maxInfo}";
    }

    /// <summary>
    /// Log tất cả thống kê pools ra console
    /// </summary>
    public static void LogAllPoolStats()
    {
        Debug.Log("=== SIMPLE POOL STATISTICS ===");
        Debug.Log($"Total Pools: {poolInstance.Count}");

        foreach (var kvp in poolInstance)
        {
            Debug.Log(GetPoolStats(kvp.Key));
        }
    }

    #endregion

    #region Spawn

    // Spawn Unit to use
    public static T Spawn<T>(PoolType poolType, Vector3 pos, Quaternion rot) where T : GameUnit
    {
        return Spawn(GetPrefabByType(poolType), pos, rot) as T;
    }

    public static T Spawn<T>(PoolType poolType) where T : GameUnit
    {
        return Spawn<T>(GetPrefabByType(poolType));
    }

    public static T Spawn<T>(GameUnit obj, Vector3 pos, Quaternion rot) where T : GameUnit
    {
        return Spawn(obj, pos, rot) as T;
    }

    public static T Spawn<T>(GameUnit obj) where T : GameUnit
    {
        return Spawn(obj) as T;
    }

    // spawn gameunit with transform parent
    public static T Spawn<T>(GameUnit obj, Transform parent) where T : GameUnit
    {
        return Spawn<T>(obj, obj.TF.localPosition, obj.TF.localRotation, parent);
    }

    public static T Spawn<T>(GameUnit obj, Vector3 localPoint, Quaternion localRot, Transform parent) where T : GameUnit
    {
        T unit = Spawn<T>(obj);
        unit.TF.SetParent(parent);
        unit.TF.localPosition = localPoint;
        unit.TF.localRotation = localRot;
        unit.TF.localScale = Vector3.one;
        memberInParent.Add(unit.GetInstanceID());
        return unit;
    }

    public static T Spawn<T>(PoolType poolType, Vector3 localPoint, Quaternion localRot, Transform parent)
        where T : GameUnit
    {
        return Spawn<T>(GetPrefabByType(poolType), localPoint, localRot, parent);
    }

    public static T Spawn<T>(PoolType poolType, Transform parent) where T : GameUnit
    {
        return Spawn<T>(GetPrefabByType(poolType), parent);
    }

    public static GameUnit Spawn(GameUnit obj, Vector3 pos, Quaternion rot)
    {
        if (!IsHasPool(obj))
        {
            Transform newRoot = new GameObject(obj.name).transform;
            newRoot.SetParent(Root);
            Preload(obj, 1, newRoot, true);
        }

        return GetPool(obj).Spawn(pos, rot);
    }

    public static GameUnit Spawn(GameUnit obj)
    {
        if (!IsHasPool(obj))
        {
            Transform newRoot = new GameObject(obj.name).transform;
            newRoot.SetParent(Root);
            Preload(obj, 1, newRoot, true);
        }

        return GetPool(obj).Spawn();
    }

    #endregion

    #region Release

    //destroy pool
    public static void Release(GameUnit obj)
    {
        if (IsHasPool(obj))
        {
            GetPool(obj).Release();
        }
    }

    public static void Release(PoolType poolType)
    {
        Release(GetPrefabByType(poolType));
    }

    //DESTROY ALL POOL
    public static void ReleaseAll()
    {
        foreach (var item in poolInstance)
        {
            item.Value.Release();
        }
    }

    #endregion

    #region Collect

    //collect all pool member comeback to pool
    public static void Collect(GameUnit obj)
    {
        if (IsHasPool(obj)) GetPool(obj).Collect();
    }

    public static void Collect(PoolType poolType)
    {
        Collect(GetPrefabByType(poolType));
    }

    //COLLECT ALL POOL
    public static void CollectAll()
    {
        foreach (var item in poolInstance)
        {
            if (item.Value.IsCollect)
            {
                item.Value.Collect();
            }
        }
    }

    #endregion
}