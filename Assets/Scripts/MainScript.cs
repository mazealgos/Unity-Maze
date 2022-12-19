using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainScript : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject backgroundImage;
    public GameObject canvas;
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
        Maze maze1 = GetMazeFromJSON();
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
            tileImage.color = new Color(255f, 255f, 255f, 1f);
            //tileButton.colors.normalColor = new Color();
        } else if (code == 1)
        {
            tileImage.color = new Color(0f, 0f, 0f, 1f);
        } else if (code == 2)
        {
            tileImage.color = new Color(0f, 255f, 0f, 1f);
        } else if (code == 3)
        {
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


    // Update is called once per frame
    void Update()
    {
        
    }
}
