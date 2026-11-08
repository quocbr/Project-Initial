/*
Author: quocbr
Github: https://github.com/quocbr
Created: 2026-02-05
Description: Base class cho game objects có thể pool - Implement IPoolable
*/

using UnityEngine;

/// <summary>
/// Base class cho tất cả game objects có thể được pool
/// Kế thừa class này hoặc implement IPoolable interface
/// </summary>
public class GameUnit : MonoBehaviour, IPoolable
{
    [Header("Pool Settings")]
    [SerializeField] private PoolType poolType = PoolType.None;

    private Transform _tf;

    /// <summary>
    /// Cached Transform component để tối ưu hiệu suất
    /// </summary>
    public Transform TF
    {
        get
        {
            if (_tf == null)
            {
                _tf = transform;
            }
            return _tf;
        }
    }

    /// <summary>
    /// Loại pool của object này
    /// </summary>
    public PoolType PoolType => poolType;

    /// <summary>
    /// Được gọi khi object được spawn từ pool
    /// Override để custom behavior
    /// </summary>
    public virtual void OnSpawn()
    {
        // Override trong class con nếu cần
    }

    /// <summary>
    /// Được gọi khi object được despawn về pool
    /// Override để cleanup
    /// </summary>
    public virtual void OnDespawn()
    {
        // Override trong class con nếu cần
    }
}