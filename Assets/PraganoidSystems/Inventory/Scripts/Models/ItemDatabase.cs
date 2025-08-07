using System;
using System.Collections.Generic;
using PraganoidSystems.Utils;
using UnityEngine;

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
            Item item = items.Find(item => item.id == id).item;

            if (item == null)
            {
                GameLogger.Log(id, "Item not found in database");
                return null;
            }

            return item;
        }
    }
}


