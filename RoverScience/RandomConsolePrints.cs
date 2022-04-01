using KSP.Localization;
using System.Collections.Generic;

namespace RoverScience
{

    partial class RoverScienceGUI
	{
		public class RandomConsolePrintOuts
		{
			List<string> strings = new List<string>();
            System.Random rand = new System.Random();

			public string GetRandomPrint()
			{
                if (strings.Count == 0)
                    return Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random1"); // "Nothing seems to be here";

				int randIndex = rand.Next (0, strings.Count);
				return strings [randIndex];
			}

			public RandomConsolePrintOuts()
			{
				for (int i = 1; i <20; i++)
                {
					strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random"+i.ToString()));
                }
#if false
				strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random1")); // "Nothing seems to be here");
				strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random2")); // "Running scans, checking");
				strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random3")); // "Weak signals of interest");
				strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random4")); // "Defragmenting - maybe that will help");
				strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random5")); // "Doing science-checks");
				strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random6")); // "No science here");
				strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random7")); // "Curiousity still increasing");
                strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random8")); // "Rocks, rocks, rocks, where are the interesting rocks?");
                strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random9")); // "Daisy . . . *cough* checking for science!");
                strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random10")); // "Nothing interesting here yet");
                strings.Add(Localizer.GetStringByTag("#LOC_RoverScience_GUI_Random11")); // "Science, science!");

				strings.Add("Doing science in spirit");
				strings.Add("Sceince...no wait that's Science");
				strings.Add("I'm sorry Jeb, I can't do -err, scanning for science...");
				strings.Add("I thought we were going to do a seance");
				strings.Add("Look at me still roving when there's science to do...");
				strings.Add("Are we there yet?");
				strings.Add("One little mistake");
				strings.Add("Can you test me now?");
#endif
			}

		}

	}

}