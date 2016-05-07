using System;
using Akka.Actor;

namespace WinTail.Actors
{
    public class TailCoordinatorActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = (StartTail)message;
                Context.ActorOf(Props.Create(() => new TailActor(msg.ReporterActor, msg.FilePath)));
            }
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                localOnlyDecider: exception =>
                {
                    if (exception is ArithmeticException)
                        return Directive.Resume;
                    if (exception is NotSupportedException)
                        return Directive.Stop;
                    return Directive.Restart;
                },
                maxNrOfRetries: 10,
                withinTimeRange: TimeSpan.FromSeconds(30));
        }

        public class StartTail
        {
            public StartTail(IActorRef reporterActor, string filePath)
            {
                ReporterActor = reporterActor;
                FilePath = filePath;
            }

            public string FilePath { get; }
            public IActorRef ReporterActor { get; }
        }

        public class StopTail
        {
            public StopTail(string filePath)
            {
                FilePath = filePath;
            }

            public string FilePath { get; }
        }
    }
}