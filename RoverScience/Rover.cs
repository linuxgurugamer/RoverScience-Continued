using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RoverScience.InitLog;


namespace RoverScience
{

    public class Rover
    {

        public System.Random rand = new System.Random();

        public ScienceSpot scienceSpot;
        public LandingSpot landingSpot;

        public Coords location = new Coords();
        public double distanceTraveled = 0;
        public double distanceCheck = 1f;
        public int distanceCheckCnt = 0;
        public double distanceTraveledTotal = 0;

        public int minRadius = 40;
        public int maxRadius = 100;

       // public List<string> anomaliesAnalyzed = new List<string>();
        //public List<int> ROCsAnalyzed = new List<int>();

        public Anomalies.Anomaly closestAnomaly = new Anomalies.Anomaly();
        public ROC_Class.SCANROC closestROC = null;
        public List<ROC_Class.SCANROC> closestUnanalyzedROCs = new List<ROC_Class.SCANROC>();
        const double CLOSE_RANGE = 10000f;


        public double DistanceFromLandingSpot
            => GeomLib.GetDistanceBetweenTwoPoints(Vessel.mainBody, location, landingSpot.location);

        double distFromScienceSpot(CelestialBody body, Coords location, Coords scienceSpotLocation, ScienceSpot scienceSpot)
        {
            //Log.Info("DistanceFromScienceSpot, location: " + scienceSpotLocation.latitude + ":" + scienceSpotLocation.longitude +
            //    ", scienceSpotLocation: " + location.latitude + ":" + location.longitude +  ", cnt: " + scienceSpot.cnt);
            return GeomLib.GetDistanceBetweenTwoPoints(body, location, scienceSpotLocation);
        }
        public double DistanceFromScienceSpot
            => distFromScienceSpot(Vessel.mainBody, location, scienceSpot.location, scienceSpot);

        //        public double DistanceFromScienceSpot
        //            => GeomLib.GetDistanceBetweenTwoPoints(Vessel.mainBody, location, scienceSpot.location);

        public double BearingToScienceSpot
            => GeomLib.GetBearingFromCoords(scienceSpot.location, location);

        public double BearingToROC
            => GeomLib.GetBearingFromCoords(closestROC.location, location);

        Vessel Vessel => FlightGlobals.ActiveVessel;

        public double Heading => GeomLib.GetRoverHeading(Vessel);

        public bool ScienceSpotReached
            => (scienceSpot.established && DistanceFromScienceSpot <= scienceSpot.minDistance);

        public bool AnomalySpotReached
            => (scienceSpot.established && DistanceToClosestAnomaly <= scienceSpot.minDistance);

        public bool AnomalyPresent
            => ((DistanceToClosestAnomaly <= 100) && !Anomalies.Instance.HasCurrentAnomalyBeenAnalyzed());

        public bool ROCSpotReached
    => (scienceSpot.established && DistanceToClosestROC <= scienceSpot.minDistance);

        public bool ROCPresent
            => ((DistanceToClosestROC <= 100) && !ROC_Class.HasCurrentROCBeenAnalyzed());



        public int NumberWheelsLanded => GetWheelsLanded();

        public int NumberWheels => GetWheelCount();

        public bool ValidStatus => CheckRoverValidStatus();

        public double DistanceToClosestAnomaly
        {
            get
            {
                if (location == null)
                    Log.Detail("location == null in DistanceToClosestAnomaly");
                if (closestAnomaly == null)
                    Log.Detail("closestAnomaly == null in DistanceToClosestAnomaly");
                if (Vessel == null)
                    Log.Detail("Vessel == null in DistanceToClosestAnomaly");

                return GeomLib.GetDistanceBetweenTwoPoints(Vessel.mainBody, location, closestAnomaly.location);
            }
        }

        public double DistanceToClosestROC
        {
            get
            {
                if (location == null)
                    Log.Detail("location == null in DistanceToClosestROC");
                if (closestROC == null)
                    Log.Detail("closestROC == null in DistanceToClosestROC");
                if (Vessel == null)
                    Log.Detail("Vessel == null in DistanceToClosestROC");
                if (closestROC == null)
                    return 999999f;
                return GeomLib.GetDistanceBetweenTwoPoints(Vessel.mainBody, location, closestROC.location);
            }
        }

        public void CalculateDistanceTraveled(double deltaTime)
        {
            distanceTraveled += (RoverScience.Instance.vessel.srfSpeed) * deltaTime;
            if (!scienceSpot.established) distanceTraveledTotal += (RoverScience.Instance.vessel.srfSpeed) * deltaTime;
        }

        public void SetRoverLocation()
        {
            location.latitude = Vessel.latitude;
            location.longitude = Vessel.longitude;
        }

        public void ResetDistanceTraveled(double value = 0)
        {
            Log.Info("ResetDistanceTraveled");
            distanceTraveled =value;
            if (value == 0)
                distanceCheckCnt = 0;
        }

        private bool CheckRoverValidStatus()
        {
            // Checks if rover is landed with at least one wheel on with no time-warp.
            return ((TimeWarp.CurrentRate == 1) && (Vessel.horizontalSrfSpeed > (double)0.01) && 
                (!HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().requireWheelTouching || NumberWheelsLanded > 0));
        }

        private int GetWheelCount()
        {
            int wheelCount = 0;

            List<Part> vesselParts = FlightGlobals.ActiveVessel.Parts;
            for (int p1 = 0; p1 < vesselParts.Count; p1++)
            {
                Part part = vesselParts[p1];
            
                for (int m1 = 0; m1 < part.Modules.Count; m1++)
                {
                    PartModule module = part.Modules[m1];
                    if ((module.moduleName.IndexOf("wheel", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        wheelCount++;
                    }
                }
            }
            return wheelCount;
        }


        private int GetWheelsLanded()
        {
            int count = 0;
            List<Part> vesselParts = FlightGlobals.ActiveVessel.Parts;
            for (int p1 = 0; p1 < vesselParts.Count; p1++)
            {
                Part part = vesselParts[p1];     
                
                for (int m1 = 0; m1 < part.Modules.Count; m1++)
                {
                    PartModule module = part.Modules[m1];

                    if (module.moduleName.IndexOf("wheel", StringComparison.OrdinalIgnoreCase) >= 0 && part.GroundContact)
                    {
                            count++;
                    }
                }
            }
            return count;
        }

        //List<Anomalies.Anomaly> anomaliesList = new List<Anomalies.Anomaly>();

        public void SetClosestAnomaly()
        {
            // this is run on establishing landing spot (to avoid expensive constant foreach loops

            SetRoverLocation(); // (update rover location)
            closestAnomaly = Anomalies.Instance.ClosestAnomaly(Vessel, Vessel.mainBody.bodyName);
        }

        public void SetClosestROC()
        {
            // this is run on establishing landing spot (to avoid expensive constant foreach loops

            SetRoverLocation(); // (update rover location)
            closestUnanalyzedROCs = ROC_Class.ClosestROCs(Vessel, CLOSE_RANGE, out closestROC);
             //closestROC = ROC_Class.ClosestROC(Vessel, closestROC);
       }


    }

}

