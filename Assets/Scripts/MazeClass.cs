public class MazeClass
{
    public int sizeX, sizeY;
    public int[] maze;
    public MazeClass(int sX, int sY)
    {
        sizeX = sX;
        sizeY = sY;
        maze = new int[sX*sY];
    }
}
