using Akka.Actor;
using WinTail.Actors;

namespace WinTail
{
    #region Program

    public static class Program
    {
        public static void Main()
        {
            // initialize _myActorSystem
            var myActorSystem = ActorSystem.Create("myActorSystem");

            // time to make your first actors!
            var consoleWriterProps = Props.Create<ConsoleWriterActor>();
            var consoleWriterActor = myActorSystem.ActorOf(consoleWriterProps, "ConsoleWriterActor");

            var tailCoordinatorProps = Props.Create<TailCoordinatorActor>();
            var tailCoordinatorActor = myActorSystem.ActorOf(tailCoordinatorProps, "TailCoordinatorActor");

            var validationProps = Props.Create(() => new FileValidatorActor(consoleWriterActor, tailCoordinatorActor));
            var validationActor = myActorSystem.ActorOf(validationProps, "ValidationActor");

            var consoleReaderProps = Props.Create<ConsoleReaderActor>(validationActor);
            var consoleReaderActor = myActorSystem.ActorOf(consoleReaderProps, "ConsoleReaderActor");


            // tell console reader to begin
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            // blocks the main thread from exiting until the actor system is shut down
            myActorSystem.AwaitTermination();
        }
    }
    #endregion
}
