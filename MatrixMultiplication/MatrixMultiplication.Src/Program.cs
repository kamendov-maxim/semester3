namespace MatrixMultiplication;

internal class Program
{
    private const string HelpMessage = "This is help message";

    private static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            bool multithreadingOff = false;
            var files = new string[3];

            int filesNumber = 0;

            foreach (var argument in args)
            {
                switch (argument)
                {
                    case "-h":
                    case "--help":
                        Console.WriteLine(HelpMessage);
                        return;

                    case "--MultithreadingOff":
                        multithreadingOff = true;
                        break;

                    default:

                        if (argument[0] == '-')
                        {
                            Console.WriteLine("Invalid argument");
                            Console.WriteLine(HelpMessage);
                            return;
                        }

                        if (filesNumber < 3)
                        {
                            files[filesNumber] = argument;
                            ++filesNumber;

                        }
                        else
                        {
                            Console.WriteLine("Number of files is more than 3");
                            Console.WriteLine(HelpMessage);
                            return;
                        }
                        break;

                }
            }

            Matrix firstMatrix;
            Matrix secondMatrix;
            try
            {
                firstMatrix = new Matrix(files[0]);
                secondMatrix = new Matrix(files[1]);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            var answer = multithreadingOff ? MatrixMultiplier.MultiplyWithoutMultiThreading(firstMatrix, secondMatrix) :
                                        MatrixMultiplier.Multiply(firstMatrix, secondMatrix);

            answer.WriteToFile(files[2]);
        }
        else
        {
            Console.WriteLine(HelpMessage);
        }
    }
}