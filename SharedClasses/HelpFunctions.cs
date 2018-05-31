using System.Collections.Generic;
using System.Linq;

namespace SharedClasses
{
    public static class HelpFunctions
    {
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k)
        {
            return k == 0 ? new[] { new T[0] } : elements.SelectMany((e, i) => elements.Skip(i + 1).Combinations(k - 1).Select(c => (new[] { e }).Concat(c)));
        }

        public static List<string> ListKeysK(List<int> Numere, int k)
        {
            List<string> lst = new List<string>();
            foreach (IEnumerable<int> x in HelpFunctions.Combinations(Numere, k))
                lst.Add(string.Join("", x.ToList()));
            return lst;
        }

        public static string[,] LoadCsv(string filename)
        {
            // Get the file's text.
            string whole_file = System.IO.File.ReadAllText(filename);

            // Split into lines.
            whole_file = whole_file.Replace('\n', '\r');
            string[] lines = whole_file.Split(new char[] { '\r' },System.StringSplitOptions.RemoveEmptyEntries);

            // See how many rows and columns there are.
            int num_rows = lines.Length;
            int num_cols = lines[0].Split(',').Length;

            // Allocate the data array.
            string[,] values = new string[num_rows, num_cols];

            // Load the array.
            for (int r = 0; r < num_rows; r++)
            {
                string[] line_r = lines[r].Split(',');
                for (int c = 0; c < num_cols; c++)
                {
                    values[r, c] = line_r[c];
                }
            }

            // Return the values.
            return values;
        }

        public static T[][] ToJaggedArray<T>(T[,] twoDimensionalArray)
        {
            int rowsFirstIndex = twoDimensionalArray.GetLowerBound(0);
            int rowsLastIndex = twoDimensionalArray.GetUpperBound(0);
            int numberOfRows = rowsLastIndex + 1;

            int columnsFirstIndex = twoDimensionalArray.GetLowerBound(1);
            int columnsLastIndex = twoDimensionalArray.GetUpperBound(1);
            int numberOfColumns = columnsLastIndex + 1;

            T[][] jaggedArray = new T[numberOfRows][];
            for (int i = rowsFirstIndex; i <= rowsLastIndex; i++)
            {
                jaggedArray[i] = new T[numberOfColumns];

                for (int j = columnsFirstIndex; j <= columnsLastIndex; j++)
                {
                    jaggedArray[i][j] = twoDimensionalArray[i, j];
                }
            }
            return jaggedArray;
        }

        public static void SplitTrainTest(List<double[]> allData, double trainPct, int seed, out double[][] trainData, out double[][] testData)
        {
            System.Random rnd = new System.Random(seed);
            int totRows = allData.Count;
            int numTrainRows = (int)(totRows * trainPct); // usually 0.80
            int numTestRows = totRows - numTrainRows;
            trainData = new double[numTrainRows][];
            testData = new double[numTestRows][];

            double[][] copy = new double[allData.Count][]; // ref copy of data
            for (int i = 0; i < copy.Length; ++i)
                copy[i] = allData[i];

            for (int i = 0; i < copy.Length; ++i) // scramble order
            {
                int r = rnd.Next(i, copy.Length); // use Fisher-Yates
                double[] tmp = copy[r];
                copy[r] = copy[i];
                copy[i] = tmp;
            }
            for (int i = 0; i < numTrainRows; ++i)
                trainData[i] = copy[i];

            for (int i = 0; i < numTestRows; ++i)
                testData[i] = copy[i + numTrainRows];
        } // SplitTrainTest

        public static Dictionary<string, Extragere> LoadData(string fileName)
        {
            Dictionary<string, Extragere> dateLoto = new Dictionary<string, Extragere>();
           
            string[][] ss = HelpFunctions.ToJaggedArray(HelpFunctions.LoadCsv(fileName));
            for (int i = 0; i < ss.GetLength(0); i++)
            {
                Extragere extragere = new Extragere(ss[i],6,++i);
                dateLoto.Add(extragere.Key, extragere);
            }
            foreach (var invalid in dateLoto.Where(z => z.Value.Invalid).ToList())
            {
                dateLoto.Remove(invalid.Key);
            }
            return dateLoto;
        }

        public class ProgressEventArgs : System.EventArgs
        {
            public string Status { get; private set; }

            private ProgressEventArgs() { }

            public ProgressEventArgs(string status)
            {
                Status = status;
            }
        }

        public static class NeuralNetworkConfig
        {
            public static int seed = 1; // gives nice demo
            public static int maxEpochs=100;
            public static double learnRate = 0.05;
            public static double momentum = 0.01;
            public static int lotoLastNumber=49;
        }
    }
}
