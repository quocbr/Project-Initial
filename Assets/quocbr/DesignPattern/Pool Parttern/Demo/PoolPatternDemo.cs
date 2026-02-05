/*
Author: quocbr
Github: https://github.com/quocbr
Created: 2026-02-05
Description: Example demonstrating improved Pool Pattern features
*/

using UnityEngine;

/// <summary>
/// Demo script cho các tính năng mới của Pool Pattern
/// Attach vào GameObject để test
/// </summary>
public class PoolPatternDemo : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameUnit bulletPrefab;
    [SerializeField] private GameUnit enemyPrefab;

    [Header("Settings")]
    [SerializeField] private int poolSize = 20;
    [SerializeField] private float spawnInterval = 0.5f;

    private float timer;
    private MiniPool<UITextElement> uiTextPool;

    void Start()
    {
        // ===== FEATURE 1: Preload with Statistics =====
        Debug.Log("=== Initializing Pools ===");
        
        SimplePool.Preload(bulletPrefab, poolSize, transform, collect: true);
        SimplePool.Preload(enemyPrefab, poolSize / 2, transform, collect: true);

        // Log initial stats
        SimplePool.LogAllPoolStats();

        // ===== FEATURE 2: MiniPool with Callbacks =====
        uiTextPool = new MiniPool<UITextElement>();
        uiTextPool.OnInit(FindObjectOfType<UITextElement>(), 10, transform);
        uiTextPool.OnSpawn(text => 
        {
            Debug.Log($"UI Text Spawned: {text.name}");
            text.ResetAnimation();
        });
        uiTextPool.OnDespawn(text => 
        {
            Debug.Log($"UI Text Despawned: {text.name}");
        });
    }

    void Update()
    {
        // Spawn bullets automatically
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnBulletExample();
        }

        // Hotkeys for testing
        if (Input.GetKeyDown(KeyCode.B))
        {
            SpawnBulletExample();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnEnemyExample();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            CollectAllExample();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            ShowStatsExample();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ReleasePoolExample();
        }
    }

    // ===== FEATURE DEMOS =====

    /// <summary>
    /// Demo: Spawn với lifecycle callbacks
    /// </summary>
    void SpawnBulletExample()
    {
        Vector3 pos = transform.position + Random.insideUnitSphere * 5f;
        
        // Spawn bullet - OnSpawn() sẽ được gọi tự động
        var bullet = SimplePool.Spawn<ExampleBullet>(bulletPrefab, pos, Quaternion.identity);
        
        if (bullet != null)
        {
            Debug.Log($"Spawned bullet at {pos}");
            
            // Auto despawn sau 3 giây
            StartCoroutine(DespawnAfterDelay(bullet, 3f));
        }
    }

    /// <summary>
    /// Demo: Spawn enemy với custom logic
    /// </summary>
    void SpawnEnemyExample()
    {
        Vector3 pos = transform.position + Random.insideUnitSphere * 10f;
        
        var enemy = SimplePool.Spawn<ExampleEnemy>(enemyPrefab, pos, Quaternion.identity);
        
        if (enemy != null)
        {
            Debug.Log($"Spawned enemy at {pos}");
        }
    }

    /// <summary>
    /// Demo: Collect all objects về pool
    /// </summary>
    void CollectAllExample()
    {
        Debug.Log("=== Collecting All Objects ===");
        SimplePool.CollectAll();
        Debug.Log("All objects returned to pools");
        
        // Show stats after collect
        SimplePool.LogAllPoolStats();
    }

    /// <summary>
    /// Demo: Show pool statistics
    /// </summary>
    void ShowStatsExample()
    {
        Debug.Log("=== Pool Statistics ===");
        SimplePool.LogAllPoolStats();
        
        // Show specific pool stats
        string bulletStats = SimplePool.GetPoolStats(bulletPrefab.PoolType);
        Debug.Log($"Bullet Pool: {bulletStats}");
        
        // MiniPool stats
        Debug.Log($"UI Text Pool: {uiTextPool.GetStats()}");
    }

    /// <summary>
    /// Demo: Release pool (cleanup memory)
    /// </summary>
    void ReleasePoolExample()
    {
        Debug.Log("=== Releasing Bullet Pool ===");
        SimplePool.Release(bulletPrefab.PoolType);
        Debug.Log("Bullet pool released");
        
        SimplePool.LogAllPoolStats();
    }

    System.Collections.IEnumerator DespawnAfterDelay(GameUnit obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // OnDespawn() sẽ được gọi tự động
        SimplePool.Despawn(obj);
        Debug.Log($"Despawned {obj.name}");
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Label("=== POOL PATTERN DEMO ===");
        GUILayout.Label("Hotkeys:");
        GUILayout.Label("B - Spawn Bullet");
        GUILayout.Label("E - Spawn Enemy");
        GUILayout.Label("C - Collect All");
        GUILayout.Label("S - Show Stats");
        GUILayout.Label("R - Release Pool");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Spawn 10 Bullets"))
        {
            for (int i = 0; i < 10; i++)
            {
                SpawnBulletExample();
            }
        }
        
        if (GUILayout.Button("Log All Stats"))
        {
            ShowStatsExample();
        }
        
        GUILayout.EndArea();
    }
}

// ===== EXAMPLE CLASSES =====

/// <summary>
/// Example Bullet với lifecycle callbacks
/// </summary>
public class ExampleBullet : GameUnit
{
    private Rigidbody rb;
    private TrailRenderer trail;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
    }

    /// <summary>
    /// Gọi tự động khi spawn từ pool
    /// </summary>
    public override void OnSpawn()
    {
        base.OnSpawn();
        
        Debug.Log($"{name} - OnSpawn called!");
        
        // Reset physics
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Reset trail
        if (trail != null)
        {
            trail.Clear();
        }
        
        // Shoot forward
        if (rb != null)
        {
            rb.AddForce(transform.forward * 10f, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Gọi tự động khi despawn về pool
    /// </summary>
    public override void OnDespawn()
    {
        base.OnDespawn();
        
        Debug.Log($"{name} - OnDespawn called!");
        
        // Cleanup
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }
    }
}

/// <summary>
/// Example Enemy với health và auto-despawn
/// </summary>
public class ExampleEnemy : GameUnit
{
    private float health = 100f;

    public override void OnSpawn()
    {
        base.OnSpawn();
        
        Debug.Log($"{name} - Enemy spawned with {health} HP");
        health = 100f;
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        
        Debug.Log($"{name} - Enemy despawned");
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.Log($"{name} took {damage} damage. HP: {health}");
        
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{name} died!");
        
        // Tự động trả về pool
        SimplePool.Despawn(this);
    }
}

/// <summary>
/// Example UI element cho MiniPool
/// </summary>
public class UITextElement : MonoBehaviour
{
    public void ResetAnimation()
    {
        // Reset animation state
        Debug.Log("UI Text animation reset");
    }
}
