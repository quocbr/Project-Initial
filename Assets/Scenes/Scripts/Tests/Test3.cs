/*
Author: quocbr
Github: https://github.com/quocbr
Created: #CREATIONDATE#
*/

using quocbr.Common;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace quocbr
{
    /// <summary>
    /// Test3 - Brief description of what this script does
    /// </summary>
    public class Test3 : MonoBehaviour
    {
        #region Private Serializable Fields

        // [Header("Flags")]
        // [SerializeField] private bool isEnabled;

        // [Header("Stats")]
        // [SerializeField] private int value;

        #endregion

        #region Private Fields

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

        #endregion

        #region Private Methods

        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Test3))]
    [CanEditMultipleObjects]
    public class Test3Editor : Editor
    {
        private Test3 _target;
        private Texture2D _icon;

        private void OnEnable()
        {
            _target = (Test3)target;
            _icon = Resources.Load<Texture2D>("frog");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            DrawActions();
        }

        private void DrawActions()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(
                    new GUIContent("Reset Values", _icon),
                    GUILayout.Width(150)))
            {
                Undo.RecordObject(_target, "Reset Values");
                _target.ResetValues();
                EditorUtility.SetDirty(_target);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}
