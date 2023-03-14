using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeSolver : MonoBehaviour
{
    public List<Vector2> GetSolution(int[,] maze, Vector2 size)
    {
        List<Vector2> solution = new List<Vector2>();

        // Get the mazeStore array, which will be filled with high numbers for where the player cannot go, leaving a path of 0's that should be the solution
        int[,] mazeStore = GetMazeStoreArray(maze, size);
        // Run through the mazeStore array, filling in any potentially missed dead ends and ensuring a path of low numbers to be the correct path
        mazeStore = TraverseMaze(maze, size, mazeStore);
        // Run through traversed maze store, going to from start to finish using lowest scores, keeping track of where the player has been
        solution = GetFinalPath(maze, size, mazeStore);
        

        // Print mazeStore
        for (int y = 0; y<size.y; y++)
        {
            //Debug.Log("-------------ROW-"+(y+1)+"-------------");
            for (int x = 0; x<size.x; x++)
            {
                //Debug.Log(mazeStore[x, y]);
            }
        }

        return solution;
    }

    int[,] GetMazeStoreArray(int[,] maze, Vector2 size)
    {
        // mazeStore should be filled with 1's where there are walls or dead ends, leaving a path of 0's that should be the solution
        int[,] mazeStore = new int[(int) size.x, (int) size.y];
        List<Vector2> deadEnds = new List<Vector2>();

        // Loop through all tiles, add 1's to mazeStore for walls, identify dead ends
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (maze[x, y] != 0)
                {
                    if (maze[x, y] == 1)
                        mazeStore[x, y] = 99;
                    continue;
                } else if (maze[x, y] == 0)
                {
                    int adjCount =
                        CheckAdjacent(maze, size, new Vector2(x, y), new Vector2(-1, 0), 1) +
                        CheckAdjacent(maze, size, new Vector2(x, y), new Vector2(1, 0), 1) +
                        CheckAdjacent(maze, size, new Vector2(x, y), new Vector2(0, 1), 1) +
                        CheckAdjacent(maze, size, new Vector2(x, y), new Vector2(0, -1), 1);
                    if (adjCount >= 3)
                        deadEnds.Add(new Vector2(x, y));
                }
            }
        }

        // Loop through all dead ends, follow dead end path until intersection is found, set all dead end tiles to 1 in mazeStore
        for (int d = 0; d < deadEnds.Count; d++)
        {
            Vector2 c = deadEnds[d];
            List<Vector2> adjacents = GetAdjacents(mazeStore, size, c, 0);

            do
            {
                mazeStore[(int) c.x, (int) c.y] = 88;
                c = adjacents[0];
                adjacents = GetAdjacents(mazeStore, size, c, 0);
            } while (adjacents.Count <= 1);
        }

        return mazeStore;
    }

    // Simulates a player choosing the best path through the mazeStore array, updating the values it's been at, to create a path of lowest numbers from start to finish
    // This is to fill up potentially missed dead ends with higher numbers to ensure a correct path of lowest numbers
    int[,] TraverseMaze(int[,] maze, Vector2 size, int[,] mazeStore)
    {
        Vector2 start = new Vector2(0, 0);
        Vector2 finish = new Vector2(0, 0);
        int[,] traversedMazeStore = mazeStore;

        // Get Start and Finish positions
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                if (maze[x, y] == 2)
                {
                    start = new Vector2(x, y);
                }
                else if (maze[x, y] == 3)
                    finish = new Vector2(x, y);

        // Traverse mazeStore, increasing the values where the "player" has already been
        Vector2 player = start;
        int iterationCount = 0;
        while (player != finish && iterationCount <= 500)
        {
            iterationCount++;
            traversedMazeStore[(int)player.x, (int)player.y] += 1;
            List<Vector2> bestAdjacents = GetBestAdjacent(traversedMazeStore, size, player);
            int randIndex = Random.Range(0, bestAdjacents.Count-1);
            player = bestAdjacents[randIndex];
        }
        Debug.Log("Traversed in "+iterationCount+" iterations");

        return traversedMazeStore;
    }

    List<Vector2> GetFinalPath(int[,] maze, Vector2 size, int[,] mazeStore)
    {
        List<Vector2> path = new List<Vector2>();
        Vector2 start = new Vector2(0, 0);
        Vector2 finish = new Vector2(0, 0);

        // Get Start and Finish positions
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                if (maze[x, y] == 2)
                {
                    start = new Vector2(x, y);
                }
                else if (maze[x, y] == 3)
                    finish = new Vector2(x, y);

        Vector2 player = start;
        int iterationCount = 0;
        while (player != finish && iterationCount <= 500)
        {
            path.Add(player);
            iterationCount++;
            mazeStore[(int)player.x, (int)player.y] += 1;
            List<Vector2> bestAdjacents = GetBestAdjacent(mazeStore, size, player);
            int randIndex = Random.Range(0, bestAdjacents.Count - 1);
            player = bestAdjacents[randIndex];
        }
        path.Add(finish);
        Debug.Log("Finished in " + iterationCount + " iterations");

        return path;
    }

    int CheckAdjacent(int[,] maze, Vector2 size, Vector2 current, Vector2 offset, int check)
    {
        Vector2 c = new Vector2(current.x + offset.x, current.y + offset.y);
        if (c.x >= 0 && c.x < size.x && c.y >= 0 && c.y < size.y)
        {
            if (maze[(int)c.x, (int)c.y] == check)
                return 1;
        }
        return 0;
    }

    List<Vector2> GetAdjacents(int[,] maze, Vector2 size, Vector2 current, int check)
    {
        List<Vector2> valid = new List<Vector2>();
        int[] adjacent =
            {
                CheckAdjacent(maze, size, current, new Vector2(-1, 0), check),
                CheckAdjacent(maze, size, current, new Vector2(1, 0), check),
                CheckAdjacent(maze, size, current, new Vector2(0, 1), check),
                CheckAdjacent(maze, size, current, new Vector2(0, -1), check)
            };
        for (int i = 0; i<4; i++)
        {
            if (adjacent[i] == 1)
                valid.Add(new Vector2(current.x + GetOffsetFromIndex(i).x, current.y + GetOffsetFromIndex(i).y));
        }
        return valid;
    }

    List<Vector2> GetBestAdjacent(int[,] mazeStore, Vector2 size, Vector2 current)
    {
        List<Vector2> best = new List<Vector2>();

        // Check all 4 adjacent if they are in bounds, and fill "best" with the lowest score adjacent tile(s)
        for (int i = 0; i<4; i++)
        {
            Vector2 o = GetOffsetFromIndex(i);
            Vector2 a = new Vector2(current.x + o.x, current.y + o.y);
            if (a.x >= 0 && a.x < size.x && a.y >= 0 && a.y < size.y)
            {
                int p = mazeStore[(int)a.x, (int)a.y];
                if (best.Count == 0)
                {
                    best.Add(a);
                } else
                {
                    int bp = mazeStore[(int)best[0].x, (int)best[0].y];
                    if (bp > p){
                        best.Clear();
                        best.Add(a);
                    } else if (bp == p)
                        best.Add(a);
                }
            }
        }
        
        return best;
    }

    Vector2 GetOffsetFromIndex(int i)
    {
        return new Vector2(0.5f*Mathf.Abs(Mathf.Sign(i-1.5f)-1)*Mathf.Sign(i-0.5f), 0.5f*Mathf.Abs(Mathf.Sign(i-1.5f)+1)*Mathf.Sign(2.5f-i));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
