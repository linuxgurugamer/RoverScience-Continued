using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP_Log;

namespace RoverScience
{

    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class InitLog : MonoBehaviour
    {
        public static KSP_Log.Log Log;

        public static void SetLogLevel(int i)
        {
            Log.SetLevel((Log.LEVEL)i);
        }

        protected void Awake()
        {
            Log = new KSP_Log.Log("RoverScience"
#if DEBUG
                , KSP_Log.Log.LEVEL.DETAIL
#endif
                );
        }
    }
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class Statics: MonoBehaviour
    {
        bool initted = false;
        public static GUIStyle consoleAreaStyle;
        public static GUIStyle boldFont;
        public static GUIStyle noWrapFont;

        void Start()
        {
                HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().SetLogLevel();
            if (!initted)
            {
                DontDestroyOnLoad(this);
               // GameEvents.onGameStatePostLoad.Add(CheckScale);
            }
            CheckScale(null);
        }

        void CheckScale(ConfigNode node)
        {
            if (HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().scaledDetectionDistance == 0)
            {
                for (int b = 0; b < FlightGlobals.Bodies.Count; b++)
                {
                    if (FlightGlobals.Bodies[b].isHomeWorld)
                    {
                        var scale = FlightGlobals.Bodies[b].Radius / 600000f;
                        HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().scaledDetectionDistance =
                            HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().scaledScience =
                            HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().scaledScienceOccurrance =(float)scale;
                        HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().minDistanceBetweenData =(float) Math.Min(20000f, 500f * scale);
                        break;
                    }
                }
            }
        }

        void OnGUI()
        {
            if (!initted)
            {
                initted = true;
                consoleAreaStyle = new GUIStyle(HighLogic.Skin.textArea);
                boldFont = new GUIStyle(GUI.skin.label);
                noWrapFont = new GUIStyle(GUI.skin.label);

                boldFont.fontStyle = FontStyle.Bold;
                boldFont.wordWrap = false;

                noWrapFont.wordWrap = false;

            }
        }
    }

}
