using System;
using System.Collections.Generic;
using UnityEngine;

public enum EFishRarity
{
    NONE,
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

    public FishSO FishSO { get => _fishSO; set => _fishSO = value; }
    public EFishRarity Rarity { get => _rarity; set => _rarity = value; }
}