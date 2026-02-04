using System.Collections.Generic;
using UnityEngine;

namespace quocbr.DesignPattern
{
    /// <summary>
    /// Factory tổng quát tái sử dụng.
    /// TKey: ID định danh (Enum, String...)
    /// TValue: Loại tài nguyên (GameObject, AudioClip, Sprite...)
    /// </summary>
    public abstract class BaseFactorySO<TKey, TValue> : ScriptableObject
    {
        [System.Serializable]
        public struct FactoryEntry
        {
            public TKey id;
            public TValue value;
        }

        [Header("Data Collection")]
        [SerializeField] private List<FactoryEntry> items;

        // Dictionary dùng để tra cứu nhanh O(1)
        private Dictionary<TKey, TValue> _lookupTable;
        private bool _isInitialized;

        private void InitializeLookup()
        {
            if (_isInitialized) return;

            _lookupTable = new Dictionary<TKey, TValue>();
            foreach (var item in items)
            {
                if (!_lookupTable.ContainsKey(item.id))
                {
                    _lookupTable.Add(item.id, item.value);
                }
                else
                {
                    Debug.LogWarning($"[Factory] Trùng lặp Key '{item.id}' trong {name}. Chỉ lấy item đầu tiên.");
                }
            }
            _isInitialized = true;
        }

        /// <summary>
        /// Lấy Item dựa trên Key. Trả về giá trị mặc định nếu không tìm thấy.
        /// </summary>
        public TValue GetItem(TKey id)
        {
            InitializeLookup();

            if (_lookupTable.TryGetValue(id, out TValue result))
            {
                return result;
            }

            Debug.LogError($"[Factory] Không tìm thấy Key '{id}' trong Factory '{name}'!");
            return default;
        }

        /// <summary>
        /// Kiểm tra xem Key có tồn tại trong Factory không.
        /// </summary>
        public bool HasItem(TKey id)
        {
            InitializeLookup();
            return _lookupTable.ContainsKey(id);
        }

        /// <summary>
        /// Lấy số lượng items trong Factory.
        /// </summary>
        public int Count
        {
            get
            {
                InitializeLookup();
                return _lookupTable.Count;
            }
        }

        /// <summary>
        /// Lấy tất cả keys có sẵn trong Factory.
        /// </summary>
        public IEnumerable<TKey> GetAllKeys()
        {
            InitializeLookup();
            return _lookupTable.Keys;
        }

        // Reset lại cache khi thay đổi dữ liệu trong Editor (chỉ chạy editor)
        private void OnValidate()
        {
            _isInitialized = false;

            // Kiểm tra null values và duplicate keys
            if (items != null && items.Count > 0)
            {
                var checkedKeys = new HashSet<TKey>();
                
                foreach (var item in items)
                {
                    // Kiểm tra null value
                    if (item.value == null || item.value.Equals(null))
                    {
                        Debug.LogWarning($"[Factory] Item với key '{item.id}' có value null trong '{name}'");
                    }

                    // Kiểm tra duplicate key
                    if (!checkedKeys.Add(item.id))
                    {
                        Debug.LogWarning($"[Factory] Phát hiện key trùng lặp '{item.id}' trong '{name}'");
                    }
                }
            }
        }
    }
}