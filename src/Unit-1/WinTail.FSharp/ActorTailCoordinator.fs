module ActorTailCoordinator

open System

open Akka.Actor
open Akka.FSharp

open Messages

let decider(e: exn) =
    match e with 
    | :? ArithmeticException -> Directive.Resume
    | :? NotSupportedException -> Directive.Stop
    | _ -> Directive.Restart

let strategy = Strategy.OneForOne(decider, retries=10, timeout=TimeSpan.FromSeconds(float 30))

let options = SpawnOption.SupervisorStrategy(strategy)

let TailCoordinatorActor(mailbox: Actor<CoordinatorMessages>) (msg: CoordinatorMessages) = 
    match msg with
    | StartTail(reporter, filePath) -> 
        let actor = ActorTail.TailActorFactory(reporter, filePath)
        let name = sprintf "TailCoordinator%i" ((new Random()).Next(1, 1000))
        spawnOpt mailbox.Context.System name actor [options] |> ignore
