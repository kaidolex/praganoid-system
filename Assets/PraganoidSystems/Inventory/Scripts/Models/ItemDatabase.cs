using System;
using System.Collections.Generic;
using System.Linq;
using PraganoidSystems.Utils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PraganoidSystems.Inventory
{
    [Serializable]
    [CreateAssetMenu(fileName = "Item Database", menuName = "Praganoid Systems/Inventory/Database/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [Serializable]
        public class ItemDatabaseSlot 
        {
            public int id;
            public Item item;
            
            // Cache item type to avoid repeated type checks
            [System.NonSerialized]
            private System.Type _cachedItemType;
            
            public System.Type GetItemType()
            {
                if (_cachedItemType != null)
                    return _cachedItemType;
                    
                if (item == null)
                {
                    _cachedItemType = null;
                    return _cachedItemType;
                }
                
                _cachedItemType = item.GetType();
                return _cachedItemType;
            }
            
            /// <summary>
            /// Checks if the item is of a specific type or inherits from it
            /// </summary>
            public bool IsItemOfType<T>() where T : Item
            {
                return item is T;
            }
            
            /// <summary>
            /// Checks if the item is of a specific type or inherits from it
            /// </summary>
            public bool IsItemOfType(System.Type type)
            {
                if (item == null) return false;
                return type.IsAssignableFrom(item.GetType());
            }
            
            /// <summary>
            /// Gets the exact type name for the item
            /// </summary>
            public string GetItemTypeName()
            {
                return item?.GetType().Name ?? "None";
            }
            
            public void InvalidateCache()
            {
                _cachedItemType = null;
            }
        }

        [SerializeField] private List<ItemDatabaseSlot> items;
        
        // Runtime optimizations - not serialized
        [System.NonSerialized] private Dictionary<int, ItemDatabaseSlot> _itemLookup;
        [System.NonSerialized] private int _cachedMaxId = -1;
        [System.NonSerialized] private bool _isDirty = true;
        [System.NonSerialized] private Dictionary<string, List<ItemDatabaseSlot>> _searchCache;
        [System.NonSerialized] private List<string> _cachedValidationIssues;

        private void EnsureLookupTable()
        {
            if (_itemLookup == null || _isDirty)
            {
                RebuildLookupTable();
            }
        }
        
        private void RebuildLookupTable()
        {
            _itemLookup = new Dictionary<int, ItemDatabaseSlot>();
            _cachedMaxId = 0;
            
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item != null)
                    {
                        _itemLookup[item.id] = item;
                        if (item.id > _cachedMaxId)
                            _cachedMaxId = item.id;
                    }
                }
            }
            
            _isDirty = false;
            _searchCache?.Clear(); // Clear search cache when data changes
            _cachedValidationIssues = null; // Clear validation cache
        }
        
        private void MarkDirty()
        {
            _isDirty = true;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        
        private void OnEnable()
        {
            // Ensure lookup table is built when the ScriptableObject is loaded
            _isDirty = true;
        }

        public Item GetItem(int id)
        {
            EnsureLookupTable();
            
            if (_itemLookup.TryGetValue(id, out ItemDatabaseSlot slot) && slot?.item != null)
            {
                return slot.item;
            }
            
            GameLogger.Log(id, "Item not found in database");
            return null;
        }
        
        /// <summary>
        /// Gets an item slot by ID with O(1) lookup performance
        /// </summary>
        public ItemDatabaseSlot GetItemSlot(int id)
        {
            EnsureLookupTable();
            return _itemLookup.TryGetValue(id, out ItemDatabaseSlot slot) ? slot : null;
        }
        
        /// <summary>
        /// Checks if an item exists in the database with O(1) performance
        /// </summary>
        public bool HasItem(int id)
        {
            EnsureLookupTable();
            return _itemLookup.ContainsKey(id);
        }
        
        /// <summary>
        /// Gets the current item count without creating a new list
        /// </summary>
        public int GetItemCount()
        {
            return items?.Count ?? 0;
        }

        public List<ItemDatabaseSlot> GetItemsList()
        {
            if (items == null)
            {
                items = new List<ItemDatabaseSlot>();
            }
            return items;
        }

        public int GetNextAvailableId()
        {
            EnsureLookupTable();
            return Math.Max(_cachedMaxId + 1, 1); // Ensure ID is always at least 1
        }

        public ItemDatabaseSlot AddItem(Item item)
        {
            if (items == null)
                items = new List<ItemDatabaseSlot>();

            int newId = GetNextAvailableId();
            var newSlot = new ItemDatabaseSlot
            {
                id = newId,
                item = item
            };

            items.Add(newSlot);
            
            // Update lookup table immediately instead of marking dirty
            EnsureLookupTable();
            _itemLookup[newId] = newSlot;
            if (newId > _cachedMaxId)
                _cachedMaxId = newId;
            
            MarkDirty();
            return newSlot;
        }

        public ItemDatabaseSlot AddEmptySlot()
        {
            if (items == null)
                items = new List<ItemDatabaseSlot>();

            int newId = GetNextAvailableId();
            var newSlot = new ItemDatabaseSlot
            {
                id = newId,
                item = null
            };

            items.Add(newSlot);
            
            // Update lookup table immediately
            EnsureLookupTable();
            _itemLookup[newId] = newSlot;
            if (newId > _cachedMaxId)
                _cachedMaxId = newId;
            
            MarkDirty();
            return newSlot;
        }

        public bool RemoveItem(int id)
        {
            if (items == null) return false;

            EnsureLookupTable();
            if (_itemLookup.TryGetValue(id, out ItemDatabaseSlot itemToRemove))
            {
                items.Remove(itemToRemove);
                _itemLookup.Remove(id);
                
                // Only recalculate max ID if we removed the max item
                if (id == _cachedMaxId)
                {
                    _cachedMaxId = _itemLookup.Count > 0 ? _itemLookup.Keys.Max() : 0;
                }
                
                MarkDirty();
                return true;
            }
            return false;
        }

        public bool RemoveItemSlot(ItemDatabaseSlot slot)
        {
            if (items == null || slot == null) return false;

            bool removed = items.Remove(slot);
            if (removed)
            {
                EnsureLookupTable();
                _itemLookup.Remove(slot.id);
                
                // Only recalculate max ID if we removed the max item
                if (slot.id == _cachedMaxId)
                {
                    _cachedMaxId = _itemLookup.Count > 0 ? _itemLookup.Keys.Max() : 0;
                }
                
                MarkDirty();
            }
            return removed;
        }

        public void ClearDatabase()
        {
            if (items == null)
                items = new List<ItemDatabaseSlot>();
            else
                items.Clear();
                
            // Clear all caches
            _itemLookup?.Clear();
            _cachedMaxId = 0;
            _searchCache?.Clear();
            _cachedValidationIssues = null;
            
            MarkDirty();
        }

        public List<string> ValidateDatabase()
        {
            // Return cached validation if available and data hasn't changed
            if (_cachedValidationIssues != null && !_isDirty)
            {
                return new List<string>(_cachedValidationIssues);
            }
            
            var issues = new List<string>();
            
            if (items == null)
            {
                _cachedValidationIssues = issues;
                return issues;
            }

            EnsureLookupTable();
            
            // Use the lookup table to detect duplicates more efficiently
            var duplicateIds = new List<int>();
            var seenIds = new HashSet<int>();
            int nullItemCount = 0;
            int invalidIdCount = 0;
            
            foreach (var item in items)
            {
                if (item == null) continue;
                
                // Check for duplicate IDs
                if (!seenIds.Add(item.id))
                {
                    duplicateIds.Add(item.id);
                }
                
                // Check for null items
                if (item.item == null)
                {
                    nullItemCount++;
                }
                
                // Check for invalid IDs
                if (item.id <= 0)
                {
                    invalidIdCount++;
                }
            }
            
            if (duplicateIds.Count > 0)
            {
                issues.Add($"Duplicate IDs found: {string.Join(", ", duplicateIds)}");
            }
            
            if (nullItemCount > 0)
            {
                issues.Add($"Empty slots found: {nullItemCount} slots without assigned items");
            }
            
            if (invalidIdCount > 0)
            {
                issues.Add($"Invalid IDs found: {invalidIdCount} items with ID <= 0 (IDs must start from 1)");
            }
            
            _cachedValidationIssues = issues;
            return new List<string>(issues);
        }

        /// <summary>
        /// Gets filtered items with flexible type filtering
        /// </summary>
        public List<ItemDatabaseSlot> GetFilteredItems(string searchText = "", bool filterByRarity = false, Rarity rarity = Rarity.Common, 
            bool filterByType = false, System.Type[] allowedTypes = null,
            bool filterByEquipmentSlot = false, EquipmentSlot equipmentSlot = EquipmentSlot.Head)
        {
            // Use default allowed types if none specified
            if (filterByType && allowedTypes == null)
            {
                allowedTypes = new System.Type[] { typeof(Item), typeof(Materials), typeof(Consumable), typeof(Equipment) };
            }
            
            return GetFilteredItemsInternal(searchText, filterByRarity, rarity, filterByType, allowedTypes, filterByEquipmentSlot, equipmentSlot);
        }
        
        /// <summary>
        /// Legacy method for backward compatibility - converts boolean flags to type array
        /// </summary>
        public List<ItemDatabaseSlot> GetFilteredItems(string searchText = "", bool filterByRarity = false, Rarity rarity = Rarity.Common, 
            bool filterByType = false, bool includeBaseItems = true, bool includeMaterials = true, bool includeConsumables = true, bool includeEquipment = true,
            bool filterByEquipmentSlot = false, EquipmentSlot equipmentSlot = EquipmentSlot.Head)
        {
            // Convert boolean flags to type array for backward compatibility
            System.Type[] allowedTypes = null;
            if (filterByType)
            {
                var typeList = new List<System.Type>();
                if (includeBaseItems) typeList.Add(typeof(Item));
                if (includeMaterials) typeList.Add(typeof(Materials));
                if (includeConsumables) typeList.Add(typeof(Consumable));
                if (includeEquipment) typeList.Add(typeof(Equipment));
                allowedTypes = typeList.ToArray();
            }
            
            return GetFilteredItemsInternal(searchText, filterByRarity, rarity, filterByType, allowedTypes, filterByEquipmentSlot, equipmentSlot);
        }
        
        private List<ItemDatabaseSlot> GetFilteredItemsInternal(string searchText, bool filterByRarity, Rarity rarity, 
            bool filterByType, System.Type[] allowedTypes, bool filterByEquipmentSlot, EquipmentSlot equipmentSlot)
        {
            if (items == null) return new List<ItemDatabaseSlot>();

            // Initialize search cache if not exists
            if (_searchCache == null)
                _searchCache = new Dictionary<string, List<ItemDatabaseSlot>>();

            // Create a cache key for this specific filter combination
            string typeKey = allowedTypes == null ? "null" : string.Join(",", allowedTypes.Select(t => t.Name));
            string cacheKey = $"{searchText}|{filterByRarity}|{rarity}|{filterByType}|{typeKey}|{filterByEquipmentSlot}|{equipmentSlot}";
            
            // Return cached result if available and data hasn't changed
            if (!_isDirty && _searchCache.TryGetValue(cacheKey, out List<ItemDatabaseSlot> cachedResult))
            {
                return new List<ItemDatabaseSlot>(cachedResult);
            }

            var filtered = new List<ItemDatabaseSlot>();
            
            // Pre-process search text for efficiency
            string normalizedSearchText = string.IsNullOrEmpty(searchText) ? null : searchText.ToLowerInvariant();
            
            // Early exit if no type filters are enabled
            if (filterByType && (allowedTypes == null || allowedTypes.Length == 0))
            {
                _searchCache[cacheKey] = filtered;
                return filtered;
            }

            foreach (var slot in items)
            {
                if (slot?.item == null) continue;

                var item = slot.item;

                // Search filter - optimized string comparisons
                if (normalizedSearchText != null)
                {
                    bool matchesSearch = item.Name.ToLowerInvariant().Contains(normalizedSearchText) ||
                                       item.Description.ToLowerInvariant().Contains(normalizedSearchText);
                    if (!matchesSearch) continue;
                }

                // Rarity filter
                if (filterByRarity && item.Rarity != rarity) continue;

                // Type filter - use flexible type checking
                if (filterByType && allowedTypes != null)
                {
                    bool matchesType = false;
                    foreach (var allowedType in allowedTypes)
                    {
                        if (slot.IsItemOfType(allowedType))
                        {
                            matchesType = true;
                            break;
                        }
                    }
                    
                    if (!matchesType) continue;
                }

                // Equipment slot filter - no reflection needed
                if (filterByEquipmentSlot)
                {
                    if (item is Equipment equipment)
                    {
                        if (equipment.EquipmentSlot != equipmentSlot) continue;
                    }
                    else
                    {
                        continue; // Not equipment, doesn't match equipment slot filter
                    }
                }

                filtered.Add(slot);
            }

            // Cache the result for future use (limit cache size to prevent memory bloat)
            if (_searchCache.Count >= 50) // Limit cache to 50 entries
            {
                _searchCache.Clear();
            }
            _searchCache[cacheKey] = filtered;
            return new List<ItemDatabaseSlot>(filtered);
        }
        
        /// <summary>
        /// Clears all runtime caches to free memory
        /// </summary>
        public void ClearCaches()
        {
            _searchCache?.Clear();
            _cachedValidationIssues = null;
            
            // Invalidate type caches on all slots
            if (items != null)
            {
                foreach (var slot in items)
                {
                    slot?.InvalidateCache();
                }
            }
        }
        
        /// <summary>
        /// Gets memory usage statistics for debugging
        /// </summary>
        public string GetCacheStats()
        {
            int searchCacheSize = _searchCache?.Count ?? 0;
            bool hasValidationCache = _cachedValidationIssues != null;
            bool hasLookupTable = _itemLookup != null;
            
            return $"Search Cache: {searchCacheSize} entries, Validation Cache: {hasValidationCache}, Lookup Table: {hasLookupTable}";
        }
        
        /// <summary>
        /// Gets all items of a specific type
        /// </summary>
        public List<ItemDatabaseSlot> GetItemsOfType<T>() where T : Item
        {
            return GetFilteredItems(filterByType: true, allowedTypes: new System.Type[] { typeof(T) });
        }
        
        /// <summary>
        /// Gets all items of specific types
        /// </summary>
        public List<ItemDatabaseSlot> GetItemsOfTypes(params System.Type[] types)
        {
            return GetFilteredItems(filterByType: true, allowedTypes: types);
        }
        
        /// <summary>
        /// Gets count of items of a specific type
        /// </summary>
        public int GetItemCountOfType<T>() where T : Item
        {
            if (items == null) return 0;
            
            int count = 0;
            foreach (var slot in items)
            {
                if (slot?.item is T)
                    count++;
            }
            return count;
        }
        
        /// <summary>
        /// Gets all unique item types currently in the database
        /// </summary>
        public List<System.Type> GetAllItemTypes()
        {
            if (items == null) return new List<System.Type>();
            
            var types = new HashSet<System.Type>();
            foreach (var slot in items)
            {
                if (slot?.item != null)
                {
                    types.Add(slot.GetItemType());
                }
            }
            return types.ToList();
        }
        
        /// <summary>
        /// Gets item type statistics for debugging and analytics
        /// </summary>
        public Dictionary<string, int> GetItemTypeStatistics()
        {
            var stats = new Dictionary<string, int>();
            
            if (items == null) return stats;
            
            foreach (var slot in items)
            {
                if (slot?.item != null)
                {
                    string typeName = slot.GetItemTypeName();
                    stats[typeName] = stats.GetValueOrDefault(typeName, 0) + 1;
                }
            }
            
            return stats;
        }
    }
}


