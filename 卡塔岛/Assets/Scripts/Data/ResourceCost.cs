using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceCost
{
    public List<ResourceAmount> items = new List<ResourceAmount>();

    public ResourceCost()
    {
    }

    public ResourceCost(params ResourceAmount[] amounts)
    {
        items.AddRange(amounts);
    }
}