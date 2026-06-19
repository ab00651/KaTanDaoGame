using System.Collections.Generic;
using UnityEngine;

public class LandControler : MonoBehaviour
{
    public int positionNumber;
    public string type;
    public int number;
    public List<int> nodeIds;   // 6个顶点ID
    public List<int> edgeIds;   // 6条边ID
    public List<GameObject> controlpoints;
    public List<GameObject> controledges;

    public void InitFromData(HexTileData data)
    {
        positionNumber = data.positionNumber;
        type = data.terrainType;
        number = data.diceNumber;
        nodeIds = new List<int>(data.nodeIds);
        edgeIds = new List<int>(data.edgeIds);
    }
}
