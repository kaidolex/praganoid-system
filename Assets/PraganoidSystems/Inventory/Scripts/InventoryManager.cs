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
        [SerializeField] private InventoryItemSlot[] inventorySlots;

        protected override void Start()
        {
            base.Start();
            InitializeInventorySlots();
        }

        private void InitializeInventorySlots()
        {
            inventorySlots = new InventoryItemSlot[inventoryCapacity];
        }

        public bool AddItem(Item item, int amount)
        {
            // if the inventory is full then return false
            if (inventorySlots.Length >= inventoryCapacity) return false;




            return false;
        }
    }
}