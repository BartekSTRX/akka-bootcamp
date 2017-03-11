module ActorFileValidator

open Messages
open Akka.FSharp
open Akka.Actor
open System
open System.IO

let FileValidatorActor(consoleWriterActor: IActorRef) (mailbox: Actor<string>) (msg: string) = 
    if (System.String.IsNullOrEmpty(msg)) then
        consoleWriterActor <! NullInputError("Blank input, try again.")
        mailbox.Sender() <! ContinueProcessing
    else
        if (File.Exists(msg)) then
            consoleWriterActor <! InputSuccess(sprintf "Starting processing for %s" msg)

            let coordinator = select "akka://myActorSystem/user/TailCoordinatorActor" mailbox.Context.System
            coordinator <! StartTail(consoleWriterActor, msg)
        else
            consoleWriterActor <! ValidationError(sprintf "%s is not valid Uri" msg)
            mailbox.Sender() <! ContinueProcessing
