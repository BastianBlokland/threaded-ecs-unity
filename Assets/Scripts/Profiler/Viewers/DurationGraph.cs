using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Profiler
{
	public class DurationGraph : MonoBehaviour
	{
		[SerializeField] private Timeline timeline;
		[SerializeField] private bool drawInGame;
		[Range(0f, .001f)]
		[SerializeField] private float minDuration;
		[Range(.001f, .5f)]
		[SerializeField] private float maxDuration = .1f;

		private readonly List<TimelineItem> itemCache = new List<TimelineItem>();

		private Material linesMat;

		public void Draw(Rect rect)
		{
			const float HEADER_HEIGHT = 20f;

			if(timeline == null)
			{
				GUI.Label(rect, "Please provide a timeline to visualize");
				return;
			}

			int numTracks = timeline.Tracks.Count;
			for (int i = 0; i < timeline.Tracks.Count; i++)
			{
				Rect itemRect = new Rect(rect.x, rect.y + (rect.height / numTracks) * i, rect.width, rect.height / numTracks);

				//Draw header
				GUI.color = Color.white;
				GUI.Label(new Rect(itemRect.x, itemRect.y, itemRect.width, HEADER_HEIGHT), timeline.Tracks[i].Label);
				
				//Draw content
				Rect contentRect = new Rect(itemRect.x, itemRect.y + HEADER_HEIGHT, itemRect.width, Mathf.Max(1f, itemRect.height - HEADER_HEIGHT));
				DrawTrack(contentRect, timeline.Tracks[i], timeline.CurrentTime);
			}
		}

		protected void Start()
		{
			linesMat = new Material(Shader.Find("Unlit/Color"));
		}

		protected void OnGUI()
		{
			if(drawInGame)
				Draw(new Rect(10f, 270f, 600f, 400));
		}

		protected void OnDestroy()
		{
			if(linesMat != null)
				Object.Destroy(linesMat);
		}

		private void DrawTrack(Rect rect, Timeline.TrackEntry trackEntry, float currentTime)
		{
			//Get the items to draw
			trackEntry.Track.GetItems(itemCache);

			//Draw background
			GUI.color = new Color(.3f, .3f, .3f, 1f);
			GUI.DrawTexture(rect, Texture2D.whiteTexture);

			//Draw info
			float averageDuration = GetAverageDuration(itemCache);
			GUI.color = Color.white;
			GUI.Label(rect, "Avg: (ms) " + (averageDuration * 1000f));

			if(linesMat != null)
				linesMat.SetPass(0);
			GL.Begin(GL.LINES);
			{
				float lastXProg = -1f;
				float lastYProg = -1f;
				for (int i = 0; i < itemCache.Count; i++)
				{
					float xProg = i == 0 ? 0 : (float)i / (itemCache.Count - 1);
					float duration = (itemCache[i].Running ? currentTime : itemCache[i].StopTime) - itemCache[i].StartTime;
					float yProg = Mathf.InverseLerp(maxDuration, minDuration, duration);
					if(i != 0)
					{
						GL.Vertex(new Vector2(rect.x + rect.width * lastXProg, rect.y + rect.height * lastYProg));
						GL.Vertex(new Vector2(rect.x + rect.width * xProg, rect.y + rect.height * yProg));
					}
					lastXProg = xProg;
					lastYProg = yProg;
				}
			}
			GL.End();
		}

		private float GetAverageDuration(List<TimelineItem> items)
		{
			if(items.Count == 0)
				return 0f;
			float sum = 0f;
			int cnt = 0;
			for (int i = 0; i < items.Count; i++)
			{
				if(items[i].Running)
					continue;
				sum += itemCache[i].StopTime - itemCache[i].StartTime;
				cnt++;
			}
			return sum / cnt;
		}
	}
}