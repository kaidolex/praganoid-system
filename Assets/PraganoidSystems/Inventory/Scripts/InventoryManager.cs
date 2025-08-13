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

        protected override void Start()
        {
            base.Start();
            InitializeInventorySlots();
        }

        private void InitializeInventorySlots()
        {
            inventorySlots.Clear();
            for (int i = 0; i < inventoryCapacity; i++)
            {
                inventorySlots.Add(new InventoryItemSlot());
            }
        }

        public bool AddItem(Item item, int amount)
        {
            if (item == null) return false;
            
            if (IsStackable(item)) return AddStackableItem(item, amount);

            // For non-stackable items, we need to add each one individually
            int itemsAdded = 0;
            for (int i = 0; i < amount; i++)
            {
                // Find first empty slot
                InventoryItemSlot emptySlot = inventorySlots.Find(slot => slot.IsEmpty);
                if (emptySlot != null)
                {
                    emptySlot.SetItem(item, 1);
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

            foreach (var slot in inventorySlots)
            {
                if (slot.CanAcceptItem(item) && !slot.IsEmpty)
                {
                    slot.AddStacks(item, remainingAmount, out int remaining);
                    remainingAmount = remaining;
                    
                    if (remainingAmount <= 0) break; // All items added successfully
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

            foreach (var slot in inventorySlots)
            {
                if (slot.IsEmpty)
                {
                    int itemsToAdd = Mathf.Min(remainingAmount, item.MaxStackSize);
                    slot.SetItem(item, itemsToAdd);
                    remainingAmount -= itemsToAdd;
                    
                    if (remainingAmount <= 0) break; // All items added successfully
                }
            }

            return remainingAmount;
        }

        public bool IsStackable(Item item) => item.MaxStackSize > 1;

        /// <summary>
        /// Gets the current number of occupied slots in the inventory
        /// </summary>
        public int GetOccupiedSlotCount()
        {
            return inventorySlots.Count(slot => !slot.IsEmpty);
        }

        /// <summary>
        /// Gets the current number of empty slots in the inventory
        /// </summary>
        public int GetEmptySlotCount()
        {
            return inventorySlots.Count(slot => slot.IsEmpty);
        }

        /// <summary>
        /// Checks if the inventory has space for a specific item and amount
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
            
            // Check existing stacks
            foreach (var slot in inventorySlots)
            {
                if (slot.CanAcceptItem(item) && !slot.IsEmpty)
                {
                    int availableSpace = slot.MaxStackSize - slot.StackSize;
                    remainingAmount -= availableSpace;
                    if (remainingAmount <= 0) return true;
                }
            }

            // Check empty slots
            foreach (var slot in inventorySlots)
            {
                if (slot.IsEmpty)
                {
                    remainingAmount -= item.MaxStackSize;
                    if (remainingAmount <= 0) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets all inventory slots (for UI purposes)
        /// </summary>
        public List<InventoryItemSlot> GetAllSlots()
        {
            return new List<InventoryItemSlot>(inventorySlots);
        }

        public int id = 1;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AddItem(database.GetItem(id), 1);
            }
        }
    }
}