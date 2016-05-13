using System;
using System.Windows.Forms;
using Akka.Actor;

namespace ChartApp.Actors
{
    public class ButtonToggleActor : UntypedActor
    {
        public class Toggle { }


        private readonly CounterType _counterType;
        private readonly Button _button;
        private readonly IActorRef _coordinatorActor;
        private bool _isToggledOn;

        public ButtonToggleActor(IActorRef coordinatorActor,Button button, 
            CounterType counterType, bool isToggledOn = false)
        {
            _counterType = counterType;
            _button = button;
            _coordinatorActor = coordinatorActor;
            _isToggledOn = isToggledOn;
        }

        protected override void OnReceive(object message)
        {
            if (message is Toggle && _isToggledOn)
            {
                _coordinatorActor.Tell(new PerformanceCounterCoordinatorActor.Unwatch(_counterType));
                FlipToggle();
            }else if (message is Toggle && !_isToggledOn)
            {
                _coordinatorActor.Tell(new PerformanceCounterCoordinatorActor.Watch(_counterType));
                FlipToggle();
            }
            else
            {
                Unhandled(message);
            }
        }

        private void FlipToggle()
        {
            _isToggledOn = !_isToggledOn;

            _button.Text = $@"{_counterType.ToString().ToUpperInvariant()} ({(_isToggledOn ? "ON" : "OFF")})";
        }
    }
}