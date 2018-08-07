
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;


namespace ToolbarControl_NS
{
    public class TC : GameParameters.CustomParameterNode
    {
        public override string Title { get { return ""; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Toolbar Control"; } }
        public override string DisplaySection { get { return "Toolbar Control"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }


        [GameParameters.CustomParameterUI("Show tooltips for stock & Blizzy toolbar",
            toolTip ="Blzzy tooltip setting may need to restart SKP")]
        public bool showStockTooltips = true;

        [GameParameters.CustomFloatParameterUI("Tooltip timeout", minValue = 0.5f, maxValue = 5.0f, asPercentage = false, displayFormat = "0.0",
                   toolTip = "Time tooltip stays around")]
        public float hoverTimeout = 0.5f;

        [GameParameters.CustomParameterUI("Debug mode",
                    toolTip = "Writes extra data to the log file")]
        public bool debugMode = false;

        public bool oldDebugMode = false;
        bool loaded = false;

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
        }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (oldDebugMode != debugMode)
            {
                ConfigInfo.Instance.SaveData();
                oldDebugMode = debugMode;
            }
            return true;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (!loaded)
            {
                ConfigInfo.Instance.LoadData();
                debugMode = ConfigInfo.debugMode;
                oldDebugMode = debugMode;
                loaded = true;
            }
            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }
}
