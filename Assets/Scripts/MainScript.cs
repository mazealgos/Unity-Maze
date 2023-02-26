using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainScript : MonoBehaviour
{
    public GameObject tilePrefab;
    public GameObject backgroundImage;
    public GameObject canvas;
    GameObject main;
    MessageText messageText;

    Maze maze1;
    // First 3 bits of each value are the "static" maze, derived from the JSON
    // Bits 4-6:
    // Value of 0 = Unchanged
    // Value of 1 = Tile where player has already been
    int[,] mazeArray;
    Vector2 playerPos;
    Vector2 origPlayerPos;
    Vector2 finishPos;
    Vector2[] adjacent = new Vector2[4];
    int tileSizePX = 30;
    
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
        maze1 = GetMazeFromJSON();
        mazeArray = Get2DArrayFromMaze(maze1);
        GenerateMaze();
        SetupButtonListeners();
        finishPos = GetFinishPos();
    }
    private void Awake()
    {
        main = GameObject.Find("Main");
        messageText = main.GetComponent<MessageText>();
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
        int[,] finalMazeArray = new int[mazeObject.sizeX, mazeObject.sizeY];
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

    void GenerateMaze()
    {
        int sX = maze1.sizeX;
        int sY = maze1.sizeY;
        for (int y = 0; y < sY; y++)
        {
            for (int x = 0; x < sX; x++)
            {
                CreateNewTile(new Vector2(x,y), new Vector2((x+1)-sX/2, (-y-1)+sY/2), mazeArray[x,y] & 7);
            }
        }
    }

    void ResetMaze()
    {
        mazeArray = Get2DArrayFromMaze(maze1);
        playerPos = origPlayerPos;
        adjacent = new Vector2[4];
        UpdateColors();
        //Debug.Log("Maze Reset");
    }

    void UpdateMazeArray()
    {
        // Update adjacent tiles in Maze Array
        for(int i = 0; i<4; i++)
        {
            var adjacentTile = TileFromPosition(adjacent[i]);
            int adjacentCode = RawCodeFromPosition(adjacent[i]);
            if (adjacentTile == null || adjacentCode == -1 || (adjacentCode & 7) == 1 || (adjacentCode >> 3) == 1)
            {
                continue;
            }
            else
            {
                
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
                ResetMaze();
            }
            //Debug.Log(playerPos);
            //UpdateColors();
        } else
        {
            messageText.displayMessage("Oops... Try Again!", new Color(1f, 0.3f, 0.3f, 1f));
            ResetMaze();
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
            var adjacentTile = TileFromPosition(adjacent[x]);
            int adjacentCode = CodeFromPosition(adjacent[x]);
            if (adjacentTile == null || adjacentCode == -1 || (adjacentCode & 7) == 1 || (adjacentCode >> 3) == 1)
                continue;
            if (adjacent[x] == pos)
                found = true;
        }
        return found;
    }

    Vector2 PosFromName(string name)
    {
        return new Vector2(int.Parse(""+name[0]), int.Parse(""+name[2]));
    }

    Vector2 GetFinishPos()
    {
        Vector2 f = new Vector2(0, 0);
        for(int x = 0; x<maze1.sizeX; x++)
        {
            for(int y = 0; y<maze1.sizeY; y++)
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

    void ColorTile(int x, int y)
    {
        Transform tileTransform = TileFromPosition(new Vector2(x, y));
        GameObject tileObject = tileTransform.gameObject;
        Image tileImage = tileObject.GetComponent<Image>();
        tileImage.color = ColorFromCode(mazeArray[x, y]);
    }

    Transform TileFromPosition(Vector2 pos)
    {
        if (pos.x < 0 || pos.x > 9 || pos.y < 0 || pos.y > 9)
            return null;
        return backgroundImage.transform.Find((pos.x).ToString() + "," + (pos.y).ToString());
    }

    int RawCodeFromPosition(Vector2 pos)
    {
        //Debug.Log(pos);
        if(pos.x <=9 && pos.x >= 0 && pos.y <=9 && pos.y >= 0)
        {
            return maze1.maze[(int)(pos.x + (pos.y * maze1.sizeX))];
        } else
        {
            return -1;
        }
        
    }

    int CodeFromPosition(Vector2 pos)
    {
        if (pos.x <= 9 && pos.x >= 0 && pos.y <= 9 && pos.y >= 0)
        {
            return mazeArray[(int)pos.x, (int)pos.y];
        }
        else
        {
            return -1;
        }
    }

   void UpdateColors()
    {
        // --- LEFT OFF: ---
        // Need to create a single function that colors all
        // tiles based on their codes.
        // Also need second function that manages all the
        // tile codes and makes sure all game progress,
        // including player position, trail, and adjacents
        // are all stored in the tile codes. (see new codes
        // for tiles above, in ColorFromCode().
        for (int y = 0; y < maze1.sizeY; y++)
        {
            for (int x = 0; x < maze1.sizeX; x++)
            {
                Transform tileTransform = TileFromPosition(new Vector2(x, y));
                //Debug.Log(tileTransform.name);
                GameObject tileObject = tileTransform.gameObject;
                Image tileImage = tileObject.GetComponent<Image>();
                tileImage.color = ColorFromCode(mazeArray[x,y]);
                //Debug.Log("Code: "+CodeFromPosition(new Vector2(x, y)));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
