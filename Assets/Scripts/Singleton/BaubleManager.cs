using System.Collections.Generic;
using UnityEngine;

public class BaubleManager : Singleton<BaubleManager>
{
    private List<Bauble> _activeBaubles = new List<Bauble>(); //Every baubles currently equipped by the player

    public void ActivateBauble(EBaubleType ET)
    {
        foreach (Bauble bauble in _activeBaubles)
        {
            foreach (EBaubleType type in bauble.Types)
            {
                if (type == ET)
                {
                    bauble.Activate();
                    break;
                }
            }
        }
    }
}
