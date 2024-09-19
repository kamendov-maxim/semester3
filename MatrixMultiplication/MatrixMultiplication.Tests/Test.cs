namespace MatrixMultiplication.Tests;

using MatrixMultiplication;

public class MatrixMultiplicationTests
{

    private readonly string[] TestDirs = Directory.GetDirectories("../../../TestFiles");

    [Test]
    public void Tests()
    {
        foreach (var dir in TestDirs)
        {
            var files = Directory.GetFiles(dir);
            if (dir == "../../../TestFiles/EmptyFiles")
            {
                var ex = Assert.Throws<ArgumentException>(() => new MatrixMultiplication.Matrix(files[0]));
                Assert.That(ex.Message, Is.EqualTo($"Input file {files[0]} is empty"));
                continue;
            }
            else if (dir == "../../../TestFiles/DifferentRows")
            {
                var ex = Assert.Throws<ArgumentException>(() => new MatrixMultiplication.Matrix(files[0]));
                Assert.That(ex.Message, Is.EqualTo("Rows of the matrix have different length"));
                continue;
            }
            var firstMatrix = new MatrixMultiplication.Matrix(files[0]);
            var secondMatrix = new MatrixMultiplication.Matrix(files[1]);
            if (dir == "../../../TestFiles/DifferentHeigthAndWidth")
            {
                var ex = Assert.Throws<ArgumentException>(() =>
                        MatrixMultiplication.MatrixMultiplier.MultiplyWithoutMultiThreading(firstMatrix, secondMatrix));
                Assert.That(ex.Message, Is.EqualTo("First matrix width is not equal to second matrix height"));
                ex = Assert.Throws<ArgumentException>(() =>
                        MatrixMultiplication.MatrixMultiplier.Multiply(firstMatrix, secondMatrix));
                Assert.That(ex.Message, Is.EqualTo("First matrix width is not equal to second matrix height"));
                continue;
            }
            var answer = MatrixMultiplication.MatrixMultiplier.MultiplyWithoutMultiThreading(firstMatrix, secondMatrix);
            answer.WriteToFile("output");
            Assert.That(File.ReadAllBytes("output").SequenceEqual(File.ReadAllBytes(files[2])));
            answer = MatrixMultiplication.MatrixMultiplier.Multiply(firstMatrix, secondMatrix);
            answer.WriteToFile("output");
            Assert.That(File.ReadAllBytes("output").SequenceEqual(File.ReadAllBytes(files[2])));

        }
    }
}