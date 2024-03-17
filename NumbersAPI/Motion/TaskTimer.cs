using System;
using NumbersCore.CoreConcepts.Time;
using NumbersCore.Primitives;

namespace NumbersAPI.Motion
{
    // todo: a task timer for animation is really just a transform.
    public class TaskTimer : ITimeable
    {
        public float InterpolationT { get; set; }
        public bool IsComplete { get; protected set; } = false;

        public double StartTime { get; set; }
        private double _runningTime;
        private double _currentTime;
        private DateTime _pauseTime;
        private bool _isPaused;
        private float _delayTime = 0;
        protected bool IsReverse { get; set; } = false;

        public MillisecondNumber DelayDuration { get; } // can 'type' numbers, this would be on a domain of 0-30 focal unit, and 0-max range, time trait.
        public double DelayValue => DelayDuration.StartValue; // Delay is unot, duration is unit
        public double DurationValue => DelayDuration.EndValue;


        public event TimedEventHandler StartTimedEvent;
        public event TimedEventHandler StepTimedEvent;
        public event TimedEventHandler EndTimedEvent;

        public TaskTimer(MillisecondNumber delayDuration)
        {
	        DelayDuration = delayDuration;
            _pauseTime = DateTime.Now;
        }

        public void Restart()
        {
            StartTime = (float)(DateTime.Now - Runner.StartTime).TotalMilliseconds;
            _currentTime = StartTime;
            _runningTime = 0;
            _delayTime = 0;
            IsComplete = false;
            StartTimedEvent?.Invoke(this, EventArgs.Empty);
        }
        public void Reverse()
        {
            IsReverse = !IsReverse;
        }

        public void StartUpdate(double ct, double deltaTime)
        {
            if (!_isPaused)
            {
                _runningTime += deltaTime + _delayTime;
                _delayTime = 0;
                _currentTime = StartTime + _runningTime;
                if (_currentTime > StartTime + DurationValue)
                {
                    IsComplete = true;
                    InterpolationT = 1f;
                }
                else
                {
                    InterpolationT = (float)(_currentTime < StartTime ? 0 :
                        _currentTime > StartTime + DurationValue ? 1f :
                        (_currentTime - StartTime) / DurationValue);
                }

                InterpolationT = IsReverse ? 1f - InterpolationT : InterpolationT;
                StepTimedEvent?.Invoke(this, EventArgs.Empty);
            }
        }
        public void EndUpdate(double currentTime, double deltaTime)
        {
            if (IsComplete)
            {
                EndTimedEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        //public override ParametricSeries GetNormalizedPropertyAtT(PropertyId propertyId, ParametricSeries seriesT)
        //{
        //    ParametricSeries result;
        //    if (propertyId == PropertyId.InterpolationT)
        //    {
        //        result = new ParametricSeries(1, InterpolationT);
        //    }
        //    else
        //    {
        //        result = base.GetNormalizedPropertyAtT(propertyId, new ParametricSeries(1, InterpolationT)); // todo: include seriesT, probably needed when scrubbing.
        //    }

        //    return result;
        //}

        //public override ISeries GetSeriesAtT(PropertyId propertyId, float t, ISeries parentSeries)
        //{
        //    ParametricSeries result;
        //    if (propertyId == PropertyId.InterpolationT)//PropertyIdSet.IsTSampling(propertyId))
        //    {
        //        result = new ParametricSeries(1, InterpolationT); // this is straight timer lookup, so no ref to input t needed.
        //    }
        //    else
        //    {
        //        result = base.GetNormalizedPropertyAtT(propertyId, new ParametricSeries(1, InterpolationT));
        //    }
        //    return result;
        //}

        public void Pause()
        {
            _isPaused = true;
            _pauseTime = DateTime.Now;
        }

        public void Resume()
        {
            _isPaused = false;
            _delayTime = (float)(DateTime.Now - _pauseTime).TotalMilliseconds;
        }
    }
}
