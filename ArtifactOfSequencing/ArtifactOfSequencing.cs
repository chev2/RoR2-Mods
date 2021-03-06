﻿using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace Chev
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Chev.ArtifactOfSequencing", "Artifact of Sequencing", "1.0.2")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ArtifactAPI))]
    public class ArtifactOfSequencingMod : BaseUnityPlugin
    {
        public static ArtifactDef Artifact;

        public static ItemDef CommonItem;
        public static ItemDef UncommonItem;
        public static ItemDef LegendaryItem;
        public static ItemDef BossItem;
        public static ItemDef LunarItem;

        private static readonly System.Random _random = new System.Random();

        public static ConfigEntry<int> CommonItemCount { get; set; }
        public static ConfigEntry<int> UncommonItemCount { get; set; }
        public static ConfigEntry<int> LegendaryItemCount { get; set; }
        public static ConfigEntry<int> BossItemCount { get; set; }
        public static ConfigEntry<int> LunarItemCount { get; set; }

        public void Awake()
        {
            // Initialize config
            CommonItemCount = Config.Bind("Quantities", "CommonItemCount", 1, "Quantity of the common (white) item to start with.");
            UncommonItemCount = Config.Bind("Quantities", "UncommonItemCount", 1, "Quantity of the uncommon (green) item to start with.");
            LegendaryItemCount = Config.Bind("Quantities", "LegendaryItemCount", 1, "Quantity of the legendary (red) item to start with.");
            BossItemCount = Config.Bind("Quantities", "BossItemCount", 1, "Quantity of the boss (yellow) item to start with.");
            LunarItemCount = Config.Bind("Quantities", "LunarItemCount", 1, "Quantity of the lunar (blue) item to start with.");

            // Initialize the artifact
            Artifact = ScriptableObject.CreateInstance<ArtifactDef>();

            // Artifact info
            Artifact.nameToken = "Artifact of Sequencing";
            Artifact.descriptionToken = "Spawn with a starting item of every tier. Any picked up items will be converted to the starting item of the same tier.";
            Artifact.smallIconSelectedSprite = LoadIcon(ArtifactOfSequencing.Properties.Resources.texArtifactSequencingEnabled);
            Artifact.smallIconDeselectedSprite = LoadIcon(ArtifactOfSequencing.Properties.Resources.texArtifactSequencingDisabled);

            // Add our custom artifact to the artifact list
            // This uses the ArtifactAPI submodule in R2API
            ArtifactAPI.Add(Artifact);

            // On run start event
            // This is when we add our items
            Run.onRunStartGlobal += AddBeginningItems;
        }

        /// <summary>
        /// Loads a Unity Sprite from resources.
        /// </summary>
        /// <param name="resourceBytes">The byte array of the resource.</param>
        /// <returns></returns>
        private Sprite LoadIcon(Byte[] resourceBytes)
        {
            if (resourceBytes == null)
                throw new ArgumentNullException(nameof(resourceBytes));

            Texture2D iconTex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            iconTex.LoadImage(resourceBytes);

            return Sprite.Create(iconTex, new Rect(0f, 0f, iconTex.width, iconTex.height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Gets a random item from a chosen tier.
        /// </summary>
        /// <param name="tier">The item tier, e.g. common, uncommon, legendary, lunar, boss</param>
        /// <returns></returns>
        private ItemDef RandomItem(ItemTier tier)
        {
            // Get all available items by tier
            ItemDef[] itemsOfTier = (from item in ItemCatalog.allItems
                                     where Run.instance.IsItemAvailable(item) && ItemCatalog.GetItemDef(item).tier == tier
                                     select ItemCatalog.GetItemDef(item)).ToArray();

            return itemsOfTier[_random.Next(0, itemsOfTier.Length)];
        }

        private ItemDef ItemFromTier(ItemTier tier)
        {
            switch (tier)
            {
                case ItemTier.Tier1:
                    return CommonItem;
                case ItemTier.Tier2:
                    return UncommonItem;
                case ItemTier.Tier3:
                    return LegendaryItem;
                case ItemTier.Boss:
                    return BossItem;
                case ItemTier.Lunar:
                    return LunarItem;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts any potential items to their corresponding starter item
        /// </summary>
        private void ConvertPotentialItems(Inventory inv, ItemIndex itemIndex, int count)
        {
            ItemDef starterItemToCompare = ItemFromTier(ItemCatalog.GetItemDef(itemIndex).tier);

            // If they are different items
            if (starterItemToCompare != null && starterItemToCompare.itemIndex != itemIndex)
            {
                int itemCount = inv.GetItemCount(itemIndex);

                inv.RemoveItem(itemIndex, itemCount);
                inv.GiveItem(starterItemToCompare, itemCount);
            }
        }

        public void AddBeginningItems(Run run)
        {
            // If our artifact is enabled
            if (RunArtifactManager.instance.IsArtifactEnabled(Artifact.artifactIndex))
            {
                // Called every time an item is given
                // If the item is not a starter item, remove it and give the starter item
                Inventory.onServerItemGiven += ConvertPotentialItems;
            }
            // Else, if our artifact isn't enabled
            else
            {
                // Unsubscribe from the event if the artifact is disabled
                Inventory.onServerItemGiven -= ConvertPotentialItems;
                return;
            }

            // Set starting items
            CommonItem = RandomItem(ItemTier.Tier1);
            UncommonItem = RandomItem(ItemTier.Tier2);
            LegendaryItem = RandomItem(ItemTier.Tier3);
            BossItem = RandomItem(ItemTier.Boss);
            LunarItem = RandomItem(ItemTier.Lunar);

            // Give the starting items to every palyer
            for (int i = 0; i < CharacterMaster.readOnlyInstancesList.Count; i++)
            {
                CharacterMaster character = CharacterMaster.readOnlyInstancesList[i];

                character.inventory.GiveItem(CommonItem, CommonItemCount.Value);
                character.inventory.GiveItem(UncommonItem, UncommonItemCount.Value);
                character.inventory.GiveItem(LegendaryItem, LegendaryItemCount.Value);
                character.inventory.GiveItem(BossItem, BossItemCount.Value);
                character.inventory.GiveItem(LunarItem, LunarItemCount.Value);
            }
        }
    }
}