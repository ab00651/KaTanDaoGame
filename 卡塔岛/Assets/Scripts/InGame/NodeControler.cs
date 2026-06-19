using System.Collections.Generic;
using UnityEngine;

public class NodeControler : MonoBehaviour
{
    public int id;
    public List<int> edgeIds;   // 与之连接的3条边ID

    public void InitFromData(NodeData data)
    {
        id = data.id;
        edgeIds = new List<int>(data.edgeIds);
    }
}
