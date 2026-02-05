/*
Author: quocbr
Github: https://github.com/quocbr
Created: 2026-02-05
Description: Particle Pool - Pool chuyên biệt cho ParticleSystem effects
*/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Static pool manager cho ParticleSystem effects
/// Tối ưu cho VFX, explosions, và các hiệu ứng particles
/// </summary>
public static class ParticlePool
{
    private const int DEFAULT_POOL_SIZE = 3;

    private static Transform root;

    //--------------------------------------------------------------------------------------------------

    private static Dictionary<ParticleType, ParticleSystem> shortcuts = new Dictionary<ParticleType, ParticleSystem>();

    // All of our pools
    private static Dictionary<int, Pool> pools = new Dictionary<int, Pool>();

    public static Transform Root
    {
        get
        {
            if (root == null)
            {
                PoolController controler = GameObject.FindObjectOfType<PoolController>();
                root = controler != null ? controler.transform : new GameObject("ParticlePool").transform;
            }

            return root;
        }
    }

    /// <summary>
    /// Init our dictionary.
    /// </summary>
    private static void Init(ParticleSystem prefab = null, int qty = DEFAULT_POOL_SIZE, Transform parent = null)
    {
        if (prefab != null && !pools.ContainsKey(prefab.GetInstanceID()))
        {
            pools[prefab.GetInstanceID()] = new Pool(prefab, qty, parent);
        }
    }

    public static void Preload(ParticleSystem prefab, int qty = 1, Transform parent = null)
    {
        Init(prefab, qty, parent);
    }

    public static void Play(ParticleSystem prefab, Vector3 pos, Quaternion rot)
    {
#if UNITY_EDITOR
        if (prefab == null)
        {
            Debug.LogError(prefab.name + " is null!");
            return;
        }
#endif

        if (!pools.ContainsKey(prefab.GetInstanceID()))
        {
            Transform newRoot = new GameObject("VFX_" + prefab.name).transform;
            newRoot.SetParent(Root);
            pools[prefab.GetInstanceID()] = new Pool(prefab, 10, newRoot);
        }

        pools[prefab.GetInstanceID()].Play(pos, rot);
    }

    public static void Play(ParticleType particleType, Vector3 pos, Quaternion rot)
    {
#if UNITY_EDITOR
        if (!shortcuts.ContainsKey(particleType))
        {
            Debug.LogError(particleType + " is nees install at pool container!!!");
        }
#endif

        Play(shortcuts[particleType], pos, rot);
    }

    public static void Release(ParticleSystem prefab)
    {
        if (pools.ContainsKey(prefab.GetInstanceID()))
        {
            pools[prefab.GetInstanceID()].Release();
            pools.Remove(prefab.GetInstanceID());
        }
        else
        {
            GameObject.DestroyImmediate(prefab);
        }
    }

    public static void Release(ParticleType particleType)
    {
#if UNITY_EDITOR
        if (!shortcuts.ContainsKey(particleType))
        {
            Debug.LogError(particleType + " is nees install at pool container!!!");
        }
#endif

        Release(shortcuts[particleType]);
    }

    public static void Shortcut(ParticleType particleType, ParticleSystem particleSystem)
    {
        shortcuts.Add(particleType, particleSystem);
    }

    /// <summary>
    /// The Pool class represents the pool for a particular prefab.
    /// </summary>
    private class Pool
    {
        //list prefab ready
        private List<ParticleSystem> inactive;

        private int index;
        private Transform m_sRoot;

        // The prefab that we are pooling
        private ParticleSystem prefab;

        // Constructor
        public Pool(ParticleSystem prefab, int initialQty, Transform parent)
        {
#if UNITY_EDITOR
            if (prefab.main.loop)
            {
                var main = prefab.main;
                main.loop = false;

                //save prefab
                Undo.RegisterCompleteObjectUndo(prefab, "Fix To Not Loop");
                Debug.Log(prefab.name + " ~ Fix To Not Loop");
            }

            if (prefab.main.playOnAwake)
            {
                var main = prefab.main;
                main.playOnAwake = false;

                //save prefab
                Undo.RegisterCompleteObjectUndo(prefab, "Fix To Not PlayAwake");
                Debug.Log(prefab.name + " ~ Fix To Not PlayAwake");
            }

            if (prefab.main.stopAction != ParticleSystemStopAction.None)
            {
                var main = prefab.main;
                main.stopAction = ParticleSystemStopAction.None;

                //save prefab
                Undo.RegisterCompleteObjectUndo(prefab, "Fix To Stop Action None");
                Debug.Log(prefab.name + " ~ Fix To  Stop Action None");
            }

            if (prefab.main.duration > 1)
            {
                var main = prefab.main;
                main.duration = 1;

                //save prefab
                Undo.RegisterCompleteObjectUndo(prefab, "Fix To Duration By 1");
                Debug.Log(prefab.name + " ~ Fix To Duration By 1");
            }
#endif

            m_sRoot = parent;
            this.prefab = prefab;
            inactive = new List<ParticleSystem>(initialQty);

            for (int i = 0; i < initialQty; i++)
            {
                ParticleSystem particle = GameObject.Instantiate(prefab, m_sRoot);
                particle.Stop();
                inactive.Add(particle);
            }
        }

        public int Count => inactive.Count;

        // Spawn an object from our pool
        public void Play(Vector3 pos, Quaternion rot)
        {
            index = index + 1 < inactive.Count ? index + 1 : 0;

            ParticleSystem obj = inactive[index];

            if (obj.isPlaying)
            {
                obj = GameObject.Instantiate(prefab, m_sRoot);
                obj.Stop();
                inactive.Insert(index, obj);
            }

            obj.transform.SetPositionAndRotation(pos, rot);
            obj.Play();
        }

        public void Release()
        {
            while (inactive.Count > 0)
            {
                GameObject.DestroyImmediate(inactive[0]);
                inactive.RemoveAt(0);
            }

            inactive.Clear();
        }
    }
}