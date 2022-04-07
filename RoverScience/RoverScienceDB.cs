using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RoverScience.InitLog;

namespace RoverScience
{
    [KSPAddon(KSPAddon.Startup.Flight, true)]
    public class RoverScienceDB : MonoBehaviour
    {
        public static RoverScienceDB Instance;

        public RoverScienceDB()
        {
            Instance = this;
        }

        private RoverScience RoverScience
        {
            get
            {
                var m = FlightGlobals.ActiveVessel.FindPartModuleImplementing<RoverScience>();
                if (m != null)
                {

                    return m;
                }
                return null;
            }
        }

        private RoverScienceGUI GUI
        {
            get
            {
                var m = FlightGlobals.ActiveVessel.FindPartModuleImplementing<RoverScience>();
                if (m != null)
                {
                    return m.roverScienceGUI;
                }
                return null;

            }
        }

        public void UpdateRoverScience()
        {
            Log.Info("UpdateRoverScience, levelMaxDistance: " + RoverScienceScenario.levelMaxDistance + ", levelPredictionAccuracy: " + RoverScienceScenario.levelPredictionAccuracy + ", levelAnalyzedDecay: " + RoverScienceScenario.levelAnalyzedDecay);
            RoverScience.levelMaxDistance = RoverScienceScenario.levelMaxDistance;
            RoverScience.levelPredictionAccuracy = RoverScienceScenario.levelPredictionAccuracy;
            RoverScience.levelAnalyzedDecay = RoverScienceScenario.levelAnalyzedDecay;

            if (RoverScienceScenario.console_x_y_show.Any())
            {
                try
                {
                    GUI.SetWindowPos(GUI.consoleGUI, (float)Convert.ToDouble(RoverScienceScenario.console_x_y_show[0]), (float)Convert.ToDouble(RoverScienceScenario.console_x_y_show[1]));
                    GUI.consoleGUI.isOpen = Convert.ToBoolean(RoverScienceScenario.console_x_y_show[2]);
                }
                catch (Exception e)
                {
                    Log.Error($"Error while parsing {RoverScienceScenario.console_x_y_show}", e);
                }
                //RoverScienceScenario.anomaliesAnalyzed = RoverScienceScenario.anomaliesAnalyzed;
                Log.Detail("Successfully updated RoverScience");
            }
        }

        public void UpdateDB()
        {
            DebugPrintAll("update[DB] - debugPrintAll");

            RoverScienceScenario.levelMaxDistance = RoverScience.levelMaxDistance;
            RoverScienceScenario.levelPredictionAccuracy = RoverScience.levelPredictionAccuracy;
            RoverScienceScenario.levelAnalyzedDecay = RoverScience.levelAnalyzedDecay;

            RoverScienceScenario.console_x_y_show = new List<string>();
            RoverScienceScenario.console_x_y_show.Add(GUI.consoleGUI.rect.x.ToString());
            RoverScienceScenario.console_x_y_show.Add(GUI.consoleGUI.rect.y.ToString());
            RoverScienceScenario.console_x_y_show.Add(GUI.consoleGUI.isOpen.ToString());

            //RoverScienceScenario.anomaliesAnalyzed = RoverScience.rover.anomaliesAnalyzed;
            Log.Detail("roverScience.rover.anomaliesAnalyzed: " + RoverScienceScenario.anomaliesAnalyzed);

            Log.Detail("Successfully updated DB");
        }

        public void DebugPrintAll(string title = "")
        {
            string ds = "======== " + title + " ========";
            ds += "\n(From RoverScience DB: debugPrintAll @ " + DateTime.Now;
            ds += "\nlevelMaxDistance: " + RoverScienceScenario.levelMaxDistance;
            ds += "\nlevelPredictionAccuracy: " + RoverScienceScenario.levelPredictionAccuracy;
            ds += "\nlevelAnalyzedDecay: " + RoverScienceScenario.levelAnalyzedDecay;
            ds += "\nconsole_x_y_show: " + string.Join(",", RoverScienceScenario.console_x_y_show.ToArray());
            ds += "\nanomaliesAnalyzed: " + string.Join(",", RoverScienceScenario.anomaliesAnalyzed.ToArray());
            ds += "\nROCsAnalyzed: " + string.Join(",", RoverScienceScenario.ROCsAnalyzed.ToArray());
            ds += "\n======================================";
            Log.Detail(ds);
        }
    }
}
