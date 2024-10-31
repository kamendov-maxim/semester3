namespace CheckSum.Tests;
using CheckSum;

public class Tests
{
    private readonly string _path = "../../../TestFiles";

    [Test]
    public async Task AsyncAndSingleThreadedResultsAreEqual()
    {
        Assert.That(CheckSumCalculator.Calculate(_path), Is.EqualTo(await CheckSumCalculator.CalculateAsync(_path)));
    }
}