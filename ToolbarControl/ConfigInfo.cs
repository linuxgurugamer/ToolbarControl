using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using KSP.IO;

namespace ToolbarControl_NS
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class ConfigInfo : MonoBehaviour
    {
        public static ConfigInfo Instance;
        const string SETTINGSNAME = "ToolbarController";
        static string PLUGINDATA = KSPUtil.ApplicationRootPath + "GameData/001_ToolbarControl/PluginData/Debug.cfg";

        static public bool debugMode = false;

        void Start()
        {
            Log.Info("ConfigInfo.Start");
            Instance = this;
            LoadData();
            DontDestroyOnLoad(this);
        }

        public void SaveData()
        {
            Log.Info("ToolbarControl.SaveData");
            ConfigNode settingsFile = new ConfigNode();
            ConfigNode settings = new ConfigNode();

            settingsFile.SetNode(SETTINGSNAME, settings, true);
            settings.AddValue("debugMode", HighLogic.CurrentGame.Parameters.CustomParams<TC>().debugMode);
            settingsFile.Save(PLUGINDATA);
        }

        public void LoadData()
        {
            Log.Info("ToolbarControl.LoadData");
            ConfigNode settingsFile = ConfigNode.Load(PLUGINDATA);
            ConfigNode node = null;
            if (settingsFile != null)
            {
                node = settingsFile.GetNode(SETTINGSNAME);
                if (node != null)
                {
                    if (node.HasValue("debugMode"))
                    {
                        debugMode = bool.Parse(node.GetValue("debugMode"));
                    }
                }
            }
            
        }
    }
}
