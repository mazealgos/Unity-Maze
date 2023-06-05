using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    MazeUtility mUtil;
    public MazeClass GenerateMaze()
    {
        int sizeX = (int)Random.Range(10, 25);
        int sizeY = (int)Random.Range(10, 25);
        MazeClass finalMaze = new MazeClass(sizeX, sizeY);

        finalMaze.maze = FillBackground(finalMaze);
        finalMaze.maze = CreateEndpoints(finalMaze);
        //finalMaze.maze = CreateSolutionPath(finalMaze);

        return finalMaze;
    }

    int[] FillBackground(MazeClass maze)
    {
        int[] m = maze.maze;
        for (int i = 0; i < maze.sizeX * maze.sizeY; i++)
            m[i] = 1;
        return m;
    }

    int[] CreateEndpoints(MazeClass maze)
    {
        int[] m = maze.maze;
        int finish = (int)Random.Range(0, maze.sizeX);
        int start = (int)Random.Range(0, maze.sizeX) + (maze.sizeY-1)*maze.sizeX;
        m[finish] = 3;
        m[start] = 2;
        return m;
    }

    int[] CreateSolutionPath(MazeClass maze)
    {
        int[] m = maze.maze;

        // Need to "draw paths" from the finish down, and from the start up, making them meet.
        int count = 0;
        int maxCount = 300;
        int[] pencil = {mUtil.FindIndexOf(m, 2), mUtil.FindIndexOf(m, 3)}; // pencil[0] = bottom pencil, pencil[1] = top pencil
        int lastMove;
        do
        {
            lastMove = Move(maze, pencil[count%2])[0];
            m[lastMove] = 0;
            count++;
        }
        while (lastMove != -1 && count < maxCount);
        return m;
    }

    int[] Move(MazeClass maze, int current)
    {
        int[] final = {current, 0};
        return final;
    }

    private void Awake()
    {
        GameObject main = GameObject.Find("Main");
        mUtil = main.GetComponent<MazeUtility>();
    }
}
