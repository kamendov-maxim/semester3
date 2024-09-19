namespace MatrixMultiplication;

/// <summary>
/// An implementation of matrix containing integers
/// </summary>
public class Matrix
{
    private readonly int[,] matrixArray;

    public int Height => this.matrixArray.GetLength(0);
    public int Width => this.matrixArray.GetLength(1);

    /// <summary>Matrix constructor which creates empty matrix with given height and width
    /// <param name="height">Height of a matrix</param>
    /// <param name="width">Width of a matrix</param>
    /// </summary>
    public Matrix(int height, int width) => this.matrixArray = new int[height, width];

    /// <summary>Matrix constructor that parses matrix given in the file in the following format:
    /// columns are separated by spaces and rows by the next line characters
    /// <param name="path">Path to the file</param>
    /// </summary>
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

    /// <summary>Function to write matrix to the file in the following format:
    /// columns are separated by spaces and rows by the next line characters
    /// <param name="path">Path to the file to write matrix to
    /// </summary>
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