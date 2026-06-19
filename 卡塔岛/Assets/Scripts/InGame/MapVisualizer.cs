using System.Collections.Generic;
using UnityEngine;

public class MapVisualizer : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private GameObject mapRoot;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject edgePrefab;
    [SerializeField] private bool showNodes = true;
    [SerializeField] private bool showEdges = true;
    [SerializeField] private bool usePreset = false;
    [SerializeField] private List<MapPreset> presets = new List<MapPreset>();
    [SerializeField] private TMPro.TMP_FontAsset labelFont;
    [SerializeField] private float tileScale = 1f;
    [SerializeField] private float nodeScale = 0.15f;
    [SerializeField] private float edgeWidth = 0.08f;

    [Header("Tile Colors")]
    [SerializeField] private Color greenColor = new Color(0.55f, 0.55f, 0.55f);
    [SerializeField] private Color blueColor = new Color(0.55f, 0.55f, 0.55f);
    [SerializeField] private Color grayColor = new Color(0.55f, 0.55f, 0.55f);

    private Transform tilesParent;
    private Transform nodesParent;
    private Transform edgesParent;

    void Start()
    {
        if (map == null) map = GetComponent<Map>();
        if (map == null)
        {
            Debug.LogError("MapVisualizer: 未找到Map组件");
            return;
        }
        Invoke(nameof(Visualize), 0.1f);
    }

    public void Visualize()
    {
        // 预设模式：用预设数据覆盖随机生成
        if (usePreset && presets.Count > 0 && presets[0] != null)
        {
            map.GenerateFromPreset(presets[0]);
        }

        ClearChildren();
        CreateParents();

        var tiles = map.GetAllTiles();
        if (tiles.Count == 0)
        {
            Debug.LogWarning("MapVisualizer: 没有地块数据");
            return;
        }

        var nodeObjs = new Dictionary<int, GameObject>();
        var edgeObjs = new Dictionary<int, GameObject>();

        CreateTiles(tiles);
        if (showNodes) CreateNodes(nodeObjs);
        if (showEdges) CreateEdges(edgeObjs);
        LinkTileReferences(tiles, nodeObjs, edgeObjs);
    }

    private void CreateParents()
    {
        var root = mapRoot != null ? mapRoot.transform : transform;

        tilesParent = CreateChild("Tiles", root);
        nodesParent = CreateChild("Nodes", root);
        edgesParent = CreateChild("Edges", root);
    }

    private Transform CreateChild(string name, Transform parent)
    {
        var t = parent.Find(name);
        if (t != null) DestroyImmediate(t.gameObject);
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        return go.transform;
    }

    private void ClearChildren()
    {
        var root = mapRoot != null ? mapRoot.transform : transform;
        for (int i = root.childCount - 1; i >= 0; i--)
            DestroyImmediate(root.GetChild(i).gameObject);
    }

    // ==================== 地块 ====================

    private void CreateTiles(System.Collections.Generic.List<HexTileData> tiles)
    {
        foreach (var tile in tiles)
        {
            Vector3 pos = HexUtils.AxialToWorld(tile.axialCoord, map.TileSize);
            var go = Instantiate(tilePrefab, pos, Quaternion.identity, tilesParent);
            go.name = $"Tile_{tile.positionNumber}";

            // 缩放 Visual 子对象（不影响 TMP 等其他子对象）
            var visual = go.transform.Find("Visual");
            if (visual != null)
                visual.localScale = Vector3.one * tileScale;

            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                sr.color = GetTileColor(tile.category);

            var ctrl = go.GetComponent<LandControler>();
            if (ctrl != null) ctrl.InitFromData(tile);

            // 自动创建文字标签
            CreateLabel(go, "NumberLabel", tile.diceNumber.ToString(), new Vector3(0, 0.3f, -0.5f));
            CreateLabel(go, "TypeLabel", tile.terrainType, new Vector3(0, -0.3f, -0.5f));
        }
    }

    private void CreateLabel(GameObject parent, string name, string text, Vector3 localPos)
    {
        var labelGo = new GameObject(name);
        labelGo.transform.SetParent(parent.transform);
        labelGo.transform.localPosition = localPos;

        var tmp = labelGo.AddComponent<TMPro.TextMeshPro>();
        tmp.text = text;
        if (labelFont != null) tmp.font = labelFont;
        tmp.fontSize = 1.5f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.sortingOrder = 100;

        var rt = labelGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(3f, 1.5f);

        var mr = labelGo.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingLayerName = "Default";
            mr.sortingOrder = 100;
        }
    }

    // ==================== 顶点 ====================

    private void CreateNodes(Dictionary<int, GameObject> nodeObjs)
    {
        foreach (var kv in map.Nodes)
        {
            NodeData node = kv.Value;
            var go = Instantiate(nodePrefab, node.worldPosition, Quaternion.identity, nodesParent);
            go.name = $"Node_{node.id}";
            go.transform.localScale = Vector3.one * nodeScale;
            nodeObjs[node.id] = go;

            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = Color.green;

            var ctrl = go.GetComponent<NodeControler>();
            if (ctrl != null) ctrl.InitFromData(node);
        }
    }

    // ==================== 边 ====================

    private void CreateEdges(Dictionary<int, GameObject> edgeObjs)
    {
        var nodes = map.Nodes;
        foreach (var kv in map.Edges)
        {
            EdgeData edge = kv.Value;
            if (!nodes.TryGetValue(edge.nodeIdA, out var nodeA) ||
                !nodes.TryGetValue(edge.nodeIdB, out var nodeB))
                continue;

            Vector3 a = nodeA.worldPosition;
            Vector3 b = nodeB.worldPosition;
            Vector3 mid = (a + b) / 2f;
            float length = Vector3.Distance(a, b);
            float angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0, 0, angle);

            var go = Instantiate(edgePrefab, mid, rot, edgesParent);
            go.name = $"Edge_{edge.id}";
            go.transform.localScale = new Vector3(length, edgeWidth, 1f);
            edgeObjs[edge.id] = go;

            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = Color.red;

            var ctrl = go.GetComponent<EdgeControler>();
            if (ctrl != null) ctrl.InitFromData(edge);
        }
    }

    // ==================== 关联地块→顶点/边引用 ====================

    private void LinkTileReferences(System.Collections.Generic.List<HexTileData> tiles,
        Dictionary<int, GameObject> nodeObjs, Dictionary<int, GameObject> edgeObjs)
    {
        foreach (var tile in tiles)
        {
            var go = GameObject.Find($"Tile_{tile.positionNumber}");
            if (go == null) continue;

            var ctrl = go.GetComponent<LandControler>();
            if (ctrl == null) continue;

            ctrl.controlpoints = new List<GameObject>();
            foreach (int nid in tile.nodeIds)
            {
                if (nodeObjs.TryGetValue(nid, out var nodeGo))
                    ctrl.controlpoints.Add(nodeGo);
            }

            ctrl.controledges = new List<GameObject>();
            foreach (int eid in tile.edgeIds)
            {
                if (edgeObjs.TryGetValue(eid, out var edgeGo))
                    ctrl.controledges.Add(edgeGo);
            }
        }
    }

    // ==================== 辅助 ====================

    private Color GetTileColor(TileCategory category)
    {
        switch (category)
        {
            case TileCategory.Green: return greenColor;
            case TileCategory.Blue: return blueColor;
            case TileCategory.Gray: return grayColor;
            default: return Color.white;
        }
    }
}
