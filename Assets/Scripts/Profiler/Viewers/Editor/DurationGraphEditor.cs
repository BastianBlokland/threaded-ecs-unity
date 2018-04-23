using UnityEngine;
using UnityEditor;

namespace Profiler
{
	[CustomEditor(typeof(DurationGraph))]
	public class DurationGraphEditor : Editor
	{
		const float WIDTH = 300f;
		const float HEIGHT = 300f;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			DurationGraph durationGraph = target as DurationGraph;
			if(durationGraph != null && durationGraph.enabled)
			{
				Rect rect = GUILayoutUtility.GetRect(minWidth: WIDTH, maxWidth: WIDTH * 2f, minHeight: HEIGHT, maxHeight: HEIGHT * 2f);

				//Draw background
				GUI.Box(rect, GUIContent.none);

				//Draw content
				GUI.color = Color.white;
				durationGraph.Draw(rect);

				//Keep refreshing
				if(Application.isPlaying)
					Repaint();
			}
		}
	}
}