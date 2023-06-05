using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeUtility : MonoBehaviour
{
    public int FindIndexOf(int[] maze, int check)
    {
        int lastFound = -1;
        for (int i = 0; i < maze.Length; i++)
            if (maze[i] == check)
                lastFound = i;
        return lastFound;
    }
    public int CheckAdjacent(int[,] maze, Vector2 size, Vector2 current, Vector2 offset, int check)
    {
        Vector2 c = new Vector2(current.x + offset.x, current.y + offset.y);
        if (c.x >= 0 && c.x < size.x && c.y >= 0 && c.y < size.y)
        {
            if (maze[(int)c.x, (int)c.y] == check)
                return 1;
        }
        else
        {
            return -1;
        }
        return 0;
    }

    public List<Vector2> GetAdjacents(int[,] maze, Vector2 size, Vector2 current, int check)
    {
        List<Vector2> valid = new List<Vector2>();
        for (int i = 0; i < 4; i++)
        {
            if (CheckAdjacent(maze, size, current, GetOffsetFromIndex(i), check) == 1)
                valid.Add(new Vector2(current.x + GetOffsetFromIndex(i).x, current.y + GetOffsetFromIndex(i).y));
        }
        return valid;
    }

    public List<Vector2> GetBestAdjacent(int[,] mazeStore, Vector2 size, Vector2 current)
    {
        List<Vector2> best = new List<Vector2>();

        // Check all 4 adjacent if they are in bounds, and fill "best" with the lowest score adjacent tile(s)
        for (int i = 0; i < 4; i++)
        {
            Vector2 o = GetOffsetFromIndex(i);
            Vector2 a = new Vector2(current.x + o.x, current.y + o.y);
            if (a.x >= 0 && a.x < size.x && a.y >= 0 && a.y < size.y)
            {
                int p = mazeStore[(int)a.x, (int)a.y];
                if (best.Count == 0)
                {
                    best.Add(a);
                }
                else
                {
                    int bp = mazeStore[(int)best[0].x, (int)best[0].y];
                    if (bp > p)
                    {
                        best.Clear();
                        best.Add(a);
                    }
                    else if (bp == p)
                        best.Add(a);
                }
            }
        }

        return best;
    }

    public Vector2 GetOffsetFromIndex(int i)
    {
        // 0 = (-1, 0), 1 = (1, 0), 2 = (0, 1), 3 = (0, -1)
        return new Vector2(0.5f * Mathf.Abs(Mathf.Sign(i - 1.5f) - 1) * Mathf.Sign(i - 0.5f), 0.5f * Mathf.Abs(Mathf.Sign(i - 1.5f) + 1) * Mathf.Sign(2.5f - i));
    }
}
