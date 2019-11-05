using System.Collections;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoHostPlayer : MonoBehaviour {


        //Raw Image to Show Video Images [Assign from the Editor]
        [SerializeField] private RawImage image;

        [SerializeField] private VideoPlayer videoPlayer;
        private VideoSource videoSource;

        //Audio
        [SerializeField] private AudioSource audioSource;


        //Server url
        private string url = "http://127.0.0.1:8080/";

        public RawImage Image { get; set; }
    

        public VideoPlayer Player
        {
            get { return videoPlayer; }
            set { videoPlayer = value; }
        }

        public AudioSource Source
        {
            get { return audioSource; }
            set { audioSource = value; }
        }

        // Use this for initialization
        void Awake()
        {
            Application.runInBackground = true;
        }

        public void StartPlayFromHost(string hostUrl)
        {
            url = hostUrl;
            StartCoroutine(PlayVideo());
        }

        IEnumerator PlayVideo()
        {
            
            if (videoPlayer == null)
            {
                //Add VideoPlayer to the GameObject
                videoPlayer = gameObject.AddComponent<VideoPlayer>();  
            }

            if (audioSource == null)
            {
                //Add AudioSource
                audioSource = gameObject.AddComponent<AudioSource>(); 
            }
            

            //Disable Play on Awake for both Video and Audio
            videoPlayer.playOnAwake = false;
            audioSource.playOnAwake = false;

            //We want to play from url
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = url;

            //Set Audio Output to AudioSource
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

            //Assign the Audio from Video to AudioSource to be played
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audioSource);

            //Prepare Audio to prevent Buffering
            videoPlayer.Prepare();

            //Wait until video is prepared
            while (!videoPlayer.isPrepared)
            {
                Console.instance.Log("Preparing Video");
                yield return null;
            }

            Console.instance.Log("Done Preparing Video");

            //Assign the Texture from Video to RawImage to be displayed
            image.texture = videoPlayer.texture;

            //Play Video
            videoPlayer.Play();

            //Play Sound
            audioSource.Play();

            Console.instance.Log("Playing Video");
            while (videoPlayer.isPlaying)
            {
                Debug.LogWarning("Video Time: " + Mathf.FloorToInt((float) videoPlayer.time));
                yield return null;
            }

            Console.instance.Log("Done Playing Video");
        }
}
