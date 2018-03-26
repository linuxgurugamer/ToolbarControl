using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using KSP.IO;
using KSP.UI.Screens;
using System.Linq;
using DDSHeaders;

namespace ToolbarControl_NS
{
    /// <summary>
	/// Determines visibility of a button in relation to the currently running game scene.
	/// </summary>
	/// <example>
	/// <code>
	/// IButton button = ...
	/// button.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.FLIGHT);
	/// </code>
	/// </example>
	/// <seealso cref="IButton.Visibility"/>
	public class TC_GameScenesVisibility : IVisibility
    {
        public bool Visible
        {
            get
            {
                return (HighLogic.LoadedScene == GameScenes.FLIGHT && !MapView.MapIsEnabled &&
                    (visibleInScenes & ApplicationLauncher.AppScenes.FLIGHT) != ApplicationLauncher.AppScenes.NEVER) ||
                    (HighLogic.LoadedScene == GameScenes.FLIGHT && MapView.MapIsEnabled &&
                    (visibleInScenes & ApplicationLauncher.AppScenes.MAPVIEW) != ApplicationLauncher.AppScenes.NEVER) ||
                    (HighLogic.LoadedScene == GameScenes.SPACECENTER &&
                    (visibleInScenes & ApplicationLauncher.AppScenes.SPACECENTER) != ApplicationLauncher.AppScenes.NEVER) ||
                    (HighLogic.LoadedScene == GameScenes.EDITOR &&
                    (visibleInScenes & (ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH)) != ApplicationLauncher.AppScenes.NEVER) ||
                    (HighLogic.LoadedScene == GameScenes.TRACKSTATION && (visibleInScenes & ApplicationLauncher.AppScenes.TRACKSTATION) != ApplicationLauncher.AppScenes.NEVER) ||
                    (HighLogic.LoadedScene == GameScenes.MAINMENU && (visibleInScenes & ApplicationLauncher.AppScenes.MAINMENU) != ApplicationLauncher.AppScenes.NEVER);
            }
        }

        ApplicationLauncher.AppScenes visibleInScenes;

        public TC_GameScenesVisibility(ApplicationLauncher.AppScenes visibleInScenes)
        {
            this.visibleInScenes = visibleInScenes;
        }
    }

    public class ToolbarControl : MonoBehaviour
    {
        private static List<ToolbarControl> tcList = null;
        private string nameSpace = "";
        private string toolbarId = "";
        // private GameScenes[] gameScenes;

        private string BlizzyToolbarIconActive = "";
        private string BlizzyToolbarIconInactive = "";
        private string StockToolbarIconActive = "";
        private string StockToolbarIconInactive = "";

        private ApplicationLauncher.AppScenes visibleInScenes;
        private string toolTip = null;

        //private bool spaceCenterVisited = false;
        /// <summary>
        /// The button's tool tip text. Set to null if no tool tip is desired.
        /// </summary>
        /// <remarks>
        /// Tool Tip Text Should Always Use Headline Style Like This.
        /// </remarks>
        public string ToolTip
        {
            set { toolTip = value; }
            get { return toolTip; }
        }

        public Vector2 buttonClickedMousePos
        {
            get;
            private set;
        }


        public delegate void TC_ClickHandler();

        /// <summary>
        /// Sets flag to use either use or not use the Blizzy toolbar
        /// </summary>
        /// <param name="useBlizzy"></param>
        public void UseBlizzy(bool useBlizzy)
        {
            if (activeToolbarType == ToolBarSelected.none)
            {
                prestartUseBlizzy = useBlizzy;
                return;
            }

            if (ToolbarManager.ToolbarAvailable && useBlizzy)
            {
                if (activeToolbarType == ToolBarSelected.stock)
                    SetBlizzySettings();
            }
            else
            {
                if (activeToolbarType != ToolBarSelected.stock)
                {
                    SetStockSettings();
                }
            }
        }

        /// <summary>
        /// Whether this button is currently enabled (clickable) or not. This does not affect the player's ability to
        /// position the button on their toolbar.
        /// </summary>
        public bool Enabled
        {
            set { SetIsEnabled(value); }
            get { return isEnabled; }
        }

        private bool isEnabled = true;
        private void SetIsEnabled(bool b)
        {
            isEnabled = b;
            if (activeToolbarType == ToolBarSelected.stock)
            {
                if (this.stockButton == null)
                    return;
                if (b)
                    this.stockButton.Enable();
                else
                    this.stockButton.Disable();
            }
            if (activeToolbarType == ToolBarSelected.blizzy)
            {
                if (blizzyButton == null)
                    return;
                this.blizzyButton.Enabled = b;
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
            ApplicationLauncher.AppScenes visibleInScenes, string nameSpace, string toolbarId,
            string largeToolbarIcon,
            string smallToolbarIcon,
            string toolTip = null)
        {
            AddToAllToolbars(onTrue, onFalse, null, null, null, null,
                visibleInScenes, nameSpace, toolbarId, largeToolbarIcon, largeToolbarIcon, smallToolbarIcon, smallToolbarIcon, toolTip);
        }

        public void AddToAllToolbars(TC_ClickHandler onTrue, TC_ClickHandler onFalse,
            ApplicationLauncher.AppScenes visibleInScenes, string nameSpace, string toolbarId,
            string largeToolbarIconActive,
            string largeToolbarIconInactive,
            string smallToolbarIconActive,
            string smallToolbarIconInactive, string toolTip = null)
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

        /// <summary>
        /// AddLeftRightClickCallbacks
        /// </summary>
        /// <param name="onLeftClick"></param>
        /// <param name="onRightClick"></param>
        public void AddLeftRightClickCallbacks(Callback onLeftClick, Callback onRightClick)
        {
            Log.Info("AddLeftRightClickCallbacks 1");
            this.onLeftClick = onLeftClick;
            Log.Info("AddLeftRightClickCallbacks 2");
            this.onRightClick = onRightClick;

            if (stockButton != null)
            {
                if (onLeftClick != null)
                    stockButton.onLeftClick = (Callback)Delegate.Combine(stockButton.onLeftClick, this.onLeftClick); //combine delegates together
                if (onRightClick != null)
                    stockButton.onRightClick = (Callback)Delegate.Combine(stockButton.onRightClick, this.onRightClick); //combine delegates together
            }

        }

        public void AddToAllToolbars(TC_ClickHandler onTrue, TC_ClickHandler onFalse, TC_ClickHandler onHover, TC_ClickHandler onHoverOut, TC_ClickHandler onEnable, TC_ClickHandler onDisable,
            ApplicationLauncher.AppScenes visibleInScenes, string nameSpace, string toolbarId, string largeToolbarIconActive, string largeToolbarIconInactive, string smallToolbarIconActive, string smallToolbarIconInactive, string toolTip = null)

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
            try
            {
                if (HighLogic.CurrentGame.Parameters.CustomParams<TC>().showStockTooltips)
                    this.ToolTip = toolTip;
            }
            catch { }
#if false
            SetupGameScenes(visibleInScenes);
#endif
            StartAfterInit();
        }

        string lastLarge = "";
        string lastSmall = "";
        public void SetTexture(string large, string small)
        {
            if (large == "" && small == "")
            {
                lastLarge = "";
                lastSmall = "";
                UpdateToolbarIcon();
                return;
            }
            lastSmall = small;
            Log.Info("ToolbarControl.SetTexture, lastLarge: " + lastLarge + ", large: " + large + ", small: " + small);
            if (ToolbarManager.ToolbarAvailable && activeToolbarType == ToolBarSelected.blizzy)
            {
                lastLarge = large;
                blizzyButton.TexturePath = small;
            }
            else
            {
                if (lastLarge != large)
                {
                    lastLarge = large;

                    Texture2D tex = GetTexture(lastLarge, false);
                    if (tex != null)
                        stockButton.SetTexture((Texture)tex);
                }

            }
        }

#if false
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
#endif

        void SetButtonPos()
        {
            Vector2 pos = Input.mousePosition;
            pos.y = Screen.height - pos.y;
            buttonClickedMousePos = pos;
        }

        event TC_ClickHandler onTrue = null;
        event TC_ClickHandler onFalse = null;
        event Callback onLeftClick = null;
        event Callback onRightClick = null;
        event TC_ClickHandler onHover = null;
        event TC_ClickHandler onHoverOut = null;
        event TC_ClickHandler onEnable = null;
        event TC_ClickHandler onDisable = null;


        private ApplicationLauncherButton stockButton;
        private IButton blizzyButton;

        //private string toolbarIconActive = "";
        //private string toolbarIconInactive = "";

        public static bool buttonActive = false;

        enum ToolBarSelected { stock, blizzy, none };
        ToolBarSelected activeToolbarType = ToolBarSelected.none;
        bool prestartUseBlizzy = false;

        public void SetFalse()
        {
            //buttonActive = false;
            if (stockButton != null)
            {
                stockButton.SetFalse();
            }
            else
            {
                ToggleButtonActive();
            }
            UpdateToolbarIcon();
        }

        private void RemoveStockButton()
        {
            if (this.stockButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(this.stockButton);
                GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                GameEvents.onGUIApplicationLauncherDestroyed.Remove(OnGUIAppLauncherDestroyed);

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

        #region SetButtonSettings
        private void SetBlizzySettings()
        {
            Log.Info("SetBlizzySettings, namespace: " + nameSpace);
            if (activeToolbarType == ToolBarSelected.stock)
            {
                RemoveStockButton();
            }
            //this.toolbarIconActive = BlizzyToolbarIconActive;
            //this.toolbarIconInactive = BlizzyToolbarIconInactive;

            this.blizzyButton = ToolbarManager.Instance.add(nameSpace, toolbarId);
            this.blizzyButton.ToolTip = ToolTip;
            this.blizzyButton.OnClick += this.button_Click;
            this.blizzyButton.Visibility = new TC_GameScenesVisibility(visibleInScenes);


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

            //this.toolbarIconActive = StockToolbarIconActive;
            //this.toolbarIconInactive = StockToolbarIconInactive;

            activeToolbarType = ToolBarSelected.stock;
            this.UpdateToolbarIcon(true);
        }
        #endregion

        private void StartAfterInit()
        {
            Log.Info("StartAfterInit, prestartuseBlizzy: " + prestartUseBlizzy);
            if (prestartUseBlizzy)
                SetBlizzySettings();
            else
                SetStockSettings();

            if (tcList == null)
                tcList = new List<ToolbarControl>();

            tcList.Add(this);

            // this is needed because of a bug in KSP with event onGUIAppLauncherReady.
            //if (activeToolbarType == ToolBarSelected.stock || !ToolbarManager.ToolbarAvailable)
            {
                //OnGUIAppLauncherReady();
            }
        }
        bool destroyed = false;
        public void OnDestroy()
        {
            tcList.Remove(this);
            destroyed = true;
            if (activeToolbarType == ToolBarSelected.stock)
            {
                RemoveStockButton();

            }
            else
            {
                RemoveBlizzyButton();
            }
        }

        private void UpdateToolbarIcon(bool firstTime = false)
        {
            SetIsEnabled(isEnabled);
            // if (lastLarge != "")
            {
                // SetTexture(lastLarge, lastSmall);
                //return;                    
            }
            if (activeToolbarType == ToolBarSelected.blizzy)
            {
                if (lastSmall != "")
                    this.blizzyButton.TexturePath = lastSmall;
                else
                    this.blizzyButton.TexturePath = buttonActive ? this.BlizzyToolbarIconActive : this.BlizzyToolbarIconInactive;
            }
            else
            {
                if (stockButton == null && !firstTime)
                    Log.Error("stockButton is null, " + ",  namespace: " + nameSpace);
                else
                {
                    if (stockButton != null)
                    {
                        if (lastLarge != "")
                            this.stockButton.SetTexture((Texture)GetTexture(lastLarge, false));
                        else
                            this.stockButton.SetTexture((Texture)GetTexture(buttonActive ? this.StockToolbarIconActive : this.StockToolbarIconInactive, false));
                    }
                }
            }
        }

        //
        // The following function was initially copied from @JPLRepo's AmpYear mod, which is covered by the GPL, as is this mod
        //
        // This function will attempt to load either a PNG or a JPG from the specified path.  
        // It first checks to see if the actual file is there, if not, it then looks for either a PNG or a JPG
        //
        // easier to specify different cases than to change case to lower.  This will fail on MacOS and Linux
        // if a suffix has mixed case
        static string[] imgSuffixes = new string[] { ".png", ".jpg", ".gif", ".PNG", ".JPG", ".GIF", ".dds", ".DDS" };
        public static Boolean LoadImageFromFile(ref Texture2D tex, String fileNamePath)
        {

            Boolean blnReturn = false;
            bool dds = false;
            try
            {
                string path = fileNamePath;
                if (!System.IO.File.Exists(fileNamePath))
                {
                    // Look for the file with an appended suffix.
                    for (int i = 0; i < imgSuffixes.Length; i++)

                        if (System.IO.File.Exists(fileNamePath + imgSuffixes[i]))
                        {
                            path = fileNamePath + imgSuffixes[i];
                            dds = imgSuffixes[i] == ".dds" || imgSuffixes[i] == ".DDS";
                            break;
                        }
                }

                //File Exists check
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        if (dds)
                        {
                            byte[] bytes = System.IO.File.ReadAllBytes(path);


                            System.IO.BinaryReader binaryReader = new System.IO.BinaryReader(new System.IO.MemoryStream(bytes));
                            uint num = binaryReader.ReadUInt32();

                            if (num != DDSValues.uintMagic)
                            {
                                UnityEngine.Debug.LogError("DDS: File is not a DDS format file!");
                                return false;
                            }
                            DDSHeader ddSHeader = new DDSHeader(binaryReader);

                            TextureFormat tf = TextureFormat.Alpha8;
                            if (ddSHeader.ddspf.dwFourCC == DDSValues.uintDXT1)
                                tf = TextureFormat.DXT1;
                            if (ddSHeader.ddspf.dwFourCC == DDSValues.uintDXT5)
                                tf = TextureFormat.DXT5;
                            if (tf == TextureFormat.Alpha8)
                                return false;


                            tex = LoadTextureDXT(bytes, tf);
                        }
                        else
                            tex.LoadImage(System.IO.File.ReadAllBytes(path));
                        blnReturn = true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Failed to load the texture:" + path);
                        Log.Error(ex.Message);
                    }
                }
                else
                {
                    Log.Error("Cannot find texture to load:" + fileNamePath);
                }


            }
            catch (Exception ex)
            {
                Log.Error("Failed to load (are you missing a file):" + fileNamePath);
                Log.Error(ex.Message);
            }
            return blnReturn;
        }
        public static Texture2D LoadTextureDXT(byte[] ddsBytes, TextureFormat textureFormat)
        {
            if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
                throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");

            byte ddsSizeCheck = ddsBytes[4];
            if (ddsSizeCheck != 124)
                throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

            int height = ddsBytes[13] * 256 + ddsBytes[12];
            int width = ddsBytes[17] * 256 + ddsBytes[16];

            int DDS_HEADER_SIZE = 128;
            byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
            Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

            Texture2D texture = new Texture2D(width, height, textureFormat, false);
            texture.LoadRawTextureData(dxtBytes);
            texture.Apply();

            return (texture);
        }

        Texture2D GetTexture(string path, bool b)
        {

            Texture2D tex = new Texture2D(16, 16, TextureFormat.ARGB32, false);

            if (LoadImageFromFile(ref tex, KSPUtil.ApplicationRootPath + "GameData/" + path))
                return tex;
            return tex;
        }
        private void OnGUIAppLauncherReady()
        {
            if (destroyed)
                return;
            // Setup PW Stock Toolbar button
            //bool hidden = false;
            if (ApplicationLauncher.Ready && (stockButton == null /*|| !ApplicationLauncher.Instance.Contains(stockButton, out hidden) */ ))
            {
                Log.Info("Adding stock button");
                stockButton = ApplicationLauncher.Instance.AddModApplication(
                    doOnTrue,
                    doOnFalse,
                    doOnHover,
                    doOnHoverOut,
                    doOnEnable,
                    doOnDisable,
                    visibleInScenes,
                    (Texture)GetTexture(StockToolbarIconActive, false));

                if (onLeftClick != null)
                    stockButton.onLeftClick = (Callback)Delegate.Combine(stockButton.onLeftClick, onLeftClick); //combine delegates together
                if (onRightClick != null)
                    stockButton.onRightClick = (Callback)Delegate.Combine(stockButton.onRightClick, onRightClick); //combine delegates together

                SetStockSettings();
            }
        }

        private void doOnTrue()
        {
            SetButtonPos();
            if (this.onTrue != null)
                SetButtonActive();
            //ToggleButtonActive();
        }

        private void doOnFalse()
        {
            SetButtonPos();
            if (this.onFalse != null)
                SetButtonInactive();
            //ToggleButtonActive();
        }

        private void doOnHover()
        {
            if (activeToolbarType == ToolBarSelected.stock)
            {
                drawTooltip = true;
                starttimeToolTipShown = Time.fixedTime;
            }
            if (this.onHover != null) onHover();
        }

        private void doOnHoverOut() { drawTooltip = false; if (this.onHoverOut != null) onHoverOut(); }
        private void doOnEnable() { if (this.onEnable != null) onEnable(); }
        private void doOnDisable() { if (this.onDisable != null) onDisable(); }

        private void button_Click(ClickEvent e)
        {
            SetButtonPos();
            if (e.MouseButton == 0)
            {
                if (this.onTrue != null)
                    this.ToggleButtonActive();

                if (onLeftClick != null)
                    onLeftClick();
            }
            if (e.MouseButton == 1)
            {
                if (onRightClick != null)
                    onRightClick();
            }
        }

        #region ActiveInactive
        void SetButtonActive()
        {
            buttonActive = true;
            if (onTrue != null)
                onTrue();
            UpdateToolbarIcon();
        }

        void SetButtonInactive()
        {
            buttonActive = false;
            if (onFalse != null)
                onFalse();
            UpdateToolbarIcon();
        }

        private void ToggleButtonActive()
        {
            buttonActive = !buttonActive;

            if (buttonActive)
            {
                SetButtonActive();
            }
            else
            {
                SetButtonInactive();
            }
        }
        #endregion

        private void OnGUIAppLauncherDestroyed()
        {
            RemoveStockButton();
        }

        #region tooltip
        bool drawTooltip = false;
        float starttimeToolTipShown = 0;
        Vector2 tooltipSize;
        float tooltipX, tooltipY;
        Rect tooltipRect;

        void OnGUI()
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER ||
                HighLogic.LoadedScene == GameScenes.EDITOR ||
                HighLogic.LoadedScene == GameScenes.FLIGHT ||
                HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                if (!HighLogic.CurrentGame.Parameters.CustomParams<TC>().showStockTooltips)
                    return;
                if (drawTooltip && ToolTip != null && ToolTip.Trim().Length > 0)
                {
                    if (Time.fixedTime - starttimeToolTipShown > HighLogic.CurrentGame.Parameters.CustomParams<TC>().hoverTimeout)
                        return;

                    Rect brect = new Rect(Input.mousePosition.x, Input.mousePosition.y, 38, 38);
                    SetupTooltip();
                    GUI.Window(12342, tooltipRect, TooltipWindow, "");
                }
            }
        }


        void SetupTooltip()
        {
            if (ToolTip != null && ToolTip.Trim().Length > 0)
            {
                Vector2 mousePosition;
                mousePosition.x = Input.mousePosition.x;
                mousePosition.y = Screen.height - Input.mousePosition.y;

                int buttonsize = (int)(42 * GameSettings.UI_SCALE) + 2;
                tooltipSize = HighLogic.Skin.label.CalcSize(new GUIContent(ToolTip));

                if (HighLogic.LoadedScene == GameScenes.EDITOR || HighLogic.LoadedScene == GameScenes.SPACECENTER ||
                    HighLogic.LoadedScene == GameScenes.TRACKSTATION)
                {
                    tooltipX = (mousePosition.x + tooltipSize.x > Screen.width) ? (Screen.width - tooltipSize.x) : mousePosition.x;
                    tooltipY = Math.Min(mousePosition.y, Screen.height - buttonsize);
                }
                else
                {
                    tooltipX = Math.Min(mousePosition.x, Screen.width - buttonsize - tooltipSize.x);
                    tooltipY = mousePosition.y;
                }

                if (tooltipX < 0) tooltipX = 0;
                if (tooltipY < 0) tooltipY = 0;
                tooltipRect = new Rect(tooltipX - 1, tooltipY - tooltipSize.y, tooltipSize.x + 4, tooltipSize.y);

            }
        }
        void TooltipWindow(int id)
        {
            GUI.Label(new Rect(2, 0, tooltipRect.width - 2, tooltipRect.height), ToolTip, HighLogic.Skin.label);
        }
        protected void DrawTooltip()
        {
            if (ToolTip != null && ToolTip.Trim().Length > 0)
            {
                GUI.Label(tooltipRect, ToolTip, HighLogic.Skin.label);
            }
        }
        #endregion

        /// <summary>
        /// Checks whether the given stock button was created by this mod.
        /// </summary>
        /// <param name="button">the button to check</param>
        /// <param name="nameSpace">The namespace of the button</param>
        /// <param name="id">the unique ID of the button</param>
        /// <returns>true, if the button was created by the mod, false otherwise</returns>
        public static bool IsStockButtonManaged(ApplicationLauncherButton button, out string nameSpace, out string id)
        {
            nameSpace = "";
            id = "";
            if (tcList == null)
                return false;
            foreach (var b in tcList)
            {
                if (b.activeToolbarType == ToolBarSelected.stock)
                {
                    if (b.stockButton == button)
                    {
                        nameSpace = b.nameSpace;
                        id = b.toolbarId;
                        return true;
                    }
                }
            }
            return false;
        }

        public void SetTrue(bool makeCall = false)
        {
            if (activeToolbarType == ToolBarSelected.stock)
            {
                if (stockButton != null)
                    stockButton.SetTrue(makeCall);
                else
                    Log.Error("SetTrue called before stockButton is initialized");
            }
            else
            {
                if (blizzyButton != null)
                    blizzyButton.TexturePath = BlizzyToolbarIconActive;
                else
                    Log.Error("SetTrue called before blizzyButton is initialized");

                if (onTrue != null && makeCall)
                    onTrue();
            }
            buttonActive = true;

            UpdateToolbarIcon(false);
        }

        public void SetFalse(bool makeCall = false)
        {
            if (activeToolbarType == ToolBarSelected.stock)
            {
                stockButton.SetFalse(makeCall);
            }
            else
            {
                blizzyButton.TexturePath = BlizzyToolbarIconInactive;
                if (onFalse != null && makeCall)
                    onFalse();
            }
            buttonActive = false;

            UpdateToolbarIcon(false);
        }
    }
}
