using System;
using UnityEngine;
using System.Collections.Generic;
using KSP.IO;
using KSP.UI.Screens;
using System.Linq;


namespace ToolbarControl_NS
{

    public class ToolbarControl : MonoBehaviour
    {
        private string nameSpace = "";
        private string toolbarId = "";
        private GameScenes[] gameScenes;

        private string BlizzyToolbarIconActive = "";
        private string BlizzyToolbarIconInactive = "";
        private string StockToolbarIconActive = "";
        private string StockToolbarIconInactive = "";

        private ApplicationLauncher.AppScenes visibleInScenes;
        private string ToolTip = "";



        public void UseBlizzy(bool useBlizzy)
        {

            if (ToolbarManager.ToolbarAvailable && useBlizzy)
            {
                if (activeToolbarType == ToolBarSelected.stock)
                    SetBlizzySettings();
            }
            else
            {
                if (activeToolbarType == ToolBarSelected.blizzy)
                    SetStockSettings();
            }
        }
        /// <summary>
        /// Only pass in the onTrue and onFalse
        /// </summary>
        /// <param name="onTrue"></param>
        /// <param name="onFalse"></param>
        /// <param name="visibleInScenes"></param>
        /// <param name="nameSpace"></param>
        /// <param name="toolbarId"></param>
        /// <param name="largeToolbarIcon"></param>
        /// <param name="smallToolbarIcon"></param>
        public void AddToAllToolbars(TC_ClickHandler onTrue, TC_ClickHandler onFalse,
            ApplicationLauncher.AppScenes visibleInScenes, string nameSpace, string toolbarId, string largeToolbarIcon, string smallToolbarIcon, string toolTip = "")
        {
            AddToAllToolbars(onTrue, onFalse, null, null, null, null,
                visibleInScenes, nameSpace, toolbarId, largeToolbarIcon, largeToolbarIcon, smallToolbarIcon, smallToolbarIcon, toolTip);
        }
        public void AddToAllToolbars(TC_ClickHandler onTrue, TC_ClickHandler onFalse,
            ApplicationLauncher.AppScenes visibleInScenes, string nameSpace, string toolbarId, string largeToolbarIconActive,
            string largeToolbarIconInactive,
            string smallToolbarIconActive,
            string smallToolbarIconInactive, string toolTip = "")
        {
            AddToAllToolbars(onTrue, onFalse, null, null, null, null,
                visibleInScenes, nameSpace, toolbarId, largeToolbarIconActive, largeToolbarIconInactive, smallToolbarIconActive, smallToolbarIconInactive, toolTip);
        }
        /// <summary>
        /// Pass in all the callbacks
        /// </summary>
        /// <param name="onTrue"></param>
        /// <param name="onFalse"></param>
        /// <param name="onHover"></param>
        /// <param name="onHoverOut"></param>
        /// <param name="onEnable"></param>
        /// <param name="onDisable"></param>
        /// <param name="visibleInScenes"></param>
        /// <param name="nameSpace"></param>
        /// <param name="toolbarId"></param>
        /// <param name="largeToolbarIcon"></param>
        /// <param name="smallToolbarIcon"></param>
        public void AddToAllToolbars(TC_ClickHandler onTrue, TC_ClickHandler onFalse, TC_ClickHandler onHover, TC_ClickHandler onHoverOut, TC_ClickHandler onEnable, TC_ClickHandler onDisable,
            ApplicationLauncher.AppScenes visibleInScenes, string nameSpace, string toolbarId, string largeToolbarIcon, string smallToolbarIcon, string toolTip = ""
            )
        {
            AddToAllToolbars(onTrue, onFalse, onHover, onHoverOut, onEnable, onDisable,
                visibleInScenes, nameSpace, toolbarId, largeToolbarIcon, largeToolbarIcon, smallToolbarIcon, smallToolbarIcon, toolTip);
        }

        public void AddToAllToolbars(TC_ClickHandler onTrue, TC_ClickHandler onFalse, TC_ClickHandler onHover, TC_ClickHandler onHoverOut, TC_ClickHandler onEnable, TC_ClickHandler onDisable,
            ApplicationLauncher.AppScenes visibleInScenes, string nameSpace, string toolbarId, string largeToolbarIconActive, string largeToolbarIconInactive, string smallToolbarIconActive, string smallToolbarIconInactive, string toolTip = "")
        {
            this.onTrue = onTrue;
            this.onFalse = onFalse;
            this.onHover = onHover;
            this.onHoverOut = onHoverOut;
            this.onEnable = onEnable;
            this.onDisable = onDisable;

            this.visibleInScenes = visibleInScenes;
            this.nameSpace = nameSpace;
            this.toolbarId = toolbarId;
            this.BlizzyToolbarIconActive = smallToolbarIconActive;
            this.BlizzyToolbarIconInactive = smallToolbarIconInactive;
            this.StockToolbarIconActive = largeToolbarIconActive;
            this.StockToolbarIconInactive = largeToolbarIconInactive;
            this.ToolTip = toolTip;
            SetupGameScenes(visibleInScenes);
            StartAfterInit();
        }

        void SetupGameScenes(ApplicationLauncher.AppScenes visibleInScenes)
        {
            List<GameScenes> g = new List<GameScenes>();

            if ((visibleInScenes & ApplicationLauncher.AppScenes.MAINMENU) == ApplicationLauncher.AppScenes.MAINMENU)
                g.Add(GameScenes.MAINMENU);
            if ((visibleInScenes & ApplicationLauncher.AppScenes.TRACKSTATION) == ApplicationLauncher.AppScenes.TRACKSTATION)
                g.Add(GameScenes.TRACKSTATION);
            if (
                ((visibleInScenes & ApplicationLauncher.AppScenes.SPH) == ApplicationLauncher.AppScenes.SPH) ||
                ((visibleInScenes & ApplicationLauncher.AppScenes.VAB) == ApplicationLauncher.AppScenes.VAB))
                g.Add(GameScenes.EDITOR);
            if (((visibleInScenes & ApplicationLauncher.AppScenes.MAPVIEW) == ApplicationLauncher.AppScenes.MAPVIEW) ||
               ((visibleInScenes & ApplicationLauncher.AppScenes.FLIGHT) == ApplicationLauncher.AppScenes.FLIGHT))
                g.Add(GameScenes.FLIGHT);
            if ((visibleInScenes & ApplicationLauncher.AppScenes.SPACECENTER) == ApplicationLauncher.AppScenes.SPACECENTER)
                g.Add(GameScenes.SPACECENTER);
            gameScenes = g.ToArray();
        }

        public delegate void TC_ClickHandler();

        event TC_ClickHandler onTrue = null;
        event TC_ClickHandler onFalse = null;
        event TC_ClickHandler onHover = null;
        event TC_ClickHandler onHoverOut = null;
        event TC_ClickHandler onEnable = null;
        event TC_ClickHandler onDisable = null;


        private ApplicationLauncherButton stockButton;
        private IButton blizzyButton;

        private string toolbarIconActive = "";
        private string toolbarIconInactive = "";

        public static bool buttonActive = false;

        enum ToolBarSelected { stock, blizzy, none };
        ToolBarSelected activeToolbarType = ToolBarSelected.none;

        public void SetFalse()
        {
            buttonActive = false;
            if (stockButton != null)
                stockButton.SetFalse();
            UpdateToolbarIcon();
        }
        private void RemoveStockButton()
        {
            if (this.stockButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(this.stockButton);
                this.stockButton = null;
            }
        }
        private void RemoveBlizzyButton()
        {
            if (this.blizzyButton != null)
            {
                this.blizzyButton.Destroy();
                this.blizzyButton = null;
            }
        }


        private void SetBlizzySettings()
        {
            if (activeToolbarType == ToolBarSelected.stock)
            {
                RemoveStockButton();
            }
            this.toolbarIconActive = BlizzyToolbarIconActive;
            this.toolbarIconInactive = BlizzyToolbarIconInactive;

            this.blizzyButton = ToolbarManager.Instance.add(nameSpace, toolbarId);
            this.blizzyButton.ToolTip = ToolTip;
            this.blizzyButton.OnClick += this.button_Click;
            this.blizzyButton.Visibility = new GameScenesVisibility(gameScenes);


            activeToolbarType = ToolBarSelected.blizzy;
            this.UpdateToolbarIcon();
        }

        private void SetStockSettings()
        {
            if (activeToolbarType == ToolBarSelected.blizzy)
            {
                RemoveBlizzyButton();
            }
            // Blizzy toolbar not available, or Stock Toolbar selected Let's go stock :(
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
            OnGUIAppLauncherReady();

            this.toolbarIconActive = StockToolbarIconActive;
            this.toolbarIconInactive = StockToolbarIconInactive;

            activeToolbarType = ToolBarSelected.stock;
            this.UpdateToolbarIcon();
        }


        private void StartAfterInit()
        {
            if (ToolbarManager.ToolbarAvailable)
            {
                if (activeToolbarType == ToolBarSelected.stock)
                    SetBlizzySettings();
            }
            else
            {
                if (activeToolbarType == ToolBarSelected.blizzy)
                    SetStockSettings();
            }
            // this is needed because of a bug in KSP with event onGUIAppLauncherReady.
            if (activeToolbarType == ToolBarSelected.stock)
            {
                OnGUIAppLauncherReady();
            }
        }

        public void OnDestroy()
        {
            Debug.Log("ToolbarControl OnDestroy");
            if (activeToolbarType == ToolBarSelected.stock)
            {
                GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIAppLauncherDestroyed);

                RemoveStockButton();
            }
            else
            {
                RemoveBlizzyButton();
            }
        }

        private void UpdateToolbarIcon()
        {
            if (activeToolbarType == ToolBarSelected.blizzy)
            {
                this.blizzyButton.TexturePath = buttonActive ? this.toolbarIconActive : this.toolbarIconInactive;
            }
            else
            {
                if (stockButton == null)
                    Debug.Log("stockButton is null");
                else
                    this.stockButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(buttonActive ? this.toolbarIconActive : this.toolbarIconInactive, false));
            }
        }
        private void OnGUIAppLauncherReady()
        {
            // Setup PW Stock Toolbar button
            bool hidden = false;
            if (ApplicationLauncher.Ready && (stockButton == null || !ApplicationLauncher.Instance.Contains(stockButton, out hidden)))
            {
                stockButton = ApplicationLauncher.Instance.AddModApplication(
                    //ToggleButtonActive,
                    //ToggleButtonActive,
                    //null,
                    //null,
                    //null,
                    //null,
                    doOnTrue,
                    doOnFalse,
                    doOnHover,
                    doOnHoverOut,
                    doOonEnable,
                    doOnDisable,
                    //ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB,
                    visibleInScenes,
                    (Texture)GameDatabase.Instance.GetTexture(StockToolbarIconActive, false));
                SetStockSettings();
            }
        }
        private void doOnTrue() { onTrue(); }
        private void doOnFalse() { onFalse(); }
        private void doOnHover() { onHover(); }
        private void doOnHoverOut() { onHoverOut(); }
        private void doOonEnable() { onEnable(); }
        private void doOnDisable() { onDisable(); }

        private void button_Click(ClickEvent e)
        {
            this.ToggleButtonActive();
        }

        private void ToggleButtonActive()
        {
            buttonActive = !buttonActive;
            UpdateToolbarIcon();
            if (buttonActive)
            {
                onTrue();
            }
            else
            {
                onFalse();
            }
        }
        private void OnGUIAppLauncherDestroyed()
        {
            if (stockButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(stockButton);
                stockButton = null;
            }
        }

    }
}
