using System;
using UnityEngine;

public class FishModel : MonoBehaviour
{
    // For testing purposes, we can assign a FishSO in the inspector to create a FishData instance.
    [SerializeField] private FishSO _fishSO; //Debug
    private FishData _fishData;

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_fishSO != null)
        {
            _fishData = new FishData(_fishSO);
        }
    }
}
