using System.IO;
using UnityEngine;

public class Archive : MonoBehaviour
{
    [SerializeField] private Map map;

    public string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

    void Start()
    {
        if (map == null) map = FindObjectOfType<Map>();
    }

    /// <summary>
    /// 保存当前地图到文件
    /// </summary>
    public void Save()
    {
        if (map == null)
        {
            Debug.LogError("Archive: Map 未找到");
            return;
        }

        var data = map.GetSaveData();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
        Debug.Log($"存档完成: {FilePath} ({data.tiles.Count} 个地块)");
    }

    /// <summary>
    /// 从文件加载地图
    /// </summary>
    public void Load()
    {
        if (map == null)
        {
            Debug.LogError("Archive: Map 未找到");
            return;
        }

        if (!File.Exists(FilePath))
        {
            Debug.LogWarning($"未找到存档文件: {FilePath}");
            return;
        }

        string json = File.ReadAllText(FilePath);
        var data = JsonUtility.FromJson<MapSaveData>(json);
        map.GenerateFromSaveData(data);
        Debug.Log($"读档完成: {FilePath} ({data.tiles.Count} 个地块)");
    }

    /// <summary>
    /// 是否存在存档
    /// </summary>
    public bool HasSave()
    {
        return File.Exists(FilePath);
    }

    /// <summary>
    /// 读取存档数据（不还原到地图）
    /// </summary>
    public MapSaveData LoadMapData()
    {
        if (!HasSave()) return null;
        string json = File.ReadAllText(FilePath);
        return JsonUtility.FromJson<MapSaveData>(json);
    }

    /// <summary>
    /// 删除存档
    /// </summary>
    public void DeleteSave()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
            Debug.Log($"已删除存档: {FilePath}");
        }
    }

}
