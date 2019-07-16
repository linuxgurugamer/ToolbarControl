using System;
using UnityEngine;
using KSP.UI.Screens;
using ClickThroughFix;
using System.Linq;

namespace ToolbarControl_NS
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class RegisterToolbarBlizzyOptions : MonoBehaviour
    {
        public static bool unBlurPresent = false;

        void Start()
        {
            unBlurPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "unBlur");

            ToolbarControl.RegisterMod(BlizzyOptions.MODID, BlizzyOptions.MODNAME, false, true, false);
        }
    }
}