using UnityEngine;
using System.IO;
using System.Collections.Generic;
using SFB; // Simple File Browser plugin

public class FileImporter : MonoBehaviour
{
    [SerializeField] private SongSaveSystem saveSystem = new SongSaveSystem();

    void Awake()
    {
        GameManager.instance.importedFiles = saveSystem.Load();
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ImportFile()
    {
        var extensions = new[] {
        new ExtensionFilter("MIDI Files", "mid", "midi"),
        new ExtensionFilter("All Files", "*" ),
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel(
            "Select MIDI File",
            "",
            extensions,
            false
        );

        if (paths.Length > 0)
        {
            string selectedPath = paths[0];
            Debug.Log("Selected file: " + selectedPath);

            var item = ScriptableObject.CreateInstance<SelectSongItem>();

            item.Title = Path.GetFileNameWithoutExtension(selectedPath);
            item.FilePath = selectedPath;
            item.HighScore = 0;
            item.MaxScore = 0;
            item.added = true;

            GameManager.instance.importedFiles.Add(item);

            saveSystem.Save(GameManager.instance.importedFiles);
        }
    }
}
