using Melanchall.DryWetMidi.Interaction;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
[System.Serializable]
public class CosmeticsDataClass
{
    public int id;
    public string itemName;
    public CosmeticType type;
    public int CurrencyCost;
    public int SetNumber;
    public TMP_FontAsset? font;
    public Sprite? sprite;
    public Sprite[]? keySprites;
    public Color colorWhite;
    public Color colorBlack;

    public CosmeticsDataClass(CosmeticsData item)
    {
        id = item.id;
        itemName = item.itemName;
        type = item.type;
        CurrencyCost = item.CurrencyCost;
        SetNumber = item.SetNumber;
        font = item.font; // mo¿e byæ null
        sprite = item.sprite; // mo¿e byæ null
        keySprites = item.keySprites;
        colorWhite = item.colorWhite; // sprawdŸ konwencjê na "brak koloru"
        colorBlack = item.colorBlack; // sprawdŸ konwencjê na "brak koloru"
    }

    public CosmeticsData ToScriptableObject()
    {
        CosmeticsData copy = ScriptableObject.CreateInstance<CosmeticsData>();
        copy.id = id;
        copy.itemName = itemName;
        copy.type = type;
        copy.CurrencyCost = CurrencyCost;
        copy.SetNumber = SetNumber;
        copy.font = font; // mo¿e byæ null
        copy.sprite = sprite; // mo¿e byæ null
        copy.keySprites = keySprites;
        copy.colorWhite = colorWhite; // sprawdŸ konwencjê na "brak koloru"
        copy.colorBlack = colorBlack; // sprawdŸ konwencjê na "brak koloru"
        return copy;
    }
}