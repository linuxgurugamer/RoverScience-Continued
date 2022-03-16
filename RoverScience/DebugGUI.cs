using System;
using UnityEngine;

namespace RoverScience
{
    public partial class RoverScienceGUI
	{
        private string anomalyVisitedAdd = "1";

		private void DrawDebugGUI (int windowID)
		{

			GUILayout.BeginVertical ();

			GUILayout.Label (roverScience.RSVersion);
            
            GUILayout.Label ("# Data Stored: " + roverScience.container.GetStoredDataCount ());
			GUILayout.Label ("distCheck: " + Math.Round(Rover.distanceCheck, 2));
			GUILayout.Label ("distTrav: " + Math.Round(Rover.distanceTraveled));
			GUILayout.Label ("distTravTotal: " + Math.Round(Rover.distanceTraveledTotal));
			GUIBreakline ();
            GUILayout.Label("levelAnalyzedDecay: " + roverScience.levelAnalyzedDecay);
            GUILayout.Label ("currentScalarDecay: " + roverScience.ScienceDecayScalar);
			GUILayout.Label ("scienceDistanceScalarBoost: " + roverScience.scienceMaxRadiusBoost);

			GUILayout.Label ("ScienceSpot potential: " + Rover.scienceSpot.potentialGenerated);

			GUILayout.Label ("generatedScience: " + Rover.scienceSpot.potentialScience);
			GUILayout.Label ("with decay: " + Rover.scienceSpot.potentialScience * roverScience.ScienceDecayScalar);
			GUILayout.Label ("with distanceScalarBoost & decay & bodyScalar: " + Rover.scienceSpot.potentialScience * 
				roverScience.ScienceDecayScalar * roverScience.scienceMaxRadiusBoost * roverScience.BodyScienceScalar);

            GUIBreakline();
            GUILayout.Label("Distance travelled for spot: " + Rover.distanceTraveledTotal);
            
            GUIBreakline();
            GUILayout.Label("consoleGUI height: " + consoleGUI.rect.height);

            GUIBreakline();
            GUILayout.Label("Closest Anomaly ID: " + roverScience.rover.closestAnomaly.id);
            GUILayout.Label("Closest Anomaly Name: " + roverScience.rover.closestAnomaly.name);
            GUILayout.Label("Has current anomaly been analyzed? " + "[" + Anomalies.Instance.HasCurrentAnomalyBeenAnalyzed() + "]");



            GUILayout.BeginHorizontal();
            anomalyVisitedAdd = GUILayout.TextField(anomalyVisitedAdd, 3, new GUILayoutOption[] { GUILayout.Width(50) });
            if (GUILayout.Button("add anomaly ID"))
            {
                Rover.anomaliesAnalyzed.Add(anomalyVisitedAdd);
            }
            if (GUILayout.Button("X"))
            {
                Rover.anomaliesAnalyzed.Clear();
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("anomaly id visited: " + string.Join(",", Rover.anomaliesAnalyzed.ToArray()));

			if (GUILayout.Button ("Find Science Spot")) {
				Rover.scienceSpot.SetLocation (random: true);
			}

            if (GUILayout.Button ("Cheat Spot Here")) {
				if ((!Rover.scienceSpot.established) && (consoleGUI.isOpen)) {
					Rover.scienceSpot.SetLocation (Vessel.longitude, Vessel.latitude);
				} else if (Rover.scienceSpot.established){
					Rover.scienceSpot.Reset ();
				}
			}

			if (GUILayout.Button ("CLEAR CONSOLE")) {
				consolePrintOut.Clear ();
			}

			GUIBreakline ();

			GUILayout.Label("Times Analyzed: " + roverScience.amountOfTimesAnalyzed);

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("-")) {
				if (roverScience.amountOfTimesAnalyzed > 0)
					roverScience.amountOfTimesAnalyzed--;
			}

			if (GUILayout.Button ("+")) {
				roverScience.amountOfTimesAnalyzed++;
			}

			if (GUILayout.Button("0")){
				roverScience.amountOfTimesAnalyzed = 0;
			}
			GUILayout.EndHorizontal ();

			GUIBreakline ();
			GUIBreakline ();


			GUILayout.Label("Dist. Upgraded Level: " + roverScience.levelMaxDistance);

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("-")) {
				if (roverScience.levelMaxDistance > 1)
					roverScience.levelMaxDistance--;
			}

			if (GUILayout.Button ("+")) {
				roverScience.levelMaxDistance++;
			}

			if (GUILayout.Button("0")){
				roverScience.levelMaxDistance = 1;
			}
			GUILayout.EndHorizontal ();



			GUILayout.Label("Acc. Upgraded Level: " + roverScience.levelPredictionAccuracy);

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("-")) {
				if (roverScience.levelPredictionAccuracy > 1)
					roverScience.levelPredictionAccuracy--;
			}

			if (GUILayout.Button ("+")) {
				roverScience.levelPredictionAccuracy++;
			}

			if (GUILayout.Button("0")){
				roverScience.levelPredictionAccuracy = 1;
			}
			GUILayout.EndHorizontal ();

            GUILayout.Label("levelAnalyzedDecay: " + roverScience.levelAnalyzedDecay);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-"))
            {
                if (roverScience.levelAnalyzedDecay > 1)
                    roverScience.levelAnalyzedDecay--;
            }

            if (GUILayout.Button("+"))
            {
                roverScience.levelAnalyzedDecay++;
            }

            if (GUILayout.Button("0"))
            {
                roverScience.levelAnalyzedDecay = 1;
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button ("+500 Science")) {
                ResearchAndDevelopment.Instance.CheatAddScience(500);
            }

			if (GUILayout.Button ("-500 Science")) {
                ResearchAndDevelopment.Instance.CheatAddScience(-500);
            }

			GUIBreakline ();
			if (GUILayout.Button ("Close Window")) {
				debugGUI.Hide ();
			}

			GUILayout.EndVertical ();

			GUI.DragWindow ();
		}

    }
}