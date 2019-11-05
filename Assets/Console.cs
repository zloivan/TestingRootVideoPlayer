using System;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class Console : MonoBehaviour
    {
        [SerializeField] private Text console;

        public static Console instance;

        private void Awake()
        {
            instance = this;
        }

        public void Log(string text)
        {
            console.text += $"\n{text}";
        }
        
        public void Rewrite(string text)
        {
            console.text = $"\n{text}";
        }
    }
}