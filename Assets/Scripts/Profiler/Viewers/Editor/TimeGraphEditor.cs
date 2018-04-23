using UnityEngine;
using UnityEditor;

namespace Profiler
{
	[CustomEditor(typeof(TimeGraph))]
	public class TimeGraphEditor : Editor
	{
		const float WIDTH = 300f;
		const float HEIGHT = 150f;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			TimeGraph timeGraph = target as TimeGraph;
			if(timeGraph != null && timeGraph.enabled)
			{
				Rect rect = GUILayoutUtility.GetRect(minWidth: WIDTH, maxWidth: WIDTH * 2f, minHeight: HEIGHT, maxHeight: HEIGHT * 2f);

				//Draw background
				GUI.Box(rect, GUIContent.none);

				//Draw content
				GUI.color = Color.white;
				timeGraph.Draw(rect);

				//Keep refreshing
				if(Application.isPlaying)
					Repaint();
			}
		}
	}
}