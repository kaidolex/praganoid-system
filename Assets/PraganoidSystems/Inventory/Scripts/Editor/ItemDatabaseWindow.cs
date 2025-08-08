using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace PraganoidSystems.Inventory
{
    public class ItemDatabaseWindow : EditorWindow
    {
        private ItemDatabase itemDatabase;
        private Vector2 scrollPosition;
        private string searchText = "";
        private Rarity filterRarity = Rarity.Common;
        private bool showFilterRarity = false;
        private bool showFilterType = false;
        private bool filterConsumable = false;
        private bool filterEquipment = false;
        private bool filterMaterials = false;
        private bool filterBaseItem = false;
        private bool showFilterEquipmentSlot = false;
        private EquipmentSlot filterEquipmentSlot = EquipmentSlot.Head;
        private int selectedIndex = -1;
        private bool showCreateItem = false;
        private string newItemName = "";
        private string newItemDescription = "";
        private int newItemMaxStackSize = 1;
        private int newItemSellPrice = 0;
        private int newItemBuyPrice = 0;
        private Rarity newItemRarity = Rarity.Common;
        private int newItemHealth = 0;
        private int newItemMana = 0;
        private int newItemStamina = 0;
        private bool isNewItemConsumable = false;
        private bool isNewItemEquipment = false;
        private bool isNewItemMaterial = true; // Default to Materials
        private EquipmentSlot newItemEquipmentSlot = EquipmentSlot.Head;
        private int selectedTab = 0;
        private ItemDatabase.ItemDatabaseSlot selectedItemSlot = null;
        private Sprite newItemIcon = null;

        [MenuItem("Window/Praganoid Systems/Item Database Editor")]
        public static void ShowWindow()
        {
            GetWindow<ItemDatabaseWindow>("Item Database Editor");
        }

        private void OnEnable()
        {
            LoadItemDatabase();
        }



        private void LoadItemDatabase()
        {
            // If we already have a database, keep it
            if (itemDatabase != null) return;
            
            // Try to find existing database
            string[] guids = AssetDatabase.FindAssets("t:ItemDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabase>(path);
            }
            // Don't automatically create a new database - let the user choose
        }

        private void CreateNewDatabase()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create New Item Database",
                "New Item Database",
                "asset",
                "Please enter a name for the new Item Database"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                itemDatabase = CreateInstance<ItemDatabase>();
                AssetDatabase.CreateAsset(itemDatabase, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                selectedItemSlot = null; // Clear selection for new database
            }
        }

        private void OnGUI()
        {
            // Always try to load a database on first run
            if (itemDatabase == null)
            {
                LoadItemDatabase();
            }

            EditorGUILayout.BeginVertical();

            // Always show header (includes database selection and New DB button)
            DrawHeader();

            // Only show tabs if we have a database
            DrawTabs();

            EditorGUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Item Database Editor", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                LoadItemDatabase();
            }
            
            if (GUILayout.Button("Save", GUILayout.Width(80)))
            {
                if (itemDatabase != null)
                {
                    EditorUtility.SetDirty(itemDatabase);
                    AssetDatabase.SaveAssets();
                }
            }
            
            if (GUILayout.Button("New DB", GUILayout.Width(80)))
            {
                CreateNewDatabase();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Database Selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Database:", GUILayout.Width(70));
            
            ItemDatabase newDatabase = (ItemDatabase)EditorGUILayout.ObjectField(itemDatabase, typeof(ItemDatabase), false);
            if (newDatabase != itemDatabase)
            {
                itemDatabase = newDatabase;
                selectedItemSlot = null; // Clear selection when switching databases
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            
            // Database statistics
            if (itemDatabase != null)
            {
                var items = GetItemsList();
                int totalSlots = items.Count;
                int assignedSlots = items.Count(x => x.item != null);
                int emptySlots = totalSlots - assignedSlots;
                
                EditorGUILayout.LabelField($"Database: {itemDatabase.name} | Total Slots: {totalSlots} | Assigned: {assignedSlots} | Empty: {emptySlots}", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.HelpBox("No database selected. Please select an ItemDatabase asset or create a new one.", MessageType.Warning);
            }
            
            EditorGUILayout.Space();
        }

        private void DrawTabs()
        {
            if (itemDatabase == null)
            {
                EditorGUILayout.HelpBox("Please select an ItemDatabase asset to begin editing.", MessageType.Info);
                return;
            }
            
            string[] tabNames = { "Items", "Create Item" };
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            
            EditorGUILayout.Space();
            
            switch (selectedTab)
            {
                case 0:
                    DrawItemsTab();
                    break;
                case 1:
                    DrawCreateItemTab();
                    break;
            }
        }

        private void DrawItemsTab()
        {
            EditorGUILayout.BeginHorizontal();
            
            // Left side - Item List
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            DrawItemList();
            EditorGUILayout.EndVertical();
            
            // Right side - Item Details
            EditorGUILayout.BeginVertical();
            DrawItemDetails();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCreateItemTab()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Create New Item", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Basic Properties
            EditorGUILayout.LabelField("Basic Properties", EditorStyles.boldLabel);
            newItemName = EditorGUILayout.TextField("Name:", newItemName);
            newItemDescription = EditorGUILayout.TextField("Description:", newItemDescription);
            newItemIcon = (Sprite)EditorGUILayout.ObjectField("Icon:", newItemIcon, typeof(Sprite), false);
            newItemMaxStackSize = EditorGUILayout.IntField("Max Stack Size:", newItemMaxStackSize);
            newItemSellPrice = EditorGUILayout.IntField("Sell Price:", newItemSellPrice);
            newItemBuyPrice = EditorGUILayout.IntField("Buy Price:", newItemBuyPrice);
            newItemRarity = (Rarity)EditorGUILayout.EnumPopup("Rarity:", newItemRarity);
            
            EditorGUILayout.Space();
            
            // Item Type
            EditorGUILayout.LabelField("Item Type", EditorStyles.boldLabel);
            
            // Radio button behavior for item types
            int currentSelection = 0; // 0 = Material, 1 = Consumable, 2 = Equipment
            if (isNewItemMaterial) currentSelection = 0;
            else if (isNewItemConsumable) currentSelection = 1;
            else if (isNewItemEquipment) currentSelection = 2;
            
            string[] itemTypeOptions = { "Material", "Consumable", "Equipment" };
            int newSelection = GUILayout.SelectionGrid(currentSelection, itemTypeOptions, 3);
            
            // Update boolean flags based on selection
            isNewItemMaterial = (newSelection == 0);
            isNewItemConsumable = (newSelection == 1);
            isNewItemEquipment = (newSelection == 2);
            
            // Type-specific properties
            if (isNewItemConsumable)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Consumable Properties", EditorStyles.boldLabel);
                newItemHealth = EditorGUILayout.IntField("Health:", newItemHealth);
                newItemMana = EditorGUILayout.IntField("Mana:", newItemMana);
                newItemStamina = EditorGUILayout.IntField("Stamina:", newItemStamina);
            }
            else if (isNewItemEquipment)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Equipment Properties", EditorStyles.boldLabel);
                newItemEquipmentSlot = (EquipmentSlot)EditorGUILayout.EnumPopup("Equipment Slot:", newItemEquipmentSlot);
            }
            
            EditorGUILayout.Space();
            
            // Action buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create Item"))
            {
                CreateNewItem();
            }
            
            if (GUILayout.Button("Clear Form"))
            {
                ClearCreateForm();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawSearchAndFilter()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Search & Filter", EditorStyles.boldLabel);
            
            // Search
            searchText = EditorGUILayout.TextField("Search Items:", searchText);
            
            // Filter by Rarity
            showFilterRarity = EditorGUILayout.Toggle("Filter by Rarity", showFilterRarity);
            if (showFilterRarity)
            {
                filterRarity = (Rarity)EditorGUILayout.EnumPopup("Rarity Filter:", filterRarity);
            }
            
            // Filter by Type
            showFilterType = EditorGUILayout.Toggle("Filter by Type", showFilterType);
            if (showFilterType)
            {
                EditorGUILayout.LabelField("Item Types:", EditorStyles.miniLabel);
                EditorGUI.indentLevel++;
                
                filterBaseItem = EditorGUILayout.Toggle("Base Items", filterBaseItem);
                filterMaterials = EditorGUILayout.Toggle("Materials", filterMaterials);
                filterConsumable = EditorGUILayout.Toggle("Consumables", filterConsumable);
                filterEquipment = EditorGUILayout.Toggle("Equipment", filterEquipment);
                
                EditorGUI.indentLevel--;
                
                // Add "All" and "None" buttons for convenience
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("All", EditorStyles.miniButton))
                {
                    filterBaseItem = filterMaterials = filterConsumable = filterEquipment = true;
                }
                if (GUILayout.Button("None", EditorStyles.miniButton))
                {
                    filterBaseItem = filterMaterials = filterConsumable = filterEquipment = false;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            // Filter by Equipment Slot
            showFilterEquipmentSlot = EditorGUILayout.Toggle("Filter by Equipment Slot", showFilterEquipmentSlot);
            if (showFilterEquipmentSlot)
            {
                filterEquipmentSlot = (EquipmentSlot)EditorGUILayout.EnumPopup("Equipment Slot:", filterEquipmentSlot);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawDatabaseManagement()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Database Management", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Add Empty Slot"))
            {
                AddEmptySlot();
            }
            
            if (GUILayout.Button("Clear All"))
            {
                if (EditorUtility.DisplayDialog("Clear Database", 
                    "Are you sure you want to clear all items from the database?", 
                    "Yes", "Cancel"))
                {
                    ClearDatabase();
                }
            }
            
            if (GUILayout.Button("Validate Database"))
            {
                ValidateDatabase();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawItemList()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Items", EditorStyles.boldLabel);
            
            // Search and Filter
            DrawSearchAndFilter();
            
            var filteredItems = GetFilteredItems();
            
            if (filteredItems.Count == 0)
            {
                if (GetItemsList().Count == 0)
                {
                    EditorGUILayout.HelpBox("No items found. Add some items to get started.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("No items match your search/filter criteria.", MessageType.Info);
                }
                EditorGUILayout.EndVertical();
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            for (int i = 0; i < filteredItems.Count; i++)
            {
                var itemSlot = filteredItems[i];
                DrawItemListItem(itemSlot, i);
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawItemListItem(ItemDatabase.ItemDatabaseSlot itemSlot, int index)
        {
            EditorGUILayout.BeginHorizontal("box");
            
            // Radio button selection
            bool isSelected = selectedItemSlot == itemSlot;
            bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
            if (newSelected && !isSelected)
            {
                selectedItemSlot = itemSlot;
            }
            
            string itemName = itemSlot.item != null ? itemSlot.item.Name : "Empty Slot";
            string displayText = $"ID: {itemSlot.id} - {itemName}";
            
            if (itemSlot.item != null)
            {
                displayText += $" ({itemSlot.item.Rarity})";
            }
            
            EditorGUILayout.LabelField(displayText);
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawItemDetails()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Item Details", EditorStyles.boldLabel);
            
            if (selectedItemSlot == null)
            {
                EditorGUILayout.HelpBox("Select an item from the list to view and edit its properties.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            // Item ID (read-only)
            EditorGUILayout.LabelField($"ID: {selectedItemSlot.id}");
            EditorGUILayout.Space();
            
            if (selectedItemSlot.item == null)
            {
                EditorGUILayout.HelpBox("This slot is empty. No item properties to edit.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            // Item properties
            var item = selectedItemSlot.item;
            
            // Name
            string newName = EditorGUILayout.TextField("Name:", item.Name);
            if (newName != item.Name)
            {
                SetItemProperty(item, "name", newName);
            }
            
            // Description
            string newDescription = EditorGUILayout.TextField("Description:", item.Description);
            if (newDescription != item.Description)
            {
                SetItemProperty(item, "description", newDescription);
            }
            
            // Icon
            Sprite newIcon = (Sprite)EditorGUILayout.ObjectField("Icon:", item.Icon, typeof(Sprite), false);
            if (newIcon != item.Icon)
            {
                SetItemProperty(item, "icon", newIcon);
            }
            
            // Max Stack Size
            int newMaxStack = EditorGUILayout.IntField("Max Stack Size:", item.MaxStackSize);
            if (newMaxStack != item.MaxStackSize)
            {
                SetItemProperty(item, "maxStackSize", newMaxStack);
            }
            
            // Sell Price
            int newSellPrice = EditorGUILayout.IntField("Sell Price:", item.SellPrice);
            if (newSellPrice != item.SellPrice)
            {
                SetItemProperty(item, "sellPrice", newSellPrice);
            }
            
            // Buy Price
            int newBuyPrice = EditorGUILayout.IntField("Buy Price:", item.BuyPrice);
            if (newBuyPrice != item.BuyPrice)
            {
                SetItemProperty(item, "buyPrice", newBuyPrice);
            }
            
            // Rarity
            Rarity newRarity = (Rarity)EditorGUILayout.EnumPopup("Rarity:", item.Rarity);
            if (newRarity != item.Rarity)
            {
                SetItemProperty(item, "rarity", newRarity);
            }
            
            // Type-specific properties
            if (item is Consumable consumable)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Consumable Properties", EditorStyles.boldLabel);
                
                int newHealth = EditorGUILayout.IntField("Health:", consumable.Health);
                if (newHealth != consumable.Health)
                {
                    SetItemProperty(consumable, "health", newHealth);
                }
                
                int newMana = EditorGUILayout.IntField("Mana:", consumable.Mana);
                if (newMana != consumable.Mana)
                {
                    SetItemProperty(consumable, "mana", newMana);
                }
                
                int newStamina = EditorGUILayout.IntField("Stamina:", consumable.Stamina);
                if (newStamina != consumable.Stamina)
                {
                    SetItemProperty(consumable, "stamina", newStamina);
                }
            }
            else if (item is Equipment equipment)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Equipment Properties", EditorStyles.boldLabel);
                
                // Get current equipment slot using reflection
                var equipmentSlotField = typeof(Equipment).GetField("equipmentSlot", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var currentSlot = (EquipmentSlot)equipmentSlotField?.GetValue(equipment);
                
                EquipmentSlot newSlot = (EquipmentSlot)EditorGUILayout.EnumPopup("Equipment Slot:", currentSlot);
                if (newSlot != currentSlot)
                {
                    SetItemProperty(equipment, "equipmentSlot", newSlot);
                }
            }
            
            EditorGUILayout.Space();
            
            // Action buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Delete Item"))
            {
                DeleteSelectedItem();
            }
            
            if (GUILayout.Button("Clear Slot"))
            {
                ClearSelectedSlot();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void SetItemProperty(object item, string propertyName, object value)
        {
            var field = item.GetType().GetField(propertyName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(item, value);
            EditorUtility.SetDirty((UnityEngine.Object)item);
            EditorUtility.SetDirty(itemDatabase);
        }

        private void DeleteSelectedItem()
        {
            if (selectedItemSlot == null || selectedItemSlot.item == null) return;
            
            if (EditorUtility.DisplayDialog("Delete Item", 
                $"Are you sure you want to delete '{selectedItemSlot.item.Name}' (ID: {selectedItemSlot.id})?", 
                "Yes", "Cancel"))
            {
                // Delete the asset file
                string assetPath = AssetDatabase.GetAssetPath(selectedItemSlot.item);
                AssetDatabase.DeleteAsset(assetPath);
                
                // Remove the slot from the database
                itemDatabase.RemoveItemSlot(selectedItemSlot);
                selectedItemSlot = null;
            }
        }

        private void ClearSelectedSlot()
        {
            if (selectedItemSlot == null) return;
            
            if (EditorUtility.DisplayDialog("Clear Slot", 
                $"Are you sure you want to clear slot ID {selectedItemSlot.id}?", 
                "Yes", "Cancel"))
            {
                selectedItemSlot.item = null;
                EditorUtility.SetDirty(itemDatabase);
                selectedItemSlot = null;
            }
        }

        private void DrawItemSlot(ItemDatabase.ItemDatabaseSlot itemSlot, int index)
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            
            // Selection checkbox
            bool isSelected = selectedIndex == index;
            bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
            if (newSelected != isSelected)
            {
                selectedIndex = newSelected ? index : -1;
            }
            
            // Item info
            EditorGUILayout.BeginVertical();
            
            string itemName = itemSlot.item != null ? itemSlot.item.Name : "None";
            string displayText = $"ID: {itemSlot.id} - {itemName}";
            
            if (itemSlot.item != null)
            {
                displayText += $" ({itemSlot.item.Rarity})";
            }
            
            EditorGUILayout.LabelField(displayText, EditorStyles.boldLabel);
            
            if (itemSlot.item != null)
            {
                EditorGUILayout.LabelField($"Description: {itemSlot.item.Description}");
                EditorGUILayout.LabelField($"Max Stack: {itemSlot.item.MaxStackSize} | Sell: {itemSlot.item.SellPrice} | Buy: {itemSlot.item.BuyPrice}");
            }
            
            EditorGUILayout.EndVertical();
            
            // Action buttons
            EditorGUILayout.BeginVertical();
            
            if (itemSlot.item != null)
            {
                if (GUILayout.Button("Edit", GUILayout.Width(60)))
                {
                    EditItem(itemSlot);
                }
            }
            
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                DeleteItem(index);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawRawDatabaseView()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Raw Database View", EditorStyles.boldLabel);
            
            var items = GetItemsList();
            
            if (items.Count == 0)
            {
                EditorGUILayout.HelpBox("Database is empty. Add some items to see them here.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.LabelField("Item Name", EditorStyles.boldLabel, GUILayout.Width(200));
            EditorGUILayout.LabelField("Type", EditorStyles.boldLabel, GUILayout.Width(100));
            EditorGUILayout.LabelField("Rarity", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.LabelField("Max Stack", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.LabelField("Sell Price", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.LabelField("Buy Price", EditorStyles.boldLabel, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            for (int i = 0; i < items.Count; i++)
            {
                var itemSlot = items[i];
                
                EditorGUILayout.BeginHorizontal();
                
                // ID
                EditorGUILayout.LabelField(itemSlot.id.ToString(), GUILayout.Width(50));
                
                // Item Name
                string itemName = itemSlot.item != null ? itemSlot.item.Name : "None";
                EditorGUILayout.LabelField(itemName, GUILayout.Width(200));
                
                // Type
                string itemType = "Empty";
                if (itemSlot.item != null)
                {
                    if (itemSlot.item is Materials)
                        itemType = "Material";
                    else if (itemSlot.item is Consumable)
                        itemType = "Consumable";
                    else if (itemSlot.item is Equipment)
                        itemType = "Equipment";
                    else
                        itemType = "Item";
                }
                EditorGUILayout.LabelField(itemType, GUILayout.Width(100));
                
                // Rarity
                string rarity = "N/A";
                if (itemSlot.item != null)
                {
                    rarity = itemSlot.item.Rarity.ToString();
                }
                EditorGUILayout.LabelField(rarity, GUILayout.Width(80));
                
                // Max Stack
                string maxStack = "N/A";
                if (itemSlot.item != null)
                {
                    maxStack = itemSlot.item.MaxStackSize.ToString();
                }
                EditorGUILayout.LabelField(maxStack, GUILayout.Width(80));
                
                // Sell Price
                string sellPrice = "N/A";
                if (itemSlot.item != null)
                {
                    sellPrice = itemSlot.item.SellPrice.ToString();
                }
                EditorGUILayout.LabelField(sellPrice, GUILayout.Width(80));
                
                // Buy Price
                string buyPrice = "N/A";
                if (itemSlot.item != null)
                {
                    buyPrice = itemSlot.item.BuyPrice.ToString();
                }
                EditorGUILayout.LabelField(buyPrice, GUILayout.Width(80));
                
                EditorGUILayout.EndHorizontal();
                
                // Add a separator line
                if (i < items.Count - 1)
                {
                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    EditorGUILayout.Space(2);
                }
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawCreateItemSection()
        {
            EditorGUILayout.BeginVertical("box");
            
            showCreateItem = EditorGUILayout.Foldout(showCreateItem, "Create New Item");
            
            if (showCreateItem)
            {
                EditorGUI.indentLevel++;
                
                newItemName = EditorGUILayout.TextField("Name:", newItemName);
                newItemDescription = EditorGUILayout.TextField("Description:", newItemDescription);
                newItemMaxStackSize = EditorGUILayout.IntField("Max Stack Size:", newItemMaxStackSize);
                newItemSellPrice = EditorGUILayout.IntField("Sell Price:", newItemSellPrice);
                newItemBuyPrice = EditorGUILayout.IntField("Buy Price:", newItemBuyPrice);
                newItemRarity = (Rarity)EditorGUILayout.EnumPopup("Rarity:", newItemRarity);
                
                isNewItemConsumable = EditorGUILayout.Toggle("Is Consumable:", isNewItemConsumable);
                
                if (isNewItemConsumable)
                {
                    newItemHealth = EditorGUILayout.IntField("Health:", newItemHealth);
                    newItemMana = EditorGUILayout.IntField("Mana:", newItemMana);
                    newItemStamina = EditorGUILayout.IntField("Stamina:", newItemStamina);
                }
                
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Create Item"))
                {
                    CreateNewItem();
                }
                
                if (GUILayout.Button("Clear Form"))
                {
                    ClearCreateForm();
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }

        private List<ItemDatabase.ItemDatabaseSlot> GetFilteredItems()
        {
            if (itemDatabase == null) return new List<ItemDatabase.ItemDatabaseSlot>();
            
            return itemDatabase.GetFilteredItems(
                searchText, 
                showFilterRarity, filterRarity,
                showFilterType, filterBaseItem, filterMaterials, filterConsumable, filterEquipment,
                showFilterEquipmentSlot, filterEquipmentSlot
            );
        }

        private void AddEmptySlot()
        {
            if (itemDatabase != null)
            {
                itemDatabase.AddEmptySlot();
            }
        }



        private void EditItem(ItemDatabase.ItemDatabaseSlot itemSlot)
        {
            if (itemSlot.item == null)
            {
                // Show a popup to select an item
                EditorUtility.DisplayDialog("Assign Item", 
                    "Please select an item from the Project window and assign it to this slot.", "OK");
            }
            else
            {
                // Open item editor
                Selection.activeObject = itemSlot.item;
                EditorGUIUtility.PingObject(itemSlot.item);
            }
        }

        private void DeleteItem(int index)
        {
            var filteredItems = GetFilteredItems();
            
            if (index >= 0 && index < filteredItems.Count)
            {
                var itemToDelete = filteredItems[index];
                
                if (EditorUtility.DisplayDialog("Delete Item", 
                    $"Are you sure you want to delete '{itemToDelete.item?.Name ?? "Empty Slot"}' (ID: {itemToDelete.id})?", 
                    "Yes", "Cancel"))
                {
                    // Delete the asset file if it exists
                    if (itemToDelete.item != null)
                    {
                        string assetPath = AssetDatabase.GetAssetPath(itemToDelete.item);
                        AssetDatabase.DeleteAsset(assetPath);
                    }
                    
                    // Remove the slot from the database
                    itemDatabase.RemoveItemSlot(itemToDelete);
                    selectedItemSlot = null;
                }
            }
        }

        private void CreateNewItem()
        {
            if (string.IsNullOrEmpty(newItemName))
            {
                EditorUtility.DisplayDialog("Error", "Item name cannot be empty!", "OK");
                return;
            }
            
            // Create the item asset
            Item newItem;
            if (isNewItemMaterial)
            {
                newItem = CreateInstance<Materials>();
            }
            else if (isNewItemConsumable)
            {
                newItem = CreateInstance<Consumable>();
                var consumable = (Consumable)newItem;
                // Set consumable-specific properties using reflection
                var healthField = typeof(Consumable).GetField("health", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var manaField = typeof(Consumable).GetField("mana", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var staminaField = typeof(Consumable).GetField("stamina", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                healthField?.SetValue(consumable, newItemHealth);
                manaField?.SetValue(consumable, newItemMana);
                staminaField?.SetValue(consumable, newItemStamina);
            }
            else if (isNewItemEquipment)
            {
                newItem = CreateInstance<Equipment>();
                var equipment = (Equipment)newItem;
                // Set equipment-specific properties using reflection
                var equipmentSlotField = typeof(Equipment).GetField("equipmentSlot", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                equipmentSlotField?.SetValue(equipment, newItemEquipmentSlot);
            }
            else
            {
                newItem = CreateInstance<Item>();
            }
            
            // Set base properties using reflection
            var nameField = typeof(Item).GetField("name", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descField = typeof(Item).GetField("description", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxStackField = typeof(Item).GetField("maxStackSize", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sellPriceField = typeof(Item).GetField("sellPrice", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var buyPriceField = typeof(Item).GetField("buyPrice", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var rarityField = typeof(Item).GetField("rarity", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            nameField?.SetValue(newItem, newItemName);
            descField?.SetValue(newItem, newItemDescription);
            maxStackField?.SetValue(newItem, newItemMaxStackSize);
            sellPriceField?.SetValue(newItem, newItemSellPrice);
            buyPriceField?.SetValue(newItem, newItemBuyPrice);
            rarityField?.SetValue(newItem, newItemRarity);
            
            // Set icon
            var iconField = typeof(Item).GetField("icon", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            iconField?.SetValue(newItem, newItemIcon);
            
            // Add to database first to get the ID
            var newSlot = itemDatabase.AddItem(newItem);
            
            // Save the asset with ID-based naming
            string fileName = $"{newSlot.id} - {newItemName}";
            string path = $"Assets/PraganoidSystems/Inventory/Assets/Items/{fileName}.asset";
            AssetDatabase.CreateAsset(newItem, path);
            AssetDatabase.SaveAssets();
            
            ClearCreateForm();
            EditorUtility.DisplayDialog("Success", $"Item '{newItemName}' created and added to database with ID {newSlot.id}!", "OK");
        }

        private void ClearCreateForm()
        {
            newItemName = "";
            newItemDescription = "";
            newItemIcon = null;
            newItemMaxStackSize = 1;
            newItemSellPrice = 0;
            newItemBuyPrice = 0;
            newItemRarity = Rarity.Common;
            newItemHealth = 0;
            newItemMana = 0;
            newItemStamina = 0;
            isNewItemConsumable = false;
            isNewItemEquipment = false;
            isNewItemMaterial = true; // Reset to default (Materials)
            newItemEquipmentSlot = EquipmentSlot.Head;
        }

        private void ValidateDatabase()
        {
            if (itemDatabase == null) return;
            
            var issues = itemDatabase.ValidateDatabase();
            
            if (issues.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation Result", "Database is valid! No issues found.", "OK");
            }
            else
            {
                string message = "Database validation found the following issues:\n\n" + string.Join("\n", issues);
                EditorUtility.DisplayDialog("Validation Result", message, "OK");
            }
        }

        private void ClearDatabase()
        {
            if (itemDatabase != null)
            {
                itemDatabase.ClearDatabase();
            }
        }

        private List<ItemDatabase.ItemDatabaseSlot> GetItemsList()
        {
            if (itemDatabase == null) return new List<ItemDatabase.ItemDatabaseSlot>();
            
            return itemDatabase.GetItemsList();
        }
    }
} 