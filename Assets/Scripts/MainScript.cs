using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainScript : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject backgroundImage;
    public GameObject canvas;
    public GameObject resetButton;
    public GameObject solutionButton;
    GameObject main;
    MessageText messageText;
    MazeSolver mazeSolver;

    Maze chosenMaze;
    // First 3 bits of each value are the "static" maze, derived from the JSON
    // Bits 4-6:
    // Value of 0 = Unchanged
    // Value of 1 = Tile where player has already been
    int[,] mazeArray;
    Vector2 playerPos;
    Vector2 origPlayerPos;
    Vector2 finishPos;
    Vector2[] adjacent = new Vector2[4];
    List<Vector2> solution;
    
    [System.Serializable]
    class Maze
    {
        public int sizeX, sizeY;
        public int[] maze;
    }

    // Start is called before the first frame update
    void Start()
    {
        messageText.clearText();
        chosenMaze = GetMazeFromJSON();
        mazeArray = Get2DArrayFromMaze(chosenMaze);
        GenerateMaze(chosenMaze);
        solution = mazeSolver.GetSolution(mazeArray);
        SetupButtonListeners();
        finishPos = GetFinishPos(chosenMaze);
    }
    private void Awake()
    {
        main = GameObject.Find("Main");
        messageText = main.GetComponent<MessageText>();
        mazeSolver = main.GetComponent<MazeSolver>();
    }

    Maze GetMazeFromJSON()
    {
        var mazeJSON = Resources.Load<TextAsset>("Mazes/maze5");
        Maze myMaze = JsonUtility.FromJson<Maze>(mazeJSON.text);
        return myMaze;
    }

    int[,] Get2DArrayFromMaze(Maze mazeObject)
    {
        int mazeArraySize = mazeObject.sizeX * mazeObject.sizeY;
        int[,] finalMazeArray = new int[mazeObject.sizeX, mazeObject.sizeY];
        for (int i = 0; i<mazeArraySize; i++)
        {
            finalMazeArray[i % mazeObject.sizeX, (int)Mathf.Floor(i / mazeObject.sizeX)] = mazeObject.maze[i];
        }
        return finalMazeArray;
    }

    void CreateNewTile(Maze maze, Vector2 index, Vector2 pos, int code)
    {
        GameObject cloneTile = Instantiate(tilePrefab, backgroundImage.transform);
        RectTransform tileTransform = cloneTile.GetComponent<RectTransform>();
        Button tileButton = cloneTile.GetComponent<Button>();
        Image tileImage = cloneTile.GetComponent<Image>();
        RectTransform backgroundTransform = backgroundImage.GetComponent<RectTransform>();
        float tileSizePX = backgroundTransform.rect.width/maze.sizeX;
        tileTransform.position = new Vector3(pos.x * tileSizePX + canvas.transform.position.x, pos.y * tileSizePX + canvas.transform.position.y, 0);
        tileTransform.sizeDelta = new Vector2(tileSizePX-2, tileSizePX-2);
        //tileTransform.rect.width = tileSizePX;
        //tileTransform.rect.height = tileSizePX;
        cloneTile.name = index.x + "," + index.y;
        if (code == 0)
        {
            // Code 0 is a Walking Path
            tileImage.color = new Color(1f, 1f, 1f, 1f);
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
            tileImage.color = new Color(0f, 1f, 0f, 1f);
            playerPos = index;
            origPlayerPos = index;
        } else if (code == 3)
        {
            // Code 3 is the Finish
            tileImage.color = new Color(1f, 0f, 0f, 1f);
        }
    }

    void GenerateMaze(Maze maze)
    {
        int sX = maze.sizeX;
        int sY = maze.sizeY;
        for (int y = 0; y < sY; y++)
        {
            for (int x = 0; x < sX; x++)
            {
                CreateNewTile(maze, new Vector2(x,y), new Vector2(x-(sX-1)/2, -y+(sY-1)/2), mazeArray[x,y] & 7);
            }
        }
    }

    void ResetMaze()
    {
        mazeArray = Get2DArrayFromMaze(chosenMaze);
        playerPos = origPlayerPos;
        adjacent = new Vector2[4];
        UpdateColors(chosenMaze);
        //Debug.Log("Maze Reset");
    }

    void ShowSolution()
    {
        ColorSolution();
    }

    void SetupButtonListeners()
    {
        foreach (Transform tile in backgroundImage.transform)
        {
            Button b = tile.gameObject.GetComponent<Button>();
            b.onClick.AddListener(() => ButtonPressed(tile.gameObject));
        }
        Button r = resetButton.GetComponent<Button>();
        r.onClick.AddListener(() => ResetMaze());

        Button s = solutionButton.GetComponent<Button>();
        s.onClick.AddListener(() => ShowSolution());
    }

    void ButtonPressed(GameObject button)
    {
        messageText.clearText();
        bool valid = CheckAdjacent(PosFromName(button.transform.name));
        //Debug.Log(valid);
        if (valid)
        {
            mazeArray[(int)playerPos.x, (int)playerPos.y] = (mazeArray[(int)playerPos.x, (int)playerPos.y] & 7) + (1 << 3);
            ColorTile((int)playerPos.x, (int)playerPos.y);
            //Debug.Log("Pos (" + ((int)playerPos.x).ToString() + ", " + ((int)playerPos.y).ToString() + ") Set to: " + (mazeArray[(int)playerPos.x, (int)playerPos.y]));//(((mazeArray[(int)playerPos.x, (int)playerPos.y]) & 7) + (1 << 3)));
            playerPos = PosFromName(button.transform.name);
            mazeArray[(int)playerPos.x, (int)playerPos.y] = (mazeArray[(int)playerPos.x, (int)playerPos.y] & 7) + (3 << 3);
            ColorTile((int)playerPos.x, (int)playerPos.y);

            if (playerPos == finishPos)
            {
                messageText.displayMessage("Good job, you won!", new Color(0.3f, 1f, 0.3f, 1f));
                //ResetMaze();
            }
            //Debug.Log(playerPos);
            //UpdateColors();
        } else
        {
            messageText.displayMessage("Oops... Try Again!", new Color(1f, 0.3f, 0.3f, 1f));
            //ResetMaze();
        }
    }

    void GetAdjacent()
    {
        adjacent[0] = new Vector2(playerPos.x - 1, playerPos.y);
        adjacent[1] = new Vector2(playerPos.x, playerPos.y + 1);
        adjacent[2] = new Vector2(playerPos.x + 1, playerPos.y);
        adjacent[3] = new Vector2(playerPos.x, playerPos.y - 1);
    }

    bool CheckAdjacent(Vector2 pos)
    {
        bool found = false;
        GetAdjacent();
        for (int x = 0; x < 4; x++)
        {
            var adjacentTile = TileFromPosition(chosenMaze, adjacent[x]);
            int adjacentCode = CodeFromPosition(chosenMaze, adjacent[x]);
            if (adjacentTile == null || adjacentCode == -1 || (adjacentCode & 7) == 1 || (adjacentCode >> 3) == 1)
                continue;
            if (adjacent[x] == pos)
                found = true;
        }
        return found;
    }

    Vector2 PosFromName(string name)
    {
        string[] nameArray = name.Split(',');
        return new Vector2(int.Parse(""+nameArray[0]), int.Parse(""+nameArray[1]));
    }

    Vector2 GetFinishPos(Maze maze)
    {
        Vector2 f = new Vector2(0, 0);
        for(int x = 0; x<maze.sizeX; x++)
        {
            for(int y = 0; y<maze.sizeY; y++)
            {
                if ((mazeArray[x, y] & 7) == 3)
                    f = new Vector2(x, y);
            }
        }
        return f;
    }

    Color ColorFromCode(int code)
    {
        Color finalColor = new Color(1f, 0.7f, 0f, 1f);

        // Static Coloring
        int staticCode = code & 7; // code = 0000...011001, while code&7 = 001
        // in the normal number "124" for example, "1" is the hundreds place, "2" is the tens place, and "4" is the ones place
        // in the binary number "1010", the leftmost digit is the eights place, then the fours place, then the twos, and ones place
        // in binary, 000001 = 1, 000010 = 2, 000011 = 3, 000100 = 4, 000101 = 5, etc...
        if (staticCode == 0)
        {
            // Code 0 is a Walking Path
            finalColor = new Color(1f, 1f, 1f, 1f);
        }
        else if (staticCode == 1)
        {
            // Code 1 is a Wall
            finalColor = new Color(0f, 0f, 0f, 1f);
            //return finalColor;
        }
        else if (staticCode == 2)
        {
            // Code 2 is the Start
            finalColor = new Color(0f, 1f, 0f, 1f);
            return finalColor;
        }
        else if (staticCode == 3)
        {
            // Code 3 is the Finish
            finalColor = new Color(1f, 0f, 0f, 1f);
            return finalColor;
        }

        // Dynamic Coloring
        int dynamicCode = code >> 3; // if code = 000...011001, then (code >> 3) = 011
        //Debug.Log(dynamicCode);
        // so 011010 >> 3 = 011, and 101010 >> 3 = 101
        if (dynamicCode == 1)
        {
            // Code 4 is a tile the player has already been at
            finalColor = new Color(1f, 1f, 0f, 1f);
        }
        else if (dynamicCode == 2)
        {
            // Code 5 is an Adjacent Tile
            finalColor = new Color(0f, 0.7f, 1f, 1f);
        }
        else if (dynamicCode == 3)
        {
            // Code 6 is the Current Player Tile
            finalColor = new Color(1f, 0f, 1f, 1f);
        }
        return finalColor;
    }

    public void ColorTile(int x, int y)
    {
        Transform tileTransform = TileFromPosition(chosenMaze, new Vector2(x, y));
        GameObject tileObject = tileTransform.gameObject;
        Image tileImage = tileObject.GetComponent<Image>();
        tileImage.color = ColorFromCode(mazeArray[x, y]);
    }

    public void ColorTile(int x, int y, Color color)
    {
        Transform tileTransform = TileFromPosition(chosenMaze, new Vector2(x, y));
        GameObject tileObject = tileTransform.gameObject;
        Image tileImage = tileObject.GetComponent<Image>();
        tileImage.color = color;
    }

    Transform TileFromPosition(Maze maze, Vector2 pos)
    {
        if (pos.x < 0 || pos.x > maze.sizeX || pos.y < 0 || pos.y > maze.sizeY)
            return null;
        return backgroundImage.transform.Find((pos.x).ToString() + "," + (pos.y).ToString());
    }

    int RawCodeFromPosition(Maze maze, Vector2 pos)
    {
        //Debug.Log(pos);
        if(pos.x < maze.sizeX && pos.x >= 0 && pos.y < maze.sizeY && pos.y >= 0)
        {
            return maze.maze[(int)(pos.x + (pos.y * maze.sizeX))];
        } else
        {
            return -1;
        }
        
    }

    int CodeFromPosition(Maze maze, Vector2 pos)
    {
        if (pos.x < maze.sizeX && pos.x >= 0 && pos.y < maze.sizeY && pos.y >= 0)
        {
            return mazeArray[(int)pos.x, (int)pos.y];
        }
        else
        {
            return -1;
        }
    }

   void UpdateColors(Maze maze)
    {
        for (int y = 0; y < maze.sizeY; y++)
        {
            for (int x = 0; x < maze.sizeX; x++)
            {
                Transform tileTransform = TileFromPosition(chosenMaze, new Vector2(x, y));
                GameObject tileObject = tileTransform.gameObject;
                Image tileImage = tileObject.GetComponent<Image>();
                tileImage.color = ColorFromCode(mazeArray[x,y]);
            }
        }
    }

    void ColorSolution()
    {
        for (int i = 0; i<solution.Count; i++)
        {
            float colorC = (0f + (1f * ((float)i / (float)(solution.Count-1))));
            Transform tileTransform = TileFromPosition(chosenMaze, solution[i]);
            GameObject tileObject = tileTransform.gameObject;
            Image tileImage = tileObject.GetComponent<Image>();
            tileImage.color = Color.HSVToRGB(colorC, 1, 1);
        }
        Debug.Log(solution.Count);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
