module ActorTail

open Akka.FSharp
open Akka.Actor

open FileObserver
open Messages

open System
open System.IO
open System.Text

let private PreStart(self: IActorRef, filePath: string) = 
    let observer = new FileObserver(self, filePath)
    observer.Start() |> ignore
    
    let fileStream = new FileStream(Path.GetFullPath(filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
    let fileStreamReader = new StreamReader(fileStream, Encoding.UTF8)

    let text = fileStreamReader.ReadToEnd()
    self.Tell(InitialRead(filePath, text))

    (observer, fileStream, fileStreamReader)

let private PostStop(observer: FileObserver, fileStreamReader: StreamReader) = 
    (observer :> IDisposable).Dispose()
    fileStreamReader.Close()
    fileStreamReader.Dispose()


let TailActorFactory(reportedActor: IActorRef, filePath: string) =
    let TailActor = fun (mailbox: Actor<TailActorMessage>) ->
        let (observer, fileStream, fileStreamReader) = PreStart(mailbox.Self, filePath)
        mailbox.Defer(fun() -> PostStop(observer, fileStreamReader))

        let rec loop () =
            actor {
                let! msg = mailbox.Receive ()
                
                match msg with
                | FileWrite fileName -> 
                    let text = fileStreamReader.ReadToEnd()
                    if (System.String.IsNullOrEmpty(text) |> not) then
                        reportedActor <! text
                | FileError(filename, reason) ->
                    reportedActor <! (sprintf "Tail error: %s" reason)
                | InitialRead(filename, text) ->
                    reportedActor <! InputSuccess text

                return! loop ()
            }
        loop ()
    TailActor

    
