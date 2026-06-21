using System.Collections.Generic;
using System.Text;
using UnityEngine;

// ==================== 枚举 ====================

public enum TileCategory
{
    Green,  // 必定生成（环0+环1）
    Blue,   // 随机池（环2）
    Gray    // 随机池（环3+环4）
}

// ==================== 数据类 ====================

public class HexTileData
{
    public Vector2Int axialCoord;
    public int positionNumber;
    public TileCategory category;
    public string terrainType;
    public int diceNumber;
    public List<int> nodeIds;   // 6个顶点ID（逆时针）
    public List<int> edgeIds;   // 6条边ID（逆时针）
}

public class NodeData
{
    public int id;
    public Vector3 worldPosition;
    public List<int> edgeIds;   // 与之连接的3条边ID
}

public class EdgeData
{
    public int id;
    public int nodeIdA;
    public int nodeIdB;
}

// ==================== 存档数据 ====================

[System.Serializable]
public class TileSaveData
{
    public Vector2Int coord;
    public string type;
    public int number;
}

[System.Serializable]
public class MapSaveData
{
    public List<TileSaveData> tiles = new List<TileSaveData>();
}

// ==================== Map ====================

public class Map : MonoBehaviour
{
    [SerializeField] private int sideLength = 5;
    [SerializeField] private float tileSize = 1f;
    public float TileSize => tileSize;

    private Dictionary<Vector2Int, HexTileData> activeTiles;
    private List<HexTileData> tileList;
    private Dictionary<int, NodeData> nodeDict;
    private Dictionary<int, EdgeData> edgeDict;

    // 六边形6个邻居方向
    private static readonly Vector2Int[] NeighborDirs = new Vector2Int[]
    {
        new Vector2Int( 1,  0),
        new Vector2Int( 1, -1),
        new Vector2Int( 0, -1),
        new Vector2Int(-1,  0),
        new Vector2Int(-1,  1),
        new Vector2Int( 0,  1),
    };

    // ==================== 公开属性 ====================

    public Dictionary<int, NodeData> Nodes => nodeDict;
    public Dictionary<int, EdgeData> Edges => edgeDict;

    // ==================== 生成入口 ====================

    public void GenerateFromPreset(MapPreset preset)
    {
        activeTiles = new Dictionary<Vector2Int, HexTileData>();
        tileList = new List<HexTileData>();
        nodeDict = new Dictionary<int, NodeData>();
        edgeDict = new Dictionary<int, EdgeData>();

        var positions = preset.GetPositions();
        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int pos = positions[i];
            int ring = HexUtils.GetRing(pos);
            TileCategory cat = ring <= 1 ? TileCategory.Green
                : ring == 2 ? TileCategory.Blue
                : TileCategory.Gray;
            AddTile(pos, cat, i);
        }

        AssignTileAttributes(preset);
        GenerateNodesAndEdges();
        LogMapInfo();
    }

    public void GenerateMap(int totalTiles)
    {
        const int minTiles = 7;
        const int maxTiles = 61;
        if (totalTiles < minTiles || totalTiles > maxTiles)
        {
            Debug.LogWarning($"地块数量 {totalTiles} 超出范围 [{minTiles}, {maxTiles}]，已自动钳制");
            totalTiles = Mathf.Clamp(totalTiles, minTiles, maxTiles);
        }

        activeTiles = new Dictionary<Vector2Int, HexTileData>();
        tileList = new List<HexTileData>(totalTiles);
        nodeDict = new Dictionary<int, NodeData>();
        edgeDict = new Dictionary<int, EdgeData>();

        GenerateTiles(totalTiles);
        AssignTileAttributes();
        GenerateNodesAndEdges();
        LogMapInfo();
    }

    // ==================== 地块属性赋值 ====================

    private static readonly string[] TerrainTypes = new string[]
    {
        "farmland", "factory", "hospital", "government", "bank"
    };

    private void AssignTileAttributes(MapPreset preset = null)
    {
        foreach (var tile in tileList)
        {
            // 预设模式：优先用预设数据
            if (preset != null)
            {
                var pt = preset.GetTileAt(tile.axialCoord);
                if (pt != null && !string.IsNullOrEmpty(pt.landType) && pt.landNumber != 0)
                {
                    tile.terrainType = pt.landType;
                    tile.diceNumber = pt.landNumber;
                    continue;
                }
            }

            // 中心固定
            if (tile.axialCoord == Vector2Int.zero)
            {
                tile.terrainType = "newspaperoffice";
                tile.diceNumber = 7;
            }
            else
            {
                tile.terrainType = TerrainTypes[Random.Range(0, TerrainTypes.Length)];
                tile.diceNumber = RandomDiceNumber();
            }
        }
    }

    private static int RandomDiceNumber()
    {
        int n;
        do { n = Random.Range(1, 13); } while (n == 7);
        return n;
    }

    // ==================== 地块生成 ====================

    private void GenerateTiles(int totalTiles)
    {
        int positionCounter = 0;

        // Step 1: 固定 Green（环0 + 环1 = 7块）
        for (int ring = 0; ring <= 1; ring++)
        {
            foreach (var pos in HexUtils.GetRingPositions(ring))
            {
                AddTile(pos, TileCategory.Green, positionCounter++);
            }
        }

        // Step 2: 候选池（环2 Blue + 环3~4 Gray）
        var candidatePool = new Dictionary<Vector2Int, TileCategory>();
        int maxRing = sideLength - 1;
        for (int ring = 2; ring <= maxRing; ring++)
        {
            var category = ring == 2 ? TileCategory.Blue : TileCategory.Gray;
            foreach (var pos in HexUtils.GetRingPositions(ring))
                candidatePool[pos] = category;
        }

        // Step 3: 初始邻接候选
        var adjacentCandidates = new List<Vector2Int>();
        RefreshAdjacentCandidates(adjacentCandidates, candidatePool);

        // Step 4: 邻接扩展
        while (activeTiles.Count < totalTiles)
        {
            if (adjacentCandidates.Count == 0)
            {
                Debug.LogError("无法找到更多相邻候选！当前数量: " + activeTiles.Count);
                break;
            }

            int index = Random.Range(0, adjacentCandidates.Count);
            Vector2Int pick = adjacentCandidates[index];
            adjacentCandidates.RemoveAt(index);

            TileCategory cat = candidatePool[pick];
            candidatePool.Remove(pick);
            AddTile(pick, cat, positionCounter++);

            foreach (var n in HexUtils.GetNeighbors(pick))
            {
                if (candidatePool.ContainsKey(n) && !adjacentCandidates.Contains(n))
                    adjacentCandidates.Add(n);
            }
        }
    }

    private void AddTile(Vector2Int coord, TileCategory category, int positionNumber)
    {
        var tile = new HexTileData
        {
            axialCoord = coord,
            positionNumber = positionNumber,
            category = category,
            terrainType = category.ToString(),
            diceNumber = 0,
            nodeIds = new List<int>(6),
            edgeIds = new List<int>(6),
        };
        activeTiles[coord] = tile;
        tileList.Add(tile);
    }

    private void RefreshAdjacentCandidates(List<Vector2Int> adjacentCandidates, Dictionary<Vector2Int, TileCategory> candidatePool)
    {
        adjacentCandidates.Clear();
        foreach (var pos in candidatePool.Keys)
        {
            foreach (var n in HexUtils.GetNeighbors(pos))
            {
                if (activeTiles.ContainsKey(n))
                {
                    adjacentCandidates.Add(pos);
                    break;
                }
            }
        }
    }

    // ==================== 顶点&边生成 ====================

    private void GenerateNodesAndEdges()
    {
        var edgeKeyDict = new Dictionary<string, int>();   // edgeKey → edgeId
        var nodeKeyDict = new Dictionary<string, int>();   // nodeKey → nodeId

        int nextEdgeId = 0;
        int nextNodeId = 0;

        foreach (var tile in tileList)
        {
            Vector2Int coord = tile.axialCoord;

            for (int i = 0; i < 6; i++)
            {
                // --- 边：方向i分割本格与邻居 ---
                string edgeKey = MakeEdgeKey(coord, coord + NeighborDirs[i]);
                if (!edgeKeyDict.TryGetValue(edgeKey, out int edgeId))
                {
                    edgeId = nextEdgeId++;
                    edgeKeyDict[edgeKey] = edgeId;

                    edgeDict[edgeId] = new EdgeData
                    {
                        id = edgeId,
                        nodeIdA = -1,
                        nodeIdB = -1,
                    };
                }
                tile.edgeIds.Add(edgeId);

                // --- 顶点：位于方向i与(i+1)%6之间 ---
                Vector2Int hexA = coord;
                Vector2Int hexB = coord + NeighborDirs[i];
                Vector2Int hexC = coord + NeighborDirs[(i + 1) % 6];
                string nodeKey = MakeNodeKey(hexA, hexB, hexC);

                if (!nodeKeyDict.TryGetValue(nodeKey, out int nodeId))
                {
                    nodeId = nextNodeId++;
                    nodeKeyDict[nodeKey] = nodeId;

                    // 顶点世界坐标 = 共享它的3个六边形中心的平均值（保证相邻六边形算出来一致）
                    Vector3 posA = HexUtils.AxialToWorld(hexA, tileSize);
                    Vector3 posB = HexUtils.AxialToWorld(hexB, tileSize);
                    Vector3 posC = HexUtils.AxialToWorld(hexC, tileSize);
                    Vector3 avgPos = (posA + posB + posC) / 3f;

                    nodeDict[nodeId] = new NodeData
                    {
                        id = nodeId,
                        worldPosition = avgPos,
                        edgeIds = new List<int>(3),
                    };
                }
                tile.nodeIds.Add(nodeId);
            }
        }

        // --- 填充边与顶点的关联 ---
        foreach (var kv in nodeDict)
        {
            int nodeId = kv.Key;
            NodeData node = kv.Value;

            // 从 nodeKey 反推出三个六边形坐标
            string nodeKey = null;
            foreach (var nk in nodeKeyDict)
            {
                if (nk.Value == nodeId) { nodeKey = nk.Key; break; }
            }

            if (nodeKey == null) continue;

            Vector2Int[] hexes = ParseNodeKey(nodeKey);
            // 三个六边形两两之间的边
            for (int a = 0; a < hexes.Length; a++)
            {
                for (int b = a + 1; b < hexes.Length; b++)
                {
                    string ek = MakeEdgeKey(hexes[a], hexes[b]);
                    if (edgeKeyDict.TryGetValue(ek, out int eid))
                        node.edgeIds.Add(eid);
                }
            }

            // 将顶点ID填入对应边的端点
            foreach (int eid in node.edgeIds)
            {
                EdgeData edge = edgeDict[eid];
                if (edge.nodeIdA == -1)
                    edge.nodeIdA = nodeId;
                else if (edge.nodeIdB == -1 && edge.nodeIdA != nodeId)
                    edge.nodeIdB = nodeId;
            }
        }
    }

    // ==================== Key工具方法 ====================

    private static string MakeEdgeKey(Vector2Int a, Vector2Int b)
    {
        if (CompareHex(a, b) > 0) { var t = a; a = b; b = t; }
        return $"E_{a.x}_{a.y}_{b.x}_{b.y}";
    }

    private static string MakeNodeKey(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        // 排序三个六边形坐标
        if (CompareHex(a, b) > 0) { var t = a; a = b; b = t; }
        if (CompareHex(b, c) > 0) { var t = b; b = c; c = t; }
        if (CompareHex(a, b) > 0) { var t = a; a = b; b = t; }
        return $"N_{a.x}_{a.y}_{b.x}_{b.y}_{c.x}_{c.y}";
    }

    private static int CompareHex(Vector2Int a, Vector2Int b)
    {
        int cmp = a.x.CompareTo(b.x);
        if (cmp != 0) return cmp;
        return a.y.CompareTo(b.y);
    }

    private static Vector2Int[] ParseNodeKey(string key)
    {
        // 格式: "N_q1_r1_q2_r2_q3_r3"
        var parts = key.Split('_');
        var result = new Vector2Int[3];
        for (int i = 0; i < 3; i++)
        {
            int q = int.Parse(parts[1 + i * 2]);
            int r = int.Parse(parts[2 + i * 2]);
            result[i] = new Vector2Int(q, r);
        }
        return result;
    }

    // ==================== 存档 ====================

    /// <summary>
    /// 导出当前地图为存档数据
    /// </summary>
    public MapSaveData GetSaveData()
    {
        var data = new MapSaveData();
        foreach (var tile in tileList)
        {
            data.tiles.Add(new TileSaveData
            {
                coord = tile.axialCoord,
                type = tile.terrainType,
                number = tile.diceNumber,
            });
        }
        return data;
    }

    /// <summary>
    /// 从存档数据还原地图（跳过随机生成）
    /// </summary>
    public void GenerateFromSaveData(MapSaveData data)
    {
        activeTiles = new Dictionary<Vector2Int, HexTileData>();
        tileList = new List<HexTileData>();
        nodeDict = new Dictionary<int, NodeData>();
        edgeDict = new Dictionary<int, EdgeData>();

        for (int i = 0; i < data.tiles.Count; i++)
        {
            var t = data.tiles[i];
            int ring = HexUtils.GetRing(t.coord);
            TileCategory cat = ring <= 1 ? TileCategory.Green
                : ring == 2 ? TileCategory.Blue
                : TileCategory.Gray;

            var tile = new HexTileData
            {
                axialCoord = t.coord,
                positionNumber = i,
                category = cat,
                terrainType = t.type,
                diceNumber = t.number,
                nodeIds = new List<int>(6),
                edgeIds = new List<int>(6),
            };
            activeTiles[t.coord] = tile;
            tileList.Add(tile);
        }

        GenerateNodesAndEdges();
        LogMapInfo();
    }

    // ==================== 查询方法 ====================

    public HexTileData GetTileAt(Vector2Int coord)
    {
        activeTiles.TryGetValue(coord, out var tile);
        return tile;
    }

    public List<HexTileData> GetNeighborsOf(Vector2Int coord)
    {
        var result = new List<HexTileData>();
        foreach (var n in HexUtils.GetNeighbors(coord))
        {
            if (activeTiles.TryGetValue(n, out var tile))
                result.Add(tile);
        }
        return result;
    }

    public List<HexTileData> GetTilesByRing(int ring)
    {
        var result = new List<HexTileData>();
        foreach (var tile in tileList)
        {
            if (HexUtils.GetRing(tile.axialCoord) == ring)
                result.Add(tile);
        }
        return result;
    }

    public List<HexTileData> GetAllTiles()
    {
        return new List<HexTileData>(tileList);
    }

    public NodeData GetNode(int id) => nodeDict.TryGetValue(id, out var n) ? n : null;
    public EdgeData GetEdge(int id) => edgeDict.TryGetValue(id, out var e) ? e : null;

    // ==================== 日志 ====================

    private void LogMapInfo()
    {
        int greenCount = 0, blueCount = 0, grayCount = 0;
        foreach (var tile in tileList)
        {
            switch (tile.category)
            {
                case TileCategory.Green: greenCount++; break;
                case TileCategory.Blue: blueCount++; break;
                case TileCategory.Gray: grayCount++; break;
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine($"地图生成: {tileList.Count}块 (绿:{greenCount} 蓝:{blueCount} 灰:{grayCount})");
        sb.AppendLine($"顶点总数: {nodeDict.Count}, 边总数: {edgeDict.Count}");

        // 打印每个地块的顶点和边ID
        foreach (var tile in tileList)
        {
            sb.AppendLine($"地块[{tile.positionNumber}] ({tile.axialCoord.x},{tile.axialCoord.y}) " +
                $"类别:{tile.category} 顶点:[{string.Join(",", tile.nodeIds)}] 边:[{string.Join(",", tile.edgeIds)}]");
        }

        // 连通性验证
        if (tileList.Count > 0)
        {
            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(tileList[0].axialCoord);
            visited.Add(tileList[0].axialCoord);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                foreach (var n in HexUtils.GetNeighbors(cur))
                {
                    if (activeTiles.ContainsKey(n) && visited.Add(n))
                        queue.Enqueue(n);
                }
            }
            sb.AppendLine(visited.Count == activeTiles.Count
                ? "连通性: 通过"
                : $"连通性: 失败 (可达{visited.Count}/{activeTiles.Count})");
        }

        Debug.Log(sb.ToString());
    }
}
