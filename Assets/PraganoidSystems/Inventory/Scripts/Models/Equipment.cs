using System;
using UnityEngine;

namespace PraganoidSystems.Inventory
{
    [Serializable]
    [CreateAssetMenu(fileName = "Equipment", menuName = "Praganoid Systems/Inventory/Items/Equipment")]
    public class Equipment : Item
    {
        [SerializeField] private EquipmentSlot equipmentSlot;
        
        public EquipmentSlot EquipmentSlot => equipmentSlot;
    }

    public enum EquipmentSlot
    {
        Head,
        Chest,
        Legs,
        Feet,
        Hands,
        Neck,
    }
}