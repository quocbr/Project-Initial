/*
Author: quocbr
Github: https://github.com/quocbr
Created: 2026-02-05
Description: Mini Pool - Local pool cho một loại object cụ thể
*/

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mini Pool - Pool cục bộ cho một loại Component cụ thể
/// Dùng cho UI elements, inventory items, trajectory dots, etc.
/// </summary>
/// <typeparam name="T">Component type cần pool</typeparam>
public class MiniPool<T> where T : Component
{
    private Queue<T> pools = new Queue<T>();
    private List<T> listActives = new List<T>();

    private T prefab;
    private Transform parent;
    private int totalCreated = 0;
    private int totalSpawned = 0;

    // Callbacks
    private Action<T> onSpawn;
    private Action<T> onDespawn;

    /// <summary>
    /// Số lượng objects có sẵn trong pool
    /// </summary>
    public int AvailableCount => pools.Count;

    /// <summary>
    /// Số lượng objects đang active
    /// </summary>
    public int ActiveCount => listActives.Count;

    /// <summary>
    /// Tổng số objects đã tạo
    /// </summary>
    public int TotalCount => pools.Count + listActives.Count;

    /// <summary>
    /// Khởi tạo Mini Pool với số lượng objects ban đầu
    /// </summary>
    /// <param name="prefab">Prefab cần pool</param>
    /// <param name="amount">Số lượng khởi tạo</param>
    /// <param name="parent">Transform cha</param>
    public void OnInit(T prefab, int amount, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < amount; i++)
        {
            T obj = GameObject.Instantiate(prefab, parent);
            totalCreated++;
            Despawn(obj);
        }
    }

    /// <summary>
    /// Đăng ký callback khi spawn object
    /// </summary>
    public MiniPool<T> OnSpawn(Action<T> callback)
    {
        onSpawn = callback;
        return this;
    }

    /// <summary>
    /// Đăng ký callback khi despawn object
    /// </summary>
    public MiniPool<T> OnDespawn(Action<T> callback)
    {
        onDespawn = callback;
        return this;
    }

    /// <summary>
    /// Spawn object với vị trí và rotation
    /// </summary>
    public T Spawn(Vector3 pos, Quaternion rot)
    {
        T go = GetOrCreate();

        listActives.Add(go);

        go.transform.SetPositionAndRotation(pos, rot);
        go.gameObject.SetActive(true);
        totalSpawned++;

        // Gọi callbacks
        onSpawn?.Invoke(go);
        if (go is IPoolable poolable)
        {
            poolable.OnSpawn();
        }

        return go;
    }

    /// <summary>
    /// Spawn object tại vị trí mặc định
    /// </summary>
    public T Spawn()
    {
        T go = GetOrCreate();

        listActives.Add(go);
        go.gameObject.SetActive(true);
        totalSpawned++;

        // Gọi callbacks
        onSpawn?.Invoke(go);
        if (go is IPoolable poolable)
        {
            poolable.OnSpawn();
        }

        return go;
    }

    /// <summary>
    /// Lấy object từ pool hoặc tạo mới nếu hết
    /// </summary>
    private T GetOrCreate()
    {
        if (pools.Count > 0)
        {
            return pools.Dequeue();
        }
        else
        {
            totalCreated++;
            return GameObject.Instantiate(prefab, parent);
        }
    }

    /// <summary>
    /// Trả object về pool
    /// </summary>
    public void Despawn(T obj)
    {
        if (obj == null) return;

        if (obj.gameObject.activeSelf)
        {
            // Gọi callbacks
            onDespawn?.Invoke(obj);
            if (obj is IPoolable poolable)
            {
                poolable.OnDespawn();
            }

            obj.gameObject.SetActive(false);
        }

        pools.Enqueue(obj);
        listActives.Remove(obj);
    }

    /// <summary>
    /// Thu thập tất cả active objects về pool
    /// </summary>
    public void Collect()
    {
        List<T> temp = new List<T>(listActives);
        foreach (var obj in temp)
        {
            Despawn(obj);
        }
    }

    /// <summary>
    /// Destroy tất cả objects và clear pool
    /// </summary>
    public void Release()
    {
        Collect();

        while (pools.Count > 0)
        {
            T obj = pools.Dequeue();
            if (obj != null)
            {
                GameObject.Destroy(obj.gameObject);
            }
        }

        pools.Clear();
        listActives.Clear();
    }

    /// <summary>
    /// Lấy thống kê pool
    /// </summary>
    public string GetStats()
    {
        return $"MiniPool<{typeof(T).Name}> - Active: {ActiveCount}, Available: {AvailableCount}, Total: {TotalCount}, Created: {totalCreated}, Spawned: {totalSpawned}";
    }
}
