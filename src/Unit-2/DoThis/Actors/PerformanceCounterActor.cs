using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;

namespace ChartApp.Actors
{
    public class PerformanceCounterActor : UntypedActor
    {
        private readonly string _seriesName;
        private readonly Func<PerformanceCounter> _performanceCounterGenerator;
        private PerformanceCounter _performanceCounter;

        private readonly ISet<IActorRef> _subscribers;
        private readonly ICancelable _cancelPublishing;

        public PerformanceCounterActor(string seriesName,
            Func<PerformanceCounter> performanceCounterGenerator)
        {
            _seriesName = seriesName;
            _performanceCounterGenerator = performanceCounterGenerator;

            _subscribers = new HashSet<IActorRef>();
            _cancelPublishing = new Cancelable(Context.System.Scheduler);
        }

        protected override void PreStart()
        {
            _performanceCounter = _performanceCounterGenerator();

            Context.System.Scheduler.
                ScheduleTellRepeatedly(
                TimeSpan.FromMilliseconds(250),
                TimeSpan.FromMilliseconds(250),
                Self,
                new GatherMetrics(),
                Self,
                _cancelPublishing);
        }

        protected override void PostStop()
        {
            try
            {
                _cancelPublishing.Cancel(false);
                _performanceCounter.Dispose();
            }
            catch
            {
                // ignored
            }
            finally
            {
                base.PostStop();
            }
        }

        protected override void OnReceive(object message)
        {
            if (message is GatherMetrics)
            {
                var metric = new Metric(_seriesName, _performanceCounter.NextValue());
                foreach (var subscriber in _subscribers)
                {
                    subscriber.Tell(metric);
                }
            }
            if (message is SubscribeCounter)
            {
                _subscribers.Add(((SubscribeCounter)message).Subscriber);
            }
            if (message is UnsubscribeCounter)
            {
                _subscribers.Remove(((UnsubscribeCounter)message).Subscriber);
            }
        }
    }
}