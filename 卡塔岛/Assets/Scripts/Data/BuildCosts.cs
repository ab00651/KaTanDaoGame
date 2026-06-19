using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuildCosts
{
    public static ResourceCost CreateBuildBondCost()
    {
        return new ResourceCost(
            new ResourceAmount(ResourceType.Housing, 1),
            new ResourceAmount(ResourceType.Food, 1)
        );
    }

    public static ResourceCost CreateBuildRecognitionPointCost()
    {
        return new ResourceCost(
            new ResourceAmount(ResourceType.Housing, 1),
            new ResourceAmount(ResourceType.Food, 1),
            new ResourceAmount(ResourceType.Medical, 1),
            new ResourceAmount(ResourceType.Legal, 1)
        );
    }

    public static ResourceCost CreateUpgradeRecognitionCenterCost()
    {
        return new ResourceCost(
            new ResourceAmount(ResourceType.Legal, 2),
            new ResourceAmount(ResourceType.Medical, 3)
        );
    }
}