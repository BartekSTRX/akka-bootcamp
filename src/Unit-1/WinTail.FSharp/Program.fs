module Main

open System
open Akka.FSharp

open ActorReader
open ActorWriter
open ActorFileValidator
open ActorTailCoordinator


[<EntryPoint>]
let main argv = 
    use system = System.create "myActorSystem" (Configuration.defaultConfig())

    let reader = spawn system "ConsoleReaderActor" (actorOf2 ConsoleReaderActor)
    let writer = spawn system "ConsoleWriterActor" (actorOf2 ConsoleWriterActor)

    let validator = spawn system "ValidationActor" (actorOf2 <| FileValidatorActor writer)
    let coordinator = spawn system "TailCoordinatorActor" (actorOf2 TailCoordinatorActor)

    reader <! StartCommand

    system.AwaitTermination()
    //Console.ReadLine() |> ignore
    0
