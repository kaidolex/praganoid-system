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
        }

        [SerializeField] private List<ItemDatabaseSlot> items;

        public Item GetItem(int id)
        {
            ItemDatabaseSlot slot = items.Find(item => item.id == id);
            
            if (slot?.item == null)
            {
                GameLogger.Log(id, "Item not found in database");
                return null;
            }

            return slot.item;
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
            if (items == null || items.Count == 0) return 1;
            
            int maxId = items.Max(item => item.id);
            return Math.Max(maxId + 1, 1); // Ensure ID is always at least 1
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
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            
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
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
            
            return newSlot;
        }

        public bool RemoveItem(int id)
        {
            if (items == null) return false;

            var itemToRemove = items.Find(item => item.id == id);
            if (itemToRemove != null)
            {
                items.Remove(itemToRemove);
                
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
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
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
            return removed;
        }

        public void ClearDatabase()
        {
            if (items == null)
                items = new List<ItemDatabaseSlot>();
            else
                items.Clear();
                
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public List<string> ValidateDatabase()
        {
            var issues = new List<string>();
            
            if (items == null) return issues;

            // Check for duplicate IDs
            var duplicateIds = items.GroupBy(x => x.id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateIds.Count > 0)
            {
                issues.Add($"Duplicate IDs found: {string.Join(", ", duplicateIds)}");
            }

            // Check for null items
            var nullItems = items.Where(x => x.item == null).ToList();
            if (nullItems.Count > 0)
            {
                issues.Add($"Empty slots found: {nullItems.Count} slots without assigned items");
            }

            // Check for invalid IDs (negative or zero)
            var invalidIds = items.Where(x => x.id <= 0).ToList();
            if (invalidIds.Count > 0)
            {
                issues.Add($"Invalid IDs found: {invalidIds.Count} items with ID <= 0 (IDs must start from 1)");
            }

            return issues;
        }

        public List<ItemDatabaseSlot> GetFilteredItems(string searchText = "", bool filterByRarity = false, Rarity rarity = Rarity.Common, 
            bool filterByType = false, bool includeBaseItems = true, bool includeMaterials = true, bool includeConsumables = true, bool includeEquipment = true,
            bool filterByEquipmentSlot = false, EquipmentSlot equipmentSlot = EquipmentSlot.Head)
        {
            if (items == null) return new List<ItemDatabaseSlot>();

            var filtered = items.Where(item =>
            {
                if (item.item == null) return false;

                // Search filter
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   item.item.Name.ToLower().Contains(searchText.ToLower()) ||
                                   item.item.Description.ToLower().Contains(searchText.ToLower());

                // Rarity filter
                bool matchesRarity = !filterByRarity || item.item.Rarity == rarity;

                // Type filter
                bool matchesType = true;
                if (filterByType)
                {
                    if (!includeBaseItems && !includeMaterials && !includeConsumables && !includeEquipment)
                    {
                        matchesType = false;
                    }
                    else
                    {
                        matchesType = false;

                        if (includeMaterials && item.item is Materials)
                            matchesType = true;
                        else if (includeConsumables && item.item is Consumable)
                            matchesType = true;
                        else if (includeEquipment && item.item is Equipment)
                            matchesType = true;
                        else if (includeBaseItems && !(item.item is Materials) && !(item.item is Consumable) && !(item.item is Equipment))
                            matchesType = true;
                    }
                }

                // Equipment slot filter
                bool matchesEquipmentSlot = true;
                if (filterByEquipmentSlot && item.item is Equipment equipment)
                {
                    var equipmentSlotField = typeof(Equipment).GetField("equipmentSlot",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var currentSlot = (EquipmentSlot)equipmentSlotField?.GetValue(equipment);

                    matchesEquipmentSlot = currentSlot == equipmentSlot;
                }
                else if (filterByEquipmentSlot && !(item.item is Equipment))
                {
                    matchesEquipmentSlot = false;
                }

                return matchesSearch && matchesRarity && matchesType && matchesEquipmentSlot;
            }).ToList();

            return filtered;
        }
    }
}


