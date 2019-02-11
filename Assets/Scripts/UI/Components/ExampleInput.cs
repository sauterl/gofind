using System;
using System.Collections.Generic;
using System.IO;
using Assets.Modules.SimpleLogging;
using UnityEngine;

namespace Assets.Scripts.UI {
    
    public class ExampleInput : MonoBehaviour{
        public DeviceCam cam;

        public bool DebugStoreImage = true;
        public Action<string> PhotoCapturedHandler;
        
        public static string ToBase64DataUrl(Texture2D texture) {
            byte[] png = texture.EncodeToPNG();
            string data = Convert.ToBase64String(png);
            return PNG_DATA_PREFIX+ data;
        }

        public const string PNG_DATA_PREFIX = DATA_URL_PREFIX + IMAGE_PNG + DATA_URL_POST_IMAGE_SEQUENCE;

        public static Texture2D FromBase64DataUrl(string data) {
            if (data.StartsWith(PNG_DATA_PREFIX)) {
                data = data.Substring(PNG_DATA_PREFIX.Length);
            }

            byte[] png = Convert.FromBase64String(data);
            Texture2D texture = new Texture2D(100,100);
            texture.LoadImage(png);
            return texture;
        }
	
        private void OnEnable() {
            if (cam != null && cam.Cam != null) {
                cam.Cam.Play();
            }
        }


        private Modules.SimpleLogging.Logger logger;
	
        // Use this for initialization
        void Start () {
            logger = LogManager.GetInstance().GetLogger(GetType());
        }
	
        // Update is called once per frame
        void Update () {
		
        }

        public void TakePhoto() {
            StartCoroutine(CapturePhoto());
        }

        private string result;

        public string GetResult() {
            return result;
        }
	
        private IEnumerator<WaitForEndOfFrame> CapturePhoto() {
            yield return new WaitForEndOfFrame();

            WebCamTexture camera = cam.Cam;
            Texture2D photo = new Texture2D(camera.width, camera.height);
            photo.SetPixels32(camera.GetPixels32());
            photo.Apply();

            result = ToBase64DataUrl(photo);
            if (DebugStoreImage) {
                StoreImage(result);
            }
            logger.Debug("Stored image result as dataurl");
            if (PhotoCapturedHandler != null) {
                PhotoCapturedHandler.Invoke(result);
            }
        }

        public const string DATA_URL_PREFIX = "data:";
        public const string IMAGE_PNG = "image/png;";
        public const string IMAGE_JPEG = "image/jpeg;";
        public const string DATA_URL_POST_IMAGE_SEQUENCE = "base64,";
        
        
        
        // ======= DEBUG STORE IMAGE =========
        private void StoreImage(string img) {
            PrepareWriter();
            writer.WriteLine(img);
            writer.Flush();
            writer.Close();
        }
        
        private string imgPath;        
        private void PrepareWriter()
        {
            if (Application.isEditor)
            {
                imgPath = Application.dataPath;
            }
            else
            {
                imgPath = Application.persistentDataPath;
            }
            fileName = CreateImageFileName();
            if (fileName != null)
            {
                FileStream fs = File.Create(Path.Combine(imgPath, fileName));
                writer = new StreamWriter(fs);
            }
        }

        private StreamWriter writer = null;
        private string fileName;
        private readonly string EXTENSION = ".txt";

        private string CreateImageFileName()
        {
            return DateTime.Now.ToString("image-yyyy-MM-ddTHH-mm-ss-fff") + EXTENSION;
        }
    }
}