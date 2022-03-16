using System;
using UnityEngine;
using static RoverScience.InitLog;

namespace RoverScience
{
    // All code taken from Waypoint Manager mod
    // Still in experimental as I try to figure out what I'm doing here
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class DrawWaypoint : MonoBehaviour
    {
        System.Random rand = new System.Random();

        private GameObject marker = null;
        private MeshRenderer markerRenderer;
        private GameObject interestingObject;

        float markerSize = 30;
        float markerSizeMax = 30;

        float markerAlpha = 0.4f;
        float maxAlpha = 0.4f;
        float minAlpha = 0.05f;

        public static DrawWaypoint Instance = null;
        Color markerRed= Color.red;
        Color markerGreen = Color.green;

        string[] rockObjectNames = {"rock", "rock2"};

        private void Start()
        {
            Log.Detail("Attempting to create scienceSpot sphere");
            Instance = this;


            marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //marker.transform.parent = FlightGlobals.ActiveVessel.mainBody.transform;

            Destroy(marker.GetComponent("SphereCollider"));
            // Set initial position
            //marker.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
            marker.transform.localScale = new Vector3(markerSize, markerSize, markerSize);

            marker.transform.position = FlightGlobals.currentMainBody.GetWorldSurfacePosition(0, 0, 0);
            markerRenderer = marker.GetComponent<MeshRenderer>();
            HideMarker(); // do not render marker yet

            // Set marker material, color and alpha
            markerRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));

            markerRed.a = markerAlpha; // max alpha
            markerGreen.a = markerAlpha; // max alpha

            markerRenderer.material.color = markerRed; // set to red on awake
            Log.Detail("Reached end of marker creation");
        }

        public void DestroyInterestingObject()
        {
            if (interestingObject != null) Destroy(interestingObject);
        }

        public void SetMarkerLocation(double longitude, double latitude, bool spawningObject = true, bool update=false)
        {
            DestroyInterestingObject();

            Vector3 bottomPoint = FlightGlobals.currentMainBody.GetWorldSurfacePosition(latitude, longitude, 0);
            Vector3 topPoint = FlightGlobals.currentMainBody.GetWorldSurfacePosition(latitude, longitude, 1000);

            double surfaceAltitude = GetSurfaceAltitude(longitude, latitude);
            Log.Detail("Drawing marker @ (long/lat/alt): " + longitude.ToString() + " " + latitude.ToString() + " " + surfaceAltitude.ToString());
            marker.transform.position = FlightGlobals.currentMainBody.GetWorldSurfacePosition(latitude, longitude, surfaceAltitude);
            Log.Info("SetMarkerLocation: latitude: " + latitude + ", longitude: " + longitude + ", surfaceAltitude: " + surfaceAltitude);
            //marker.transform.up = cylinderDirectionUp;

            if (!update)
            {
                marker.transform.localScale = new Vector3(markerSizeMax, markerSizeMax, markerSizeMax);
                markerRed.a = maxAlpha;

                //attempt to get raycast surface altitude

                if (spawningObject) SpawnObject(longitude, latitude);
            }
        }

        private Vector3 GetUpDown(double longitude, double latitude, bool up = true)
        {
            double altitude = 20000;

            Vector3d topPoint = FlightGlobals.currentMainBody.GetWorldSurfacePosition(latitude, longitude, altitude);
            Vector3d bottomPoint = FlightGlobals.currentMainBody.GetWorldSurfacePosition(latitude, longitude, -altitude);

            if (up)
            {
                return topPoint - bottomPoint;
            } else {
                return bottomPoint - topPoint;
            }

        }

        public double GetSurfaceAltitude(double longitude, double latitude)
        {
            double altitude = 20000;
            Vector3d topPoint = FlightGlobals.currentMainBody.GetWorldSurfacePosition(latitude, longitude, altitude);
            Vector3d bottomPoint = FlightGlobals.currentMainBody.GetWorldSurfacePosition(latitude, longitude, -altitude);

            if (Physics.Raycast(topPoint, (bottomPoint - topPoint), out RaycastHit hit, Mathf.Infinity, 1 << 15))
            {
                return (altitude - hit.distance);
            } else
            {
                Log.Detail("No surface intersect detected!");
            }

            return -1;
        }

        public void ShowMarker()
        {
            if (RoverScience.Instance.rover.scienceSpot.established)
            {
                markerRenderer.enabled = true;
            }
        }

        public void HideMarker()
        {
            markerRenderer.enabled = false;
        }

        public void ToggleMarker()
        {

            if (!markerRenderer.enabled)
            {
                ShowMarker();
            } else
            {
                HideMarker();
            }
        }

        private void ChangeSpherewithDistance(Rover rover)
        {

            float distanceToRover = (float)rover.DistanceFromScienceSpot;

            // distance to rover 10
            // min distance 3
            // +2 = 5
            // will keep reducing size as long as distance is over 5
            if ((distanceToRover < markerSizeMax) && ((distanceToRover > (rover.scienceSpot.minDistance+4))))
            {
                // Reduce sphere size with proximity
                markerSize = distanceToRover;
                marker.transform.localScale = new Vector3(markerSize, markerSize, markerSize);

                // Reduce alpha with proximity
                markerAlpha = (float)(distanceToRover / markerSizeMax);
                if (markerAlpha >= maxAlpha)
                {
                    markerAlpha = maxAlpha;
                } else if (markerAlpha <= minAlpha)
                {
                    markerAlpha = minAlpha;
                }

                markerRed.a = markerAlpha;
                markerGreen.a = markerAlpha;

            }



            if ((distanceToRover <= (rover.scienceSpot.minDistance)) && (distanceToRover >= 0))
            {
                markerRenderer.material.color = markerGreen;
            } else {
                markerRenderer.material.color = markerRed;
            }

        }


        public void SpawnObject(double longitude, double latitude)
        {
            try
            {
                Log.Detail("Attempting to spawn object");
                string randomRockName = rockObjectNames[rand.Next(rockObjectNames.Length)];
                GameObject test = GameDatabase.Instance.GetModel("RoverScience/rock/" + randomRockName);
                Log.Detail("Random rock name: " + randomRockName);
                test.SetActive(true);

                interestingObject = GameObject.Instantiate(test) as GameObject;
                GameObject.Destroy(test);

                GameObject.Destroy(interestingObject.GetComponent("MeshCollider"));
                double srfAlt = DrawWaypoint.Instance.GetSurfaceAltitude(longitude, latitude);
                interestingObject.transform.position = FlightGlobals.currentMainBody.GetWorldSurfacePosition(latitude, longitude, srfAlt);
                Log.Info("SpawnObject: latitude: " + latitude + ", longitude: " + longitude + ", surfaceAltitude: " + srfAlt);

                interestingObject.transform.up = GetUpDown(longitude, latitude, true);
            } catch (Exception e)
            {
                Log.Info($"Exception: Spawn object failed: {e.Message}");
                Log.Info(e.StackTrace);
            }
        }
        
        private void Update()
        {
            if (markerRenderer.enabled)
            {
                ChangeSpherewithDistance(RoverScience.Instance.rover);
            }
        }

        private void OnDestroy()
        {
            Destroy(marker);
            DestroyInterestingObject();
        }
    }
}
