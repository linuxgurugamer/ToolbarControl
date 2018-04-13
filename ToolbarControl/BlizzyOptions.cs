using System;
using UnityEngine;
using KSP.UI.Screens;
using ClickThroughFix;

namespace ToolbarControl_NS
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbarBlizzyOptions : MonoBehaviour
    {

        void Start()
        {
            ToolbarControl.RegisterMod(BlizzyOptions.MODID, BlizzyOptions.MODNAME, false, true, false);
        }
    }

    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class BlizzyOptions : MonoBehaviour
    {
        internal const string MODID = "ToolbarController_NS";
        internal const string MODNAME = "Toolbar Controller";

        GUIStyle scrollbar_style = new GUIStyle(HighLogic.Skin.scrollView);
        ToolbarControl toolbarControl;
        bool GUIEnabled = false;
        Rect WindowRect;
        int scrollBarHeight = Math.Min(500, Screen.height - 200);
        Vector2 scrollVector;

        bool initted = false;

        void Start()
        {
            if (ToolbarControl.registeredMods.Count == 1)
                Destroy(this);

            scrollbar_style.padding = new RectOffset(3, 3, 3, 3);
            scrollbar_style.border = new RectOffset(3, 3, 3, 3);
            scrollbar_style.margin = new RectOffset(1, 1, 1, 1);
            scrollbar_style.overflow = new RectOffset(1, 1, 1, 1);

            AddToolbarButton();

            DontDestroyOnLoad(this);
        }

        void AddToolbarButton()
        {

            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(ToggleGUI, ToggleGUI,
                ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.FLIGHT |
                ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB |
                ApplicationLauncher.AppScenes.TRACKSTATION,
                MODID,
                "toolbarControllerButton",
                "001_ToolbarControl/PluginData/Textures/toolbar_38",
                "001_ToolbarControl/PluginData/Textures/toolbar_24",
                MODNAME
            );
        }

        void ToggleGUI()
        {
            GUIEnabled = !GUIEnabled;
        }

        void InitGUIStuff()
        {
            // Calculate height of scrollbox
            GUIContent content = new GUIContent("a");
            Vector2 size = GUI.skin.toggle.CalcSize(content);

            scrollBarHeight = Math.Min(Screen.height - 200, ToolbarControl.registeredMods.Count * (9 + (int)size.y));

            // and calculate the window dimensions
            double WindowX = (Screen.width - 400) / 2;
            double WindowY = (Screen.height - (scrollBarHeight + 150)) / 2;

            WindowRect = new Rect((float)WindowX, (float)WindowY, 400, scrollBarHeight + 150);

            initted = true;
        }

        void OnGUI()
        {
            if (!GUIEnabled)
                return;
            if (!initted)
                InitGUIStuff();

            WindowRect = ClickThruBlocker.GUILayoutWindow(4946386, WindowRect, DoWindow, "Toolbar Controller", IntroWindowClass.windowStyle);
        }

        void OnDestroy()
        {
            toolbarControl.OnDestroy();
            Destroy(toolbarControl);
        }

        void DoWindow(int id)
        {

            GUILayout.BeginHorizontal();
            GUILayout.Label("For each mod, select which toolbar to put it's button on.");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("If the Blizzy toobar is not installed, all buttons will be put on the stock toolbar, regardless of the setting");
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Stock", GUILayout.Width(50));
            GUILayout.Label("Blizzy", GUILayout.Width(50));
            GUILayout.Label("Both", GUILayout.Width(50));
            GUILayout.Label("None", GUILayout.Width(50));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            scrollVector = GUILayout.BeginScrollView(scrollVector, scrollbar_style, GUILayout.Height(scrollBarHeight));

            foreach (var mod in ToolbarControl.registeredMods)
            {
                bool doUseButtons = false;
                GUILayout.BeginHorizontal();
                bool stock = GUILayout.Toggle(mod.Value.useStock, "", GUILayout.Width(60));
                if (stock != mod.Value.useStock)
                {
                    if (ToolbarControl.registeredMods[mod.Key].useStock == ToolbarControl.registeredMods[mod.Key].useBlizzy &&
                        ToolbarControl.registeredMods[mod.Key].useStock)
                    {
                        ToolbarControl.registeredMods[mod.Key].useBlizzy = false;
                    }
                    else
                    {
                        ToolbarControl.registeredMods[mod.Key].useStock = stock;
                        ToolbarControl.registeredMods[mod.Key].useBlizzy = !stock;
                    }
               
                    doUseButtons = true;
                }

                bool blizzy = GUILayout.Toggle(mod.Value.useBlizzy, "", GUILayout.Width(50));

                if (blizzy != mod.Value.useBlizzy)
                {
                    if (ToolbarControl.registeredMods[mod.Key].useStock == ToolbarControl.registeredMods[mod.Key].useBlizzy &&
                          ToolbarControl.registeredMods[mod.Key].useBlizzy)
                    {
                        ToolbarControl.registeredMods[mod.Key].useStock = false;
                    }
                    else
                    {
                        ToolbarControl.registeredMods[mod.Key].useBlizzy = blizzy;
                        ToolbarControl.registeredMods[mod.Key].useStock = !blizzy;
                    }

                    doUseButtons = true;
                }

                bool both = (stock & blizzy);
                bool newboth = GUILayout.Toggle(both, "", GUILayout.Width(50));
                if (newboth != both)
                {
                    ToolbarControl.registeredMods[mod.Key].useBlizzy = true;
                    ToolbarControl.registeredMods[mod.Key].useStock = true;

                    doUseButtons = true;
                }
                if (!ToolbarControl.registeredMods[mod.Key].noneAllowed)
                    GUI.enabled = false;
                bool none = (!stock & !blizzy);
                bool newnone = GUILayout.Toggle(none, "", GUILayout.Width(25));
                if (newnone != none)
                {

                    ToolbarControl.registeredMods[mod.Key].useBlizzy = false;
                    ToolbarControl.registeredMods[mod.Key].useStock = false;

                    doUseButtons = true;
                }

                if (doUseButtons)
                {
                    ToolbarControl.SaveData();
                    if (ToolbarControl.registeredMods[mod.Key].modToolbarControl != null)
                        ToolbarControl.registeredMods[mod.Key].modToolbarControl.UseButtons(mod.Key);
                    else
                        Log.Info("mod.Key: " + mod.Key + " toolbarControl is null");
                }
                GUI.enabled = true;
                GUILayout.Label(" " + mod.Value.displayName);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close"))
            {
                GUIEnabled = false;
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("?"))
            {
                IntroWindowClass.showHelp = true;
                IntroWindowClass.automoved = 0;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }
    }
}
