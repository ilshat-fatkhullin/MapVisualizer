using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuildingPropertiesHelper
{
    public static float GetHeightFromProperties(IDictionary<string, dynamic> properties)
    {
        if (properties.ContainsKey("height"))
        {
            return (float)properties["height"];
        }

        if (properties.ContainsKey("levels"))
        {
            return GetHeightFromLevel((int)properties["levels"]);
        }

        Debug.Log("Building does not have any property to calculate its height.");
        return 0;
    }

    public static float GetHeightFromLevel(int level)
    {
        if (level <= 0)
        {
            return 0;
        }

        return NumericConstants.FIRST_LEVEL_HEIGHT + (level - 1) * NumericConstants.NON_FIRST_LEVEL_HEIGHT;
    }
}
