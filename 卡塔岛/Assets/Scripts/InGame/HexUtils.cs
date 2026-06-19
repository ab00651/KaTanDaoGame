using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 六边形网格工具类 — Axial坐标系统 (q, r)，隐式 s = -q - r
/// </summary>
public static class HexUtils
{
    // 6个邻居方向的轴向偏移（平顶六边形）
    private static readonly Vector2Int[] NeighborOffsets = new Vector2Int[]
    {
        new Vector2Int( 1,  0), // 右
        new Vector2Int( 1, -1), // 右上
        new Vector2Int( 0, -1), // 左上
        new Vector2Int(-1,  0), // 左
        new Vector2Int(-1,  1), // 左下
        new Vector2Int( 0,  1), // 右下
    };

    /// <summary>
    /// 获取某坐标的6个邻居坐标
    /// </summary>
    public static List<Vector2Int> GetNeighbors(Vector2Int coord)
    {
        var neighbors = new List<Vector2Int>(6);
        foreach (var offset in NeighborOffsets)
        {
            neighbors.Add(coord + offset);
        }
        return neighbors;
    }

    /// <summary>
    /// Cube坐标距离公式: max(|dq|, |dr|, |ds|)
    /// </summary>
    public static int GetDistance(Vector2Int a, Vector2Int b)
    {
        int dq = a.x - b.x;
        int dr = a.y - b.y;
        int ds = (-a.x - a.y) - (-b.x - b.y);
        return Mathf.Max(Mathf.Abs(dq), Mathf.Abs(dr), Mathf.Abs(ds));
    }

    /// <summary>
    /// 返回坐标距中心(0,0)的环编号（即距离）
    /// </summary>
    public static int GetRing(Vector2Int coord)
    {
        return Mathf.Max(Mathf.Abs(coord.x), Mathf.Abs(coord.y), Mathf.Abs(-coord.x - coord.y));
    }

    /// <summary>
    /// 生成边长范围内所有六边形坐标（包含边界）
    /// sideLength=5 → 环0~4共61格
    /// </summary>
    public static List<Vector2Int> GenerateHexagonPositions(int sideLength)
    {
        var positions = new List<Vector2Int>();
        int maxRing = sideLength - 1;
        for (int q = -maxRing; q <= maxRing; q++)
        {
            for (int r = -maxRing; r <= maxRing; r++)
            {
                int s = -q - r;
                if (Mathf.Abs(s) <= maxRing)
                {
                    positions.Add(new Vector2Int(q, r));
                }
            }
        }
        return positions;
    }

    /// <summary>
    /// 轴向坐标 → 世界坐标（尖顶六边形）
    /// </summary>
    public static Vector3 AxialToWorld(Vector2Int axial, float tileSize)
    {
        float x = tileSize * (Mathf.Sqrt(3f) * axial.x + Mathf.Sqrt(3f) / 2f * axial.y);
        float y = -tileSize * (3f / 2f * axial.y);
        return new Vector3(x, y, 0);
    }

    /// <summary>
    /// 获取六边形指定顶点(0~5)的世界坐标，顶点i位于方向i与(i+1)%6之间
    /// </summary>
    public static Vector3 GetVertexWorldPos(Vector2Int hexCoord, int cornerIndex, float tileSize)
    {
        Vector3 center = AxialToWorld(hexCoord, tileSize);
        float angle = cornerIndex * 60f * Mathf.Deg2Rad;
        return new Vector3(
            center.x + tileSize * Mathf.Cos(angle),
            center.y + tileSize * Mathf.Sin(angle),
            0
        );
    }

    /// <summary>
    /// 获取六边形所有6个顶点的世界坐标（逆时针，从右下角开始）
    /// </summary>
    public static Vector3[] GetHexVerticesWorld(Vector2Int hexCoord, float tileSize)
    {
        var verts = new Vector3[6];
        for (int i = 0; i < 6; i++)
            verts[i] = GetVertexWorldPos(hexCoord, i, tileSize);
        return verts;
    }

    /// <summary>
    /// 返回指定环的所有坐标
    /// </summary>
    public static List<Vector2Int> GetRingPositions(int ring)
    {
        var positions = new List<Vector2Int>();
        if (ring == 0)
        {
            positions.Add(Vector2Int.zero);
            return positions;
        }

        // 从 (ring, 0) 开始，沿6个方向遍历环的边界
        Vector2Int current = new Vector2Int(ring, 0);
        // 六边形6条边，每条边走 ring 步
        Vector2Int[] walkDirs = new Vector2Int[]
        {
            new Vector2Int( 0, -1),
            new Vector2Int(-1,  0),
            new Vector2Int(-1,  1),
            new Vector2Int( 0,  1),
            new Vector2Int( 1,  0),
            new Vector2Int( 1, -1),
        };

        foreach (var dir in walkDirs)
        {
            for (int step = 0; step < ring; step++)
            {
                positions.Add(current);
                current += dir;
            }
        }

        return positions;
    }
}
