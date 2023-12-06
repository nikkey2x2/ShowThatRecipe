using BepInEx;
using HarmonyLib;
using System;
using System.Linq;
using Eremite;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ShowThatRecipe
{
    [BepInPlugin("Eremite.mod.ShowThatRecipe", "Show That Recipe", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private static string logSourceName = "STR";
        private static BepInEx.Logging.ManualLogSource log;
        private void Awake()
        {
            log = BepInEx.Logging.Logger.CreateLogSource(logSourceName);
            log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Harmony.CreateAndPatchAll(typeof(Plugin));
        }
        public class ShowRecipeButton : MonoBehaviour, IPointerClickHandler
        {
            public Eremite.Model.GoodModel goodModel;
            public PointerEventData.InputButton mouseEvent;

            public void OnPointerClick(PointerEventData eventData)
            {
                if (eventData.button == mouseEvent)
                {
                    Eremite.View.Popups.Recipes.RecipesPopupRequest request = new(goodModel, playShowAnim: true);
                    Eremite.Controller.GameController.Instance.GameServices.GameBlackboardService.RecipesPopupRequested.OnNext(request);
                }
            }
        }

        static Eremite.Model.GoodModel GetModelFromGood(Eremite.Model.Good good)
        {
            return Eremite.Controller.MainController.Instance.Settings.GetGood(good);
        }

        static Eremite.Model.GoodModel GetSlotGoodModel(Eremite.GameMB goodSlot)
        {
            if (goodSlot is null)
            {
                return null;
            }
            MethodInfo GetProduct = goodSlot.GetType().GetMethod("GetProduct", BindingFlags.NonPublic | BindingFlags.Instance);
            if (GetProduct != null)
            {
                Eremite.Model.Good good = (Eremite.Model.Good)GetProduct.Invoke(goodSlot, null);
                if (good == null || good.IsNone())
                {
                    return null;
                }
                return GetModelFromGood(good);
            } else
            {
                FieldInfo goodField = goodSlot.GetType().GetField("good", BindingFlags.NonPublic | BindingFlags.Instance);
                if (goodField == null)
                {
                    return null;
                }
                Eremite.Model.Good good = (Eremite.Model.Good)goodField.GetValue(goodSlot);
                if (good == null || good.IsNone())
                {
                    return null;
                }
                return GetModelFromGood(good);
            }
        }

        static void SetUpGoodSlot(UnityEngine.Component goodSlot, PointerEventData.InputButton mouseEvent, Eremite.Model.GoodModel goodModel)
        {
            if (goodModel is null)
            {
                return;
            }
            ShowRecipeButton button = goodSlot.gameObject.GetComponent<ShowRecipeButton>();
            if (button == null)
            {
                button = goodSlot.gameObject.AddComponent(typeof(ShowRecipeButton)) as ShowRecipeButton;
                if (button == null)
                {
                    // This means we tried to add button and failed, because gameObject already has a Selectable
                    return;
                }
                else
                {
                    button.goodModel = goodModel;
                    button.mouseEvent = mouseEvent;
                }
            } else
            {
                // Check if we link correct good
                if (button.goodModel != goodModel)
                {
                    button.goodModel = goodModel;
                }
                if (button.mouseEvent != mouseEvent)
                {
                    button.mouseEvent = mouseEvent;
                }
            }
        }

        [HarmonyPatch(typeof(Eremite.Buildings.UI.GathererHutRecipeSlot), "SetUp")]
        [HarmonyPostfix]
        static void PatchGathererHutRecipeSlot(Eremite.Buildings.UI.GathererHutRecipeSlot __instance, Eremite.Buildings.GathererHutRecipeModel ___model)
        {
            // Gathering buildings' produce
            SetUpGoodSlot(__instance, PointerEventData.InputButton.Left, ___model.refGood.good);
        }

        [HarmonyPatch(typeof(Eremite.Buildings.UI.FarmRecipeSlot), "SetUp")]
        [HarmonyPostfix]
        static void PatchFarmRecipeSlot(Eremite.Buildings.UI.FarmRecipeSlot __instance, Eremite.Buildings.FarmRecipeModel ___model)
        {
            // Farms
            SetUpGoodSlot(__instance, PointerEventData.InputButton.Left, ___model.producedGood.good);
        }

        [HarmonyPatch(typeof(Eremite.Buildings.UI.HearthFuelSlot), "SetUp")]
        [HarmonyPostfix]
        static void PatchHearthFuelSlot(Eremite.Buildings.UI.HearthFuelSlot __instance, Eremite.View.HUD.GoodSlot ___goodSlot, Eremite.Model.GoodModel model)
        {
            // Hearth fuels slots
            SetUpGoodSlot(___goodSlot, PointerEventData.InputButton.Left, model);
        }

        [HarmonyPatch(typeof(Eremite.Buildings.UI.MineRecipeSlot), "SetUp")]
        [HarmonyPostfix]
        static void PatchMineRecipeSlot(Eremite.Buildings.UI.MineRecipeSlot __instance, Eremite.Buildings.MineRecipeModel ___model)
        {
            // Mine
            SetUpGoodSlot(__instance, PointerEventData.InputButton.Left, ___model.producedGood.good);
        }

        //[HarmonyPatch(typeof(Eremite.View.HUD.GoodsSetSlot), "SetUpIcon", new Type[] { typeof(Eremite.Model.Good) })]
        //[HarmonyPostfix]
        //static void PatchActiveGoodsSetSlot(Eremite.View.HUD.GoodsSetSlot __instance, Eremite.Model.Good good, Eremite.Model.GoodsSet ___set)
        //{
        //    // Middle of recipe picker (right click)
        //    if (___set.goods.Length > 1)
        //    {
        //        SetUpGoodSlot(__instance, PointerEventData.InputButton.Right, GetModelFromGood(good));
        //    }
        //}

        [HarmonyPatch(typeof(Eremite.Buildings.UI.ExtractorPanel), "Show")]
        [HarmonyPostfix]
        static void PatchExtractorPanel(Eremite.Buildings.Extractor ___current, UnityEngine.UI.Image ___waterIcon, Eremite.Buildings.UI.ExtractorPanel __instance)
        {
            // Geyser pump
            if (___current is null)
            {
                return;
            }
            Eremite.Model.WaterModel water = ___current.GetWaterType();
            SetUpGoodSlot(___waterIcon, PointerEventData.InputButton.Left, water.good);
        }

        [HarmonyPatch(typeof(Eremite.Buildings.UI.RainCatcherPanel), "UpdateWater")]
        [HarmonyPostfix]
        static void PatchRainCollectorWater(Eremite.Buildings.RainCatcher ___current, UnityEngine.UI.Image ___waterIcon, Eremite.Buildings.UI.RainCatcherPanel __instance)
        {
            // Rain collector (water)
            if (___current is null)
            {
                return;
            }
            Eremite.Model.WaterModel water = ___current.GetCurrentWaterType();
            SetUpGoodSlot(___waterIcon, PointerEventData.InputButton.Left, water.good);
        }

        [HarmonyPatch(typeof(Eremite.Buildings.UI.StorageGoodSlot), "SetUp")]
        [HarmonyPostfix]
        static void PatchStorage(Eremite.Buildings.UI.StorageGoodSlot __instance, Eremite.Model.Good good)
        {
            // Storage (only list)
            SetUpGoodSlot(__instance, PointerEventData.InputButton.Left, GetModelFromGood(good));
        }

        [HarmonyPatch(typeof(Eremite.Buildings.UI.BuildingConstructionPanel), "SetUpSlots")]
        [HarmonyPostfix]
        static void PatchConstructionSlots(List<Eremite.View.HUD.RangeGoodSlot> ___ingredientsSlots, Eremite.Buildings.UI.BuildingConstructionPanel __instance)
        {
            // Building construction materials
            foreach (Eremite.View.HUD.RangeGoodSlot slot in ___ingredientsSlots)
            {
                Eremite.Model.GoodModel goodModel = GetSlotGoodModel(slot);
                if (goodModel == null)
                    continue;
                SetUpGoodSlot(slot, PointerEventData.InputButton.Left, goodModel);
            }
        }

        [HarmonyPatch(typeof(Eremite.MapObjects.UI.DepositPanel), "Show")]
        [HarmonyPostfix]
        static void PatchDepositSlots(List<Eremite.View.HUD.GoodSlotChance> ___extraProducts, Eremite.View.HUD.GoodSlot ___mainProduct)
        {
            // Resource deposits
            foreach (Eremite.View.HUD.GoodSlotChance extraSlot in ___extraProducts)
            {
                FieldInfo slotField = extraSlot.GetType().GetField("slot", BindingFlags.NonPublic | BindingFlags.Instance);
                Eremite.View.HUD.GoodSlot actualSlot = (Eremite.View.HUD.GoodSlot)slotField.GetValue(extraSlot);
                if (actualSlot == null)
                {
                    continue;
                }
                Eremite.Model.GoodModel goodModel = GetSlotGoodModel(actualSlot);
                if (goodModel == null)
                    continue;
                SetUpGoodSlot(extraSlot, PointerEventData.InputButton.Left, goodModel);
            }
            SetUpGoodSlot(___mainProduct, PointerEventData.InputButton.Left, GetSlotGoodModel(___mainProduct));
        }

        [HarmonyPatch(typeof(Eremite.View.Popups.Recipes.RecipesPopup), "Show")]
        [HarmonyPostfix]
        static void RecipePopupTypeHelper(ref Eremite.View.Popups.Recipes.RecipesPopupRequest request, Eremite.View.Popups.Recipes.RecipesPopup __instance)
        {
            // Try to more accurately predict the category
            //{'Needs', 'Tools', 'Metal', 'Water', 'Mat Raw', 'Mat Processed', 'Packs', 'Vessel', 'Food Processed', 'Food Raw', 'Valuable', 'Crafting'}
            string goodName = request.good.Name;
            if (new string[] { "Food Raw", "Mat Raw", "Water" }.Any(x => goodName.Contains(x)))
            {
                UnityEngine.GameObject typeToggle = __instance.FindChild("Content/Recipes/SearchBar/TypeToggle");
                typeToggle.SendMessage("OnClick");
            }
        }
    }
}
