module FileObserver

open Akka.Actor
open System
open System.IO
open Messages


type FileObserver(tailActor: IActorRef, absoluteFilePath: string) =
    let fileDir = Path.GetDirectoryName(absoluteFilePath)
    let fileName = Path.GetFileName(absoluteFilePath)

    let _watcher = new FileSystemWatcher(fileDir, fileName)

    let OnFileError(sender: obj) (e: ErrorEventArgs) =
        tailActor.Tell(FileError(fileName, e.GetException().Message), ActorRefs.NoSender)
            
    let OnFileChanged (sender: obj) (e: FileSystemEventArgs) =
        if (e.ChangeType = WatcherChangeTypes.Changed) then
            tailActor.Tell(FileWrite(e.Name), ActorRefs.NoSender);

    member this.Start() =
        _watcher.NotifyFilter <- NotifyFilters.FileName ||| NotifyFilters.LastWrite
        
        _watcher.Changed.AddHandler(new FileSystemEventHandler(OnFileChanged))
        _watcher.Error.AddHandler(new ErrorEventHandler(OnFileError))

        _watcher.EnableRaisingEvents = true;

    interface IDisposable with
        member self.Dispose() =
            _watcher.Dispose()