using System;
using KSP.Localization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RoverScience.InitLog;

#if false
namespace RoverScience
{

    public class ROC_Class
    {
        private List<SCANROC> rocs;

        private bool? serenityLoaded;

        public bool SerenityLoaded
        {
            get {
                if (serenityLoaded == null)
                    serenityLoaded = Expansions.ExpansionsLoader.IsExpansionInstalled("Serenity");
                return (bool)serenityLoaded; 
            }
        }

        public class SCANROC
        {

            private ROC roc;
            private double longitude;
            private double latitude;
            private string name;
            private int id;
            //private bool known;
            private bool scanned;

            public SCANROC(ROC r, string n, double lon, double lat, /* bool k, */ bool s)
            {
                roc = r;

                id = r.rocID;
                name = n;
                longitude = lon;
                latitude = lat;
                //known = k;
                scanned = s;
            }

            public double Longitude
            {
                get { return longitude; }
            }

            public double Latitude
            {
                get { return latitude; }
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

        public List<SCANROC> ROCS(CelestialBody body, bool refresh)
        {
            if (ROCManager.Instance == null)
                return null;

            if (!ROCManager.Instance.RocsEnabledInCurrentGame)
                return null;

            if (!SerenityLoaded)
                return null;

            if (rocs == null)
                rocs = new List<SCANROC>();

            if (!refresh && rocs.Count > 0)
                return rocs;

            rocs.Clear();

            if (body == null)
                return null;

            PQS controller = body.pqsController;

            if (controller == null || controller.transform == null)
                return null;

            for (int i = controller.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = controller.transform.GetChild(i);

                if (child == null)
                    continue;

                if (child.name.StartsWith("ROC"))
                {
                    int index = child.name.IndexOf(' ');

                    if (index > 0 && index < child.name.Length - 1)
                    {
                        string id = child.name.Substring(index + 1);

                        bool scanned = false;

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
                                    if (!roc.smallROC && !roc.canbetaken)
                                    {
                                        if (roc.transform != null)
                                        {
                                            double lon = body.GetLongitude(roc.transform.position);
                                            double lat = body.GetLatitude(roc.transform.position);

                                            rocs.Add(new SCANROC(roc
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
            return rocs;
        }
    }
}

#endif