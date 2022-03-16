using KSP.Localization;
using UnityEngine;
using static RoverScience.InitLog;

namespace RoverScience
{
    public partial class RoverScienceGUI
    {
        // Use this to change saved game's science for
        // selling and purchasing tech
        // WATCH OUT FOR QUICKSAVE/QUICKLOAD

        private float CurrentScience
        {
            get
            {
                return ResearchAndDevelopment.Instance.Science;
            }
        }

        private void DrawUpgradeGUI(int windowID)
        {
            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
            {
                ScreenMessages.PostScreenMessage(Localizer.GetStringByTag("#LOC_RoverScience_GUI_NotSandbox"), 3, ScreenMessageStyle.UPPER_CENTER); // Upgrades are not available in sandbox mode
                return;
            }

            if (roverScience.part.vessel != FlightGlobals.ActiveVessel)
            {
                Log.Detail("UpgradeGUI not drawn - not active vessel");
                return;
            }

            // UPGRADE WINDOW
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_ScienceAvailable", CurrentScience)); // Science Available: <<1>>
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            DrawUpgradeType(RSUpgrade.maxDistance);
            DrawUpgradeType(RSUpgrade.predictionAccuracy);
            DrawUpgradeType(RSUpgrade.analyzedDecay);

            GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_UpgradesPermanent")); // All upgrades are permanent and apply across all rovers"
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void DrawUpgradeType(RSUpgrade upgradeType)
        {

            int currentLevel = roverScience.GetUpgradeLevel(upgradeType);
            int nextLevel = currentLevel + 1;
            //double upgradeValueNow = roverScience.getUpgradeValue(upgradeType, currentLevel);
            //double upgradeValueNext = roverScience.getUpgradeValue(upgradeType, (nextLevel));

            string upgradeValueNow = roverScience.GetUpgradeValueString(upgradeType, currentLevel);
            string upgradeValueNext = roverScience.GetUpgradeValueString(upgradeType, nextLevel, true);

            double upgradeCost = roverScience.GetUpgradeCost(upgradeType, nextLevel);



            GUILayout.BeginHorizontal();

            GUILayout.Label(roverScience.GetUpgradeName(upgradeType), GUILayout.Width(150));
            GUILayout.Space(5);
            GUILayout.TextField(" " + Localizer.Format("#LOC_RoverScience_GUI_BtnUpgCurrent", upgradeValueNow, currentLevel), GUILayout.Width(120)); // Current: <<1>> [<<2>>]
            if (upgradeValueNext == "")
                GUILayout.TextField(" ", GUILayout.Width(90));
            else
                GUILayout.TextField(" " + Localizer.Format("#LOC_RoverScience_GUI_BtnUpgNext", (upgradeValueNext == "-1" ? Localizer.GetStringByTag("#LOC_RoverScience_GUI_Max") : upgradeValueNext.ToString())), GUILayout.Width(90)); // Next <<1>>
            if (upgradeCost == -1)
                GUILayout.TextField(" ", GUILayout.Width(90));
            else
                GUILayout.TextField(" " + Localizer.Format("#LOC_RoverScience_GUI_BtnUpgCost", (upgradeCost == -1 ? Localizer.GetStringByTag("#LOC_RoverScience_GUI_Max") : upgradeCost.ToString())), GUILayout.Width(90)); // Cost <<1>>
            GUI.enabled = (upgradeCost != -1);
            if (GUILayout.Button(Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnUpgrade"), GUILayout.Width(40))) // UP
            {
                Log.Detail("Upgrade button pressed - " + upgradeType);
                roverScience.UpgradeTech(upgradeType);
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();

        }
    }
}