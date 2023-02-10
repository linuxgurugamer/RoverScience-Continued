
using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using KSP.Localization;

using static RoverScience.InitLog;


namespace RoverScience
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings


    // HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().

    public class RSSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Rover Science"; } } // Localizer.GetStringByTag("LOC_RoverScience_GUI_DefaultSettings") ; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Rover Science"; } } // Localizer.GetStringByTag("LOC_RoverScience_GUI_RoverScience"); } }
        public override string DisplaySection { get { return "Rover Science"; } } //  Localizer.GetStringByTag("LOC_RoverScience_GUI_RoverScience") ; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }

        //[GameParameters.CustomParameterUI("Use KSP skin")]
        //public bool useKSPskin = true;


        [GameParameters.CustomFloatParameterUI("Wheels must be touching the ground",
            toolTip = "At least one wheel will need to be touching the ground for the sensors to be active")]
        public bool requireWheelTouching = true;

        [GameParameters.CustomFloatParameterUI("Sensor Maximum speed", minValue = 5f, maxValue = 40f, stepCount = 351, displayFormat = "F1")]
        public float maxSpeed = 10f;


        [GameParameters.CustomFloatParameterUI("Scaled Detection Distance", minValue = 1f, maxValue = 10f, stepCount = 101, displayFormat = "F1",
            toolTip = "Maximum detection distance is multiplied by this scale")]
        public float scaledDetectionDistance = 0f;

        [GameParameters.CustomFloatParameterUI("Scaled Science", minValue = 1f, maxValue = 10f, stepCount = 101, displayFormat = "F1",
            toolTip = "Science rewards is multiplied by this value")]
        public float scaledScience = 0f;

        [GameParameters.CustomFloatParameterUI("Scaled Science Occurrance", minValue = 1f, maxValue = 10f, stepCount = 101, displayFormat = "F1",
            toolTip = "Frequency of occurrance is multiplied by this (higher means more distance between occurrances)")]
        public float scaledScienceOccurrance = 0f;

        [GameParameters.CustomFloatParameterUI("Minimum distance between sci points", minValue = 500f, maxValue = 20000f, stepCount = 19501, displayFormat = "F0")]
        public float minDistanceBetweenData = 500f;

        [GameParameters.CustomParameterUI("Show Science dome",
            toolTip = "Show the red dome highlighting where science is located")]
        public bool showScienceDome = true;

        [GameParameters.CustomParameterUI("Rock collidier active",
            toolTip = "When true, the rocks are hard")]
        public bool rockCollider = true;



        [GameParameters.CustomParameterUI("Log Level: Off")]
        public bool off = false;

#if false
        [GameParameters.CustomParameterUI("Log Level: Error")]
        public bool error = false;

        [GameParameters.CustomParameterUI("Log Level: Warning")]
        public bool warning = false;
#endif
        [GameParameters.CustomParameterUI("Log Level: Info")]
        public bool info = false;

        [GameParameters.CustomParameterUI("Log Level: Detail")]
        public bool detail = false;

#if false
        [GameParameters.CustomParameterUI("Log Level: Trace")]
        public bool trace = false;
#endif

        bool oOff, oInfo, oDetail; //, oTrace, oError, oWarning,;

        bool initted = false;

        public override void SetDifficultyPreset(GameParameters.Preset preset) { }
        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (!initted)
            {
                initted = true;
                oOff = off;
#if false
                oError = error;
                oWarning = warning;
                oTrace = trace;
#endif
                oInfo = info;
                oDetail = detail;
            }

            if (oOff != off)
            {
                oOff = off;
                /* error = warning = */
                info = detail = /*trace = */ false;
            }
            else
#if false
            if (oError != error)
            {
                oError = error;
                off = warning = info = detail = trace = false;
            }
            else
            if (oWarning != warning)
            {
                oWarning = warning;
                off = error = info = detail = trace = false;
            }
            else
#endif
            if (oInfo != info)
            {
                oInfo = info;
                off = /* error = warning =*/  detail = /* trace = */ false;
            }
            else
            if (oDetail != detail)
            {
                oDetail = detail;
                off = /* error = warning = */ info = /* trace = */ false;
            }
#if false
            else
            if (oTrace != trace)
            {
                oTrace = trace;
                off = error = warning = info = detail = false;
            }
#endif
            if (/* !error && !warning && */ !info && !detail /* && !trace */)
                off = true;

            oOff = off;
#if false
            oError = error;
            oWarning = warning;
            oTrace = trace;
#endif
            oInfo = info;
            oDetail = detail;

            SetLogLevel();

            return true;
        }

        internal void SetLogLevel()
        {
            if (off) InitLog.SetLogLevel(0);
#if false
            if (error) InitLog.SetLogLevel(1);
            if (warning) InitLog.SetLogLevel(2);
            if (trace) InitLog.SetLogLevel(5);
#endif
            if (info) InitLog.SetLogLevel(3);
            if (detail) InitLog.SetLogLevel(4);
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters) { return true; }
        public override IList ValidValues(MemberInfo member) { return null; }
    }

}
