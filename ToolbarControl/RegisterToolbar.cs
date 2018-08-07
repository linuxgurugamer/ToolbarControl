using System;
using UnityEngine;
using KSP.UI.Screens;
using ClickThroughFix;
using System.Linq;

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
}