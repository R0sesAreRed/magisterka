using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyboardManager : MonoBehaviour
{
    public static KeyboardManager instance;

    [SerializeField] public GameObject[] Keys; //tablica przechowuj¹ca klawisze do przypiêcia
    public Dictionary<GameManager.NK, GameObject> KeysDict; //s³ownik mapuj¹cy klawisze do nazw nut
    private Dictionary<GameManager.NK, BoxCollider2D> KeyColliders; //s³ownik mapuj¹cy collidery klawiszy do nazw nut
    public Dictionary<GameManager.NK, GameObject> KeyVisualsDict; //s³ownik trzymaj¹cy wizualn¹ czêœæ klawiszy


    private Dictionary<GameManager.NK, Action<InputAction.CallbackContext>> performedDelegates; //s³ownik przechowuj¹cy delegaty dla zdarzeñ "performed"
    private Dictionary<GameManager.NK, Action<InputAction.CallbackContext>> canceledDelegates; //s³ownik przechowuj¹cy delegaty dla zdarzeñ "canceled"
    private Dictionary<GameManager.NK, float> lastCollisionTime = new(); //s³ownik przechowuj¹cy czas ostatniej kolizji dla ka¿dej nuty
    private Dictionary<GameManager.NK, float> lastKeyPressTime = new(); //s³ownik przechowuj¹cy czas ostatniego naciœniêcia klawisza dla ka¿dej nuty

    public float ScreenHeight = 0;
    private void Awake()
    {

        if (instance == null && instance != this)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        } //dok instancji
        ScreenHeight = this.GetComponent<RectTransform>().rect.height;
        KeysDict = new Dictionary<GameManager.NK, GameObject>();
        KeyColliders = new Dictionary<GameManager.NK, BoxCollider2D>();
        KeyVisualsDict = new Dictionary<GameManager.NK, GameObject>();

        for (int i = 0; i < Keys.Length && i < Enum.GetValues(typeof(GameManager.NK)).Length; i++)
        {
            KeysDict[(GameManager.NK)i] = Keys[i];
            KeysDict[(GameManager.NK)i].name = ((GameManager.NK)i).ToString();
            KeyColliders[(GameManager.NK)i] = Keys[i].GetComponentInChildren<BoxCollider2D>();
            KeyVisualsDict[(GameManager.NK)i] = Keys[i].transform.GetChild(0).gameObject;
            Debug.Log(KeyVisualsDict[(GameManager.NK)i]);
            var detector = Keys[i].GetComponent<KeyCollisionDetector>();
            if (detector != null)
            {
                detector.note = (GameManager.NK)i;
            }
        }
    }

    //private IEnumerator Start()
    //{
    //    yield return null; // odczekaj jedn¹ klatkê, a¿ layout siê przeliczy
    //    for (int i = 0; i < Keys.Length && i < Enum.GetValues(typeof(GameManager.NK)).Length; i++)
    //    {
    //        Debug.Log(Keys[i].transform.position.x);
    //    }
    //}


    private void OnEnable() //przypisuje funkcje
    {
        GameManager.instance.inputActions.Piano.Enable();
        performedDelegates = new();
        canceledDelegates = new();

        foreach (GameManager.NK note in Enum.GetValues(typeof(GameManager.NK)))
        {
            performedDelegates[note] = ctx => OnKeyPerformed(note, ctx);
            canceledDelegates[note] = ctx => OnKeyCanceled(note, ctx);

            var piano = GameManager.instance.inputActions.Piano;
            var action = piano.GetType().GetProperty(note.ToString())?.GetValue(piano) as InputAction;
            if (action != null)
            {
                action.performed += performedDelegates[note];
                action.canceled += canceledDelegates[note];
            }
        }
    }

    private void OnDisable() //odpiananie funkcji przy wy³¹czeniu skryptu
    {
        foreach (GameManager.NK note in Enum.GetValues(typeof(GameManager.NK)))
        {
            var piano = GameManager.instance.inputActions.Piano;
            var action = piano.GetType().GetProperty(note.ToString())?.GetValue(piano) as InputAction;
            if (action != null)
            {
                action.performed -= performedDelegates[note];
                action.canceled -= canceledDelegates[note];
            }
        }
        GameManager.instance.inputActions.Piano.Disable();
    }


    private void OnKeyPerformed(GameManager.NK note, InputAction.CallbackContext ctx) //rozpoczyna granie nuty przy wciœniêciu klawisza do przypiêcia
    {
        KeyPressColor(ctx, note);
        float pressTime = Time.time;
        lastKeyPressTime[note] = pressTime;
        //Debug.Log($"Klawisz {note} naciœniêty.");

        if (lastCollisionTime.TryGetValue(note, out float collisionTime))
        {
            float delta = pressTime - collisionTime;
            if (delta >= -0.1f && delta <= 0.1f)
            {
                GameUIManager.instance.Score += 10;
                lastCollisionTime.Remove(note); //zapobiega podwójnym punktom
                //Debug.Log($"Punkty: {GameManager.instance.Score}");
            }
            else
            {
                //Debug.Log($"Klawisz {note} naciœniêty {delta:F3}s po kolizji (za wczeœnie/za póŸno)");
            }
            //Debug.Log($"Czas naciœniêcia: {pressTime:F3}s, Czas kolizji: {collisionTime:F3}s, Ró¿nica: {delta:F3}s");
        }
    }
    private void OnKeyCanceled(GameManager.NK note, InputAction.CallbackContext ctx) //zatrzymuje gran¹ nutê przy wypuszczeniu klawisza do przypiêcia
    {
        KeyReleaseColor(ctx, note);

    }

    private void KeyPressColor(InputAction.CallbackContext context, GameManager.NK note)
    {
        KeyVisualsDict[note].GetComponent<Image>().color = KeyVisualsDict[note].GetComponent<Image>().color == Color.white ? Color.gray : Color.blue;
    }
    private void KeyReleaseColor(InputAction.CallbackContext context, GameManager.NK note)
    {
        KeyVisualsDict[note].GetComponent<Image>().color = KeyVisualsDict[note].GetComponent<Image>().color == Color.gray ? Color.white : Color.black;
    }


    public void RegisterCollision(GameManager.NK note, float time)
    {
        lastCollisionTime[note] = time;

        if (lastKeyPressTime.TryGetValue(note, out float pressTime))
        {
            float delta = pressTime - time;
            if (delta >= -0.1f && delta <= 0.1f)
            {
                GameUIManager.instance.Score += 10;
                lastKeyPressTime.Remove(note); //zapobiega podwójnym punktom
                //Debug.Log($"Punkty (kolizja po naciœniêciu): {GameManager.instance.Score}");
            }
        }
    }

    public void CheckMissedNote(GameManager.NK note, float exitTime)
    {
        // Jeœli w s³owniku lastCollisionTime nadal jest wpis dla tej nuty,
        // oznacza to, ¿e nie by³o poprawnego naciœniêcia w oknie czasowym
        if (lastCollisionTime.ContainsKey(note))
        {
            //Debug.Log($"Nuta {note} zosta³a pominiêta! (miss)");
            lastCollisionTime.Remove(note);
        }
    }

}
