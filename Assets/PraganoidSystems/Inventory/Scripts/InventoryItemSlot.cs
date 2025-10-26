using UnityEngine;

namespace PraganoidSystems.Inventory
{
    [System.Serializable]
    public class InventoryItemSlot
    {
        [SerializeField] private int currentStackSize = 0;
        [SerializeField] private int maxStackSize = 0;
        [SerializeField] private Item item = null;

        public Item Item => item;
        public int StackSize => currentStackSize;
        public int MaxStackSize => maxStackSize;
        public bool IsEmpty => item == null;
        public bool IsFull => currentStackSize >= maxStackSize;
        public bool CanAcceptItem(Item otherItem) => IsEmpty || (item == otherItem && !IsFull);

        public InventoryItemSlot(Item item)
        {
            this.item = item;
            this.maxStackSize = item?.MaxStackSize ?? 0;
            currentStackSize = item != null ? 1 : 0;
        }

        public InventoryItemSlot()
        {
            this.item = null;
            this.maxStackSize = 0;
            currentStackSize = 0;
        }


        public bool SetItem(Item item, int amount = 1)
        {
            // if the item to add is null then return false
            if (item == null) return false;

            this.item = item;
            this.maxStackSize = item.MaxStackSize;
            this.currentStackSize = Mathf.Clamp(amount, 1, maxStackSize);
            
            return true;
        }

        public void AddStacks(Item item, int amount, out int remaining)
        {
            // Calculate how many items can actually be added
            int availableSpace = item.MaxStackSize - currentStackSize;
            int itemsToAdd = Mathf.Min(amount, availableSpace);
            
            // Add items and calculate remaining
            currentStackSize += itemsToAdd;
            remaining = amount - itemsToAdd;
        }

        public bool RemoveItem(int amount)
        {
            // if slot is empty then return false
            if (item == null) return false;

            // if the stack is not enough then return false
            if (currentStackSize - amount < 0) return false;

            // remove the amount from the stack
            currentStackSize -= amount;

            // If the stack is empty after the stack is reduced then clear the slot
            if (currentStackSize <= 0) ClearSlot();

            return true;
        }

        public void ClearSlot()
        {
            item = null;
            currentStackSize = 0;
        }
    }
}