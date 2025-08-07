using System;
using PraganoidSystems.Utils;
using Unity.VisualScripting;
using UnityEngine;


namespace PraganoidSystems.Inventory
{
    [Serializable]
    [CreateAssetMenu(fileName = "Base Item", menuName = "Praganoid Systems/Inventory/Items/Base Item")]
    public class BaseItem : ScriptableObject
    {
        [SerializeField] private new string name;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private int maxStackSize;
        [SerializeField] private int sellPrice;
        [SerializeField] private int buyPrice;
        [SerializeField] private Rarity rarity;
        [SerializeField] private ItemType itemType = ItemType.Other;

        public Action<BaseItem> OnItemUsed;

        public string Name => name;
        public string Description => description;
        public Sprite Icon => icon;
        public int MaxStackSize => maxStackSize;
        public int SellPrice => sellPrice;
        public int BuyPrice => buyPrice;
        public Rarity Rarity => rarity;
        public ItemType ItemType => itemType;

        [ContextMenu("Use Item (For Debug)")]
        public virtual void Use() 
        {
            GameLogger.Log(name, "is used");
            OnItemUsed?.Invoke(this);
        }
    }

    public enum Rarity 
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Mythic = 5,
        Unique = 6
    }

    public enum ItemType
    {
        Currency,
        Materials,
        Consumable,
        Equipment,
        Quest,
        Other
    }
}


