using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Utils;

namespace Profiler
{
	public class Timeline : MonoBehaviour
	{
		public struct TrackEntry
		{
			public readonly string Label;
			public readonly TimelineTrack Track;

			public TrackEntry(string label, TimelineTrack track)
			{
				Label = label;
				Track = track;
			}
		}

		public bool IsStarted { get { return started; } }
		public float CurrentTime { get { return (float)timer.Elapsed.TotalSeconds; } }
		public IList<TrackEntry> Tracks { get { return tracks; } }

		private readonly List<TrackEntry> tracks = new List<TrackEntry>();
		private readonly Stopwatch timer = new Stopwatch();

		private bool started;

		public T CreateTrack<T>(string label) where T : TimelineTrack, new()
		{
			if(started)
				throw new Exception("[Timeline] Unable to create a track after the timeline has allready started");
			T newTrack = new T();
			tracks.Add(new TrackEntry(label, newTrack));
			return newTrack;
		}

		public void StartTimers()
		{
			if(started)
				throw new Exception("[Timeline] Allready started");
			for (int i = 0; i < tracks.Count; i++)
				tracks[i].Track.StartTimer();
			timer.Start();
			started = true;
		}
	}
}