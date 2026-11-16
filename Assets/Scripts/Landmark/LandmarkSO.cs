using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Landmark", menuName = "ScriptableObjects/Landmark")]
public class LandmarkSO : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private List<LandmarkSpawnData> _associatedFishes;
    [SerializeField] private Mesh _mesh;

    [Header("Tier chances (always sum to 1)")]
    [Range(0, 1)][SerializeField] private float _tier1SpawnChance = 0.5f;
    [Range(0, 1)][SerializeField] private float _tier2SpawnChance = 0.25f;
    [Range(0, 1)][SerializeField] private float _tier3SpawnChance = 0.15f;
    [Range(0, 1)][SerializeField] private float _tier4SpawnChance = 0.08f;
    [Range(0, 1)][SerializeField] private float _tier5SpawnChance = 0.02f;

    [System.NonSerialized] private float[] _previousChances;

    public float[] TierChances => new[]
    {
        _tier1SpawnChance, _tier2SpawnChance, _tier3SpawnChance,
        _tier4SpawnChance, _tier5SpawnChance
    };

    private void OnValidate()
    {
        float[] current = TierChances;

        if (_previousChances == null)
        {
            Normalize(current);
        }
        else
        {
            int changed = -1;
            for (int i = 0; i < current.Length; i++)
                if (!Mathf.Approximately(current[i], _previousChances[i])) { changed = i; break; }

            if (changed >= 0)
            {
                float remaining = 1f - current[changed];
                float othersSum = 0f;
                for (int i = 0; i < current.Length; i++)
                    if (i != changed) othersSum += _previousChances[i];

                for (int i = 0; i < current.Length; i++)
                {
                    if (i == changed) continue;
                    current[i] = othersSum > 0f
                        ? _previousChances[i] / othersSum * remaining
                        : remaining / (current.Length - 1);
                }
            }
        }

        _tier1SpawnChance = current[0];
        _tier2SpawnChance = current[1];
        _tier3SpawnChance = current[2];
        _tier4SpawnChance = current[3];
        _tier5SpawnChance = current[4];

        _previousChances = current;
    }

    private static void Normalize(float[] values)
    {
        float sum = 0f;
        foreach (var v in values) sum += v;
        for (int i = 0; i < values.Length; i++)
            values[i] = sum > 0f ? values[i] / sum : 1f / values.Length;
    }
}