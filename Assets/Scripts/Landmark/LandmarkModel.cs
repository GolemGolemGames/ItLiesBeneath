using UnityEngine;

public class LandmarkModel : MonoBehaviour
{
    [SerializeField] private LandmarkSO _landmarkSO;

    public LandmarkSO LandmarkSO { get => _landmarkSO; set => _landmarkSO = value; }
}
