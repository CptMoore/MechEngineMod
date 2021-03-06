﻿using System;
using BattleTech;
using BattleTech.UI;
using CustomComponents;
using Harmony;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MechEngineer.Features.MechLabSlots
{
    public class CustomWidgetsFixMechLab
    {
        private static MechLabLocationWidget TopLeftWidget;
        private static MechLabLocationWidget TopRightWidget;

        internal static void Setup(MechLabPanel mechLabPanel)
        {
            SetupWidget(
                "TopLeftWidget",
                ref TopLeftWidget,
                mechLabPanel,
                mechLabPanel.rightArmWidget,
                MechLabSlotsFeature.settings.TopLeftWidget
            );

            SetupWidget(
                "TopRightWidget",
                ref TopRightWidget,
                mechLabPanel,
                mechLabPanel.leftArmWidget,
                MechLabSlotsFeature.settings.TopRightWidget
            );
        }

        internal static void SetupWidget(
            string id,
            ref MechLabLocationWidget topWidget,
            MechLabPanel mechLabPanel,
            MechLabLocationWidget armWidget,
            MechLabSlotsSettings.WidgetSettings settings
            )
        {
            if (topWidget != null)
            {
                topWidget.gameObject.transform.SetParent(armWidget.transform, false);
                topWidget.Init(mechLabPanel);
                return;
            }

            var template = mechLabPanel.centerTorsoWidget;
            var container = armWidget.transform.parent.gameObject;
            var clg = container.GetComponent<VerticalLayoutGroup>();
            clg.padding = new RectOffset(0, 0, MechLabSlotsFeature.settings.MechLabArmTopPadding, 0);
            var go = Object.Instantiate(template.gameObject, null);

            {
                go.transform.SetParent(armWidget.transform, false);
                go.GetComponent<LayoutElement>().ignoreLayout = true;
                go.transform.GetChild("layout_armor").gameObject.SetActive(false);
                go.transform.GetChild("layout_hardpoints").gameObject.SetActive(false);
                go.transform.GetChild("layout_locationText").GetChild("txt_structure").gameObject.SetActive(false);
                var rect = go.GetComponent<RectTransform>();
                rect.pivot = new Vector2(0, 0);
                rect.localPosition = new Vector3(0, 20);
                var vlg = go.GetComponent<VerticalLayoutGroup>();
                vlg.padding = new RectOffset(0, 0, 0, 3);
                vlg.spacing = 4;
            }

            go.name = id;
            go.transform.GetChild("layout_locationText").GetChild("txt_location").GetComponent<TextMeshProUGUI>().text =
                settings.Label;
            go.SetActive(settings.Enabled);
            topWidget = go.GetComponent<MechLabLocationWidget>();
            topWidget.Init(mechLabPanel);
            var layout = new WidgetLayout(topWidget);

            MechLabSlotsFixer.ModifyLayoutSlotCount(layout, settings.Slots);

            {
                var mechRectTransform = armWidget.transform.parent.parent.GetComponent<RectTransform>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(mechRectTransform);
            }
        }

        internal static void OnAdditem_SetParent(Transform @this, Transform parent, bool worldPositionStays)
        {
            try
            {
                var element = @this.GetComponent<MechLabItemSlotElement>();
                var widget = MechWidgetLocation(element.ComponentRef.Def);
                if (widget != null)
                {
                    var inventoryParent = widget.inventoryParent;
                    @this.SetParent(inventoryParent, worldPositionStays);
                    return;
                }
            }
            catch (Exception e)
            {
                Control.Logger.Error.Log(e);
            }

            @this.SetParent(parent, worldPositionStays);
        }

        internal static MechLabLocationWidget MechWidgetLocation(MechComponentDef def)
        {
            if (def != null && def.Is<CustomWidget>(out var config))
            {
                if (config.Location == CustomWidget.MechLabWidgetLocation.TopLeft
                    && MechLabSlotsFeature.settings.TopLeftWidget.Enabled)
                {
                    return TopLeftWidget;
                }

                if (config.Location == CustomWidget.MechLabWidgetLocation.TopRight
                    && MechLabSlotsFeature.settings.TopRightWidget.Enabled)
                {
                    return TopRightWidget;
                }
            }

            return null;
        }

        internal static bool OnDrop(MechLabLocationWidget widget, PointerEventData eventData)
        {
            if (widget == TopLeftWidget || widget == TopRightWidget)
            {
                var mechLab = (MechLabPanel)widget.parentDropTarget;
                mechLab.centerTorsoWidget.OnDrop(eventData);
                return true;
            }

            return false;
        }

        internal static void RefreshDropHighlights(MechLabLocationWidget widget, IMechLabDraggableItem item)
        {
            if (item == null)
            {
                TopLeftWidget.ShowHighlightFrame(false);
                TopRightWidget.ShowHighlightFrame(false);
            }
        }

        internal static bool ShowHighlightFrame(
            MechLabLocationWidget widget,
            bool isOriginalLocation,
            ref MechComponentRef cRef
            )
        {
            if (cRef == null)
            {
                return true;
            }

            // we only want to highlight once, CT is only called once
            if (widget.loadout.Location != ChassisLocations.CenterTorso)
            {
                return true;
            }

            if (cRef.Flags<CCFlags>().NoRemove || cRef.IsFixed)
            {
                return true;
            }

            // get the correct widget to highlight
            var nwidget = MechWidgetLocation(cRef.Def);
            if (nwidget == null)
            {
                return true;
            }

            nwidget.ShowHighlightFrame(true, isOriginalLocation ? UIColor.Blue : UIColor.Gold);
            cRef = null;
            return false;
        }
    }
}