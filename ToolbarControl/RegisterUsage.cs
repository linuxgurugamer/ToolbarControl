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

        static string ConfigFile = KSPUtil.ApplicationRootPath + "GameData/001_ToolbarControl/PluginData/ToolbarControl.cfg";
        const string TOOLBARCONTROL = "ToolbarControl";
        const string TOOLBARCONTROLDATA = "ToolbarControlData";
        const string DATA = "DATA";

        static bool initted = false;

        internal static void SaveData()
        {
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
            Log.Info("LoadData");
            if (initted)
                return;
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
                        sortedModList.Add(mod);
                    }
                    sortedModList.Sort((x, y) => x.displayName.CompareTo(y.displayName));
                }
            }
        }

        public static bool RegisterMod(string NameSpace, string DisplayName = "", bool useBlizzy = false, bool useStock = true, bool NoneAllowed = true)
        {
            LoadData();

            if (registeredMods.ContainsKey(NameSpace))
            {
                Log.Info("RegisterMod, found, NameSpace: " + NameSpace + ", DisplayName: " + DisplayName);
                if (DisplayName != "")
                    registeredMods[NameSpace].displayName = DisplayName;

                registeredMods[NameSpace].noneAllowed = NoneAllowed;
                SaveData();
                return true;
            }
            try
            {
                if (DisplayName == "")
                    DisplayName = NameSpace;
                Log.Info("RegisterMod, NameSpace: " + NameSpace + ", DisplayName: " + DisplayName);
                Mod mod = new Mod(NameSpace, DisplayName, useBlizzy, useStock, NoneAllowed);
                registeredMods.Add(NameSpace, mod);
                sortedModList.Add(mod);
                
                sortedModList.Sort((x, y) => x.displayName.CompareTo(y.displayName));

                SaveData();
                return true;
            }
            catch { return false; }
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
