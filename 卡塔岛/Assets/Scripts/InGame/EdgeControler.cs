using UnityEngine;

public class EdgeControler : MonoBehaviour
{
    public int id;
    public int nodeIdA;
    public int nodeIdB;

    public void InitFromData(EdgeData data)
    {
        id = data.id;
        nodeIdA = data.nodeIdA;
        nodeIdB = data.nodeIdB;
    }
}
