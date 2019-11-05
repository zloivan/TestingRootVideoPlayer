﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;
using Console = DefaultNamespace.Console;

public class VideoHost : MonoBehaviour
{
//Server url
        [SerializeField] private string url = "http://127.0.0.1:8080/";
        public bool IsInitialized { get; private set; }

        public string Url => url;

        //Types of media supported(Can be extended)
        private static IDictionary<string, string> mimeDic =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                {".asf", "video/x-ms-asf"},
                {".asx", "video/x-ms-asf"},
                {".avi", "video/x-msvideo"},
                {".flv", "video/x-flv"},
                {".jpeg", "image/jpeg"},
                {".jpg", "image/jpeg"},
                {".mng", "video/x-mng"},
                {".mov", "video/quicktime"},
                {".mp3", "audio/mpeg"},
                {".mpeg", "video/mpeg"},
                {".mp4", "video/mp4"},
                {".mpg", "video/mpeg"},
                {".ra", "audio/x-realaudio"},
                {".swf", "application/x-shockwave-flash"},
                {".wbmp", "image/vnd.wap.wbmp"},
                {".wmv", "video/x-ms-wmv"},
            };

        List<HttpListenerResponse> httpResponses = new List<HttpListenerResponse>();
        Thread listeningThread;

        void Awake()
        {
            Application.runInBackground = true;
        }

        private Action OnInitialized;
        public void Initialize(string filePath, Action onInit = null)
        {
            if (onInit != null)
            {
                OnInitialized = onInit;
            }

            Console.instance.Log("FilePath:"+filePath);
            //Your Video Path

            //Create Server
            StartHttpServer(filePath);
        }

        void StartHttpServer(string dataPath)
        {
            Console.instance.Log("StartHttpServer");
            listeningThread = new Thread(new ParameterizedThreadStart(ListenToClient));
            listeningThread.IsBackground = true;
            listeningThread.Start(dataPath);
            
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        void StopHttpServer()
        {
            //Stop thread
            if (listeningThread != null && listeningThread.IsAlive)
            {
                listeningThread.Abort();
            }
        }

        void DisconnectClients()
        {
            //Disconnect from each connected client
            for (int i = 0; i < httpResponses.Count; i++)
            {
                if (httpResponses[i] != null)
                {
                    httpResponses[i].StatusDescription = "Server done";
                    httpResponses[i].OutputStream.Close();
                }
            }
        }

        void ListenToClient(object path)
        {
            //Get the param
            string dataPath = (string) path;

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();


            while (true)
            {
                HttpListenerContext context = listener.GetContext();

                //Construct param that will be sent to the Thread
                ServerParamData serverData = new ServerParamData(context, dataPath);
                ThreadPool.QueueUserWorkItem(new WaitCallback(RunInNewThread), serverData);
            }
        }

        private void RunInNewThread(object ctx)
        {
            //Get the param
            ServerParamData serverData = (ServerParamData) ctx;

            //Open the file and start sending it to the client
            WriteFile(serverData.context, serverData.path);
        }

        void WriteFile(HttpListenerContext ctx, string path)
        {
            HttpListenerResponse response = ctx.Response;
            httpResponses.Add(response);

            using (FileStream fs = File.OpenRead(path))
            {
                string filename = Path.GetFileName(path);
                string mime;

                //Set the type of media to play
                if (!mimeDic.TryGetValue(Path.GetExtension(filename), out mime))
                    mime = "application/octet-stream";


                ctx.Response.ContentType = mime;
                response.ContentLength64 = fs.Length;

                //Stream the File
                response.SendChunked = true;

                //Enable Media Seek(Rewind/Fastforward)
                response.StatusCode = 206;
                response.AddHeader("Content-Range", "bytes 0-" + (fs.Length - 1) + "/" + fs.Length);
                //According to Content Range
                //https://greenbytes.de/tech/webdav/rfc7233.html#header.content-range


                //Send data to the connected client
                byte[] buffer = new byte[64 * 1024];
                int read;
                using (BinaryWriter bw = new BinaryWriter(response.OutputStream))
                {
                    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bw.Write(buffer, 0, read);
                        bw.Flush(); //seems to have no effect
                    }

                    bw.Close();
                }

                response.StatusCode = (int) HttpStatusCode.OK;
                response.StatusDescription = "OK";
                response.OutputStream.Close();
            }
        }

        void OnDisable()
        {
            //Clean Up
            StopHttpServer();
            DisconnectClients();
            IsInitialized = false;
        }

        private void OnApplicationQuit()
        {
            KillHost();
        }

        public void KillHost()
        {
            StopHttpServer();
            DisconnectClients();
            IsInitialized = false;
        }

        //Holds multiple params sent to a function in another Thread
        public class ServerParamData
        {
            public HttpListenerContext context;
            public string path;

            public ServerParamData(HttpListenerContext context, string path)
            {
                this.context = context;
                this.path = path;
            }
        }
    
}