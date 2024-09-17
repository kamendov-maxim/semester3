namespace MatrixMultiplication;

public class Matrix
{
    private readonly int[,] matrixArray;

    public int Height => this.matrixArray.GetLength(0);
    public int Width => this.matrixArray.GetLength(1);

    public Matrix(int height, int width) => this.matrixArray = new int[height, width];
    public Matrix(int[,] matrix) => this.matrixArray = matrix;
    public Matrix(string path)
    {
        var lines = File.ReadAllLines(path);
        if (lines.Length == 0)
        {
            throw new ArgumentException($"Input file {path} is empty");
        }
        string[][] splitedLines = lines.Select(line => line.Split(' ')).ToArray(); // afdafd
        matrixArray = new int[splitedLines.Length, splitedLines[0].Length];
        for (int i = 0; i < splitedLines.Length; i++)
        {
            if (splitedLines[0].Length != splitedLines[i].Length)
            {
                throw new ArgumentException("Rows of the matrix have different length");
            }
            for (int j = 0; j < splitedLines[i].Length; j++)
            {
                matrixArray[i, j] = int.Parse(splitedLines[i][j]);
            }
        }
    }

    public void WriteToFile(string path)
    {
        File.WriteAllLines(path,
            from i in Enumerable.Range(0, Height)
            select string.Join(' ', from j in Enumerable.Range(0, Width) select matrixArray[i, j])
                );
    }

    public int this[int row, int column]
    {
        get => this.matrixArray[row, column];
        set => this.matrixArray[row, column] = value;
    }
}
