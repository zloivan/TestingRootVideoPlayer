using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
    public class PathHelper
    {
        private const string hnpContentServerURL = "https://server.happynotperfect.com//assets/video/";

        public static string GetLocalPath(string id)
        {
            var localPath = Path.Combine(Application.persistentDataPath, id + ".mp4");
            return localPath;
        }
        
        public static string GetWebPath(string id)
        {
            var result = Path.Combine(hnpContentServerURL, id + ".mp4");
            return result;
        }
    }
}