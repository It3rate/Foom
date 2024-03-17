using System;
using System.Timers;
using Xamarin.Forms;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;
using NumbersCore.Utils;
using Timer = System.Timers.Timer;

namespace NumbersAPI.Motion
{
	public interface IRunner
	{
		MillisecondNumber CurrentMS { get; }
		MillisecondNumber DeltaMS { get; }
    }

	public class Runner : IRunner
	{
		public static Runner CurrentRunner;
		public static Runner GetRunnerById(int id) => CurrentRunner;

		public IAgent Agent { get; set; }
		public Brain Brain => Agent.Brain;
		public Workspace Workspace => Agent.Workspace;

        public string lbEquation; // todo: callback event

        private bool _isPaused;
		private static DateTime _pauseTime;
		private static TimeSpan _delayTime = new TimeSpan(0);
		public static DateTime StartTime { get; private set; }

		public MillisecondNumber CurrentMS { get; } = MillisecondNumber.Zero(false);
		public MillisecondNumber DeltaMS { get; } = MillisecondNumber.Zero(false);

		private Timer _sysTimer;
		private TimeSpan _lastTime;
		private TimeSpan _currentTime;
		public long CurrentTimeMs => (long)_currentTime.TotalMilliseconds;


		public Runner(IAgent agent)
		{
			Agent = agent;
			CurrentRunner = this;
			Reset();
		}

        //private float t = 0;
        private bool _isBusy = false;
        private bool _needsUpdate = true;
        public bool HasUpdated { get; set; } = false;
        public bool NeedsUpdate() => _needsUpdate = true;

        private void Tick(object sender, ElapsedEventArgs e)
		{
			if (!_isPaused && !_isBusy) // && _needsUpdate)
			{
				_isBusy = true;
				_currentTime = e.SignalTime - (StartTime + _delayTime);
				CurrentMS.EndTicks = (long)_currentTime.TotalMilliseconds;
				DeltaMS.EndTicks = (long)(CurrentMS.EndTicks - _lastTime.TotalMilliseconds);

                Agent.Update(CurrentMS, DeltaMS);

				_lastTime = _currentTime;
				_needsUpdate = !HasUpdated;
			}

			_isBusy = false;
		}

		public void ActivateComposite(int id)
		{
			//var composite = Composites[id];
			//if (composite != null)
			//{
			//	if (composite is ITimeable anim)
			//	{
			//		anim.StartTime = (float) (DateTime.Now - Runner.StartTime).TotalMilliseconds;
			//	}

			//	Composites.ActivateElement(composite.Id);
			//}
		}

		public void DeactivateComposite(int id)
		{
			//Composites.DeactivateElement(id);
		}

		public void Clear()
		{
			_sysTimer?.Stop();
            _sysTimer.Close();
            _sysTimer = null;
            Reset();
		}

		public void Reset()
		{
			StartTime = DateTime.Now; // or leave at zero?

			_currentTime = DateTime.Now - StartTime;
			_lastTime = _currentTime;
			CurrentMS.EndTicks = (long)_currentTime.TotalMilliseconds;

			_sysTimer = new Timer();
			_sysTimer.Elapsed += Tick;
			_sysTimer.Interval = 8;
			_sysTimer.Enabled = true;
        }

		public void Pause()
		{
			if (!_isPaused)
			{
				OnPause(this, null);
			}
		}

		public void UnPause()
		{
			if (_isPaused)
			{
				OnPause(this, null);
			}
		}
		public void TogglePause()
		{
			OnPause(this, null);
		}

        private void OnPause(object sender, EventArgs e)
		{
			_isPaused = !_isPaused;
			if (_isPaused)
			{
				_pauseTime = DateTime.Now;
				//foreach (var id in Composites.ActiveIds)
				//{
				//	if (Composites.ContainsKey(id) && (Composites[id] is ITimeable))
				//	{
				//		((ITimeable) Composites[id]).Pause();
				//	}
				//}
			}
			else
			{
				_delayTime += DateTime.Now - _pauseTime;
				_lastTime = DateTime.Now - (StartTime + _delayTime);
				//foreach (var id in Composites.ActiveIds)
				//{
				//	if (Composites.ContainsKey(id) && (Composites[id] is ITimeable))
				//	{
				//		((ITimeable) Composites[id]).Resume();
				//	}
				//}
			}
		}

	}
}