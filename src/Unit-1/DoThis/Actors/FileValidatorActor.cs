using System.IO;
using Akka.Actor;

namespace WinTail.Actors
{
    public class FileValidatorActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;

        public FileValidatorActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;

            if (string.IsNullOrEmpty(msg))
            {
                _consoleWriterActor.Tell(new Messages.NullInputError("Blank input, try again."));

                Sender.Tell(new Messages.ContinueProcessing());
            }
            else
            {
                if (File.Exists(msg))
                {
                    _consoleWriterActor.Tell(new Messages.InputSuccess($"Starting processing for {msg}"));

                    var tailCoordinatorActor = Context.ActorSelection("akka://MyActorSystem/user/TailCoordinatorActor");
                    tailCoordinatorActor.Tell(new TailCoordinatorActor.StartTail(_consoleWriterActor, msg));
                }
                else
                {
                    _consoleWriterActor.Tell(new Messages.ValidationError($"{msg} is not valid Uri"));

                    Sender.Tell(new Messages.ContinueProcessing());
                }
            }
        }
    }
}