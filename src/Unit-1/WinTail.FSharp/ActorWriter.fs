module ActorWriter

open Akka.FSharp
open Messages
open System

let ConsoleWriterActor(mailbox: Actor<Message>) (msg: Message) = 
    match msg with 
    | InputError reason -> 
        Console.ForegroundColor <- ConsoleColor.Red
        Console.WriteLine(reason)
    | InputSuccess reason ->
        Console.ForegroundColor <- ConsoleColor.Green
        Console.WriteLine(reason)
    | other -> 
        Console.WriteLine(other)
    Console.ResetColor()