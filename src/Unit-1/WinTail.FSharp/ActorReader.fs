module ActorReader

open System
open Akka.FSharp

let StartCommand = "start"
let ExitCommand = "exit"

let ConsoleReaderActor (mailbox: Actor<string>) (msg: string) = 
    if (msg = StartCommand) then
        printfn "Please provide the URI of a log file on disk."
    
    let system = mailbox.Context.System
    let message = Console.ReadLine()

    if (String.Equals(message, ExitCommand, StringComparison.OrdinalIgnoreCase)) then
        system.Shutdown()
        ()

    let validator = select "akka://myActorSystem/user/ValidationActor" system
    validator <! message
    ()
