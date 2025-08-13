using System;
using UnityEngine;

namespace PraganoidSystems.Inventory
{
    /// <summary>
    /// Example of a new item type that can be added without modifying ItemDatabase
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Weapon", menuName = "Praganoid Systems/Inventory/Items/Weapon")]
    public class Weapon : Equipment
    {
        [SerializeField] private WeaponType weaponType;
        [SerializeField] private int damage;
        [SerializeField] private float attackSpeed;
        [SerializeField] private float range;
        
        public WeaponType WeaponType => weaponType;
        public int Damage => damage;
        public float AttackSpeed => attackSpeed;
        public float Range => range;
        
        public override void Use()
        {
            base.Use();
            // Weapon-specific usage logic here
        }
    }
    
    public enum WeaponType
    {
        Sword,
        Bow,
        Staff,
        Dagger,
        Hammer,
        Axe
    }
}