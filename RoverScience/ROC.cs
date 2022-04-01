using System;
using KSP.Localization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RoverScience.InitLog;

#if true
namespace RoverScience
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class ROC_Class : MonoBehaviour
    {
        internal class Grid
        {
            string bodyName;
            internal List<Coords> coordList;
            internal double grid_lat;
            internal double grid_long;
            internal SCANROC roc;

            internal Grid(string bodyName, double grid_lat, double grid_long, SCANROC roc)
            {
                coordList = new List<Coords>();
                this.bodyName = bodyName;
                this.grid_lat = grid_lat;
                this.grid_long = grid_long;
                this.roc = roc;
            }

            const int MAXSENSOR = 10; // km

            /// <summary>
            /// Return a list of keys to insert this entry into
            /// </summary>
            /// <param name="body"></param>
            /// <param name="latitude"></param>
            /// <param name="longitude"></param>
            /// <returns></returns>
            internal static void GridKeys(CelestialBody body, double latitude, double longitude, ROC_Class.SCANROC roc)
            {
                List<string> keyList = new List<string>();

                // Get the circumference of the planet in km 
                var circum = body.Radius * 2 * Math.PI / 1000;

                // To reduce the number of grid entries, divide the circum by 3
                //circum /= 3;

                // Convert lat/long into grid coordinates
                //double longCoord = (longitude) * circum * Math.Cos((latitude) * Math.PI / 360) / 360;
                //double latCoord = (latitude) * circum / 360;

                //longCoord *= 10;
                //latCoord *= 10;

                for (int i = -MAXSENSOR; i < MAXSENSOR; i++)
                {
                    for (int j = -MAXSENSOR; j < MAXSENSOR; j++)
                    {
#if false
                        double lat = Math.Floor(latCoord + i);
                        if (lat < -90) lat += 180;
                        if (lat > 90) lat -= 180;
                        double longit = Math.Floor(longCoord + j);
                        if (longit < -180) longit += 360;
                        if (longit > 360) longit -= 360;

                        lat *= 100;
                        longit *= 100;

                        string key = body.bodyName + ":" + lat.ToString("F0") + ":" + longit.ToString("F0");
#else
                        string key = GridKey(body, (latitude + i), (longitude + j));
#endif
                        keyList.Add(key);
                        if (!gridData.ContainsKey(key))
                            gridData.Add(key, new List<Grid>());
                        gridData[key].Add(new Grid(body.bodyName, latitude, longitude, roc));


                    }
                }
            }

            internal static string GridKey(CelestialBody body, double latitude, double longitude)
            {
                var circum = body.Radius * 2 * Math.PI / 1000;

                double longCoord = (longitude) * circum * Math.Cos((latitude) * Math.PI / 360) / 360;
                double latCoord = (latitude) * circum / 360;

                string key = body.bodyName + ":" + latCoord.ToString("F0") + ":" + longCoord.ToString("F0");
                return key;
            }
        }

        IEnumerator BuildGridData(float f)
        {
            yield return new WaitForSeconds(f);

            gridDataInitted = false;
            // clear gridData?
            gridData.Clear();

            foreach (var bodyRoc in bodyRockDict)
            {
                Log.Info("GridData, body: " + FlightGlobals.GetBodyByName(bodyRoc.Key) + ", rocCount: " + bodyRoc.Value.Count);
                foreach (var roc in bodyRoc.Value)
                {
                    Grid.GridKeys(FlightGlobals.GetBodyByName(bodyRoc.Key), roc.Latitude, roc.Longitude, roc);
                    yield return null;
#if false
                    foreach (var s in list)
                    {
                        if (!gridData.ContainsKey(s))
                            gridData.Add(s, new List<Grid>());

                        gridData[s].Add(new Grid(FlightGlobals.GetBodyByName(bodyRoc.Key).bodyName, roc.Latitude, roc.Longitude, roc));
                        if (cnt++ > 1000)
                        {
                            cnt = 0;
                            yield return null; // new WaitForSeconds(0.01f);
                        }
                    }
#endif
                }
            }
            gridDataInitted = true;
            DumpGridData();

            var body = FlightGlobals.ActiveVessel.mainBody;
            var rocDict = ROCS(body, bodyRockDict[body.bodyName], true);

            if (bodyRockDict.ContainsKey(body.name))
                bodyRockDict[body.bodyName] = rocDict;
            else
                bodyRockDict.Add(body.bodyName, rocDict);
        }

        void DumpGridData()
        {
            int cnt = 0, m = 0;
            foreach (var g in gridData)
            {
                if (g.Value.Count > 10)
                    Log.Info("GridData, key: " + g.Key + ", count: " + g.Value.Count);
                cnt += g.Value.Count;
                m = Math.Max(m, g.Value.Count);
            }
            Log.Info("GridData, total count: " + cnt + ", max in a grid: " + m);
        }

        public static Dictionary<string, List<SCANROC>> bodyRockDict = new Dictionary<string, List<SCANROC>>();
        internal static Dictionary<string, List<Grid>> gridData = new Dictionary<string, List<Grid>>();
        internal static bool gridDataInitted = false;

        private static bool? serenityLoaded;


        public void Start()
        {
            Log.Info("ROC.Start");
            //rocDict = null;
            if (!SerenityLoaded)
                return;

            GameEvents.VesselSituation.onLand.Add(this.CallbackOnLand);

            foreach (CelestialBody body in FlightGlobals.Bodies)
            {

                List<SCANROC> rocDict = new List<SCANROC>();
                if (bodyRockDict.ContainsKey(body.bodyName))
                {
                    rocDict = bodyRockDict[body.bodyName];
                }

                rocDict = ROCS(body, rocDict, true);

                if (bodyRockDict.ContainsKey(body.bodyName))
                    bodyRockDict[body.bodyName] = rocDict;
                else
                    bodyRockDict.Add(body.bodyName, rocDict);
                if (rocDict != null)
                {
                    foreach (var roc in rocDict)
                    {
                        var c = roc.location;

                        Log.Info("body: " + body.name + ", ROC id: " + roc.ID + ", name: " + roc.Name + ", Coords: " + c.latitude + ", " + c.longitude);
                    }
                }
            }

            StartCoroutine(BuildGridData(5f));
            InvokeRepeating("CheckForNewROCs", 60f, 60f);
        }



        public static SCANROC ClosestROC(Vessel vessel, List<ROC_Class.SCANROC> closestRocDict)
        {
            Log.Detail("Checking for closest ROC");
            if (!bodyRockDict.ContainsKey(vessel.mainBody.bodyName))
                Log.Info("Body missing from bodyRockDict");
            else
            {
                //List<SCANROC> rocDict;

                //if (bodyRockDict.TryGetValue(vessel.mainBody.bodyName, out rocDict))
                {
                    if (ROC_Class.SerenityLoaded &&  closestRocDict.Count == 0)
                    {
                        Log.Detail("No ROCs found for " + vessel.mainBody.name);
                        return null;
                    }

                    SCANROC closestROC = closestRocDict[0]; // set initial

                    // check and find closest anomaly
                    int i = 0;

                    double distanceCheck;
                    Coords location = new Coords { latitude = vessel.latitude, longitude = vessel.longitude };
                    double distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestROC.location);
                    for (int a1 = 0; a1 < closestRocDict.Count; a1++)
                    {
                        SCANROC roc = closestRocDict[a1];

                        distanceCheck = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, roc.location);

                        if (distanceCheck < distanceClosest)
                        {
                            closestROC = roc;
                            distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestROC.location);
                        }
                        i++;
                    }

                    distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestROC.location);
                    Log.Detail("======= RS: closest ROC details =======");
                    Log.Detail("long/lat: " + closestROC.location.longitude + "/" + closestROC.location.latitude);
                    Log.Detail("instantaneous distance: " + distanceClosest);
                    Log.Detail("id: " + closestROC.id);
                    Log.Detail("name: " + closestROC.name);
                    Log.Detail("=== RS: closest anomaly details <<END>>====");

                    return closestROC;
                }
            }
            Log.Detail("No ROCs found for " + vessel.mainBody.name);
            return null;
        }



        /// <summary>
        /// Find the closest ROC to the vessel. Uses the pregenerated gridList to minimize the number of ROCS to check
        /// </summary>
        /// <param name="vessel"></param>
        /// <param name="range"></param>
        /// <param name="closestROC"></param>
        /// <returns></returns>
        public static List<ROC_Class.SCANROC> ClosestROCs(Vessel vessel, double range, out ROC_Class.SCANROC closestROC)
        {
            Log.Detail("Checking for closest ROCs");
            List<ROC_Class.SCANROC> closestRocs = new List<ROC_Class.SCANROC>();

            Log.Info("ClosestROC, body.bodyName: " + vessel.mainBody.bodyName);

            string key = Grid.GridKey(vessel.mainBody, vessel.latitude, vessel.longitude);
            Log.Info("GridKey for vessel location: " + key);
            if (gridData.TryGetValue(key, out List<Grid> gridList))
            {
                Log.Info("GridData.Count for key: " + key + " = " + gridList.Count);
                Coords location = new Coords { latitude = vessel.latitude, longitude = vessel.longitude };
                closestROC = gridList[0].roc; // set initial
                double distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestROC.location);
                for (int a1 = 0; a1 < gridList.Count; a1++)
                {
                    SCANROC roc = gridList[a1].roc;

                    double distanceCheck = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, roc.location);
                    if (distanceCheck < range)
                        closestRocs.Add(roc);

                    if (distanceCheck < distanceClosest)
                    {
                        closestROC = roc;
                        distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestROC.location);
                    }
                }

#if false
                    if (bodyRockDict.TryGetValue(vessel.mainBody.bodyName, out rocDict))
                    {

                        if (rocDict.Count == 0)
                        {
                            Log.Detail("No ROCs found for " + vessel.mainBody.name);
                            closestROC = null;
                            return null;
                        }

                        closestROC = rocDict[0]; // set initial

                        // check and find closest anomaly
                        int i = 0;

                        double distanceCheck;
                        Coords location = new Coords { latitude = vessel.latitude, longitude = vessel.longitude };
                        double distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestROC.location);
                        for (int a1 = 0; a1 < rocDict.Count; a1++)
                        {
                            SCANROC roc = rocDict[a1];

                            distanceCheck = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, roc.location);
                            if (distanceCheck < range)
                                closestRocs.Add(roc);

                            if (distanceCheck < distanceClosest)
                            {
                                closestROC = roc;
                                distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestROC.location);
                            }
                            i++;
                        }
#endif
                location = new Coords { latitude = vessel.latitude, longitude = vessel.longitude };
                distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestROC.location);

                distanceClosest = GeomLib.GetDistanceBetweenTwoPoints(vessel.mainBody, location, closestROC.location);
                Log.Detail("======= RS: closest ROC details =======");
                Log.Detail("long/lat: " + closestROC.location.longitude + "/" + closestROC.location.latitude);
                Log.Detail("instantaneous distance: " + distanceClosest);
                Log.Detail("id: " + closestROC.id);
                Log.Detail("name: " + closestROC.name);
                Log.Detail("=== RS: closest anomaly details <<END>>====");
            }
            else
            {
                // Fallback to brute force, only if nothing is close
                Log.Detail("No close ROCs found for " + vessel.mainBody.name);

                List<SCANROC> lst = bodyRockDict[vessel.mainBody.bodyName];
                closestROC = ClosestROC(vessel, lst);

                closestRocs.Add(closestROC);

            }
            return closestRocs;
        }

        public static bool HasCurrentROCBeenAnalyzed()
        {
            Rover rover = RoverScience.Instance.rover;
            string closestROCID = rover.closestROC.id.ToString();
            return RoverScienceScenario.ROCsAnalyzed.Contains(closestROCID);
        }


        Vessel lastVessel;
        CelestialBody lastBody;
        Coords lastLocation;

        /// <summary>
        /// Check to see if we should check for new ROCs. 
        /// </summary>
        /// <param name="vsl"></param>
        /// <param name="destbody"></param>
        private void CallbackOnLand(Vessel vsl, CelestialBody destbody)
        {
            double d = 9999;
            if (lastVessel == vsl && lastBody == destbody)
                d = GeomLib.GetDistanceBetweenTwoPoints(destbody, new Coords(vsl.latitude, vsl.longitude), lastLocation);

            if (d > 1000)
            {
                StartCoroutine(BuildGridData(0f));
                lastLocation = new Coords(vsl.latitude, vsl.longitude);
            }
            lastVessel = vsl;
            lastBody = destbody;

        }

        /// <summary>
        /// Checks for new ROCs, runs 1/minute, called by an InvokeRepeating
        /// </summary>
        void CheckForNewROCs()
        {
            if (lastVessel != null && lastVessel.Landed)
            {
                double d = GeomLib.GetDistanceBetweenTwoPoints(lastBody, new Coords(lastVessel.latitude, lastVessel.longitude), lastLocation);

                if (d > 1000)
                {
                    StartCoroutine(BuildGridData(0f));
                    lastLocation = new Coords(lastVessel.latitude, lastVessel.longitude);
                }
            }
        }

        public static bool SerenityLoaded
        {
            get
            {
                if (serenityLoaded == null)
                    serenityLoaded = Expansions.ExpansionsLoader.IsExpansionInstalled("Serenity");
                return (bool)serenityLoaded;
            }
        }

        /// <summary>
        /// Summary info on each ROC
        /// </summary>
        public class SCANROC
        {

            internal ROC roc;
            public Coords location = new Coords();

            internal string name;
            internal int id;
            //private bool known;
            private bool scanned;


            public SCANROC(ROC r, string n, double lon, double lat, /* bool k, */ bool s)
            {
                roc = r;

                id = r.rocID;

                name = n;
                location = new Coords
                {
                    longitude = lon,
                    latitude = lat
                };
                //known = k;
                scanned = s;
            }

            public double Longitude
            {
                get { return location.longitude; }
            }

            public double Latitude
            {
                get { return location.latitude; }
            }

            public string Name
            {
                get { return name; }
            }

            public int ID
            {
                get { return id; }
            }

#if false
            public bool Known
            {
                get { return known; }
            }
#endif

            public bool Scanned
            {
                get { return scanned; }
            }

            public ROC Roc
            {
                get { return roc; }
            }
        }

        public List<SCANROC> ROCS(CelestialBody body, List<SCANROC> rocDict, bool refresh)
        {
            if (rocDict == null)
                rocDict = new List<SCANROC>();

            Log.Info("body: " + body.displayName);
            if (ROCManager.Instance == null)
            {
                Log.Info("ROCManager.Instance is null");
                return rocDict;
            }
            if (!ROCManager.Instance.RocsEnabledInCurrentGame)
            {
                Log.Info("ROCManager.Instance.RocsEnabledInCurrentGame is false");
                return rocDict;
            }
            if (!SerenityLoaded)
            {
                Log.Info("SerenityLoaded is false");
                return rocDict;
            }


            if (!refresh && rocDict.Count > 0)
            {
                Log.Info("!refresh && rocDict.Count > 0");
                return rocDict;
            }
            //rocDict.Clear();

            if (body == null)
            {
                Log.Info("body == null");
                return rocDict;
            }
            PQS controller = body.pqsController;

            if (controller == null || controller.transform == null)
            {
                Log.Info("controller == null || controller.transform == null");
                return rocDict;
            }

            for (int i = controller.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = controller.transform.GetChild(i);

                if (child == null)
                    continue;
                Log.Info("ROC name before processing: " + child.name);

                //if (child.name.StartsWith("ROC"))
                {

                    int index = child.name.IndexOf(' ');

                    if (index > 0 && index < child.name.Length - 1)
                    {
                        string id = child.name.Substring(index + 1);

                        bool scanned = false;

                        // Check to see if this ROC has been scanned yet (anywhere, not just this one)
                        if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX || HighLogic.CurrentGame.Mode == Game.Modes.MISSION)
                        {
                            scanned = true;
                        }
                        else
                        {
                            List<ScienceSubject> subjects = ResearchAndDevelopment.GetSubjects();

                            if (subjects != null)
                            {
                                for (int k = subjects.Count - 1; k >= 0; k--)
                                {
                                    if (subjects[k].id.Contains(id))
                                    {
                                        scanned = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (child.childCount == 0)
                        {
                            Component[] components = child.GetComponents(typeof(Component));

                            Log.Info("ROC name: " + child.name);

                            var c = child.GetComponent<Transform>();
                            if (c != null)
                            {
                                double lon = body.GetLongitude(c.transform.position);
                                double lat = body.GetLatitude(c.transform.position);

                                Log.Info("ROC name: " + child.name + ", lat.lon: " + lat + "." + lon);
                                bool found = false;
                                foreach (var r in rocDict)
                                {
                                    if (r.roc.rocID == child.GetInstanceID())
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    var r = new ROC();
                                    r.rocID = child.GetInstanceID();
                                    rocDict.Add(new SCANROC(r
                                        , child.name
                                        , lon
                                        , lat
                                        //, /*SCANUtil.isCovered(lon, lat, this, SCANtype.Anomaly) &&*/ SCANUtil.isCovered(lon, lat, this, SCANtype.AnomalyDetail)
                                        , scanned));
                                }
                            }
                        }
                        else
                            for (int j = child.childCount - 1; j >= 0; j--)
                            {
                                Transform cache = child.GetChild(j);

                                if (cache == null)
                                    continue;

                                if (cache.name != ("Unassigned"))
                                {
                                    ROC roc = cache.GetComponentInChildren<ROC>();

                                    if (roc != null)
                                    {
                                        //if (!roc.smallROC && !roc.canbetaken)
                                        {
                                            if (roc.transform != null)
                                            {
                                                double lon = body.GetLongitude(roc.transform.position);
                                                double lat = body.GetLatitude(roc.transform.position);

                                                Log.Info("ROC name: " + child.name + ", lat:lon: " + lat + ":" + lon);

                                                bool found = false;
                                                foreach (var r in rocDict)
                                                {
                                                    if (r.roc.rocID == child.GetInstanceID())
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                }
                                                if (!found)
                                                    rocDict.Add(new SCANROC(roc
                                                        , roc.displayName
                                                        , lon
                                                        , lat
                                                        //, /*SCANUtil.isCovered(lon, lat, this, SCANtype.Anomaly) &&*/ SCANUtil.isCovered(lon, lat, this, SCANtype.AnomalyDetail)
                                                        , scanned));
                                            }
                                        }
                                    }
                                }
                            }
                    }
                }
            }
            return rocDict;
        }
    }
}

#endif