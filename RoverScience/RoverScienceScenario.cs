using System;
using System.Collections.Generic;
using static RoverScience.InitLog;

namespace RoverScience
{
    // This will handle future saving of upgrades
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] { GameScenes.FLIGHT })]
    public class RoverScienceScenario : ScenarioModule
    {

        //private RoverScienceDB DB;
        //private RoverScience RS;
        //private RoverScienceGUI.GUIClass ConsoleGUI;

        public bool loaded = false;

        public static int levelMaxDistance = 1;
        public static int levelPredictionAccuracy = 1;
        public static int levelAnalyzedDecay = 2;

        public static List<string> console_x_y_show = new List<string>();
        public static List<string> anomaliesAnalyzed = new List<string>();
        public static List<string> ROCsAnalyzed = new List<string>();
        public override void OnLoad(ConfigNode node)
        {
            //if (RoverScience.Instance == null) return; // do not do if RoverScience not do

            Log.Detail("RoverScienceScenario OnLoad @" + DateTime.Now);

            //DB  = RoverScienceDB.Instance;
            //RS = RoverScience.Instance;
            //ConsoleGUI = RoverScience.Instance.roverScienceGUI.consoleGUI;

            LoadAnomaliesAnalyzed(node); // load anomalies
            LoadROCsAnalyzed(node);

            // LEVELMAXDISTANCE
            if (node.HasValue("levelMaxDistance"))
            {
                levelMaxDistance = Convert.ToInt32(node.GetValue("levelMaxDistance"));
            }
            else
            {
                node.AddValue("levelMaxDistance", levelMaxDistance.ToString());
            }

            // LEVELPREDICTIONACCURACY
            if (node.HasValue("levelPredictionAccuracy"))
            {
                levelPredictionAccuracy = Convert.ToInt32(node.GetValue("levelPredictionAccuracy"));
            }
            else
            {
                node.AddValue("levelPredictionAccuracy", levelPredictionAccuracy.ToString());
            }

            // LEVELANALYZEDDECAY
            if (node.HasValue("levelAnalyzedDecay"))
            {
                levelAnalyzedDecay = Convert.ToInt32(node.GetValue("levelAnalyzedDecay"));
            }
            else
            {
                node.AddValue("levelAnalyzedDecay", levelAnalyzedDecay.ToString());
            }

            // CONSOLEGUI
            if (node.HasValue("console_x_y_show"))
            {
                string loadedString = node.GetValue("console_x_y_show");
                console_x_y_show = new List<string>(loadedString.Split(','));
            }
            else
            {
                node.AddValue("console_x_y_show", string.Join(",", console_x_y_show.ToArray()));
            }

            if (RoverScience.Instance.rover != null)
            {
                RoverScienceDB.Instance.UpdateRoverScience();
            }            
        }

        public override void OnSave(ConfigNode node)
        {
            //if (RoverScience.Instance == null) return; // do not do if RoverScience not do

            Log.Info("RoverScienceScenario OnSave @" + DateTime.Now);

            SaveAnomaliesAnalyzed(node);
            SaveROCsAnalyzed(node);

            node.SetValue("levelMaxDistance", levelMaxDistance.ToString(), true);
            node.SetValue("levelPredictionAccuracy", levelPredictionAccuracy.ToString(), true);
            node.SetValue("levelAnalyzedDecay", levelAnalyzedDecay.ToString(), true);
            node.SetValue("console_x_y_show", string.Join(",", console_x_y_show.ToArray()), true);
        }

        public void SaveAnomaliesAnalyzed(ConfigNode node)
        {
            Log.Info("Attempting to save anomalies analyzed");
            //List<string> anomaliesAnalyzed = anomaliesAnalyzed;

            if (anomaliesAnalyzed.Count > 0)
            {
                if (anomaliesAnalyzed.Count > 1)
                {
                    node.SetValue("anomalies_visited_id", string.Join(",", anomaliesAnalyzed.ToArray()), true);
                }
                else
                {
                    node.SetValue("anomalies_visited_id", anomaliesAnalyzed[0], true);
                }
            }
            else
            {
                Log.Detail("no anomalies id to save");
            }
        }


        public void SaveROCsAnalyzed(ConfigNode node)
        {
            Log.Info("Attempting to save ROCs analyzed");

            if (ROCsAnalyzed.Count > 0)
            {
                if (ROCsAnalyzed.Count > 1)
                {
                    node.SetValue("rocs_visited_id", string.Join(",", ROCsAnalyzed.ToArray()), true);
                }
                else
                {
                    node.SetValue("rocs_visited_id", ROCsAnalyzed[0], true);
                }
            }
            else
            {
                Log.Detail("no ROCs id to save");
            }
        }





        public void LoadAnomaliesAnalyzed(ConfigNode node)
        {
            if (node.HasValue("anomalies_visited_id"))
            {
                string loadedString = node.GetValue("anomalies_visited_id");
                anomaliesAnalyzed = new List<string>(loadedString.Split(','));

                Log.Detail("loadedString: " + loadedString);
                for (int s1 = 0; s1 < anomaliesAnalyzed.Count; s1++)
                {
                    string s = anomaliesAnalyzed[s1];

                    Log.Detail("ID: " + s);
                }
                Log.Detail("Anomalies LOAD END");
                //RoverScienceDB.Instance.anomaliesAnalyzed = this.anomaliesAnalyzed; // load in new values in anomalies

            }
            else
            {
                Log.Detail("No anomalies have been analyzed");
            }

        }
        public void LoadROCsAnalyzed(ConfigNode node)
        {
            if (node.HasValue("rocs_visited_id"))
            {
                string loadedString = node.GetValue("rocs_visited_id");
                ROCsAnalyzed = new List<string>(loadedString.Split(','));

                Log.Detail("loadedString: " + loadedString);
                for (int s1 = 0; s1 < ROCsAnalyzed.Count; s1++)
                {
                    string s = ROCsAnalyzed[s1];

                    Log.Detail("ROC ID: " + s);
                }
                Log.Detail("ROCs LOAD END");

            }
            else
            {
                Log.Detail("No ROCs have been analyzed");
            }

        }



        


    }
}
