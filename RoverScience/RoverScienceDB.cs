using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RoverScience.InitLog;

#if true
namespace RoverScience
{
    //[KSPAddon(KSPAddon.Startup.Flight, true)]
    public class RoverScienceDB // : MonoBehaviour
    {
        public RoverScienceDB(RoverScience rs)
        {
            roverScience = rs;
        }

        RoverScience roverScience;

        private RoverScience RoverScience
        {
            get { return roverScience; }
        }

        private RoverScienceGUI GUI
        {
            get
            {
                return roverScience.roverScienceGUI;
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

#endif
