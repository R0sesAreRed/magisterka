using UnityEngine;

public class KeyCollisionDetector : MonoBehaviour
{
    public GameManager.NK note;

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"Kolizja na klawiszu {note} z {other.gameObject.name}");
        KeyboardManager.instance.RegisterCollision(note, Time.time);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        KeyboardManager.instance.CheckMissedNote(note, Time.time);
    }
}