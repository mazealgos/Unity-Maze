using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainScript : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject backgroundImage;
    public GameObject canvas;
    Maze maze1;
    Vector2 playerPos;
    Vector2[] adjacent = new Vector2[4];
    int tileSizePX = 30;
    // Start is called before the first frame update
    [System.Serializable]
    class Maze
    {
        public int sizeX, sizeY;
        public int[] maze;
    }
    void Start()
    {
        maze1 = GetMazeFromJSON();
        int[,] mazeArray = Get2DArrayFromMaze(maze1);
        GenerateMaze(maze1, mazeArray);
        SetupButtonListeners();
    }

    Maze GetMazeFromJSON()
    {
        var mazeJSON = Resources.Load<TextAsset>("Mazes/maze1");
        Maze myMaze = JsonUtility.FromJson<Maze>(mazeJSON.text);
        return myMaze;
    }

    int[,] Get2DArrayFromMaze(Maze mazeObject)
    {
        int mazeArraySize = mazeObject.sizeX * mazeObject.sizeY;
        int[,] finalMazeArray = new int[10,10];
        for (int i = 0; i<mazeArraySize; i++)
        {
            finalMazeArray[i % mazeObject.sizeX, (int)Mathf.Floor(i / mazeObject.sizeY)] = mazeObject.maze[i];
        }
        return finalMazeArray;
    }

    void CreateNewTile(Vector2 index, Vector2 pos, int code)
    {
        GameObject cloneTile = Instantiate(tilePrefab, backgroundImage.transform);
        RectTransform tileTransform = cloneTile.GetComponent<RectTransform>();
        Button tileButton = cloneTile.GetComponent<Button>();
        Image tileImage = cloneTile.GetComponent<Image>();
        tileTransform.position = new Vector3(pos.x * tileSizePX + canvas.transform.position.x-14f, pos.y * tileSizePX + canvas.transform.position.y+14f, 0);
        cloneTile.name = index.x + "," + index.y;
        if (code == 0)
        {
            // Code 0 is a Walking Path
            tileImage.color = new Color(255f, 255f, 255f, 1f);
            //tileButton.colors.normalColor = new Color();
        } else if (code == 1)
        {
            // Code 1 is a Wall
            tileImage.color = new Color(0f, 0f, 0f, 1f);
            //Rigidbody2D rigidBody = cloneTile.AddComponent<Rigidbody2D>();
            //BoxCollider2D boxCollider = cloneTile.AddComponent<BoxCollider2D>();
            //ConstantForce2D force2D = cloneTile.AddComponent<ConstantForce2D>();
            //force2D.relativeForce = new Vector2(1, 0);
        } else if (code == 2)
        {
            // Code 2 is the Start
            tileImage.color = new Color(0f, 255f, 0f, 1f);
            playerPos = pos;
        } else if (code == 3)
        {
            // Code 3 is the Finish
            tileImage.color = new Color(255f, 0f, 0f, 1f);
        }
    }

    void GenerateMaze(Maze mazeObject, int[,] mazeArray)
    {
        int sX = mazeObject.sizeX;
        int sY = mazeObject.sizeY;
        for (int x = 0; x < sX; x++)
        {
            for (int y = 0; y < sY; y++)
            {
                CreateNewTile(new Vector2(x,y), new Vector2((x+1)-sX/2, (-y-1)+sY/2), mazeArray[x,y]);
            }
        }
    }

    void SetupButtonListeners()
    {
        foreach (Transform tile in backgroundImage.transform)
        {
            Button b = tile.gameObject.GetComponent<Button>();
            b.onClick.AddListener(() => ButtonPressed(tile.gameObject));
        }
    }

    void ButtonPressed(GameObject button)
    {
        Debug.Log(button.transform.name);
    }

    void CheckAdjacent()
    {
        adjacent[0] = new Vector2(playerPos.x - 1, playerPos.y);
        adjacent[1] = new Vector2(playerPos.x, playerPos.y + 1);
        adjacent[2] = new Vector2(playerPos.x + 1, playerPos.y);
        adjacent[3] = new Vector2(playerPos.x, playerPos.y - 1);
    }

    Transform TileFromPosition(Vector2 pos)
    {
        //return maze1.maze[pos.x * 10 + pos.y];
        if (pos.x < 0 || pos.x > 9 || pos.y < 0 || pos.y > 9)
            return null;
        return backgroundImage.transform.Find((pos.x).ToString() + "," + (pos.y).ToString());
    }

    // Update is called once per frame
    void Update()
    {
        CheckAdjacent();
        for(int x = 0; x < 4; x++)
        {
            var adjacentTile = TileFromPosition(adjacent[x]);
            if (adjacentTile == null)
                continue;
            Image tileImage = adjacentTile.parent.GetComponent<Image>();
            tileImage.color = new Color(220f, 220f, 220f, 1f);
        }
    }
}
