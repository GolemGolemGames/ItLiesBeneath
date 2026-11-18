using System.Collections.Generic;
using UnityEngine;

public abstract class Bauble : MonoBehaviour
{
    [SerializeField] private List<EBaubleType> types; //Value. Used by children to activate

    public List<EBaubleType> Types { get => types; set => types = value; }

    public abstract void Activate(); //Used by children to activate their effects
}
