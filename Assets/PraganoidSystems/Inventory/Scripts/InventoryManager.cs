using System.Collections.Generic;
using System.ComponentModel;
using PraganoidSystems.Utils;
using UnityEngine;
using UnityEngine.AI;

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
        }

        public bool AddItem(Item item, int amount)
        {
            if (IsStackable(item)) return AddStackableItem(item, amount);


            // if the inventory is full then return false
            if (inventorySlots.Count >= inventoryCapacity) return false;

            return false;
        }

        public bool AddStackableItem(Item item, int amount)
        {
            return true;
        }

        public bool IsStackable(Item item) => item.MaxStackSize > 1;
    }
}