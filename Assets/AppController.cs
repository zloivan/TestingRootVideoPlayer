using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;
using Console = DefaultNamespace.Console;

public class AppController : MonoBehaviour
{
    [SerializeField] private VideoHost videoHost;
    [SerializeField] private VideoHostPlayer player;

    [SerializeField] private InputField fileName;
    [SerializeField] private DownloadSystem downlaodSystem;

    private string filePath;

    public void OnDownloadButtonClick()
    {
        filePath = fileName.text;
        downlaodSystem.StartDownload(filePath, OnFinished);
    }

    private void OnFinished(string obj)
    {
        Console.instance.Rewrite(obj);
    }

    public void OnStartPlayButtonClick()
    {
        filePath = fileName.text;
        videoHost.Initialize(PathHelper.GetLocalPath(filePath), OnInit);
    }

    public void OnKillHostClicked()
    {
        videoHost.KillHost();
    }

    private void OnInit()
    {
        player.StartPlayFromHost(videoHost.Url);
    }
}