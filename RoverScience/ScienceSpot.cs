using System;
using KSP.Localization;
using static RoverScience.InitLog;

namespace RoverScience
{
    public class ScienceSpot
    {
        public int cnt;
        static int globalCnt = 0;

        System.Random rand = new System.Random();
        public Coords location = new Coords();

        public int potentialScience;
        public int randomRadius = 0;

        // The spot's actual potential name
        public string potentialGenerated = "";
        public string adjustedPotentialGenerated = "";

        public Potentials potential;

        public bool isAnomaly = false;

        // This is what will be shown as the prediction
        public string predictedSpot = "";
        private static string[] potentialStrings = new string[] {
            // "Very High!", "High", "Normal", "Low", "Very Low!"
            Localizer.GetStringByTag("#LOC_RoverScience_GUI_Potential1"),
            Localizer.GetStringByTag("#LOC_RoverScience_GUI_Potential2"),
            Localizer.GetStringByTag("#LOC_RoverScience_GUI_Potential3"),
            Localizer.GetStringByTag("#LOC_RoverScience_GUI_Potential4"),
            Localizer.GetStringByTag("#LOC_RoverScience_GUI_Potential5")
        };



        public bool established = false;
        RoverScience roverScience = null;

        public int minDistance = 5;
        public int minDistanceDefault = 5;

        public ScienceSpot(RoverScience roverScience)
        {
            this.roverScience = roverScience;
            globalCnt++;
            cnt = globalCnt;
            Log.Info("New ScienceSpot.cnt: " + cnt);
        }

        public Rover Rover
        {
            get
            {
                return roverScience.rover;
            }
        }

        public RoverScienceGUI RoverScienceGUI
        {
            get
            {
                return roverScience.roverScienceGUI;
            }
        }

        Vessel Vessel
        {
            get
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    return FlightGlobals.ActiveVessel;
                }
                else
                {
                    Log.Detail("ScienceSpot.Vessel null - not flight!");
                    return null;
                }
            }

        }

        public enum Potentials
        {
            vhigh, high, normal, low, vlow, anomaly
        }

        public static string GetPotentialString(Potentials potential)
        {
            switch (potential)
            {
                case (Potentials.vhigh):
                    return potentialStrings[0];
                case (Potentials.high):
                    return potentialStrings[1];
                case (Potentials.normal):
                    return potentialStrings[2];
                case (Potentials.low):
                    return potentialStrings[3];
                case (Potentials.vlow):
                    return potentialStrings[4];
            }
            return "Potential string unresolved";
        }

        public void GenerateScience(bool anomaly = false)
        {
            Log.Detail("generateScience()");

            // anomaly flag will set a high science value
            if (anomaly)
            {
                potential = Potentials.anomaly;
                adjustedPotentialGenerated = "anomaly"; // (doesn't really matter what we write here, as it's superceded by predictScience()

                if (rand.Next(0, 100) < 1)
                {
                    potentialScience = 500;
                    return;
                }
                else
                {
                    potentialScience = 300;
                    return;
                }
            }

            if (rand.Next(0, 100) < 1)
            {
                potential = Potentials.vhigh;
                potentialGenerated = GetPotentialString(potential);
                potentialScience = rand.Next(400, 500);
                return;
            }

            if (rand.Next(0, 100) < 8)
            {
                potential = Potentials.high;
                potentialGenerated = GetPotentialString(potential);
                potentialScience = rand.Next(200, 400);
                return;
            }

            if (rand.Next(0, 100) < 65)
            {
                potential = Potentials.normal;
                potentialGenerated = GetPotentialString(potential);
                potentialScience = rand.Next(70, 200);
                return;
            }

            if (rand.Next(0, 100) < 70)
            {
                potential = Potentials.low;
                potentialGenerated = GetPotentialString(potential);
                potentialScience = rand.Next(30, 70);
                return;
            }
            potential = Potentials.vlow;
            potentialGenerated = GetPotentialString(potential);
            potentialScience = rand.Next(0, 30);


        }

        // This handles what happens after the distance traveled passes the distance roll
        // If the roll is successful establish a science spot
        public void CheckAndSet()
        {
            int currentLevel = roverScience.GetUpgradeLevel(RSUpgrade.maxDistance);
            var maxDistance = roverScience.GetUpgradeValue(RSUpgrade.maxDistance, currentLevel);

            // Once distance traveled passes the random check distance
            if ((Rover.DistanceToClosestAnomaly <= maxDistance) && (!Anomalies.Instance.HasCurrentAnomalyBeenAnalyzed(roverScience)))
            {
                SetLocation(3, Rover.closestAnomaly.location.longitude, Rover.closestAnomaly.location.latitude, anomaly: true);
                Rover.ResetDistanceTraveled();
            }
            else if (ROC_Class.SerenityLoaded && (Rover.DistanceToClosestROC <= maxDistance) && (!ROC_Class.HasCurrentROCBeenAnalyzed(roverScience)))
            {
                SetLocation(4, Rover.closestROC.location.longitude, Rover.closestROC.location.latitude, anomaly: true);
                Rover.ResetDistanceTraveled();
            }            
            else if (Rover.distanceTraveled >= HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().minDistanceBetweenData * Rover.distanceCheck)
            {   

                Rover.ResetDistanceTraveled( Rover.distanceTraveled /2f);

                RoverScienceGUI.AddRandomConsoleJunk();

                // Reroll distanceCheck value
                Rover.distanceCheck = rand.NextDouble() +0.5f;

                // farther you are from established site the higher the chance of striking science!

                int rNum = rand.Next(1, 100) - Rover.distanceCheckCnt *10;
                Rover.distanceCheckCnt++;
#if false
            double dist = Rover.DistanceFromLandingSpot;

                // chanceAlgorithm will be modelled on y = 7 * sqrt(x) with y as chance and x as distance from landingspot

                double chanceAlgorithm = (7 * Math.Sqrt(dist));


                double chance = (chanceAlgorithm < 75) ? chanceAlgorithm : 75;

                Log.Detail("rNum: " + rNum);
                Log.Detail("chance: " + chance);
                Log.Detail("rNum <= chance: " + ((double)rNum <= chance));
#endif
                // rNum is a random number between 0 and 100
                // chance is the percentage number we check for to determine a successful roll
                // higher chance == higher success roll chance
                if ((double)rNum <= 25) //chance)
                {
                    Rover.ResetDistanceTraveled();
                    SetLocation(5, random: true);
                    Log.Detail("setLocation");

                    RoverScienceGUI.ClearConsole();

                    Log.Detail("Distance from spot is: " + Rover.DistanceFromScienceSpot);
                    Log.Detail("Bearing is: " + Rover.BearingToScienceSpot);
                    Log.Detail("Something found");

                }
                else
                {
                    // Science hotspot not found
                    Log.Detail("Science hotspot not found!");
                }


            }

        }

        public Coords GenerateRandomLocation(int minRadius, int maxRadius)
        {
            Coords randomSpot = new Coords();

            randomRadius = (int)((rand.NextDouble() * (maxRadius - minRadius) + minRadius) * HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().scaledScienceOccurrance);


            //                minRadius* HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().scaledScienceOccurrance, maxRadius* HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().scaledScienceOccurrance);
            roverScience.SetScienceMaxRadiusBoost(randomRadius);


            double bodyRadius = Vessel.mainBody.Radius;
            double randomAngle = rand.NextDouble() * (double)(1.9);
            double randomTheta = (randomAngle * (Math.PI));
            double angularDistance = randomRadius / bodyRadius;
            double currentLatitude = Vessel.latitude.ToRadians();
            double currentLongitude = Vessel.longitude.ToRadians();

            double spotLat = Math.Asin(Math.Sin(currentLatitude) * Math.Cos(angularDistance) +
                Math.Cos(currentLatitude) * Math.Sin(angularDistance) * Math.Cos(randomTheta));

            double spotLon = currentLongitude + Math.Atan2(Math.Sin(randomTheta) * Math.Sin(angularDistance) * Math.Cos(currentLatitude),
                Math.Cos(angularDistance) - Math.Sin(currentLatitude) * Math.Sin(spotLat));

            randomSpot.latitude = spotLat.ToDegrees();
            randomSpot.longitude = spotLon.ToDegrees();

            return randomSpot;
        }

        // Method to set location
        public void SetLocation(int from, double longitude = 0, double latitude = 0, bool random = false, bool anomaly = false)
        {
            // generate random radius for where to spot placement
            // bool random will override whatever is entered
            Log.Info("SetLocation, from: " + from);
            if (!random)
            {
                location.latitude = latitude;
                location.longitude = longitude;
            }
            else
            {
                Coords randomSpot = GenerateRandomLocation(Rover.minRadius, Rover.maxRadius);
                location.latitude = randomSpot.latitude;
                location.longitude = randomSpot.longitude;
            }

            established = true;

            this.GenerateScience(anomaly);
            isAnomaly = anomaly;
            //PredictSpot(anomaly);

            Log.Info("potentialScience: " + potentialScience);

            Rover.distanceTraveledTotal = 0;
#if false
            Log.Info("== setLocation() ==");
            Log.Info("randomAngle: " + Math.Round(randomAngle, 4));
            Log.Info("randomTheta (radians): " + Math.Round(randomTheta, 4));
            Log.Info("randomTheta (degrees?): " + Math.Round((randomTheta.ToDegrees()), 4));
            Log.Info(" ");
            Log.Info("randomRadius selected: " + randomRadius);
            Log.Info("distance to ScienceSpot: " + rover.distanceFromScienceSpot);

            Log.Info("lat/long: " + location.latitude + " " + location.longitude);
            Log.Info("==================");
#endif
            roverScience.drawWaypoint.SetMarkerLocation(location.longitude, location.latitude, spawningObject: !anomaly);
            roverScience.drawWaypoint.ShowMarker();
        }



        public void PredictSpot(bool anomaly = false)
        {
            double predictionAccuracyChance = roverScience.CurrentPredictionAccuracy;

            int rNum = rand.Next(0, 100);

            if (anomaly)
            {
                predictedSpot = Localizer.GetStringByTag("#LOC_RoverScience_GUI_Anomaly3"); // "Anomaly detected! There is something very interesting nearby...";
            }
            else
            {
                predictedSpot = adjustedPotentialGenerated; // (full confidence)
            }
#if false
            if (rNum < predictionAccuracyChance)
            {
                predictedSpot = adjustedPotentialGenerated; // (full confidence)
            }
            else
            {
                // Select a random name if the roll was not successful
                // This is to possibly mislead the player
                predictedSpot = potentialStrings[rand.Next(0, potentialStrings.Length)];
            }
#endif

            Log.Detail("Spot prediction attempted!");
        }

        public void Reset()
        {
            established = false;
            potentialScience = 0;
            location.longitude = 0;
            location.latitude = 0;

            Rover.ResetDistanceTraveled();
            Rover.distanceTraveledTotal = 0;

            roverScience.drawWaypoint.HideMarker();
            roverScience.drawWaypoint.DestroyInterestingObject();

        }
    }



}


