using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace FileSystemWatcher.Services
{
    public class FileSystemWatcher
    {
        private IList<FileInfo> _oldFileList = new List<FileInfo>();

        private readonly string _wachingFilePath;

        Timer _timer;

        public delegate void NewFileEventHandler(object sender, NewFileEventArgs e);
        public event NewFileEventHandler NewFileCreated;


        private void RaiseNewFileEvent(FileInfo fileInfo)
        {
            NewFileCreated?.Invoke(this, new NewFileEventArgs(fileInfo));
        }

        public FileSystemWatcher(string wachingFilePath, TimeSpan Duration)
        {
            _wachingFilePath = wachingFilePath;

            //_oldFileList = GetFileRecusive();
            _oldFileList = new List<FileInfo>();

            _timer = new Timer(Duration.TotalMilliseconds);

            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = false;
            _timer.Start();


        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckFile();
            _timer.Start();
        }

        private List<FileInfo> GetFileRecusive()
        {

            var List = new List<FileInfo>();

            foreach (var File in Directory.GetFiles(_wachingFilePath, "*.*", SearchOption.AllDirectories))
            {
                var fileInfo = new FileInfo(File);
                List.Add(fileInfo);
            }

            return List;
        }

        private void CheckFile()
        {
            var NewList = GetFileRecusive();


            if (_oldFileList.Count > NewList.Count)
            {
                _oldFileList = NewList;
                return;
            }

            var compared = NewList.Where(p => !_oldFileList.Any(l => p.FullName == l.FullName)).ToList();

            foreach (var fileInfo in compared)
            {
                RaiseNewFileEvent(fileInfo);
            }


            _oldFileList = NewList;
            
        }




    }

    public class NewFileEventArgs
    {
        public FileInfo FileInfo { get; private set; }

        public NewFileEventArgs(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
        }
    }


}
