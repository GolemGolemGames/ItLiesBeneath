using System;
using System.Collections.Generic;
using UnityEngine;

public enum EFishRarity
{
    TIER1,
    TIER2,
    TIER3,
    TIER4,
    TIER5
}

[Serializable]
public class LandmarkSpawnData
{
    [SerializeField] private FishSO _fishSO;
    [SerializeField] private EFishRarity _rarity;
}