namespace MatrixMultiplication.Tests;

public class MatrixMultiplicationTests
{

    [Test]
    public void EmptyFilesTests()
    {

        var files = Directory.GetFiles("../../../TestFiles/EmptyFiles");
        var ex = Assert.Throws<ArgumentException>(() => new MatrixMultiplication.Matrix(files[0]));
        Assert.That(ex.Message, Is.EqualTo($"Input file {files[0]} is empty"));
    }

    [Test]
    public void TestCaseWithDifferentRows()
    {
        var files = Directory.GetFiles("../../../TestFiles/DifferentRows");
        var ex = Assert.Throws<ArgumentException>(() => new MatrixMultiplication.Matrix(files[0]));
        Assert.That(ex.Message, Is.EqualTo("Rows of the matrix have different length"));
    }

    [Test]
    public void DifferentHeigthAndWidth()
    {
        var files = Directory.GetFiles("../../../TestFiles/DifferentHeigthAndWidth");
        var firstMatrix = new MatrixMultiplication.Matrix(files[0]);
        var secondMatrix = new MatrixMultiplication.Matrix(files[1]);
        var ex = Assert.Throws<ArgumentException>(() =>
                MatrixMultiplication.MatrixMultiplier.MultiplyWithoutMultiThreading(firstMatrix, secondMatrix));
        Assert.That(ex.Message, Is.EqualTo("First matrix width is not equal to second matrix height"));
        ex = Assert.Throws<ArgumentException>(() =>
                MatrixMultiplication.MatrixMultiplier.Multiply(firstMatrix, secondMatrix));
        Assert.That(ex.Message, Is.EqualTo("First matrix width is not equal to second matrix height"));
    }

    [Test]
    public void MultiplicationTests()
    {
        string[] multiplicationTestDirs = Directory.GetDirectories("../../../TestFiles/MultiplicationTests");
        foreach (var dir in multiplicationTestDirs)
        {
            var files = Directory.GetFiles(dir);
            var firstMatrix = new MatrixMultiplication.Matrix(dir + "/firstMatrix");
            var secondMatrix = new MatrixMultiplication.Matrix(dir + "/secondMatrix");
            var answer = MatrixMultiplication.MatrixMultiplier.MultiplyWithoutMultiThreading(firstMatrix, secondMatrix);
            answer.WriteToFile("output");
            Assert.That(File.ReadAllBytes("output").SequenceEqual(File.ReadAllBytes(dir + "/Answer")));
            answer = MatrixMultiplication.MatrixMultiplier.Multiply(firstMatrix, secondMatrix);
            answer.WriteToFile("output");
            Assert.That(File.ReadAllBytes("output").SequenceEqual(File.ReadAllBytes(files[2])));

        }
    }
}