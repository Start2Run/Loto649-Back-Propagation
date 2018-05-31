using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedClasses
{
    class NeuronLoto
    {
        public NeuralNetwork nn;
        public double[] weights;
        private Dictionary<string, Extragere> _dateLoto;
        private DateTime _firstDate;
        private int[] _inputNumbers;

        private int _numInput; //The number of inputs
        private int _numHidden; //The number of hidden nodes
        private const int _numOutput = 2; //This should have always the value 2 -> the probability for extraction status true/false
        
        List<double[]> _allData;
        double[][] _trainData;
        double[][] _testData;

        public NeuronLoto(Dictionary<string, Extragere> dateLoto, int[] inputNumbers, int offsetHidden=0) //inputNumbers are the inputs for neuronal network additional to the date
        {
            if (inputNumbers.Count() < 1 || dateLoto.Count()<1) return;

            _dateLoto = dateLoto;
            _inputNumbers = inputNumbers;
            _firstDate = _dateLoto.First().Value.Date;

            // The count of inputs are given by the count of exxtraction numbers that we want to calculate the probabilities,
            //and the bits count of extraction date
            _numInput = DateToBits(DateTime.Now).Count();
            if (inputNumbers.Count() == HelpFunctions.NeuralNetworkConfig.lotoLastNumber)
                _numInput+=inputNumbers.Count();
            _numHidden = _numInput + _numOutput + offsetHidden;
            if (_numHidden < 1) return;
            InitNeuronalNetwork();
        }

        public void InitNeuronalNetwork()
        {
            nn = new NeuralNetwork(_numInput, _numHidden, _numOutput);
            _allData = new List<double[]>();
            FillData(ref _allData);
            HelpFunctions.SplitTrainTest(_allData, 0.80, HelpFunctions.NeuralNetworkConfig.seed, out _trainData, out _testData);
            nn = new NeuralNetwork(_numInput, _numHidden, _numOutput);
            nn.InitializeWeights();
        }

        public void Train()
        {                 
            nn.Train(_trainData, HelpFunctions.NeuralNetworkConfig.maxEpochs, HelpFunctions.NeuralNetworkConfig.learnRate, HelpFunctions.NeuralNetworkConfig.momentum);
            weights = nn.GetWeights();
        }

        #region "Help functions"

        // The structure of normalize is: date_as_binary/numbers_as_aparitionStatus
        public double[] NormalizeData(DateTime date,int[] numbers, bool fake = false)
        {
            int[] dateToBits = DateToBits(date);

            int[] normalizedNumebrs =new int[0];
            if (HelpFunctions.NeuralNetworkConfig.lotoLastNumber == _inputNumbers.Length)
            {
                normalizedNumebrs = Enumerable.Repeat(0, _inputNumbers.Count()).ToArray();
                for (int i = 0; i < numbers.Count(); i++)
                    if (_inputNumbers.Contains(numbers[i])) normalizedNumebrs[numbers[i] - 1] = 1;
            }

            int[] extractionProbabilityStatus = Enumerable.Repeat(0, _numOutput).ToArray();
            if (HelpFunctions.NeuralNetworkConfig.lotoLastNumber == _inputNumbers.Length)
            {
                extractionProbabilityStatus[0] = fake ? 0 : 1; //If the extraction is valid
                extractionProbabilityStatus[1] = fake ? 1 : 0; //If the extraction is a fake 
            }
            else
            {
                extractionProbabilityStatus = ChosedNumbersExtractionProbabilityStatus(numbers);
            }
                      
            double[] normalized = new double[dateToBits.Length + normalizedNumebrs.Length + extractionProbabilityStatus.Length];
            Array.Copy(dateToBits, normalized, dateToBits.Length);
            if(HelpFunctions.NeuralNetworkConfig.lotoLastNumber == _inputNumbers.Length) Array.Copy(normalizedNumebrs, 0, normalized, dateToBits.Length, normalizedNumebrs.Length);
            Array.Copy(extractionProbabilityStatus, 0, normalized, dateToBits.Length+ normalizedNumebrs.Length, extractionProbabilityStatus.Length);
            return normalized;
        }

        private int[] ChosedNumbersExtractionProbabilityStatus(int[] numereExtragere)
        {
            bool status = true;
            foreach (int number in _inputNumbers)
                if (!numereExtragere.Contains(number))
                {
                    status = false;break;
                }
            if (status) return new int[] { 1, 0 };
            else return new int[] { 0, 1 };
        }

        private int[] DateToBits(DateTime dateTime)
        {
            byte days = (byte)(dateTime - _firstDate).TotalDays;
            System.Collections.BitArray b = new System.Collections.BitArray(new int[] { days });
            return b.Cast<bool>().Select(bit => bit ? 1 : 0).ToArray();
        }

        private void FillData(ref List<double[]> allData)
        {
            Dictionary<string, Extragere> fakesDic = new Dictionary<string, Extragere>();
            foreach (Extragere ex in _dateLoto.Values)
            {
                allData.Add(NormalizeData(ex.Date,ex.Numbers));
                if (_inputNumbers.Count() < HelpFunctions.NeuralNetworkConfig.lotoLastNumber) continue;
                Extragere fake = FakeExtragere(ex);
                for (int i = 0; i < 50; i++)
                {
                    if (fakesDic.ContainsKey(fake.Key) || _dateLoto.ContainsKey(fake.Key)) continue;
                    fakesDic.Add(fake.Key, fake);
                    allData.Add(NormalizeData(fake.Date,fake.Numbers, true));
                }
            }
        }

        Random rd = new Random();
        private Extragere FakeExtragere(Extragere ex)
        {
            string[] s = new string[ex.Numbers.Length + 1];

            s[0] = ex.Date.ToString();
            for (int i = 1; i <= ex.Numbers.Length; i++)
            {
                string randNr = rd.Next(1, 50).ToString();
                if (s.Contains(randNr)) { i--; continue; }
                s[i] = randNr;
            }

            Extragere exFake = new Extragere(s,ex.Numbers.Count(),_dateLoto.Count);
            return exFake;
        }
        #endregion
    }
}
