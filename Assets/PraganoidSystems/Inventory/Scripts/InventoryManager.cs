using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PraganoidSystems.Utils;
using UnityEngine;


namespace PraganoidSystems.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>
    {

        [SerializeField] private ItemDatabase database;
        [SerializeField] private int inventoryCapacity = 3;
        [ReadOnly(true)]
        [SerializeField] private List<InventoryItemSlot> inventorySlots = new List<InventoryItemSlot>();

        // Empty Slot Locator
        private Queue<int> emptySlotIndices = new Queue<int>();

        // Item Locator
        private Dictionary<Item, List<int>> itemSlotMap = new Dictionary<Item, List<int>>();

        // Occupied Slot Counter
        private int occupiedSlotCount = 0;

        protected override void Start()
        {
            base.Start();
            InitializeInventorySlots();
        }

        private void InitializeInventorySlots()
        {
            inventorySlots.Clear();
            emptySlotIndices.Clear();
            itemSlotMap.Clear();
            occupiedSlotCount = 0;

            for (int i = 0; i < inventoryCapacity; i++)
            {
                inventorySlots.Add(new InventoryItemSlot());
                emptySlotIndices.Enqueue(i);
            }
        }

        /// <summary>
        /// Updates caches when a slot is filled
        /// </summary>
        private void OnSlotFilled(int slotIndex, Item item)
        {
            occupiedSlotCount++;
            
            if (!itemSlotMap.ContainsKey(item))
            {
                itemSlotMap[item] = new List<int>();
            }
            itemSlotMap[item].Add(slotIndex);
        }

        /// <summary>
        /// Updates caches when a slot is emptied
        /// </summary>
        private void OnSlotEmptied(int slotIndex, Item item)
        {
            occupiedSlotCount--;
            emptySlotIndices.Enqueue(slotIndex);
            
            if (itemSlotMap.ContainsKey(item))
            {
                itemSlotMap[item].Remove(slotIndex);
                if (itemSlotMap[item].Count == 0)
                {
                    itemSlotMap.Remove(item);
                }
            }
        }

        /// <summary>
        /// Gets the next available empty slot index, O(1) operation
        /// </summary>
        private int GetNextEmptySlotIndex()
        {
            return emptySlotIndices.Count > 0 ? emptySlotIndices.Dequeue() : -1;
        }

        public bool AddItem(Item item, int amount)
        {
            if (item == null) return false;
            
            if (IsStackable(item)) return AddStackableItem(item, amount);

            // For non-stackable items, we need to add each one individually
            int itemsAdded = 0;
            for (int i = 0; i < amount; i++)
            {
                int emptyIndex = GetNextEmptySlotIndex();
                if (emptyIndex != -1)
                {
                    inventorySlots[emptyIndex].SetItem(item, 1);
                    OnSlotFilled(emptyIndex, item);
                    itemsAdded++;
                }
                else
                {
                    break; // No more empty slots
                }
            }

            return itemsAdded > 0;
        }

        public bool AddStackableItem(Item item, int amount)
        {
            if (item == null || amount <= 0) return false;

            int remainingAmount = amount;

            // First, try to add to existing stacks of the same item
            remainingAmount = TryAddToExistingStacks(item, remainingAmount);
            if (remainingAmount <= 0) return true; // All items added successfully

            // If there are still items to add, try to find empty slots
            remainingAmount = TryAddToEmptySlots(item, remainingAmount);

            // Return true if we managed to add at least some items
            return remainingAmount < amount;
        }

        /// <summary>
        /// Attempts to add items to existing stacks of the same item type
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="amount">The amount to add</param>
        /// <returns>The remaining amount that couldn't be added</returns>
        private int TryAddToExistingStacks(Item item, int amount)
        {
            int remainingAmount = amount;

            // Use cached item slot map for O(1) lookup instead of O(n) search
            if (itemSlotMap.ContainsKey(item))
            {
                var slotsWithItem = itemSlotMap[item];
                for (int i = 0; i < slotsWithItem.Count && remainingAmount > 0; i++)
                {
                    var slot = inventorySlots[slotsWithItem[i]];
                    if (slot.CanAcceptItem(item))
                    {
                        slot.AddStacks(item, remainingAmount, out int remaining);
                        remainingAmount = remaining;
                    }
                }
            }

            return remainingAmount;
        }

        /// <summary>
        /// Attempts to add items to empty slots
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="amount">The amount to add</param>
        /// <returns>The remaining amount that couldn't be added</returns>
        private int TryAddToEmptySlots(Item item, int amount)
        {
            int remainingAmount = amount;

            // Use cached empty slot indices for O(1) lookups
            while (remainingAmount > 0 && emptySlotIndices.Count > 0)
            {
                int emptyIndex = GetNextEmptySlotIndex();
                if (emptyIndex != -1)
                {
                    int itemsToAdd = Mathf.Min(remainingAmount, item.MaxStackSize);
                    inventorySlots[emptyIndex].SetItem(item, itemsToAdd);
                    OnSlotFilled(emptyIndex, item);
                    remainingAmount -= itemsToAdd;
                }
                else
                {
                    break;
                }
            }

            return remainingAmount;
        }

        public bool IsStackable(Item item) => item.MaxStackSize > 1;

        /// <summary>
        /// Gets the current number of occupied slots in the inventory - O(1) operation
        /// </summary>
        public int GetOccupiedSlotCount()
        {
            return occupiedSlotCount;
        }

        /// <summary>
        /// Gets the current number of empty slots in the inventory - O(1) operation
        /// </summary>
        public int GetEmptySlotCount()
        {
            return inventoryCapacity - occupiedSlotCount;
        }

        /// <summary>
        /// Checks if the inventory has space for a specific item and amount - Optimized version
        /// </summary>
        public bool CanAddItem(Item item, int amount)
        {
            if (item == null || amount <= 0) return false;

            if (!IsStackable(item))
            {
                return GetEmptySlotCount() >= amount;
            }

            // For stackable items, simulate adding to see if there's space
            int remainingAmount = amount;
            
            // Check existing stacks using cached item slot map
            if (itemSlotMap.ContainsKey(item))
            {
                var slotsWithItem = itemSlotMap[item];
                for (int i = 0; i < slotsWithItem.Count; i++)
                {
                    var slot = inventorySlots[slotsWithItem[i]];
                    if (slot.CanAcceptItem(item))
                    {
                        int availableSpace = slot.MaxStackSize - slot.StackSize;
                        remainingAmount -= availableSpace;
                        if (remainingAmount <= 0) return true;
                    }
                }
            }

            // Check empty slots using cached count
            int emptySlots = GetEmptySlotCount();
            remainingAmount -= emptySlots * item.MaxStackSize;

            return remainingAmount <= 0;
        }

        /// <summary>
        /// Gets all inventory slots (for UI purposes) - Returns direct reference to avoid allocation
        /// </summary>
        public List<InventoryItemSlot> GetAllSlots()
        {
            return inventorySlots;
        }

        /// <summary>
        /// Gets a read-only view of inventory slots if you need immutability
        /// </summary>
        public IReadOnlyList<InventoryItemSlot> GetSlotsReadOnly()
        {
            return inventorySlots.AsReadOnly();
        }

        /// <summary>
        /// Removes items from inventory efficiently using cached lookups
        /// </summary>
        public bool RemoveItem(Item item, int amount)
        {
            if (item == null || amount <= 0 || !itemSlotMap.ContainsKey(item)) return false;

            int remainingToRemove = amount;
            var slotsWithItem = itemSlotMap[item];

            // Work backwards to avoid index issues when removing from list
            for (int i = slotsWithItem.Count - 1; i >= 0 && remainingToRemove > 0; i--)
            {
                var slotIndex = slotsWithItem[i];
                var slot = inventorySlots[slotIndex];
                
                int canRemove = Mathf.Min(remainingToRemove, slot.StackSize);
                if (slot.RemoveItem(canRemove))
                {
                    remainingToRemove -= canRemove;
                    
                    // If slot is now empty, update caches
                    if (slot.IsEmpty)
                    {
                        OnSlotEmptied(slotIndex, item);
                    }
                }
            }

            return remainingToRemove < amount; // Return true if we removed at least some items
        }


        // For debugging purposes
        public int itemId = 1;
        public int itemAmount = 100;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AddItem(database.GetItem(itemId), itemAmount);
            }
        }
    }
}