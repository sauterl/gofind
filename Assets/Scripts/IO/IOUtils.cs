using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Assets.Modules.SimpleLogging;
using UnityEngine;

namespace Assets.Scripts.IO
{
    public class IOUtils
    {
        public const string FILE_NAME = "targets.json";

        public const string HEADINGS_FILE_NAME = "headings.json";


        private static Assets.Modules.SimpleLogging.Logger logger = LogManager.GetInstance().GetLogger(typeof(IOUtils));

        public static bool ExistsPersitentData()
        {
            return File.Exists(GetFilePath());
        }

        public static SaveState LoadData(bool deleteAfterRead = false)
        {
            SaveState state = null;
            if (ExistsPersitentData()) {
                state = ReadJson<SaveState>(GetFilePath());
                logger.Debug("Read savestate from file");
                if (deleteAfterRead) {
                    File.Delete(GetFilePath());
                    logger.Debug("Deleted savestate file after reading");
                }

            }
            return state;
        }

        public static void RemoveData() {
            File.Delete(GetFilePath());
            logger.Debug("Removed data file");
            logger.Debug("Did data file survive: "+(ExistsPersitentData() ? "yes" : "no"));
        }

        private static void WriteJson(string json, string path) {
            StreamWriter sw = File.CreateText(path);
            sw.WriteLine(json);
            sw.Flush();
            sw.Close();
            logger.Info("Wrote successfully to "+path);
        }

        private static T ReadJson<T>(string path) {
            StreamReader sr = File.OpenText(path);
            string content = sr.ReadToEnd();
            sr.Close();
            return UnityEngine.JsonUtility.FromJson<T>(content);
        }

        public static void StoreData(SaveState state)
        {
            WriteJson(UnityEngine.JsonUtility.ToJson(state), GetFilePath());
            logger.Info("Wrote SaveState to file.\nFile: "+GetFilePath() +"\nContent: "+UnityEngine.JsonUtility.ToJson(state));
        }

        public static bool ExistsHeadingFile()
        {
            return File.Exists(GetHeadingsPath());
        }

        public static void StoreHeadings(HeadingDictionary headings)
        {
            WriteJson(UnityEngine.JsonUtility.ToJson(headings),GetHeadingsPath());
            logger.Info("Wrote headings dictionary to file.");
        }

        public static HeadingDictionary LoadHeadings(bool deleteAfterRead=false)
        {
            HeadingDictionary ret = null;
            if (ExistsHeadingFile()) {
                ret = ReadJson<HeadingDictionary>(GetHeadingsPath());
                logger.Debug("Successfully read headings from file");
                if (deleteAfterRead)
                {
                    File.Delete(GetHeadingsPath() );
                    logger.Debug("Deleted headings file after reading");
                }
            }
            return ret;
        }

        private static string GetFilePath()
        {
            return Application.persistentDataPath + "/" + FILE_NAME;
        }

        public static string GetHeadingsPath()
        {
            return Application.persistentDataPath + "/" + HEADINGS_FILE_NAME;
        }
    }
}