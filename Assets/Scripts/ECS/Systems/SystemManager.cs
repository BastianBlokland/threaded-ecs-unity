using System;

namespace ECS.Systems
{
	public class SystemManager : IDisposable
	{
		public bool IsRunning { get { return !isCompleted; } }

		private readonly SystemRunner runner;
		private readonly System[][] systems;

		private volatile bool isCompleted = true;

		/// <summary>
		/// Wierd syntax but this 'systems' jagged array is a array of system tracks. So the outer array
		/// is scheduled linearly but the items in the inner array run in parallel. Need to see if i can
		/// come up with nicer syntax for this. 
		/// </summary>
		public SystemManager(bool multiThreaded, params System[][] systems)
		{
			this.runner = new SystemRunner(multiThreaded);
			this.systems = systems;
		}

		public void Complete()
		{
			while(!isCompleted)
				runner.Help();
		}

		public void Schedule()
		{
			//First complete the previous run
			Complete();

			isCompleted = false;

			TrackExecuteHandle firstTrack = null;
			TrackExecuteHandle previousTrack = null;
			for (int i = 0; i < systems.Length; i++)
			{
				TrackExecuteHandle trackHandle = new TrackExecuteHandle(runner, systems[i]);
				if(firstTrack == null)
					firstTrack = trackHandle;

				//Chain all the track together in a linear fashion
				if(previousTrack != null)
					previousTrack.Completed += trackHandle.Schedule;

				previousTrack = trackHandle;
			}

			//Follow the completion of the last track
			if(previousTrack != null)
				previousTrack.Completed += LastTrackCompleted;

			//Start the chain
			if(firstTrack != null)
				firstTrack.Schedule();
			else //If there where no track then consider it to be complete allready
				LastTrackCompleted();
		}

		public void Dispose()
		{
			runner.Dispose();
		}

		private void LastTrackCompleted()
		{
			isCompleted = true;
		}
	}
}