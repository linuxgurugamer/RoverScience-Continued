using System;
using System.Collections.Generic;
using UnityEngine;
using static RoverScience.InitLog;

namespace RoverScience
{
    [KSPAddon(KSPAddon.Startup.Flight, true)]
    public class Anomalies : MonoBehaviour
    {
        // public static CelestialBody HomeWorld = 
        public static Anomalies Instance = null;
        public static Dictionary<string, List<Anomaly>> anomaliesDict = new Dictionary<string, List<Anomaly>>();

        public class Anomaly
        {
            public string name = "anomaly";
            public Coords location = new Coords();
            //public double longitude = 0;
            //public double latitude = 0;
            public string id = "NA";
            // surface altitude is determined by DrawWaypoint
        }

        //public Anomalies()
        public void Start()
        {
            Instance = this;
            LoadAnomalies();
        }

        public bool HasCurrentAnomalyBeenAnalyzed()
        {
            Rover rover = RoverScience.Instance.rover;
            string closestAnomalyID = rover.closestAnomaly.id;
            if (RoverScienceScenario.anomaliesAnalyzed.Contains(closestAnomalyID))
            {
                return true;
            } else
            {
                return false;
            }

        }

        public List<Anomaly> GetAnomalies(string bodyName)
        {
            if (anomaliesDict.ContainsKey(bodyName))
            {
                return anomaliesDict[bodyName];
            } else
            {
                Log.Detail($"No anomalies for body {bodyName}");
                return new List<Anomaly>();
            }
        }

        public bool HasAnomalies(string bodyName)
        {
            if (anomaliesDict.ContainsKey(bodyName))
            {
                return true;
            } else
            {
                return false;
            }
        }

        private void LoadAnomalies()
        {
            // anomaliesDict contains a list of [Anomaly]s that each contain longitude/latitude, name and id
            try
            {
                Log.Info("Reading anomalies from game database");
                var bodies = FlightGlobals.Bodies;
                int counter;
                for (int b1 =0; b1 < bodies.Count; b1++)
                {
                    var body = bodies[b1];
                
                    var anomalies = new List<Anomaly>();
                    PQSSurfaceObject[] sites = body.pqsSurfaceObjects;
                    counter = 0;
                    for (int s1 =0; s1 < sites.Length; s1++)
                    {
                        var site = sites[s1];
                    
                        anomalies.Add( new Anomaly {
                            id = (++counter).ToString(),
                            name = site.name,
                            location = new Coords
                            {
                                longitude = body.GetLongitude(site.transform.position),
                                latitude = body.GetLatitude(site.transform.position)
                            }
                        });
                    }
                    if (anomalies.Count > 0)
                    {
                        anomaliesDict.Add(body.name, anomalies);
                        Log.Info($"Added {anomalies.Count}  anomalies for body '{body.name}'");
                    }
                }

            }
            catch (Exception e)
            {
                Log.Info($"Exception: anomaly initialisation problem {e.Message}");
                Log.Info(e.StackTrace);
            }
        }

        public Anomaly ClosestAnomaly(Vessel vessel, string bodyName)
        {
            Log.Detail("Checking for closest anomaly");
            if (Anomalies.Instance.HasAnomalies(bodyName))
            {
                var anomaliesList = Anomalies.Instance.GetAnomalies(bodyName);

                var closestAnomaly = anomaliesList[0]; // set initial

                // check and find closest anomaly
                int i = 0;
                double distanceClosest;
                double distanceCheck;
                Coords location = new Coords { latitude = vessel.latitude, longitude = vessel.longitude };
                for (int a1 = 0; a1 < anomaliesList.Count; a1++)
                {
                    Anomaly anomaly = anomaliesList[a1];
                
                    distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestAnomaly.location);
                    distanceCheck = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, anomaly.location);

                    if (distanceCheck < distanceClosest)
                    {
                        closestAnomaly = anomaly;
                    }
                    i++;
                }

                distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestAnomaly.location);
                Log.Detail("======= RS: closest anomaly details =======");
                Log.Detail("long/lat: " + closestAnomaly.location.longitude + "/" + closestAnomaly.location.latitude);
                Log.Detail("instantaneous distance: " + distanceClosest);
                Log.Detail("id: " + closestAnomaly.id);
                Log.Detail("name: " + closestAnomaly.name);
                Log.Detail("=== RS: closest anomaly details <<END>>====");

                return closestAnomaly;
            }
            Log.Detail("No anomalies found for " + bodyName);
            return new Anomaly();
        }
    }
}
