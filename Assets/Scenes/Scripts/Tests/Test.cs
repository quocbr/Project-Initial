/*
Author: quocbr
Github: https://github.com/quocbr
Created: #CREATIONDATE#
*/

using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
#endif

/// <summary>
/// Test - Brief description of what this script does
/// </summary>
public class Test : MonoBehaviour
{
    #region Private Serializable Fields

    // [Header("Flags")]
    // [SerializeField] private bool isEnabled;

    // [Header("Stats")]
    // [SerializeField] private int value;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
    }

    private void Start()
    {
    }

    private void Update()
    {
    }

    #endregion

    #region Public Methods

    public void ResetValues()
    {
    }

    [Button]
    public void TestDebugLog()
    {
        for (int i = 0; i < 20; i++)
        {
            //SimplePool.Spawn<GameUnit>(PoolType.Type, transform);
        }
    }

    [Button]
    public void DespawnALl()
    {
        //SimplePool.Collect(PoolType.Type);
    }

    #endregion
}