using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Profiler
{
	public class TimeGraph : MonoBehaviour
	{
		public float LeftTime { get { return RightTime - timeRange; } }
		public float RightTime { get { return viewTime; } }
	
		[SerializeField] private Timeline timeline;
		[SerializeField] private bool drawInGame;
		[Range(.01f, 1f)]
		[SerializeField] private float timeRange = .25f;
		[SerializeField] private bool paused;

		private readonly List<TimelineItem> itemCache = new List<TimelineItem>();
		private readonly Color[] trackColors = new Color[] { Color.blue, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.red };

		private float viewTime;

		public void Draw(Rect rect)
		{
			const float HEADER_HEIGHT = 20f;

			if(timeline == null)
			{
				GUI.Label(rect, "Please provide a timeline to visualize");
				return;
			}
			if(!timeline.IsStarted)
			{
				GUI.Label(rect, "Not yet started");
				return;
			}
			if(!paused)
				viewTime = timeline.CurrentTime;

			int numTracks = timeline.Tracks.Count;
			for (int i = 0; i < timeline.Tracks.Count; i++)
			{
				Rect itemRect = new Rect(rect.x, rect.y + (rect.height / numTracks) * i, rect.width, rect.height / numTracks);

				//Draw header
				GUI.color = Color.white;
				GUI.Label(new Rect(itemRect.x, itemRect.y, itemRect.width, HEADER_HEIGHT), timeline.Tracks[i].Label);
				
				//Draw content
				Rect contentRect = new Rect(itemRect.x, itemRect.y + HEADER_HEIGHT, itemRect.width, Mathf.Max(1f, itemRect.height - HEADER_HEIGHT));
				DrawTrack(contentRect, trackColors[i % trackColors.Length], timeline.Tracks[i], LeftTime, RightTime, timeline.CurrentTime);
			}
		}

		//Implemented to get the 'enabled' tick-box in unity on the component
		protected void Start() {}

		protected void OnGUI()
		{
			if(drawInGame)
				Draw(new Rect(10f, 10f, 600f, 250f));
		}

		private void DrawTrack(Rect rect, Color color, Timeline.TrackEntry trackEntry, float leftTime, float rightTime, float currentTime)
		{
			//Get the items to draw
			trackEntry.Track.GetItems(itemCache);

			//Draw background
			GUI.color = new Color(.3f, .3f, .3f, 1f);
			GUI.DrawTexture(rect, Texture2D.whiteTexture);

			//Draw content
			GUI.color = color;
			for (int i = 0; i < itemCache.Count; i++)
			{
				TimelineItem item = itemCache[i];
				float itemStartTime = item.StartTime;
				float itemStopTime = item.Running ? currentTime : item.StopTime;
				bool inView = MathUtils.DoesRangeOverlap(itemStartTime, itemStopTime, leftTime, rightTime);
				if(inView)
				{
					//Convert to progress in the view
					float p1 = Mathf.InverseLerp(leftTime, rightTime, itemStartTime);
					float p2 = Mathf.InverseLerp(leftTime, rightTime, itemStopTime); 

					Rect itemRect = new Rect(rect.x + p1 * rect.width, rect.y, (p2 - p1) * rect.width, rect.height);
					GUI.DrawTexture(itemRect, Texture2D.whiteTexture);
				}
			}
		}
	}
}