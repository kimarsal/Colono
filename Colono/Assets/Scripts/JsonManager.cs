using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonManager : MonoBehaviour
{
    public enum FileTypes { Ship, Island };

    private string directoryPath;
    public 

    void Awake()
    {
        directoryPath = Application.persistentDataPath;
    }

    private string ReadFile(string path)
    {
        if (File.Exists(path))
        {
            return File.ReadAllText(directoryPath);
        }
        return null;
    }

    public void WriteFile(string path, string content)
    {
        File.WriteAllText(path, content);
    }
}