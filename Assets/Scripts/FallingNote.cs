using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class FallingNote : MonoBehaviour
{
    private float noteStartTime; // w sekundach od startu utworu
    private float noteLength;    // w sekundach
    private float fallSpeed;     // px/s
    private float spawnY;        // pozycja Y, z której nuta startuje
    private float destroyY = -4000f; //temporary
    public void Init(double start, double len, float spawnY)
    {
        noteStartTime = (float)start / 1000f; // zamiana ms na s
        noteLength = (float)len / 1000f;
        fallSpeed = KeyboardManager.instance.ScreenHeight / 2f;
        this.spawnY = spawnY;
        CosmeticsData noteskin = GameManager.instance.playerEquippedCosmetics.Find(c => c.type == CosmeticType.NoteSkin);
        this.GetComponent<Image>().sprite = noteskin.sprite;
        this.GetComponent<Image>().type = noteskin.tiled? Image.Type.Tiled : Image.Type.Sliced;
    }


    void Update() //rusza nut¹ w dó³ na podstawie czasu jaki mina³ od poprzedniej klatki (nie zale¿nie od frameratu)
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
}
