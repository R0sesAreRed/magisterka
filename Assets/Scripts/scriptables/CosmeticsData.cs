using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "CosmeticsData", menuName = "Scriptable Objects/CosmeticsData")]
public class CosmeticsData : ScriptableObject
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
}

public enum CosmeticType
{
    Background,
    NoteSkin,
    KeySkin,
    Font
}

//progressbar??