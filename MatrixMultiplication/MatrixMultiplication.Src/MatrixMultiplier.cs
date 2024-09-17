namespace MatrixMultiplication;

public static class MatrixMultiplier
{

    public static Matrix MultiplyWithoutMultiThreading(Matrix firstMatrix, Matrix secondMatrix)
    {
        if (firstMatrix.Width != secondMatrix.Height)
        {
            throw new ArgumentException("First matrix width is not equal to second matrix height");
        }

        var answer = new Matrix(firstMatrix.Height, secondMatrix.Width);

        for (int i = 0; i < firstMatrix.Height; i++)
        {
            for (int j = 0; j < secondMatrix.Width; j++)
            {
                answer[i, j] = Enumerable.Range(0, firstMatrix.Width).Sum(k => firstMatrix[i, k] * secondMatrix[k, j]);
            }
        }


        return answer;
    }

    public static Matrix Multiply(Matrix firstMatrix, Matrix secondMatrix)
    {
        if (firstMatrix.Width != secondMatrix.Height)
        {
            throw new ArgumentException("First matrix width is not equal to second matrix height");
        }

        var answer = new Matrix(firstMatrix.Height, secondMatrix.Width);

        var threadCount = Math.Min(Environment.ProcessorCount, firstMatrix.Height);
        var rowsPerThread = (int)Math.Ceiling((double)(firstMatrix.Height / threadCount));
        var threads = Enumerable.Range(0, threadCount).Select(
                x => new Thread(() =>
                    {
                        for (int i = x * rowsPerThread; i < firstMatrix.Height && i < (x + 1) * rowsPerThread; i++)
                        {
                            for (int j = 0; j < secondMatrix.Width; j++)
                            {
                                answer[i, j] = Enumerable.Range(0, firstMatrix.Width).Sum(k => firstMatrix[i, k] * secondMatrix[k, j]);
                            }
                        }
                    }
                    )
                ).ToArray();

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        return answer;
    }
}