using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeSolver : MonoBehaviour
{
    MazeUtility mUtil;
    public List<Vector2> GetSolution(int[,] maze)
    {
        List<Vector2> solution = new List<Vector2>();
        Vector2 size = new Vector2(maze.GetLength(0), maze.GetLength(1));
        
        // Get the mazeStore array, which will be filled with high numbers for where the player cannot go, leaving a path of 0's that should be the solution
        // **TODO: Split GetMazeStoreArray() by its two parts (fill wall stores, and dead end filling) and fill dead ends many times over until none left
        // **TODO: Progress on previous todo was made, but now the MazeSolver works incorrectly and doesn't seem to be filling up the dead ends, making the solution length 68, not 28.
        int[,] mazeStore = GetMazeStoreArray(maze, size);
        mazeStore = FillDeadEnds(maze, mazeStore, size);

        // Run through the mazeStore array, filling in any potentially missed dead ends and ensuring a path of low numbers to be the correct path
        // **TODO: Only need GetFinalPath(), not TraverseMaze()
        //mazeStore = TraverseMaze(maze, size, mazeStore);

        // Run through traversed maze store, going from start to finish using lowest scores, keeping track of where the player has been
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

        // Loop through all tiles, add 99's to mazeStore for walls
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (maze[x, y] == 1)
                    mazeStore[x, y] = 99;
                else
                    mazeStore[x, y] = 0;
                //else if (maze[x, y] == 2 || maze[x, y] == 3)
                //    mazeStore[x, y] = 1;
                //else
                //    mazeStore[x, y] = 0;
            }
        }

        return mazeStore;
    }

    List<Vector2> GetDeadEnds(int[,] maze, int[,] mazeStore, Vector2 size)
    {
        List<Vector2> deadEnds = new List<Vector2>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (mazeStore[x, y] == 0 && maze[x, y] != 2 && maze[x, y] != 3)
                {
                    int adjCount = 0;
                    bool wasEndpoint = false;
                    for (int i = 0; i<4; i++)
                    {
                        Vector2 adjOffset = mUtil.GetOffsetFromIndex(i);
                        int adjCode = mUtil.CheckAdjacent(mazeStore, size, new Vector2(x, y), adjOffset, 0);
                        if (adjCode == 1)
                            adjCount++;
                        if (adjCode == -1)
                            wasEndpoint = true;
                    }

                    if (adjCount == 1 && wasEndpoint == false)
                        deadEnds.Add(new Vector2(x, y));
                }
            }
        }

        return deadEnds;
    }

    int[,] IterateDeadEnds(int[,] mazeStore, List<Vector2> deadEnds, Vector2 size)
    {
        //Debug.Log("************** New Iteration **************");
        int[,] newStore = mazeStore;
        for (int d = 0; d < deadEnds.Count; d++)
        {
            Vector2 c = deadEnds[d];
            List<Vector2> adjacents = mUtil.GetAdjacents(newStore, size, c, 0);

            do
            {
                //Debug.Log(c.x+", "+c.y);
                newStore[(int)c.x, (int)c.y] = 88;
                if (adjacents.Count == 0)
                    break;
                c = adjacents[0];
                adjacents = mUtil.GetAdjacents(newStore, size, c, 0);
            } while (adjacents.Count == 1);
        }

        return newStore;
    }

    int[,] FillDeadEnds(int[,] maze, int[,] mazeStore, Vector2 size)
    {
        int cycleCount = 0;
        int[,] newStore = mazeStore;
        List<Vector2> deadEnds = GetDeadEnds(maze, newStore, size);
        while(deadEnds.Count > 0 && cycleCount <= 100)
        {
            cycleCount++;
            newStore = IterateDeadEnds(newStore, deadEnds, size);
            deadEnds = GetDeadEnds(maze, newStore, size);
        }

        return newStore;
    }

    List<Vector2> GetFinalPath(int[,] maze, Vector2 size, int[,] mazeStore)
    {
        List<Vector2> path = new List<Vector2>();
        Vector2 start = new Vector2(0, 0);
        Vector2 finish = new Vector2(0, 0);
        int[,] newStore = mazeStore;

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
            newStore[(int)player.x, (int)player.y] += 1;
            List<Vector2> bestAdjacents = mUtil.GetBestAdjacent(newStore, size, player);
            int randIndex = Random.Range(0, bestAdjacents.Count - 1);
            //player = bestAdjacents[randIndex];
            player = bestAdjacents[0];
        }
        path.Add(finish);
        Debug.Log("Finished in " + iterationCount + " iterations");

        return path;
    }

    private void Awake()
    {
        GameObject main = GameObject.Find("Main");
        mUtil = main.GetComponent<MazeUtility>();
    }
}
