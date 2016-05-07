using System.IO;
using Akka.Actor;

namespace WinTail.Actors
{
    public class FileValidatorActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;
        private readonly IActorRef _tailCoordinatorActor;

        public FileValidatorActor(IActorRef consoleWriterActor, IActorRef tailCoordinatorActor)
        {
            _consoleWriterActor = consoleWriterActor;
            _tailCoordinatorActor = tailCoordinatorActor;
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

                    _tailCoordinatorActor.Tell(new TailCoordinatorActor.StartTail(_consoleWriterActor, msg));
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