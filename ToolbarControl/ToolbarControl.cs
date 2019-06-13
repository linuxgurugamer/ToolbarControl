using System;
using UnityEngine;
using System.Collections.Generic;
using KSP.UI;
using KSP.UI.Screens;


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

    public partial class ToolbarControl : MonoBehaviour
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

            if (ToolbarManager.ToolbarAvailable && useBlizzy)
            {
                if (!blizzyActive)
                {
                    blizzyActive = true;
                    stockActive = false;
                    SetBlizzySettings();
                }
            }
            else
            {
                if (!stockActive)
                {
                    stockActive = true;
                    blizzyActive = false;
                    SetStockSettings();
                }
            }
        }

        /// <summary>
        /// Sets flag to use either use or not use the Blizzy toolbar
        /// </summary>
        /// <param name="useStock"></param>
        public void UseStock(bool useStock)
        {

            if (!ToolbarManager.ToolbarAvailable || useStock)
            {
                if (!stockActive)
                {
                    stockActive = true;
                    blizzyActive = false;
                    SetStockSettings();
                }
            }
            else
            {
                if (!blizzyActive)
                {
                    blizzyActive = true;
                    stockActive = false;
                    SetBlizzySettings();
                }
            }
        }

        public void UseButtons(string NameSpace)
        {

            bool s = registeredMods[NameSpace].modToolbarControl.stockActive;
            bool b = registeredMods[NameSpace].modToolbarControl.blizzyActive;

            registeredMods[NameSpace].modToolbarControl.stockActive = registeredMods[NameSpace].useStock;
            registeredMods[NameSpace].modToolbarControl.blizzyActive = true;


            if (registeredMods[NameSpace].modToolbarControl.stockActive != s)
                registeredMods[NameSpace].modToolbarControl.SetStockSettings();

            registeredMods[NameSpace].modToolbarControl.blizzyActive = registeredMods[NameSpace].useBlizzy;

            if (registeredMods[NameSpace].modToolbarControl.blizzyActive != b)
                registeredMods[NameSpace].modToolbarControl.SetBlizzySettings();

            registeredMods[NameSpace].modToolbarControl.UpdateToolbarIcon();
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
            if (stockActive)
            {
                if (this.stockButton == null)
                    return;
                if (b)
                    this.stockButton.Enable();
                else
                    this.stockButton.Disable();
            }
            if (ToolbarManager.ToolbarAvailable && blizzyActive)
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
            this.onLeftClick = onLeftClick;
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
            Log.Debug(ConfigInfo.debugMode, "AddToAlltoolbars main, nameSpace: " + nameSpace + ",  toolbarId: " + toolbarId +
                ",  largeToolbarIconActive: " + largeToolbarIconActive + ", largeToolbarIconInactive: " + largeToolbarIconInactive +
                ", smallToolbarIconActive: " + smallToolbarIconActive + ", smallToolbarIconInactive: " + smallToolbarIconInactive
                );
            if (toolTip == null)
                Log.Debug(ConfigInfo.debugMode, "toolTip is null");
            else
                Log.Debug(ConfigInfo.debugMode, "toolTip: " + toolTip);

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

            StartAfterInit();
            if (registeredMods.ContainsKey(nameSpace))
            {
                registeredMods[nameSpace].modToolbarControl = this;
                UseButtons(nameSpace);
            }
            else
                Log.Debug(ConfigInfo.debugMode, "Missing namespace: " + nameSpace);
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
            if (ToolbarManager.ToolbarAvailable && blizzyActive)
            {
               
                blizzyButton.TexturePath = small;
            }
            if (stockActive)
            {
                if (lastLarge != large)
                {
                    lastLarge = large;

                    Texture2D tex = GetTexture(lastLarge, false);
                    if (tex != null && stockButton != null)
                        stockButton.SetTexture((Texture)tex);
                }
            }
        }

        public bool IsHovering
        {
            get
            {
                return stockButton.IsHovering || blizzyButton.IsHovering;
            }
        }

        public Rect? StockPosition
        {
            get
            {
                if (stockButton != null)
                {
                    Camera _camera = UIMainCamera.Camera;
                    Vector3 _pos = _camera.WorldToScreenPoint(stockButton.GetAnchorUL());
                    return new Rect(_pos.x, Screen.height - _pos.y, 41, 41);
                }
                return null;
            }
        }

        public Rect? BlizzyPosition
        {
            get { return null;  }
        }

        public void DisableMutuallyExclusive()
        {
            mutuallyExclusive = false;
            if (stockButton != null)
                ApplicationLauncher.Instance.DisableMutuallyExclusive(stockButton);
        }

        public void EnableMutuallyExclusive()
        {
            if (stockButton != null)
            {
                Log.Info("EnableMutuallyExclusive, stock button is not null");
            }
            else
            {
                Log.Info("EnableMutuallyExclusive, stock button is null");
            }
            mutuallyExclusive = true;
            if (stockButton != null)
                ApplicationLauncher.Instance.EnableMutuallyExclusive(stockButton);

        }


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

        public bool buttonActive = false;

        bool stockActive = false;
        bool blizzyActive = false;
        private bool mutuallyExclusive = false;

        public void SetFalse()
        {
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
                if (mutuallyExclusive)
                    ApplicationLauncher.Instance.DisableMutuallyExclusive(stockButton);
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
            if (!ToolbarManager.ToolbarAvailable)
            {
                this.stockActive = true;
                SetStockSettings();
                return;
            }
            if (!this.stockActive)
            {
                this.RemoveStockButton();
            }

            if (this.blizzyButton == null && this.blizzyActive)
            {
                Log.Debug(ConfigInfo.debugMode, "Adding blizzyButton, nameSpace: " + nameSpace + ", toolbarId: " + toolbarId + ", ToolTip: " + ToolTip);
                this.blizzyButton = ToolbarManager.Instance.add(nameSpace, toolbarId);
                this.blizzyButton.ToolTip = ToolTip;
                this.blizzyButton.OnClick += this.button_Click;

                this.blizzyButton.OnMouseLeave += (e) =>
                {
                    doOnHoverOut();
                };

                this.blizzyButton.OnMouseEnter += (e) =>
                {
                    doOnHover();
                };

                this.blizzyButton.Visibility = new TC_GameScenesVisibility(visibleInScenes);
            }
            if (!this.blizzyActive)
            {
                this.RemoveBlizzyButton();
            }
            this.UpdateToolbarIcon();
        }
#if false
        private void OnMouseEnter()
        {

        }
        private void OnMouseLeave()
        {

        }
#endif
        private void SetStockSettings()
        {
            if (!this.blizzyActive)
            {
                this.RemoveBlizzyButton();
            }
            if (this.stockButton == null && this.stockActive)
            {
                // Blizzy toolbar not available, or Stock Toolbar selected Let's go stock :(
                GameEvents.onGUIApplicationLauncherReady.Add(this.OnGUIAppLauncherReady);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(this.OnGUIAppLauncherDestroyed);
                this.OnGUIAppLauncherReady();
            }
            if (!this.stockActive && ToolbarManager.ToolbarAvailable)
            {
                this.RemoveStockButton();
            }
            this.UpdateToolbarIcon(true);
        }
#endregion

        private void StartAfterInit()
        {
            if (tcList == null)
                tcList = new List<ToolbarControl>();

            tcList.Add(this);
        }

        bool destroyed = false;
        public void OnDestroy()
        {
            tcList.Remove(this);
            destroyed = true;

            if (stockActive)
            {
                RemoveStockButton();

            }

            if (ToolbarManager.ToolbarAvailable && blizzyActive)
            {
                RemoveBlizzyButton();
            }
        }

        private void UpdateToolbarIcon(bool firstTime = false)
        {
            SetIsEnabled(isEnabled);

            if (ToolbarManager.ToolbarAvailable && this.blizzyActive && this.blizzyButton != null)
            {
                if (this.lastSmall != "")
                    this.blizzyButton.TexturePath = lastSmall;
                else
                    this.blizzyButton.TexturePath = this.buttonActive ? this.BlizzyToolbarIconActive : this.BlizzyToolbarIconInactive;
            }
            //else
            if (this.stockActive)
            {
                if (this.stockButton == null && !firstTime)
                    Log.Debug(ConfigInfo.debugMode, "stockButton is null, namespace: " + this.nameSpace);
                else
                {
                    if (this.stockButton != null)
                    {
                        if (this.lastLarge != "")
                        {
                            var tex = (Texture)GetTexture(this.lastLarge, false);
                            if (tex != null)
                            {
                                this.stockButton.SetTexture(tex);
                            }
                        }
                        else
                        {
                            var tex = (Texture)GetTexture(this.buttonActive ? this.StockToolbarIconActive : this.StockToolbarIconInactive, false);
                            if (tex != null)
                            {
                                this.stockButton.SetTexture(tex);
                            }
                        }
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
#if false
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
                            Log.Info("LoadIimageFromFile, dds");
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
                        {
                            tex.LoadImage(System.IO.File.ReadAllBytes(path));
                        }
                        blnReturn = true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Failed to load the texture: " + path);
                        Log.Error(ex.Message);
                    }
                }
                else
                {
                    Log.Debug(ConfigInfo.debugMode, "Cannot find texture to load from file:" + fileNamePath);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Failed to load (are you missing a file):" + fileNamePath);
                Log.Error(ex.Message);
            }
            return blnReturn;
        }
#else
        public static Boolean LoadImageFromFile(ref Texture2D tex, String fileNamePath)
        {
            Texture2D tx;
            bool rc = LoadImageFromFile(out tx, fileNamePath);
            tex = tx;
            return rc;
        }
        static internal Boolean LoadImageFromFile(out Texture2D tex, String fileNamePath, bool b = true)
        {

            Boolean blnReturn = false;
            tex = null;
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
                            tex = UnBlur.UnBlur.LoadDDS(path, false);
                            if (tex == null)
                                throw new Exception("LoadDDS failed.");
                        }
                        else
                        {
                            tex = new Texture2D(16, 16, TextureFormat.ARGB32, false);
                            tex.LoadImage(System.IO.File.ReadAllBytes(path));
                        }
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
#endif
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
#if false
        Texture2D GetTexture(string path, bool b)
        {
            Log.Debug(ConfigInfo.debugMode, "GetTexture, path: " + KSPUtil.ApplicationRootPath + "GameData/" + path);

            Texture2D tex = new Texture2D(16, 16, TextureFormat.ARGB32, false);

            // Since most mods have their button textures in files, try loading from the file first

            if (LoadImageFromFile(ref tex, KSPUtil.ApplicationRootPath + "GameData/" + path))
                return tex;
            if (GameDatabase.Instance.ExistsTexture(path))
            {
                tex = GameDatabase.Instance.GetTexture(path, false);
                return tex;
            }

     
            Log.Error("Cannot find texture to load:" + path);
  
            return null;
        }
#else
        internal static string TexPathname(string path)
        {
            string s = KSPUtil.ApplicationRootPath + "GameData/" + path;
            Log.Info("TexPathname: " + s);
            return s;
        }
        Texture2D GetTexture(string path, bool asNormalMap)
        {
            // ask unBlur to look for the texture in GameDatabase, remove mipmaps if necessary, and return it
            Texture2D tex = UnBlur.UnBlur.Instance?.GetTexture(path, asNormalMap);
            if (tex != null) return tex;

            // texture not found in GameDatabase
            LoadImageFromFile(out tex, TexPathname(path));
            return tex;
        }
#endif

        private void OnGUIAppLauncherReady()
        {
            if (destroyed)
                return;
            // Setup PW Stock Toolbar button
            if (ApplicationLauncher.Ready && stockButton == null)
            {
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
                if (doSetTrue)
                    SetTrue(doSetTrueValue);
                if (doSetFalse)
                    SetFalse(doSetFalseValue);
                if (mutuallyExclusive)
                {
                    Log.Info("OnGUIAppLauncherReady, EnableMutuallyExclusive, stock button is not null");
                    ApplicationLauncher.Instance.EnableMutuallyExclusive(stockButton);
                }
            }
        }

        private void doOnTrue()
        {
            SetButtonPos();
            if (this.onTrue != null)
                SetButtonActive();
        }

        private void doOnFalse()
        {
            SetButtonPos();
            if (this.onFalse != null)
                SetButtonInactive();
        }

        private void doOnHover()
        {
            if (stockActive)
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
            this.buttonActive = true;
            if (onTrue != null)
                onTrue();
            UpdateToolbarIcon();
        }

        void SetButtonInactive()
        {
            this.buttonActive = false;
            if (onFalse != null)
                onFalse();
            UpdateToolbarIcon();
        }

        private void ToggleButtonActive()
        {
            this.buttonActive = !this.buttonActive;

            if (this.buttonActive)
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
        public static bool IsStockButtonManaged(ApplicationLauncherButton button, out string nameSpace, out string id, out string toolTip)
        {
            nameSpace = "";
            id = "";
            toolTip = "";
            if (tcList == null)
                return false;
            foreach (var b in tcList)
            {
                if (b.stockActive)
                {
                    if (b.stockButton == button)
                    {
                        nameSpace = b.nameSpace;
                        id = b.toolbarId;
                        toolTip = b.toolTip;
                        return true;
                    }
                }
            }
            return false;
        }
  

        /// <summary>
        /// Checks whether the given stock button was created by this mod.
        /// </summary>
        /// <param name="button">the button to check</param>
        /// <param name="nameSpace">The namespace of the button</param>
        /// <param name="id">the unique ID of the button</param>
        /// <returns>true, if the button was created by the mod, false otherwise</returns>
        public static bool IsBlizzyButtonManaged(IButton blizzyButton, out string nameSpace, out string id, out string toolTip)
        {
            nameSpace = "";
            id = "";
            toolTip = "";
            if (tcList == null)
                return false;
            foreach (var b in tcList)
            {
                if (b.stockActive)
                {
                    if (b.blizzyButton == blizzyButton)
                    {
                        nameSpace = b.nameSpace;
                        id = b.toolbarId;
                        toolTip = b.toolTip;
                        return true;
                    }
                }
            }
            return false;
        }

        public void SetTrue(bool makeCall = false)
        {
            doSetTrue = false;

            if (stockButton == null && stockActive)
            {
                doSetTrue = true;
                doSetTrueValue = makeCall;
            }
            if (ToolbarManager.ToolbarAvailable && blizzyButton == null && blizzyActive)
            {
                doSetTrue = true;
                doSetTrueValue = makeCall;
            }
            if (stockButton == null && blizzyButton == null)
                return;
            doSetTrue = false;

            if (stockActive)
            {
                if (stockButton != null)
                {
                    stockButton.SetTrue(makeCall);
                    makeCall = false;
                }
                else
                    Log.Error("SetTrue called before stockButton is initialized");
            }
            //else
            if (ToolbarManager.ToolbarAvailable && blizzyActive)
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

        bool doSetFalse = false, doSetFalseValue = false;
        bool doSetTrue = false, doSetTrueValue = false;

        public void SetFalse(bool makeCall = false)
        {
            doSetFalse = false;


            if (stockButton == null && stockActive)
            {
                doSetFalse = true;
                doSetFalseValue = makeCall;
            }
            if (blizzyButton == null && ToolbarManager.ToolbarAvailable && blizzyActive)
            {
                doSetFalse = true;
                doSetFalseValue = makeCall;
            }
            if (stockButton == null && blizzyButton == null)
                return;
            doSetFalse = false;

            if (stockButton != null && stockActive)
            {
                stockButton.SetFalse(makeCall);
                makeCall = false;
            }
            //else
            if (ToolbarManager.ToolbarAvailable && blizzyActive)
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
