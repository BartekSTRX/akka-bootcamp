module Messages

open Akka.Actor

type Message = 
    | ContinueProcessing
    | InputSuccess of reason:string
    | InputError of reason:string
    | NullInputError of reason:string
    | ValidationError of reason:string

type TailActorMessage = 
    | FileError of fileName:string * reason:string
    | FileWrite of fileName:string
    | InitialRead of fileName:string * text:string

type CoordinatorMessages = 
    StartTail of reporterActor:IActorRef * filePath:string