/*
Author: quocbr
Github: https://github.com/quocbr
Created: 2026-02-05
Description: Pool Type Enums - Tách ra file riêng để dễ quản lý
*/

/// <summary>
/// Enum type cho enum generator dialog
/// </summary>
public enum EnumType
{
    PoolType,
    ParticleType
}

/// <summary>
/// Enum định nghĩa các loại Pool Type cho game objects
/// Thêm type mới khi cần pool object mới
/// </summary>
public enum PoolType
{
    None = 0,

  rt = 1
}

/// <summary>
/// Enum định nghĩa các loại Particle Type cho VFX
/// Thêm type mới khi cần particle effect mới
/// </summary>
public enum ParticleType
{
    None = 0
}