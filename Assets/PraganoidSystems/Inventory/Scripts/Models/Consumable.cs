using System;
using UnityEngine;

namespace PraganoidSystems.Inventory
{
    [Serializable]
    [CreateAssetMenu(fileName = "Consumable", menuName = "Praganoid Systems/Inventory/Items/Consumable")]
    public class Consumable : Item
    {
        [SerializeField] private int health;
        [SerializeField] private int mana;
        [SerializeField] private int stamina;

        public int Health => health;
        public int Mana => mana;
        public int Stamina => stamina;

        public override void Use()
        {
            base.Use();
        }
    }
}