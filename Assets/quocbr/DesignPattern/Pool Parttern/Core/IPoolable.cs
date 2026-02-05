/*
Author: quocbr
Github: https://github.com/quocbr
Created: 2026-02-05
Description: Interface cho objects có thể pool - Linh hoạt hơn base class
*/

/// <summary>
/// Interface cho các objects có thể được pool
/// Thay thế cho việc bắt buộc kế thừa GameUnit
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// Loại pool của object này
    /// </summary>
    PoolType PoolType { get; }

    /// <summary>
    /// Được gọi khi object được spawn từ pool
    /// Dùng để reset state, init values
    /// </summary>
    void OnSpawn();

    /// <summary>
    /// Được gọi khi object được despawn về pool
    /// Dùng để cleanup, reset effects
    /// </summary>
    void OnDespawn();
}
