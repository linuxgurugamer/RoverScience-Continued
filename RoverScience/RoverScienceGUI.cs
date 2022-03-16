using UnityEngine;

namespace RoverScience
{
    public partial class RoverScienceGUI
	{

		public GUIClass consoleGUI = new GUIClass();
		public GUIClass debugGUI = new GUIClass();
        public GUIClass upgradeGUI = new GUIClass();

		public class GUIClass
		{
			public Rect rect = new Rect ();

			public bool isOpen = false;

			public void Show()
			{
				isOpen = true;
			}

			public void Hide()
			{
				isOpen = false;
			}

			public void Toggle()
			{
				isOpen = !isOpen;
			}
		}

        private void GUIBreakline()
        {
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
        }

        private void GUICenter(string s)
        {
            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
            GUILayout.Label(s);
            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
        }

    }

}

