using UnityEngine;

public class GameControler : MonoBehaviour
{
    [SerializeField] private int totalTiles = 19;

    void Start()
    {
        var map = GetComponent<Map>();
        if (map == null)
        {
            Debug.LogError("GameControler: 未找到Map组件，请确保Map挂载在同一GameObject上");
            return;
        }
        map.GenerateMap(totalTiles);
    }
}
