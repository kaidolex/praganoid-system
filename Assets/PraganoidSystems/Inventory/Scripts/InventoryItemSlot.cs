using UnityEngine;

namespace PraganoidSystems.Inventory
{
    public class InventoryItemSlot : MonoBehaviour
    {
        [SerializeField] private int stackSize;
        [SerializeField] private Item item;
        
        public bool AddItem(Item item) 
        {

            // item is null then return false
            if (item == null) return false;

            // if the item is the same as the item in the slot then add to the stack
            


















            // if (item == null) return false; 

            // this.item = item;
            // stackSize++;

            // return true;
        }

        public bool RemoveItem(int amount)
        {
            // if slot is empty then return false
            if (item == null) return false;

            // if the stack is not enough then return false
            if (stackSize - amount < 0) return false;

            // remove the amount from the stack
            stackSize -= amount;

            // If the stack is empty after the stack is reduced then clear the slot
            if (stackSize <= 0) ClearSlot();

            return true;
        }

        public void ClearSlot()
        {
            item = null;
            stackSize = 0;
        }
    }
}