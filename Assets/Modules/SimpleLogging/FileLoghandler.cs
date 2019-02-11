using System;
using System.IO;
using UnityEngine;

namespace Assets.Modules.SimpleLogging
{
    public class FileLogHandler : LogHandlingProvider
    {

        private LogLevel lvl;

        public void Log(LogRecord record)
        {
            writer.WriteLine(LogManager.Format(record, true));
            writer.Flush();
        }

        public void SetLevel(LogLevel level)
        {
            lvl = level;
        }

        public FileLogHandler(LogLevel level)
        {
            lvl = level;
            /*
            FileStream fs = File.Create(Path.Combine(Application.persistentDataPath, CreateLogFileName()));
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("Hello");
            sw.Flush();*/
            

            if (Application.isEditor)
            {
                Debug.Log("Editor, trying "+Application.dataPath);
                LOGGING_DIR = Application.dataPath /*+ DIR*/;
            }
            else
            {
                Debug.Log("Normal, trying "+Application.persistentDataPath);
                LOGGING_DIR = Application.persistentDataPath /*+ DIR*/;
            }

            Initialize();
        }

        private StreamWriter writer = null;

        private void Initialize()
        {
            /*// NOT WORKING
            System.IO.Directory.CreateDirectory(LOGGING_DIR);
            writer = System.IO.File.AppendText(LOGGING_DIR);*/

            fileName = CreateLogFileName();
            CreateFile();

            writer.WriteLine("Started logging @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            writer.Flush();

        }

        private string fileName;

        public void ReopenFile()
        {
            if (fileName != null)
            {
                if (closed)
                {
                   FileStream fs = File.OpenWrite(Path.Combine(LOGGING_DIR, fileName));
                    writer = new StreamWriter(fs); 
                }
                
            }
        }

        private void CreateFile()
        {
            if (fileName != null)
            {
                FileStream fs = File.Create(Path.Combine(LOGGING_DIR, fileName));
                writer = new StreamWriter(fs);
            }
        }

        private bool closed = false;

        public void CloseWriter()
        {
            writer.Close();
            closed = true;
        }

        private readonly string EXTENSION = ".log";

        private readonly string DIR = "/logs/";
        private readonly string LOGGING_DIR;

        private string CreateLogFilePath()
        {
            return LOGGING_DIR + CreateLogFileName();
        }

        private string CreateLogFileName()
        {
            return DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss-fff") + EXTENSION;
        }
    }
}