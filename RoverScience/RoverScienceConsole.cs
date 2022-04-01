using KSP.Localization;
using System.Collections.Generic;
using UnityEngine;
using ClickThroughFix;
using static RoverScience.InitLog;

namespace RoverScience
{
    public partial class RoverScienceGUI
    {

        public System.Random rand = new System.Random();

        public RandomConsolePrintOuts randomConsolePrintOuts = new RandomConsolePrintOuts();
        public Vector2 scrollPosition = new Vector2();

        private List<string> consolePrintOut = new List<string>();

        private RoverScience roverScience
        {
            get
            {
                return RoverScience.Instance;
            }

        }

        private Vessel Vessel
        {
            get
            {
                return FlightGlobals.ActiveVessel;
            }
        }

        private Rover Rover
        {
            get
            {
                return RoverScience.Instance.rover;
            }
        }


        public RoverScienceGUI()
        {
            Log.Info("RoverScienceGUI started");
            consoleGUI.rect.width = 250;
            debugGUI.rect.width = 400;
            upgradeGUI.rect.width = 500;

            // center consoleGUI
            consoleGUI.rect.x = (Screen.width / 2) - (consoleGUI.rect.width / 2);
            consoleGUI.rect.y = (Screen.height / 2) - (250);

            upgradeGUI.rect.x = consoleGUI.rect.x + consoleGUI.rect.width + 50;
            upgradeGUI.rect.y = consoleGUI.rect.y;
        }

        public void DrawGUI()
        {
            if (consoleGUI.isOpen)
            {
                consoleGUI.rect = ClickThruBlocker.GUILayoutWindow(25639814, consoleGUI.rect, DrawRoverConsoleGUI, Localizer.Format("#LOC_RoverScience_GUI_TerminalTitle", roverScience.RSVersion)); //  Rover Terminal - <<1>>

                if (upgradeGUI.isOpen)
                {
                    upgradeGUI.rect = ClickThruBlocker.GUILayoutWindow(2389233, upgradeGUI.rect, DrawUpgradeGUI, Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnUpgradeMenu")); // Upgrade Menu
                }

            }

            if (debugGUI.isOpen)
            {
                debugGUI.rect = ClickThruBlocker.GUILayoutWindow(9358921, debugGUI.rect, DrawDebugGUI, Localizer.GetStringByTag("#LOC_RoverScience_GUI_Debug")); // Debug
            }
        }

        public void SetWindowPos(GUIClass guiWindow, float x, float y)
        {
            guiWindow.rect.x = x;
            guiWindow.rect.y = y;
        }

        public void AddToConsole(string line)
        {
            if (consolePrintOut.Count >= 50)
            {
                consolePrintOut.Clear();
            }
            consolePrintOut.Add("> " + line);
            scrollPosition.y = 10000;
        }

        public void AddRandomConsoleJunk()
        {
            AddToConsole(randomConsolePrintOuts.GetRandomPrint());
        }

        public void ClearConsole()
        {
            consolePrintOut.Clear();
        }



    }
}

