using System;
using UnityEngine;

public class FishModel : MonoBehaviour
{
    // For testing purposes, we can assign a FishSO in the inspector to create a FishData instance.
    [SerializeField] private FishSO _fishSO;
    private FishData _fishData;

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        
    }

    private void Initialize()
    {
        if (_fishSO != null)
        {
            _fishData = new FishData(_fishSO);
        }
    }
}
