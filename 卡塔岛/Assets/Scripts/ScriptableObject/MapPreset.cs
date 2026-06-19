using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PresetTile
{
    public string landID = "";       // 格式: "q,r" 如 "0,0"
    public string landType = "";     // 为空则随机
    public int landNumber = 0;       // 0则随机
}

[CreateAssetMenu(fileName = "Map", menuName = "Map/preset")]
public class MapPreset : ScriptableObject
{
    public List<PresetTile> tiles = new List<PresetTile>();

    /// <summary>
    /// 解析所有地块坐标为轴向坐标列表
    /// </summary>
    public List<Vector2Int> GetPositions()
    {
        var list = new List<Vector2Int>();
        foreach (var t in tiles)
        {
            var parts = t.landID.Split(',');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int q) &&
                int.TryParse(parts[1], out int r))
            {
                list.Add(new Vector2Int(q, r));
            }
            else
            {
                Debug.LogWarning($"MapPreset: 无法解析 '{t.landID}'，格式应为 q,r");
            }
        }
        return list;
    }

    /// <summary>
    /// 获取指定坐标的地块预设数据，若无则返回 null
    /// </summary>
    public PresetTile GetTileAt(Vector2Int coord)
    {
        foreach (var t in tiles)
        {
            var parts = t.landID.Split(',');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int q) &&
                int.TryParse(parts[1], out int r) &&
                q == coord.x && r == coord.y)
            {
                return t;
            }
        }
        return null;
    }

    /// <summary>
    /// 从坐标列表写入预设
    /// </summary>
    public void SetPositions(List<Vector2Int> positions)
    {
        tiles.Clear();
        foreach (var p in positions)
            tiles.Add(new PresetTile { landID = $"{p.x},{p.y}" });
    }
}
