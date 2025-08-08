using PraganoidSystems.Utils;
using UnityEngine;

namespace PraganoidSystems.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        [SerializeField] private ItemDatabase database;
    }
}