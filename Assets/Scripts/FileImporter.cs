using UnityEngine;
using System.IO;
using System.Collections.Generic;
using SFB; // Simple File Browser plugin

public class FileImporter : MonoBehaviour
{
    [SerializeField] private SongSaveSystem saveSystem = new SongSaveSystem();
    [SerializeField] private PopulateSongList songList;
    void Start()
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

            // Copy imported file into a runtime-safe folder so the app can access it across sessions
            string importDir = Path.Combine(Application.persistentDataPath, "ImportedMidis");
            if (!Directory.Exists(importDir))
            {
                Directory.CreateDirectory(importDir);
            }

            string destPath = Path.Combine(importDir, Path.GetFileName(selectedPath));
            try
            {
                File.Copy(selectedPath, destPath, true);
                Debug.Log($"Imported MIDI copied to: {destPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to copy imported MIDI: {ex.Message}. Falling back to original path.");
                destPath = selectedPath;
            }

            item.Title = Path.GetFileNameWithoutExtension(destPath);
            item.FilePath = destPath;
            item.BestScore = 0;
            item.Level = 0; //TODO: zrobi� �eby level by� liczony na podstawie trudno�ci piosenki
            item.added = true;

            GameManager.instance.importedFiles.Add(item);
            Debug.Log("added song" + item.Title);
            songList.RefreshList();
            saveSystem.Save(GameManager.instance.importedFiles);
        }
    
        
    }
}
