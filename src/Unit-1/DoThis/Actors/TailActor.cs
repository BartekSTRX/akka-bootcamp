using System.IO;
using System.Text;
using Akka.Actor;
using WinTail.WinTail;

namespace WinTail.Actors
{
    public class TailActor : UntypedActor
    {
        private readonly IActorRef _reportedActor;
        private readonly string _filePath;
        private FileObserver _observer;
        private FileStream _fileStream;
        private StreamReader _fileStreamReader;

        public TailActor(IActorRef reportedActor, string filePath)
        {
            _reportedActor = reportedActor;
            _filePath = filePath;
        }

        protected override void PreStart()
        {
            _observer = new FileObserver(Self, Path.GetFullPath(_filePath));
            _observer.Start();

            _fileStream = new FileStream(Path.GetFullPath(_filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(_filePath, text));
        }

        protected override void PostStop()
        {
            _observer.Dispose();
            _observer = null;

            _fileStreamReader.Close();
            _fileStreamReader.Dispose();

            base.PostStop();
        }

        protected override void OnReceive(object message)
        {
            if (message is FileWrite)
            {
                var text = _fileStreamReader.ReadToEnd();

                if (!string.IsNullOrEmpty(text))
                {
                    _reportedActor.Tell(text);
                }
            }
            else if (message is FileError)
            {
                var fe = (FileError)message;
                _reportedActor.Tell($"Tail error: {fe.Reason}");
            }
            else if (message is InitialRead)
            {
                var ir = (InitialRead)message;
                _reportedActor.Tell(ir.Text);
            }
        }

        public class FileError
        {
            public FileError(string fileName, string reason)
            {
                FileName = fileName;
                Reason = reason;
            }

            public string FileName { get; set; }
            public string Reason { get; set; }
        }

        public class FileWrite
        {
            public FileWrite(string fileName)
            {
                FileName = fileName;
            }

            public string FileName { get; set; }
        }

        public class InitialRead
        {
            public InitialRead(string fileName, string text)
            {
                FileName = fileName;
                Text = text;
            }

            public string FileName { get; set; }
            public string Text { get; set; }
        }
    }
}