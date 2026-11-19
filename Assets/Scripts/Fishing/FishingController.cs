using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FishingController : MonoBehaviour
{
    [SerializeField] private List<LandmarkModel> _landmarkModels;
    [SerializeField] private Transform _player;

    private void Start()
    {
        // For testing purposes, we can call DrawFish() here to see the result in the console
        FishData fish;
        if (DrawFish(out fish))
        {
            Debug.Log(fish.Subspecy);
        }
    }

#if UNITY_EDITOR
[CustomEditor(typeof(FishingController))]
public class FishingControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Fishing test", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Lancez le jeu avant d'effectuer un tirage.",
                MessageType.Info);
        }

        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("Draw Fish"))
        {
            FishingController fishingController =
                (FishingController)target;

            fishingController.DrawFishFromEditor();
        }

        GUI.enabled = true;
    }
}
#endif

    private void Update()
    {
        
    }

    public void DrawFishFromEditor()
    {
        FishData fish;
        if (DrawFish(out fish))
        {
            Debug.Log($"Poisson obtenu : {fish.Subspecy}", this);
        }
    }

    private bool DrawFish(out FishData fish)
    {
        LandmarkModel selectedLandmark = GetRandomLandmarkByDistance(_player, _landmarkModels);
        if (selectedLandmark == null)
        {
            Debug.LogError("Aucun landmark valide n'a été trouvé.");
            fish = default(FishData);
            return false;
        }

        EFishRarity selectedRarity = GetRandomRarity(selectedLandmark);
        Debug.Log($"Rareté sélectionnée : {selectedRarity}");
        Debug.Log($"Landmark sélectionné : {selectedLandmark.LandmarkSO.name}");
        return GetRandomFish(selectedLandmark, selectedRarity, out fish);
    }

    private bool GetRandomFish(
        LandmarkModel selectedLandmark,
        EFishRarity selectedRarity,
        out FishData fish)
    {
        // Get a random fish from the selected rarity tier
        // Get all fishes of the selected rarity tier from the landmark's associated fishes
        List<LandmarkSpawnData> fishesOfSelectedRarity =    
        selectedLandmark.LandmarkSO.AssociatedFishes.FindAll(fish => fish.Rarity == selectedRarity);

        if (fishesOfSelectedRarity.Count == 0)
        {
            Debug.LogError($"Aucun poisson de rareté {selectedRarity} n'est configuré pour le landmark {selectedLandmark.LandmarkSO.name}.");
            fish = default(FishData);
            return false;
        }

        int randomFishIndex = Random.Range(0, fishesOfSelectedRarity.Count);
        fish = new FishData(fishesOfSelectedRarity[randomFishIndex].FishSO);
        return true;
    }

    // Get a random rarity tier based on the selected landmark's spawn chances
    private EFishRarity GetRandomRarity(LandmarkModel landmark)
    {
        float[] tierChances = landmark.LandmarkSO.TierChances;
        float totalChance = 0f;
        foreach (float chance in tierChances)
        {
            totalChance += Mathf.Max(0f, chance);
        }

        if (totalChance <= 0f)
        {
            Debug.LogWarning($"Aucune chance de rareté n'est configurée pour le landmark {landmark.LandmarkSO.name}. Retour de NONE.");
            return EFishRarity.NONE;
        }

        float randomValue = Random.Range(0f, totalChance);
        for (int i = 0; i < tierChances.Length; i++)
        {
            randomValue -= Mathf.Max(0f, tierChances[i]);
            if (randomValue <= 0f)
            {
                return (EFishRarity)(i + 1);
            }
        }

        Debug.LogWarning("Aucune rareté n'a été sélectionnée, retour de TIER5 par défaut.");
        return EFishRarity.TIER5;
    }

    // Get a random landmark based on the distance to the player
    private LandmarkModel GetRandomLandmarkByDistance(Transform player, List<LandmarkModel> landmarks)
    {
        if (player == null || landmarks == null || landmarks.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;
        LandmarkModel fallbackLandmark = null;

        foreach (LandmarkModel landmark in landmarks)
        {
            if (landmark == null || landmark.LandmarkSO == null)
            {
                continue;
            }

            float weight = GetLandmarkWeight(player, landmark);
            if (weight > 0f)
            {
                totalWeight += weight;
                fallbackLandmark = landmark;
            }
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float randomLandmarkValue = Random.Range(0f, totalWeight);
        foreach (LandmarkModel landmark in landmarks)
        {
            if (landmark == null || landmark.LandmarkSO == null)
            {
                continue;
            }

            randomLandmarkValue -= GetLandmarkWeight(player, landmark);
            if (randomLandmarkValue <= 0f)
            {
                return landmark;
            }
        }

        Debug.LogWarning("Aucun landmark n'a été sélectionné, retour du fallbackLandmark.");
        return fallbackLandmark;
    }

    private float GetLandmarkWeight(Transform player, LandmarkModel landmark)
    {
        LandmarkSO landmarkSO = landmark.LandmarkSO;
        float distance;
        if (landmarkSO.UseFixedDistance)
        {
            distance = landmarkSO.FixedDistance;
        }
        else
        {
            distance = Vector3.Distance(player.position, landmark.transform.position);
        }

        float safeDistance = Mathf.Max(0.01f, distance);
        return (1f / safeDistance) * Mathf.Max(0f, landmarkSO.DistanceMultiplier);
    }
}
