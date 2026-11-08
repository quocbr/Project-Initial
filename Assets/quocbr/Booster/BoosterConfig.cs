using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BoosterConfig", menuName = "SO/BoosterConfig")]
public class BoosterConfig : ScriptableObject
{
    public List<BoosterInfo> boosterInfos;
}

[Serializable]
public class BoosterInfo
{
    [Title("========= Base Info ==========", titleAlignment: TitleAlignments.Centered)]
    public BoosterType type;
    [Title("UI")]
    public string name;
    public string description;
    public int levelUnlock;
    public Sprite icon;
    public Sprite titleIcon;
    [Title("Gameplay")]
    public int amountFirst;
    public int price;
    public int amountBuy;
}
