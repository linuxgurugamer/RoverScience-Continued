using KSP.Localization;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static RoverScience.InitLog;

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
// ROVERSCIENCE PLUGIN WAS CREATED BY THESPEARE					  //
// FOR KERBAL SPACE PROGRAM - PLEASE SEE FORUM THREAD FOR DETAILS //
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
namespace RoverScience
{


    public class RoverScience : PartModule
    {
        private const float HomeWorldScienceScalar = 0.01f;
        private const float SunScienceScalar = 0f;
        private const float NearMoonScienceScalar = 0.3f;
        private const float FarMoonScienceScalar = 0.2f;

        // Not necessarily updated per build. Mostly updated per major commits
        public readonly string RSVersion = typeof(RoverScience).Assembly.GetName().Version.ToString();
        public  CelestialBody HomeWorld;

        public System.Random rand = new System.Random();
        public ModuleScienceContainer container;
        public ModuleCommand command;
        public Rover rover;

        public DrawWaypoint drawWaypoint;

        public int levelMaxDistance = 1;
        public int levelPredictionAccuracy = 1;
        public int levelAnalyzedDecay = 2;
        public readonly int maximum_levelMaxDistance = 5;
        public readonly int maximum_predictionAccuracy = 5;
        public readonly int maximum_levelAnalyzedDecay = 5;

        public double CurrentPredictionAccuracy
        {
            get
            {
                return GetUpgradeValue(RSUpgrade.predictionAccuracy, levelPredictionAccuracy);
            }
        }

        public double CurrentMaxDistance
        {
            get
            {
                return GetUpgradeValue(RSUpgrade.maxDistance, levelMaxDistance);
            }
        }

        public RoverScienceDB roverScienceDB;
        private RoverScienceDB DB
        {
            get { return roverScienceDB; }
        }


        public RoverScienceGUI roverScienceGUI;
        public double distCounter;

        [KSPField(isPersistant = true)]
        public int amountOfTimesAnalyzed = 0;
        // Leave this alone. PartModule has its own vessel class which SHOULD do the job but
        // for some reason removing this seemed to destroy a lot of function
        Vessel Vessel
        {
            get
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                return vessel;
                }
                else
                {
                    Log.Detail("RoverScience.Vessel null! - not flight");
                    return null;
                }
            }

        }

        public float scienceMaxRadiusBoost = 1;

        public double ScienceDecayPercentage
        {
            get
            {
                return Math.Round((1 - ScienceDecayScalar) * 100);
            }
        }

        public float ScienceDecayScalar
        {
            get
            {
                return GetScienceDecayScalar(amountOfTimesAnalyzed);
            }
        }

        public float BodyScienceScalar
        {
            get
            {
                return GetBodyScienceScalar(Vessel.mainBody);
            }
        }

        public float BodyScienceCap
        {
            get
            {
                return GetBodyScienceCap(Vessel.mainBody);
            }
        }

        [KSPEvent(guiActive = true, guiName = "#LOC_RoverScience_GUI_ToggleTerminal")] // Toggle Rover Terminal
        private void ShowGUI()
        {
            roverScienceGUI.consoleGUI.Toggle();
            drawWaypoint.ToggleMarker();
        }

        [KSPAction("#LOC_RoverScience_GUI_ActivateConsole", actionGroup = KSPActionGroup.None)] // Activate Console
        private void ShowGUIAction(KSPActionParam param)
        {
            if (IsPrimary)
                ShowGUI();
        }

        public void Start()
        {
            Log.Info("RoverScience.Start, vessel: " + this.vessel.id);
            if (HighLogic.LoadedSceneIsFlight)
            {
                drawWaypoint = new DrawWaypoint(this);
                roverScienceGUI = new RoverScienceGUI(this);
                roverScienceDB = new RoverScienceDB(this);
            }
             GameEvents.onHideUI.Add(onHideUI);
            GameEvents.onShowUI.Add(onShowUI);
            GameEvents.onGamePause.Add(onHideUI);
            GameEvents.onGameUnpause.Add(onShowUI);

            InvokeRepeating("UpdateMarkerPositionInvoked", 0.1f, 0.1f);
            if (ROC_Class.SerenityLoaded)
                StartCoroutine(ScheduleClosestROCCheck());
        }

        public void Update()
        {
            if (HighLogic.LoadedSceneIsFlight && drawWaypoint != null)
                drawWaypoint.Update();
        }
        IEnumerator ScheduleClosestROCCheck()
        {
            Log.Info("ScheduleClosestROCCheck");
            while (!ROC_Class.gridDataInitted)
            {
                Log.Info("ScheduleClosestROCCheck 1 sec delay");

                yield return new WaitForSeconds(1f);
            }
            rover.SetClosestROC();
            yield return null;
        }
        
#if true
        /// <summary>
        /// Updates the marker location 10x/sec, called by InvokeRepeatig
        /// </summary>
        void UpdateMarkerPositionInvoked()
        {
            if (HighLogic.LoadedSceneIsFlight && drawWaypoint.InterestingObjectExists)
                drawWaypoint.SetMarkerLocation(rover.scienceSpot.location.longitude, rover.scienceSpot.location.latitude, spawningObject: false, update:true);
        }
#endif

        void OnDestroy()
        {
            Log.Detail("RoverScience OnDestroy()");
            GameEvents.onHideUI.Remove(onHideUI);
            GameEvents.onShowUI.Remove(onShowUI);
            GameEvents.onGamePause.Remove(onHideUI);
            GameEvents.onGameUnpause.Remove(onShowUI);
            if (HighLogic.LoadedSceneIsFlight)
                drawWaypoint.OnDestroy();
        }
        bool hideUI = false;
        void onHideUI()
        {
            hideUI = true;
        }
        void onShowUI()
        {
            hideUI = false;
        }
        public void OnGUI()
        {
            if (!hideUI)
                roverScienceGUI.DrawGUI();
        }

        public override void OnLoad(ConfigNode vesselNode)
        {
            Log.Detail("#X1 RoverScience OnLoad @" + DateTime.Now);
            //Instance = this;

            if (rover == null)
            {
                Log.Detail("rover was null, creating new rover class (OnLoad)");
                rover = new Rover(this);
            }
            if (DB != null) DB.UpdateRoverScience();

        }

        public override void OnSave(ConfigNode vesselNode)
        {
            Log.Detail("RoverScience OnSave @" + DateTime.Now);
            if (DB != null) DB.UpdateDB();
        }

        public override void OnStart(PartModule.StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (IsPrimary)
                {
                    Log.Info("Initiated! Version: " + RSVersion + ", vessel: " + this.vessel.id); 

                    //Instance = this;

                    //Log.Detail("RS Instance set - " + Instance);

                    HomeWorld = FlightGlobals.Bodies.Where(cb => cb.isHomeWorld).First(); // TODO: move this somewhere more appropriate

                    container = part.Modules["ModuleScienceContainer"] as ModuleScienceContainer;
                    command = part.Modules["ModuleCommand"] as ModuleCommand;

                    // HACK: Must be called here otherwise they won't run their constructors for some reason
                    if (rover == null)
                    {
                        Log.Detail("rover was null, creating new rover class (OnStart)");
                        rover = new Rover(this);
                    }
                    rover.scienceSpot = new ScienceSpot(this);
                    rover.landingSpot = new LandingSpot(this);

                    if (rover.scienceSpot.potential != ScienceSpot.Potentials.anomaly)
                        rover.scienceSpot.adjustedPotentialGenerated = AdjustedPotentialGenerated();
                    rover.scienceSpot.PredictSpot(rover.scienceSpot.isAnomaly);
                    Log.Info("rover.scienceSpot.potential: " + rover.scienceSpot.potential + ", rover.scienceSpot.adjustedPotentialGenerated: " + rover.scienceSpot.adjustedPotentialGenerated +
                        ", predictedSpot: " + rover.scienceSpot.predictedSpot);


                    if (DB != null) DB.UpdateRoverScience();

                    rover.SetClosestAnomaly();
                    rover.SetClosestROC();

                }
                else
                {
                    Log.Info("ONSTART - Not primary, vessel: " + this.vessel.id);
                }

                // HACK: instance null unexpected.
#if false
                if (Instance == null)
                {
                    Instance = this;
                    Log.Info("Instance was null; workaround fix by declaring Instance anyway");
                }
#endif
            }

        }

        // roverScience == RoverScience.Instance
        // Rover == RoverScience.Instance.rover

        string AdjustedPotentialGenerated()
        {
            double confidence = CurrentPredictionAccuracy;
            var value = rover.scienceSpot.potentialScience;
            var rnd = rand.NextDouble();

            var c1 = value * (1 - (1 - confidence) * rnd);
            var c2 = (1 - confidence) * (500 * rnd - 250);
            var c3 =  c1 + c2;

            Log.Info("AdjustedPotentialGenerated, confidence: " + confidence + ", value: " + value + ", rnd: " + rnd + ", c3: " + c3);

            if (c3 >= 400)
                return ScienceSpot.GetPotentialString(ScienceSpot.Potentials.vhigh);
            if (c3 >= 200)
                return ScienceSpot.GetPotentialString(ScienceSpot.Potentials.high);
            if (c3 >= 70)
                return ScienceSpot.GetPotentialString(ScienceSpot.Potentials.normal);
            if (c3 >= 30)
                return ScienceSpot.GetPotentialString(ScienceSpot.Potentials.low);
            return ScienceSpot.GetPotentialString(ScienceSpot.Potentials.vlow);

        }

        public override void OnUpdate()
        {
            if (IsPrimary)
            {

                if (roverScienceGUI.consoleGUI.isOpen)
                {
                    // Calculate rover traveled distance
                    if (rover.ValidStatus)
                        rover.CalculateDistanceTraveled(TimeWarp.deltaTime);

                    rover.landingSpot.SetSpot();
                    if (rover.landingSpot.established)
                    {
                        rover.SetRoverLocation();
                    }

                    if ((!rover.scienceSpot.established) && (!rover.ScienceSpotReached) && (ScienceDecayPercentage < 100))
                    {
                        rover.scienceSpot.CheckAndSet();
                    }
                }
            }
            KeyboardShortcuts();
        }

        // Much credit to a.g. as his source helped to figure out how to utilize the experiment and its data
        // https://github.com/angavrilov/ksp-surface-survey/blob/master/SurfaceSurvey.cs#L276
        public void AnalyzeScienceSample()
        {
            if (rover.ScienceSpotReached)
            {

                ScienceExperiment sciExperiment = ResearchAndDevelopment.GetExperiment("RoverScienceExperiment");
                ScienceSubject sciSubject = ResearchAndDevelopment.GetExperimentSubject(sciExperiment, ExperimentSituations.SrfLanded, Vessel.mainBody, "", "");

                // 20 science per data
                sciSubject.subjectValue = 20;
                sciSubject.scienceCap = BodyScienceCap;

                // Divide by 20 to convert to data form
                float sciData = (rover.scienceSpot.potentialScience) / sciSubject.subjectValue;

                Log.Detail("sciData (potential/20): " + sciData);


                // Apply multipliers

                if (rover.AnomalySpotReached)
                {
                    Log.Detail("analyzed science at anomaly");

                    if (!RoverScienceScenario.anomaliesAnalyzed.Contains(rover.closestAnomaly.id))
                    {
                        RoverScienceScenario.anomaliesAnalyzed.Add(rover.closestAnomaly.id);
                        Log.Detail($"added anomaly id: {rover.closestAnomaly.id} to save!");
                    }

                }
                else if (rover.ROCSpotReached)
                {
                    Log.Detail("analyzed science at ROC");

                    if (!RoverScienceScenario.ROCsAnalyzed.Contains(rover.closestROC.id.ToString()))
                    {
                        RoverScienceScenario.ROCsAnalyzed.Add(rover.closestROC.id.ToString());
                        Log.Detail($"added ROC id: {rover.closestROC.id} to save!");
                    }

                }
                else
                {
                    // if a normal spot, we shall apply factors
                    Log.Detail("analyzed science at science spot");
                    sciData = sciData * ScienceDecayScalar * BodyScienceScalar * scienceMaxRadiusBoost;
                }



                Log.Detail("rover.scienceSpot.potentialScience: " + rover.scienceSpot.potentialScience);
                Log.Detail("sciData (post scalar): " + sciData);
                Log.Detail("scienceDecayScalar: " + ScienceDecayScalar);
                Log.Detail("bodyScienceScalar: " + BodyScienceScalar);

                sciData *= HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().scaledScience;

                if (sciData > 0.1)
                {
                    ScienceData sd = StoreScience(container, sciSubject, sciData);
                    if (sd != null)
                    {
                        container.ReviewDataItem(StoreScience(container, sciSubject, sciData));
                        amountOfTimesAnalyzed++;
                        Log.Detail("Science retrieved! - " + sciData);
                    }
                    else
                    {
                        Log.Info("Failed to add science to container!");
                    }
                }
                else
                {

                    ScreenMessages.PostScreenMessage(Localizer.GetStringByTag("#LOC_RoverScience_GUI_ScienceTooLow"), 5, ScreenMessageStyle.UPPER_CENTER); // Science value was too low - deleting data!
                }

                rover.scienceSpot.Reset();

            }
            else
            {
                Log.Detail("Tried to analyze while not at spot?");
            }
        }

        public ScienceData StoreScience(ModuleScienceContainer container, ScienceSubject subject, float data)
        {

            if (container.capacity > 0 && container.GetScienceCount() >= container.capacity)
                return null;

            if (container.GetStoredDataCount() >= container.capacity && container.capacity != 0)
                return null;

            float xmitValue = 0.85f;
            float labBoost = 0.1f;

            ScienceData new_data = new ScienceData(data, xmitValue, labBoost, subject.id, subject.title);

            if (container.AddData(new_data))
                return new_data;


            return null;
        }

        private float GetScienceDecayScalar(int numberOfTimes)
        {
            // This is the equation that models the decay of science per analysis made
            // y = 1.20^(-0.9*(x-2))
            // Always subject to adjustment
            //double scalar = (1.20 * Math.Exp (-0.9 * (numberOfTimes - levelAnalyzedDecay)));

            double scalar = (1.20 * Math.Exp(-0.4 * (numberOfTimes - levelAnalyzedDecay)));
            // decay pattern as such:  0.8, 0.54, 0.36, 0.24, 0.16, 0.1

            if (scalar > 1)
            {
                return 1;
            }

            return (float)scalar;
        }



        private float GetBodyScienceScalar(CelestialBody currentBody)
        {

            if (currentBody.isHomeWorld)
                return HomeWorldScienceScalar;
            if (currentBody == FlightGlobals.Bodies[0])
                return SunScienceScalar;

            if (currentBody.HasParent(HomeWorld))
            {
                return NearMoonScienceScalar; // TODO: Distinguish Mun/Minmus (and generic case)
            }
            return 1;
        }

        private float GetBodyScienceCap(CelestialBody currentBody)
        {
            float scalar = 1;
            float scienceCap = 1500;

            if (currentBody.isHomeWorld)
                scalar = 0.09f;
            else if (currentBody == FlightGlobals.Bodies[0])
                scalar = 0f;
            else if (currentBody.HasParent(HomeWorld))
            {
                scalar = 0.3f;
            }
            else
                scalar = 1f;

            return (scalar * scienceCap);
        }

        public string GetUpgradeName(RSUpgrade upgrade)
        {
            switch (upgrade)
            {
                case (RSUpgrade.maxDistance):
                    return Localizer.GetStringByTag("#LOC_RoverScience_GUI_MaxScanDistance"); //  "Max Scan Distance";
                case (RSUpgrade.predictionAccuracy):
                    return Localizer.GetStringByTag("#LOC_RoverScience_GUI_PrecisionAccuracy"); // "Prediction Accuracy";
                case (RSUpgrade.analyzedDecay):
                    return Localizer.GetStringByTag("#LOC_RoverScience_GUI_DecayLimit"); // "Analyzed Decay Limit";
                default:
                    return Localizer.GetStringByTag("#LOC_RoverScience_GUI_ErrorUpgradeName"); // "Failed to resolve getUpgradeName";
            }

        }

        float[] UpgradeCostDistance = { 0, 200, 250, 400, 550, 1000 };
        float[] UpgradeCostAccuracy = { 0, 200, 400, 500, 1000, 2100 };
        float[] UpgradeCostDecay = { 0, 0, 0, 1000, 1000, 1000 };
        public float GetUpgradeCost(RSUpgrade upgrade, int level)
        {

            //if (level == 0) level = 1;
            level = Math.Max(level, 1);
            if (level > GetUpgradeMaxLevel(upgrade) || level > 5) return -1;

            switch (upgrade)
            {
                case (RSUpgrade.maxDistance):
                    return UpgradeCostDistance[level];
#if false
                    if (level == 1) return 200;
                    if (level == 2) return 250;
                    if (level == 3) return 400;
                    if (level == 4) return 550;
                    if (level == 5) return 1000;

                    return -1;
#endif

                case (RSUpgrade.predictionAccuracy):
                    return UpgradeCostAccuracy[level];
#if false
                    if (level == 1) return 200;
                    if (level == 2) return 400;
                    if (level == 3) return 500;
                    if (level == 4) return 1000;
                    if (level == 5) return 2100;

                    return -1;
#endif

                case (RSUpgrade.analyzedDecay):
                    return UpgradeCostDecay[level];
#if false
                    if (level == 1) return 0;
                    if (level == 2) return 0;
                    if (level == 3) return 1000;
                    if (level == 4) return 1000;
                    if (level == 5) return 1000;

                    return -1;
#endif
                default:
                    return -1;
            }
        }

        public string GetUpgradeValueString(RSUpgrade upgrade, int level, bool nextLevel = false)
        {
            // This will come with unit for display
            switch (upgrade)
            {
                case (RSUpgrade.maxDistance):
                    if (levelMaxDistance >= maximum_levelMaxDistance)
                    {
                        if (nextLevel)
                            return "";
                        return Localizer.GetStringByTag("#LOC_RoverScience_GUI_Max"); // "MAX";
                    }
                    else
                    {
                        return Localizer.Format("#LOC_RoverScience_GUI_Metres", GetUpgradeValue(RSUpgrade.maxDistance, level)); // <<1>>m
                    }

                case (RSUpgrade.predictionAccuracy):
                    if (levelPredictionAccuracy >= maximum_predictionAccuracy)
                    {
                        if (nextLevel)
                            return "";
                        return Localizer.GetStringByTag("#LOC_RoverScience_GUI_Max"); // "MAX";
                    }
                    else
                    {
                        return Localizer.Format("#LOC_RoverScience_GUI_Percentage", GetUpgradeValue(RSUpgrade.predictionAccuracy, level)); // <<1>>%
                    }

                case (RSUpgrade.analyzedDecay):
                    if (levelAnalyzedDecay >= maximum_levelAnalyzedDecay)
                    {
                        return Localizer.GetStringByTag("#LOC_RoverScience_GUI_Max"); // "MAX";
                    }
                    else
                    {
                        return Localizer.Format("#LOC_RoverScience_GUI_nvalue", GetUpgradeValue(RSUpgrade.analyzedDecay, level)); // <<1>>n
                    }

                default:
                    return "Unable to resolve getUpgradeValueString()";
            }
        }

        double[] UpgradeMaxDistance = { -1, 100, 500, 1000, 2000, 4000 };
        double[] UpgradePredictionAccuracy = { -1, 10, 20, 50, 70, 80 };
        double[] UpgradeAnalyzedDecay = { 2, 2, 2, 3, 4, 5 };

        public double GetUpgradeValue(RSUpgrade upgrade, int level)
        {

            //if (level == 0) level = 1;
            level = Math.Max(level, 1);
            if (level > GetUpgradeMaxLevel(upgrade) || level > 5) return -1;

            switch (upgrade)
            {
                case (RSUpgrade.maxDistance):
                    return UpgradeMaxDistance[level] * HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().scaledDetectionDistance;
#if false
                    if (level == 1) return 100;
                    if (level == 2) return 500;
                    if (level == 3) return 1000;
                    if (level == 4) return 2000;
                    if (level == 5) return 4000;

                    return -1;
#endif

                case (RSUpgrade.predictionAccuracy):
                    return UpgradePredictionAccuracy[level];
#if false
                    if (level == 1) return 10;
                    if (level == 2) return 20;
                    if (level == 3) return 50;
                    if (level == 4) return 70;
                    if (level == 5) return 80;

                    return -1;
#endif

                case (RSUpgrade.analyzedDecay):
                    return UpgradeAnalyzedDecay[level];
#if false
                    if (level <= 2) return 2;
                    if (level == 3) return 3;
                    if (level == 4) return 4;
                    if (level == 5) return 5;

                    return -1;
#endif
                default:
                    return -1;
            }
        }

        public int GetUpgradeLevel(RSUpgrade upgradeType)
        {
            switch (upgradeType)
            {
                case (RSUpgrade.maxDistance):
                    return levelMaxDistance;
                case (RSUpgrade.predictionAccuracy):
                    return levelPredictionAccuracy;
                case (RSUpgrade.analyzedDecay):
                    return levelAnalyzedDecay;
                default:
                    return -1;
            }
        }

        public void SetUpgradeLevel(RSUpgrade upgradeType, int newValue)
        {
            if (upgradeType == RSUpgrade.maxDistance)
            {
                levelMaxDistance = newValue;
            }
            else
            if (upgradeType == RSUpgrade.predictionAccuracy)
            {
                levelPredictionAccuracy = newValue;
            }
            else
            if (upgradeType == RSUpgrade.analyzedDecay)
            {
                levelAnalyzedDecay = newValue;
            }
        }

        public int GetUpgradeMaxLevel(RSUpgrade upgradeType)
        {
            switch (upgradeType)
            {
                case (RSUpgrade.maxDistance):
                    return maximum_levelMaxDistance;
                case (RSUpgrade.predictionAccuracy):
                    return maximum_predictionAccuracy;
                case (RSUpgrade.analyzedDecay):
                    return maximum_levelAnalyzedDecay;
                default:
                    return -1;
            }
        }

        public void UpgradeTech(RSUpgrade upgradeType)
        {
            Log.Detail("upgradeTech called: " + upgradeType);
            int nextLevel = GetUpgradeLevel(upgradeType) + 1;
            int currentLevel = GetUpgradeLevel(upgradeType);
            int maxLevel = GetUpgradeMaxLevel(upgradeType);
            float nextCost = GetUpgradeCost(upgradeType, nextLevel);
            string upgradeName = GetUpgradeName(upgradeType);

            // MAX LEVEL REACHED
            if (currentLevel >= maxLevel)
            {
                ScreenMessages.PostScreenMessage(Localizer.GetStringByTag("#LOC_RoverScience_GUI_MaxUpgradeLevel"), 3, ScreenMessageStyle.UPPER_CENTER); // Max Level reached for this upgrade
                return;
            }

            // NOT ENOUGH SCIENCE
            if (nextCost > ResearchAndDevelopment.Instance.Science)
            {
                ScreenMessages.PostScreenMessage(Localizer.GetStringByTag("#LOC_RoverScience_GUI_NotEnoughScience"), 3, ScreenMessageStyle.UPPER_CENTER); // "Not enough science to upgrade"
                return;
            }

            // UPGRADE METHOD
            if (upgradeType == RSUpgrade.maxDistance)
            {
                levelMaxDistance++;
                Log.Detail("Upgraded levelMaxDistance. Now level: " + levelMaxDistance);
            }
            else if (upgradeType == RSUpgrade.predictionAccuracy)
            {
                levelPredictionAccuracy++;
                Log.Detail("Upgraded predictionAccuracy. Now level: " + levelPredictionAccuracy);
            }
            else if (upgradeType == RSUpgrade.analyzedDecay)
            {
                levelAnalyzedDecay++;
                Log.Detail("Upgraded levelAnalyzedDecay. Now level: " + levelAnalyzedDecay);
            }

            ResearchAndDevelopment.Instance.CheatAddScience(-nextCost);

            ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_RoverScience_GUI_UpgradeMessage", upgradeName), 3, ScreenMessageStyle.UPPER_CENTER); // <<1>> has been upgraded"
        }

        public void SetScienceMaxRadiusBoost(int maxRadius)
        {
            //maxRadius' maximum value would only ever reach 2km (2000 meters)
            //this method updates the factor used to increase the science depending
            //on how far a given science spot has been spawned
            if (maxRadius < 150)
                scienceMaxRadiusBoost = 1;

            scienceMaxRadiusBoost = ((1f / 2000f) * maxRadius) + 1f;
        }

        public void KeyboardShortcuts()
        {

            if (HighLogic.LoadedSceneIsFlight)
            {
                // CONSOLE WINDOW
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.R) && Input.GetKeyUp(KeyCode.S))
                {
                    roverScienceGUI.consoleGUI.Toggle();
                    drawWaypoint.ToggleMarker();
                }

                // DEBUG WINDOW
                if (Input.GetKey(KeyCode.RightControl) && Input.GetKeyUp(KeyCode.Keypad5))
                {
                    roverScienceGUI.debugGUI.Toggle();
                }
            }
        }
        // TAKEN FROM KERBAL ENGINEERING REDUX SOURCE by cybutek
        // http://creativecommons.org/licenses/by-nc-sa/3.0/deed.en_GB
        // This is to hopefully prevent multiple instances of this PartModule from running simultaneously
        public bool IsPrimary
        {
            get
            {
                if (this.Vessel != null)
                {
                    for (int p1 = 0; p1 < this.Vessel.parts.Count; p1++)
                    {
                        Part part = this.Vessel.parts[p1];
                    
                        if (part.Modules.Contains(this.ClassID))
                        {
                            if (this.part == part)
                            {
                                return true;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                return false;
            }
        }
    }
}

