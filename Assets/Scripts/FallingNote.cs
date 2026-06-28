using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class FallingNote : MonoBehaviour
{
    public GameManager.NK myNote;
    public float noteStartTime; // w sekundach od startu utworu
    public float noteLength;    // w sekundach
    private float fallSpeed;     // px/s
    private float spawnY;        // pozycja Y, z kt�rej nuta startuje
    private float destroyY = -4000f; //temporary

    private BoxCollider2D noteCollider;
    public bool hit = false;
    public bool chordHandler = true;
    public void Init(GameManager.NK note, double start, double len, float spawnY)
    {
        myNote = note;
        noteStartTime = (float)start / 1000f;
        noteLength = (float)len / 1000f;
        fallSpeed = KeyboardManager.instance.ScreenHeight / 2f;
        this.spawnY = spawnY;
    }


    void Update() //rusza nut� w d� na podstawie czasu jaki mina� od poprzedniej klatki (nie zale�nie od frameratu)
    {
        if (GameManager.instance.IsPaused)
            return;

        float songElapsed = Time.time - GameManager.instance.songStartTime;
        float timeSinceNoteStart = songElapsed - noteStartTime;
        if (timeSinceNoteStart < 0)
            return;
        float newY = spawnY - timeSinceNoteStart * fallSpeed;
        Vector3 pos = transform.localPosition;
        pos.y = newY;
        transform.localPosition = pos;
        //float noteHeight = noteLength * fallSpeed;
        if (newY < destroyY) //zmienic to na cos co ma sens
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(KeyboardManager.instance.isKeyPressed[myNote] == true)
        {
            Debug.Log("Setting hit to true for note " + myNote);
            if(GameManager.instance.notesHighlightingOn)
                gameObject.GetComponent<Image>().color = UnityEngine.Color.green;
            hit = true;
        }
        else if(GameManager.instance.notesHighlightingOn)
        {
            gameObject.GetComponent<Image>().color = UnityEngine.Color.red;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Exiting trigger. hit: " + hit + " chordHandler: " + chordHandler);
        if (KeyboardManager.instance.isKeyPressed[myNote] == true)
        {
            Debug.Log("Setting hit to true for note " + myNote);
            hit = true;
        }
        else if(!hit)
        {
            foreach(var note in FindObjectsByType<FallingNote>(FindObjectsSortMode.None).Where(note => note.myNote != myNote))
            {
                if ((note.noteStartTime == noteStartTime || note.noteStartTime+note.noteLength == noteStartTime + noteLength) && chordHandler)
                {
                    Debug.Log("Setting chordHandler to true for note " + myNote);
                    note.chordHandler = false;
                }

            }
            if(chordHandler)
            {   
                if(!GameManager.instance.comboOn)
                {
                    GameUIManager.instance.HealthPoints -= 1000;
                    GameUIManager.instance.audioSource.PlayOneShot(GameUIManager.instance.loseHP);
                }
                //combo tutaj ig 
            }

        }
    }
}
