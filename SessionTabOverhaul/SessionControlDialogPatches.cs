using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionTabOverhaul
{
    [HarmonyPatch(typeof(SessionControlDialog))]
    internal static class SessionControlDialogPatches
    {
        private static readonly float2 rectOffset = new float2(4, 4);

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SessionControlDialog.GenerateUi))]
        private static void GenerateUiPostfix(SessionControlDialog __instance, SessionControlDialog.Tab tab)
        {
            if (tab != SessionControlDialog.Tab.Settings)
                return;

            foreach (Radio accessLevelRadio in __instance._accessLevelRadios)
            {
                Slot accessLevelRow = accessLevelRadio.Slot.Parent.Parent.Parent;

                LayoutElement layoutElement = accessLevelRow.GetComponent<LayoutElement>();
                layoutElement.PreferredHeight.Value += 8;
                layoutElement.MinHeight.Value += 8;

                foreach (Slot? child in accessLevelRow.Children)
                {
                    RectTransform rectTransform = child.GetComponent<RectTransform>();
                    rectTransform.OffsetMin.Value += rectOffset;
                    rectTransform.OffsetMax.Value -= rectOffset;
                }

                Image image = accessLevelRow.AttachComponent<Image>();
                image.Tint.Value = (accessLevelRow.ChildIndex & 1) == 0 ? SessionTabOverhaul.FirstRowColor : SessionTabOverhaul.SecondRowColor;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SessionControlDialog.OnAttach))]
        private static void OnAttachPostfix(SessionControlDialog __instance)
        {
            RectTransform rectTransform = __instance.Slot.GetComponent<RectTransform>();
            rectTransform.OffsetMin.Value = new float2(16, 16);
            rectTransform.OffsetMax.Value = new float2(-16, -16);
        }
    }
}