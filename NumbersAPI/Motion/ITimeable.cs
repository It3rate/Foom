﻿using NumbersCore.CoreConcepts.Time;

namespace NumbersAPI.Motion;

using System;

public delegate void TimedEventHandler(object sender, EventArgs e);

public interface ITimeable
{
    float InterpolationT { get; set; }
    //double DeltaTime { get; }
    //double _currentTime { get; }
    //double PreviousTime { get; }

    double StartTime { get; set; }

    MillisecondNumber DelayDuration { get; }
    double DelayValue { get; }
    double DurationValue { get; }

    //bool IsReverse { get; }
    //bool IsComplete { get; }

    event TimedEventHandler StartTimedEvent;
    event TimedEventHandler StepTimedEvent;
    event TimedEventHandler EndTimedEvent;

    void Restart();
    void Reverse();

    void Pause();
    void Resume();
}
