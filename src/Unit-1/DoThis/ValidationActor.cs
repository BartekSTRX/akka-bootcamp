using System;
using Akka.Actor;

namespace WinTail
{
    public class ValidationActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;

        public ValidationActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }

        private static bool IsValid(string message) => message.Length % 2 == 0;

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                _consoleWriterActor.Tell(new Messages.NullInputError("No input received. "));
            }
            else
            {
                if (IsValid(msg))
                {
                    _consoleWriterActor.Tell(new Messages.InputSuccess("Thank you. Message was valid. "));
                }
                else
                {
                    _consoleWriterActor.Tell(new Messages.ValidationError("Input was not valid. "));
                }
            }
            Sender.Tell(new Messages.ContinueProcessing());
        }
    }
}