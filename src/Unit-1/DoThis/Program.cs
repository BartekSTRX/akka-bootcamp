using Akka.Actor;

namespace WinTail
{
    #region Program

    public static class Program
    {
        public static void Main()
        {
            // initialize _myActorSystem
            var myActorSystem = ActorSystem.Create("_myActorSystem");

            // time to make your first actors!
            var consoleWriterActor = 
                myActorSystem.ActorOf(Props.Create(() => new ConsoleWriterActor()));
            var consoleReaderActor =
                myActorSystem.ActorOf(Props.Create(() => new ConsoleReaderActor(consoleWriterActor)));


            // tell console reader to begin
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            // blocks the main thread from exiting until the actor system is shut down
            myActorSystem.AwaitTermination();
        }
    }
    #endregion
}
