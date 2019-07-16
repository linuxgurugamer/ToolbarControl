/**
 * Based on the InstallChecker from the Kethane mod for Kerbal Space Program.
 * https://github.com/Majiir/Kethane/blob/b93b1171ec42b4be6c44b257ad31c7efd7ea1702/Plugin/InstallChecker.cs
 * 
 * Original is (C) Copyright Majiir.
 * CC0 Public Domain (http://creativecommons.org/publicdomain/zero/1.0/)
 * http://forum.kerbalspaceprogram.com/threads/65395-CompatibilityChecker-Discussion-Thread?p=899895&viewfull=1#post899895
 * 
 * This file has been modified extensively and is released under the same license.
 */
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ToolbarControl_NS
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class InstallChecker : MonoBehaviour
    {
        private const string MODNAME = "Toolbar Controller";
        private const string FOLDERNAME = "001_ToolbarControl";
        private const string EXPECTEDPATH = FOLDERNAME + "/Plugins";

        protected void Start()
        {
            // Search for this mod's DLL existing in the wrong location. This will also detect duplicate copies because only one can be in the right place.
            var assemblies = AssemblyLoader.loadedAssemblies.Where(a => a.assembly.GetName().Name == Assembly.GetExecutingAssembly().GetName().Name).Where(a => a.url != EXPECTEDPATH);
            if (assemblies.Any())
            {
                var badPaths = assemblies.Select(a => a.path).Select(p => Uri.UnescapeDataString(new Uri(Path.GetFullPath(KSPUtil.ApplicationRootPath)).MakeRelativeUri(new Uri(p)).ToString().Replace('/', Path.DirectorySeparatorChar)));
                PopupDialog.SpawnPopupDialog
                (
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    "test",
                    "Incorrect " + MODNAME + " Installation",
                    MODNAME + " has been installed incorrectly and will not function properly. All files should be located in KSP/GameData/" + FOLDERNAME + ". Do not move any files from inside that folder.\n\nIncorrect path(s):\n" + String.Join("\n", badPaths.ToArray()),
                    "OK",
                    false,
                    HighLogic.UISkin
                );
                Debug.Log("Incorrect " + MODNAME + " Installation: " + MODNAME + " has been installed incorrectly and will not function properly. All files should be located in KSP/GameData/" + EXPECTEDPATH + ". Do not move any files from inside that folder.\n\nIncorrect path(s):\n" + String.Join("\n", badPaths.ToArray())

                     );

            }

            //// Check for Module Manager
            //if (!AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.StartsWith("ModuleManager") && a.url == ""))
            //{
            //    PopupDialog.SpawnPopupDialog("Missing Module Manager",
            //        modName + " requires the Module Manager mod in order to function properly.\n\nPlease download from http://forum.kerbalspaceprogram.com/threads/55219 and copy to the KSP/GameData/ directory.",
            //        "OK", false, HighLogic.Skin);
            //}

            CleanupOldVersions();
        }

        /*
         * Tries to fix the install if it was installed over the top of a previous version
         */
        void CleanupOldVersions()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Debug.LogError("-ERROR- " + this.GetType().FullName + "[" + this.GetInstanceID().ToString("X") + "][" + Time.time.ToString("0.00") + "]: " +
                   "Exception caught while cleaning up old files.\n" + ex.Message + "\n" + ex.StackTrace);

            }
        }
    }
}

