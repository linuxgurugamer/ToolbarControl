using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using KSP.UI.Screens;

namespace ToolbarControl_NS
{
    public partial class ToolbarControl
    {
        internal class Mod
        {
            public string modId;
            public string displayName;
            public bool useBlizzy;
            public bool useStock;
            public bool noneAllowed = true;
            public ToolbarControl modToolbarControl;
            public bool registered = false;

            public Mod(string ModId, string DisplayName, bool UseBlizzy, bool UseStock, bool NoneAllowed = true)
            {
                modId = ModId;
                displayName = DisplayName;
                useBlizzy = UseBlizzy;
                useStock = UseStock;

                noneAllowed = NoneAllowed;
            }
        }

        //internal static SortedDictionary<string, Mod> registeredMods = new SortedDictionary<string, Mod>();

        internal static Dictionary<string, Mod> registeredMods = new Dictionary<string, Mod>();
        internal static List<Mod> sortedModList = new List<Mod>();

        //internal static List<Mod> registeredMods = new List<Mod>();

        static string ConfigFile;
        const string TOOLBARCONTROL = "ToolbarControl";
        const string TOOLBARCONTROLDATA = "ToolbarControlData";
        const string DATA = "DATA";

        static bool initted = false;

        private void Awake()
        {
            SetConfigFilePath();
        }

        private static void SetConfigFilePath()
        {
            ConfigFile = KSPUtil.ApplicationRootPath + "GameData/001_ToolbarControl/PluginData/ToolbarControl.cfg";
        }

        internal static void SaveData()
        {
            if (ConfigFile == null)
                SetConfigFilePath();

            ConfigNode node = new ConfigNode(TOOLBARCONTROL);
            ConfigNode data = new ConfigNode(TOOLBARCONTROLDATA);
            data.AddValue("showWindowAtStartup", IntroWindowClass.showIntroAtStartup);
            foreach (var s in registeredMods)
            {
                ConfigNode nodeData = new ConfigNode();
                nodeData.AddValue("name", s.Key);
                nodeData.AddValue("displayName", s.Value.displayName);
                nodeData.AddValue("useBlizzy", s.Value.useBlizzy);
                nodeData.AddValue("useStock", s.Value.useStock);
                nodeData.AddValue("noneAllowed", s.Value.noneAllowed);
                data.AddNode(DATA, nodeData);
            }
            node.AddNode(data);
            node.Save(ConfigFile);
        }

        static bool ToBool(string aText)
        {
            if (aText == null)
                return false;
            return aText.ToLower() == "true" || aText.ToLower() == "on" || aText.ToLower() == "yes";
        }

        internal static void LoadData()
        {
            if (initted)
                return;

            if (ConfigFile == null)
                SetConfigFilePath();

            if (File.Exists(ConfigFile))
            {
                ConfigNode tempNode = ConfigNode.Load(ConfigFile);
                ConfigNode data = tempNode.GetNode(TOOLBARCONTROLDATA);
                initted = true;

                registeredMods.Clear();
                if (data.HasValue("showWindowAtStartup"))
                {
                    IntroWindowClass.showIntroAtStartup = ToBool(data.GetValue("showWindowAtStartup"));
                }

                foreach (var node in data.GetNodes())
                {
                    if (node.HasValue("name") && node.HasValue("useBlizzy"))
                    {
                        string modName = node.GetValue("name");
                        string displayName = modName;
                        try
                        {
                            displayName = node.GetValue("displayName");
                            if (displayName == "")
                                displayName = modName;
                        }
                        catch { }
                        bool useBlizzy = ToBool(node.GetValue("useBlizzy"));
                        bool useStock = true;
                        if (node.HasValue("useStock"))
                            useStock = ToBool(node.GetValue("useStock"));

                        bool noneAllowed = true;
                        if (node.HasValue("noneAllowed"))
                            noneAllowed = ToBool(node.GetValue("noneAllowed"));

                        Mod mod = new Mod(modName, displayName, useBlizzy, useStock, noneAllowed);
                        registeredMods.Add(modName, mod);
                        // sortedModList.Add(mod);
                    }
                    //sortedModList.Sort((x, y) => x.displayName.CompareTo(y.displayName));
                }
            }
        }

        public static bool RegisterMod(string NameSpace, string DisplayName = "", bool useBlizzy = false, bool useStock = true, bool NoneAllowed = true)
        {
            if (BlizzyOptions.startupCompleted)
            {
                Log.Error("WARNING: RegisterMod, LoadedScene: " + HighLogic.LoadedScene + ", called too late for: " + NameSpace + ", " + DisplayName + ", button may not be registered properly");
            }
            LoadData();
            Mod mod = null;
            //Log.Info("RegisterMod, NameSpace: " + NameSpace + ", DisplayName: " + DisplayName);
            if (registeredMods.ContainsKey(NameSpace))
            {
                //Log.Info("RegisterMod, found, NameSpace: " + NameSpace + ", DisplayName: " + DisplayName);
                if (DisplayName != "")
                    registeredMods[NameSpace].displayName = DisplayName;

                registeredMods[NameSpace].noneAllowed = NoneAllowed;
                registeredMods[NameSpace].registered = true;
                mod = registeredMods[NameSpace];
                SaveData();

            }
            else
                try
                {
                    if (DisplayName == "")
                        DisplayName = NameSpace;
                    //Log.Info("RegisterMod, NameSpace: " + NameSpace + ", DisplayName: " + DisplayName);
                    mod = new Mod(NameSpace, DisplayName, useBlizzy, useStock, NoneAllowed);
                    registeredMods.Add(NameSpace, mod);

                    SaveData();

                }
                catch { return false; }
            if (mod != null)
            {
                sortedModList.Add(mod);

                sortedModList.Sort((x, y) => x.displayName.CompareTo(y.displayName));

                return true;
            }
            // Should never get here
            Log.Error("Impossible Error");
            return false;
        }


        public static bool BlizzyActive(string NameSpace, bool? useBlizzy = null)
        {
            LoadData();

            if (useBlizzy == null)
            {

                if (registeredMods.ContainsKey(NameSpace))
                {
                    return registeredMods[NameSpace].useBlizzy;
                }
                else return false;
            }

            registeredMods[NameSpace].useBlizzy = (bool)useBlizzy;
            SaveData();
            return (bool)useBlizzy;
        }

        public static bool StockActive(string NameSpace, bool? useStock = null)
        {
            LoadData();
            if (useStock == null)
            {
                if (registeredMods.ContainsKey(NameSpace))
                {
                    return registeredMods[NameSpace].useStock;
                }
                else return false;
            }

            registeredMods[NameSpace].useStock = (bool)useStock;
            SaveData();
            return (bool)useStock;
        }

        public static void ButtonsActive(string NameSpace, bool? useStock, bool? useBlizzy)
        {
            LoadData();

            registeredMods[NameSpace].useStock = (bool)useStock;
            registeredMods[NameSpace].useBlizzy = (bool)useBlizzy;
            SaveData();
        }
    }
}
