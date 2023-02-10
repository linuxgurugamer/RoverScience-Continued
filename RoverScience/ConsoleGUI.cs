using KSP.Localization;
using System;
using System.Collections.Generic;
using UnityEngine;
using static RoverScience.InitLog;


namespace RoverScience
{

    public partial class RoverScienceGUI
    {
        private bool analyzeButtonPressedOnce = false;
        private string inputMaxDistance = "100";

        private Dictionary<string, string> RichColors = new Dictionary<string, string>
        {
            { "green", "<color=#00ff00ff>" },
            { "red", "<color=#ff0000ff>" },
            { "blue", "<color=#add8e6ff>" },
            { "yellow", "<color=#ffff00ff>" },
            { "orange", "<color=#ffa500ff>" },

            { "Very High!", "<color=#00ff00ff>" },
            { "High", "<color=#00ff00ff>" },
            { "Normal", "<color=#add8e6ff>" },
            { "Low", "<color=#ffff00ff>" }

        };

        private string SetRichColor(string s, string color) => $"{RichColors[color]}{s}</color>";

        private string PotentialFontColor(string name)
            => RichColors.ContainsKey(name) ? SetRichColor(name, name) : SetRichColor(name, "red");

        private string PredictionFontColor(double percentage)
        {
            if (percentage > 70) return SetRichColor(Localizer.Format("#LOC_RoverScience_GUI_Percentage", percentage.ToString()), "green");
            else if (percentage >= 50) return SetRichColor(Localizer.Format("#LOC_RoverScience_GUI_Percentage", percentage.ToString()), "yellow");
            else return SetRichColor(Localizer.Format("#LOC_RoverScience_GUI_Percentage", percentage.ToString()), "red");
        }

        private string DecayFontColor(double percentage)
        {
            if (percentage > 70) return SetRichColor(Localizer.Format("#LOC_RoverScience_GUI_Percentage", percentage.ToString()), "red");
            else if (percentage >= 50) return SetRichColor(Localizer.Format("#LOC_RoverScience_GUI_Percentage", percentage.ToString()), "yellow");
            else return SetRichColor(Localizer.Format("#LOC_RoverScience_GUI_Percentage", percentage.ToString()), "green");
        }


        private void DrawRoverConsoleGUI(int windowID)
        {
            if (roverScience.part.vessel != FlightGlobals.ActiveVessel)
            {
                Log.Detail("ConsoleGUI not drawn - not active vessel");
                return;
            }
            if (Rover.scienceSpot.established && Rover.ScienceSpotReached)
            {
                consoleGUI.rect.height = 559;
            }
            else if (Rover.scienceSpot.established)
            {
                consoleGUI.rect.height = 495;
            }
            else if (!Rover.scienceSpot.established)
            {
                consoleGUI.rect.height = 466;
            }

            GUILayout.BeginVertical(Statics.consoleAreaStyle);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[] { GUILayout.Width(240), GUILayout.Height(340) });

            if (Vessel.srfSpeed > HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().maxSpeed)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_RoverMovingTooFast"), "yellow"), Statics.boldFont); // "Science Spots Analyzed: "
                    }
                    GUILayout.FlexibleSpace();
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_forSensorsToWork"), "yellow"), Statics.boldFont); // "Science Spots Analyzed: "
                    }
                    GUILayout.FlexibleSpace();
                }
                // Since we are returning here, need to end the scrollview and vertical
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
                return;
            }

            if (Vessel.srfSpeed > HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().maxSpeed - 1f)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_RoverNearMaxSpeed"), "yellow"), Statics.boldFont); // "Science Spots Analyzed: "
                    }
                    GUILayout.FlexibleSpace();
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_forSensorsToWork"), "yellow"), Statics.boldFont); // "Science Spots Analyzed: "
                    }
                    GUILayout.FlexibleSpace();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_SpotsAnalyzed"), Statics.boldFont); // "Science Spots Analyzed: "
                    GUILayout.Label(roverScience.amountOfTimesAnalyzed.ToString(), Statics.boldFont);
                }
                GUILayout.FlexibleSpace();
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_ReuseLoss") + DecayFontColor(roverScience.ScienceDecayPercentage), Statics.boldFont); // "Science Loss due to re-use: "
                GUILayout.FlexibleSpace();
            }


            GUICenter("_____________________");
            GUIBreakline();

            if (!Rover.landingSpot.established)
            {
                GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_NoLanding")); // > No landing spot established!
                GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_NoLandingWheels")); // > You must first establish a landing spot by landing somewhere. Make sure you have wheels!
                GUIBreakline();
                GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_WheelsDetected", Rover.NumberWheels)); // > Rover wheels detected: <<1>> 
                if (HighLogic.CurrentGame.Parameters.CustomParams<RSSettings>().requireWheelTouching)
                    GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_WheelsLanded", Rover.NumberWheelsLanded)); // > Rover wheels landed: <<1>> 

            }
            else
            {
                if (!Rover.scienceSpot.established)
                {
                    // PRINT OUT CONSOLE CONTENTS

                    if (roverScience.ScienceDecayPercentage >= 100)
                    {
                        GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_ScienceLimit"), "red")); //  > You have analyzed too many times.\n> Science loss is now at 100%.\n> Send another rover.
                    }
                    else
                    {
                        GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_DriveAround")); // > Drive around to search for science spots . . .
                        GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_ScanningRange", Rover.maxRadius)); // > Currently scanning at range: " + rover.maxRadius + "m");
                        //GUILayout.Label("> Total dist. traveled searching: " + Math.Round(rover.distanceTraveledTotal, 2));
                        GUIBreakline();
                        for (int s1 = 0; s1 < consolePrintOut.Count; s1++)
                        {
                            GUILayout.Label(consolePrintOut[s1]);
                        }

                        if (Vessel.mainBody.bodyName == "Kerbin")
                        {
                            GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_HomeWorld"), "red")); // > WARNING - there is very little rover science for Kerbin!
                        }
                    }

                }
                else
                {
                    if (!Rover.ScienceSpotReached)
                    {
                        double relativeROCBearing = 0;
                        double relativeBearing = Rover.Heading - Rover.BearingToScienceSpot;
                        if (ROC_Class.SerenityLoaded)
                            relativeROCBearing = Rover.Heading - Rover.BearingToROC;

                        GUILayout.BeginVertical();
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if (!Rover.AnomalyPresent)
                            {
                                GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Potential"), "yellow")); // "[POTENTIAL SCIENCE SPOT]"
                            }
                            else
                            {
                                GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Anomaly"), "yellow")); // "[ANOMALY DETECTED]"
                            }

                            GUILayout.FlexibleSpace();
                        }

                        GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_DistToSpot", Math.Round(Rover.DistanceFromScienceSpot, 1))); // "> Distance to spot (m): <<1>>" + );

                        // GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_BearingOfSite") +" " + Math.Round(Rover.BearingToScienceSpot, 1));
                        GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_RoverBearing") + " " + Math.Round(Rover.Heading, 1));
                        GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_RelBearing") + " " + Math.Round(relativeBearing, 1));
                        if (ROC_Class.SerenityLoaded)
                            GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_RelBearing") + " to ROC " + Math.Round(relativeROCBearing, 1));


                        if (!Rover.AnomalyPresent)
                        {
                            //GUILayout.Label("> Science Potential: " + rover.scienceSpot.predictedSpot + " (" + roverScience.currentPredictionAccuracy + "% confident)");
                            GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_Prediction", PotentialFontColor(Rover.scienceSpot.predictedSpot))); // > Science Prediction: <<1>>"
                            GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_Confidence", PredictionFontColor(roverScience.CurrentPredictionAccuracy))); // > Prediction is <<1>> confident");

                            // roverScience == RoverScience.Instance
                            // Rover == RoverScience.Instance.rover

                        }
                        else
                        {
                            GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Anomaly2")); //"> ANOMALY DETECTED. Something interesting is nearby . . . the science potential should be very significant")
                        }
                        //GUIBreakline();
                        //GUIBreakline();

                        //This block handles writing getDriveDirection
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.Label(GetDriveDirection(Rover.BearingToScienceSpot, Rover.Heading));
                            GUILayout.FlexibleSpace();
                        }
                        if (ROC_Class.SerenityLoaded)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();
                                GUILayout.Label(GetDriveDirection(Rover.BearingToScienceSpot, Rover.BearingToROC) + "to ROC");
                                GUILayout.FlexibleSpace();
                            }
                            //GUILayout.EndVertical();
                        }
                        GUILayout.EndVertical();


                    }
                    else
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.Label(SetRichColor(Localizer.GetStringByTag("#LOC_RoverScience_GUI_SpotReached"), "green")); // "[SCIENCE SPOT REACHED]"
                            GUILayout.FlexibleSpace();
                        }

                        //GUILayout.Label("Total dist. traveled for this spot: " + Math.Round(rover.distanceTraveledTotal, 1));
                        //GUILayout.Label("Distance from landing site: " +
                        //Math.Round(rover.getDistanceBetweenTwoPoints(rover.scienceSpot.location, rover.landingSpot.location), 1));
                        GUILayout.BeginHorizontal();
                        //GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_ActualPotential", PotentialFontColor(Rover.scienceSpot.potentialGenerated))); // "> Science Potential: <<1>> (actual)");
                        GUILayout.Label(Localizer.Format("#LOC_RoverScience_GUI_ActualPotential", PotentialFontColor(Rover.scienceSpot.adjustedPotentialGenerated))); // "> Science Potential: <<1>> (actual)");
                        GUILayout.EndHorizontal();

                        GUIBreakline();

                        GUILayout.Label(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Note")); // "> NOTE: The more you analyze, the less you will get each time!");
                    }

                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // ACTIVATE ROVER BUTTON
            if (Rover.ScienceSpotReached)
            {

                if (!analyzeButtonPressedOnce)
                {
                    GUI.enabled = FlightGlobals.ActiveVessel.srfSpeed <= 0.05f;
                    if (GUILayout.Button(Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnAnalyze"), GUILayout.Height(60))) // "Analyze Science"
                    {
                        if (roverScience.container.GetStoredDataCount() < roverScience.container.capacity || roverScience.container.capacity == 0)
                        {
                            if (Rover.ScienceSpotReached)
                            {
                                analyzeButtonPressedOnce = true;
                                consolePrintOut.Clear();

                            }
                            else if (!Rover.ScienceSpotReached)
                            {
                                ScreenMessages.PostScreenMessage(Localizer.GetStringByTag("#LOC_RoverScience_GUI_GetToSpot"), 3, ScreenMessageStyle.UPPER_CENTER); // "Cannot analyze - Get to the science spot first!"
                            }
                        }
                        else
                        {
                            ScreenMessages.PostScreenMessage(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Full"), 3, ScreenMessageStyle.UPPER_CENTER); // "Cannot analyze - Rover Brain already contains data!"
                        }
                    }
                    GUI.enabled = true;
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnConfirm"))) // "Confirm"
                    {
                        analyzeButtonPressedOnce = false;
                        roverScience.AnalyzeScienceSample();
                    }
                    if (GUILayout.Button(Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnCancel"))) //"Cancel"
                    {
                        analyzeButtonPressedOnce = false;
                    }
                    GUILayout.EndHorizontal();
                }
            }

            if (Rover.scienceSpot.established)
            {
                if (GUILayout.Button(Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnResetSpot"))) // "Reset Science Spot"
                {
                    Rover.scienceSpot.established = false;
                    Rover.ResetDistanceTraveled();

                    roverScience.drawWaypoint.DestroyInterestingObject();
                    roverScience.drawWaypoint.HideMarker();
                    consolePrintOut.Clear();

                }
            }

            //if (GUILayout.Button ("Reorient from Part")) {
            //roverScience.command.MakeReference ();
            //}
            GUIBreakline();
            GUIBreakline();
            if (roverScience.ScienceDecayPercentage < 100)
            {
                using (new GUILayout.HorizontalScope())
                {
                    inputMaxDistance = GUILayout.TextField(inputMaxDistance, 5, new GUILayoutOption[] { GUILayout.Width(40) });

                    if (GUILayout.Button(Localizer.Format("#LOC_RoverScience_GUI_BtnScanRange", roverScience.CurrentMaxDistance))) // "Set Scan Range [Max: <<1>>]"
                    {

                        int inputMaxDistanceInt;
                        bool isNumber = int.TryParse(inputMaxDistance, out inputMaxDistanceInt);


                        if ((isNumber) && (inputMaxDistanceInt <= roverScience.CurrentMaxDistance) && (inputMaxDistanceInt >= 40))
                        {
                            Rover.maxRadius = inputMaxDistanceInt;
                            Log.Info("Set maxRadius to input: " + Rover.maxRadius);
                            ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_RoverScience_GUI_Scanning", Rover.maxRadius), 3, ScreenMessageStyle.UPPER_CENTER); // "Now scanning for science spots at range: <<1>>
                        }

                        if (inputMaxDistanceInt > roverScience.CurrentMaxDistance)
                        {
                            ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_RoverScience_GUI_OverMax", roverScience.CurrentMaxDistance), 3, ScreenMessageStyle.UPPER_CENTER); // "Cannot set scan range over max distance of: <<1>>
                        }
                        else if (inputMaxDistanceInt < 40)
                        {
                            ScreenMessages.PostScreenMessage(Localizer.GetStringByTag("#LOC_RoverScience_GUI_MinRange"), 3, ScreenMessageStyle.UPPER_CENTER); // "Minimum of 40m scan range is required"
                        }
                    }
                }
            }

            if (HighLogic.CurrentGame.Mode != Game.Modes.SANDBOX)
            {
                if (GUILayout.Button(Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnUpgradeMenu")))
                { // "Upgrade Menu"
                    upgradeGUI.Toggle();
                }
            }
            GUILayout.Space(5);
            if (GUILayout.Button(Localizer.GetStringByTag("#LOC_RoverScience_GUI_BtnShutdown")))
            { // "Close and Shutdown"
                Rover.scienceSpot.established = false;
                Rover.ResetDistanceTraveled();
                consolePrintOut.Clear();

                roverScience.drawWaypoint.HideMarker();

                consoleGUI.Hide();
                upgradeGUI.Hide();
            }
            GUI.DragWindow();
        }


        private string GetDriveDirection(double destination, double currentHeading)
        {
            // This will calculate the closest angle to the destination, given a current heading.
            // Everything here will be in degrees, not radians

            // Shift destination angle to 000 bearing. Apply this shift to the currentHeading in the same direction.
            double delDestAngle = 0;
            double shiftedCurrentHeading = 0;

            if (destination > 180)
            {
                // Delta will be clockwise
                delDestAngle = 360 - destination;
                shiftedCurrentHeading = currentHeading + delDestAngle;

                if (shiftedCurrentHeading > 360) shiftedCurrentHeading -= 360;
            }
            else
            {
                // Delta will be anti-clockwise
                delDestAngle = destination;
                shiftedCurrentHeading = currentHeading - delDestAngle;

                if (shiftedCurrentHeading < 0) shiftedCurrentHeading += 360;
            }

            double absShiftedCurrentHeading = Math.Abs(shiftedCurrentHeading);

            if (absShiftedCurrentHeading < 6)
            {
                return Localizer.GetStringByTag("#LOC_RoverScience_GUI_DriveFwd"); // "DRIVE FORWARD";
            }

            if ((absShiftedCurrentHeading > 174) && (absShiftedCurrentHeading < 186))
            {
                return Localizer.GetStringByTag("#LOC_RoverScience_GUI_TurnAround"); // "TURN AROUND";
            }

            if (absShiftedCurrentHeading < 180)
            {
                return Localizer.GetStringByTag("#LOC_RoverScience_GUI_TurnLeft"); // "TURN LEFT";
            }

            if (absShiftedCurrentHeading > 180)
            {
                return Localizer.GetStringByTag("#LOC_RoverScience_GUI_TurnRight"); //  "TURN RIGHT";
            }



            return Localizer.GetStringByTag("#LOC_RoverScience_GUI_DirectionError"); // "ERROR: FAILED TO RESOLVE DRIVE DIRECTION";

        }


    }


}

