using System;
using UnityEngine;

public struct FishData
{
    [SerializeField] private ESpecy _specy;
    [SerializeField] private EFishSubspecy _subspecy;
    [SerializeField] private float _size; // Size in centimeters
    [SerializeField] private int _weight; // Weight in grams (used for blackjack)
    [SerializeField] private Mesh _mesh;

    public FishData(FishSO fishSO)
    {
        _specy = fishSO.Specy;
        _subspecy = fishSO.Subspecy;
        _size = fishSO.BaseSize + UnityEngine.Random.Range(-fishSO.SizeVariationRange * fishSO.BaseSize, fishSO.SizeVariationRange * fishSO.BaseSize);
        _weight = fishSO.Weight;
        _mesh = fishSO.Mesh;
    }

    public FishData(FishData fishData)
    {
        _specy = fishData.Specy;
        _subspecy = fishData.Subspecy;
        _size = fishData.Size;
        _weight = fishData.Weight;
        _mesh = fishData.Mesh;
    }

    public FishData(ESpecy specy, EFishSubspecy subspecy, float size, int weight, Mesh mesh)
    {
        _specy = specy;
        _subspecy = subspecy;
        _size = size;
        _weight = weight;
        _mesh = mesh;
    }

    public ESpecy Specy { get => _specy; set => _specy = value; }
    public EFishSubspecy Subspecy { get => _subspecy; set => _subspecy = value; }
    public float Size { get => _size; set => _size = value; }
    public int Weight { get => _weight; set => _weight = value; }
    public Mesh Mesh { get => _mesh; set => _mesh = value; }
}

[CreateAssetMenu(fileName = "New Fish", menuName = "ScriptableObjects/Fish")]
public class FishSO : ScriptableObject
{
    [SerializeField] private ESpecy _specy;
    [SerializeField] private EFishSubspecy _subspecy;
    [SerializeField] private float _baseSize; // Size in centimeters
    [SerializeField] private int _weight; // Used for blackjack
    [SerializeField] private Mesh _mesh;
    [SerializeField] private float _sizeVariationRange = 0.5f; // Percentage of size variation (e.g., 0.5 for ±50% variation)

    public ESpecy Specy { get => _specy; set => _specy = value; }
    public EFishSubspecy Subspecy { get => _subspecy; set => _subspecy = value; }
    public float BaseSize { get => _baseSize; set => _baseSize = value; }
    public int Weight { get => _weight; set => _weight = value; }
    public Mesh Mesh { get => _mesh; set => _mesh = value; }
    public float SizeVariationRange { get => _sizeVariationRange; set => _sizeVariationRange = value; }
}
