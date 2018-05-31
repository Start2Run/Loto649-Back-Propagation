using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SharedClasses;

namespace Loto649BackPropagation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NeuronLoto _neuron;
        Dictionary<string, Extragere> _data;
        System.ComponentModel.BackgroundWorker _bw;

        Dictionary<int,TextBox> testNumbersDic;

        string _fileName;
        Action _action;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            testNumbersDic = new Dictionary<int, TextBox>();
            testNumbersDic.Add(1, TxtBoxNr1);
            testNumbersDic.Add(2, TxtBoxNr2);
            testNumbersDic.Add(3, TxtBoxNr3);
            testNumbersDic.Add(4, TxtBoxNr4);
            testNumbersDic.Add(5, TxtBoxNr5);
            testNumbersDic.Add(6, TxtBoxNr6);
        }
        
        private void WriteToConsole(string text)
        {
            RtboxConsole.Dispatcher.Invoke(delegate { RtboxConsole.AppendText("\n" + text); RtboxConsole.ScrollToEnd(); });
        }

        private void DGVFill()
        {
            System.Collections.ObjectModel.ObservableCollection<DgvDataFormat> obs = new System.Collections.ObjectModel.ObservableCollection<DgvDataFormat>();
            foreach (Extragere ex in _data.Values)
            {
                obs.Add(ex.DgvData);
            }
            System.Windows.Data.ListCollectionView collection = new System.Windows.Data.ListCollectionView(obs);
            collection.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription("Year"));
            DGV.Dispatcher.Invoke(delegate { DGV.ItemsSource = collection; DGV.Columns[3].Visibility = Visibility.Collapsed; });            
        }

        private void LoadData()
        {
            _data = HelpFunctions.LoadData(_fileName);
            DGVFill();
        }

        private void Train()
        {
            int selectedIndex = 0;
            ckboxTrain.Dispatcher.Invoke(delegate { selectedIndex = ckboxTrain.SelectedIndex; });
            if (selectedIndex > 1) return;
            int[] numbers;
            ChosedNumberForTraining(selectedIndex, out numbers);

            _neuron = new NeuronLoto(_data, numbers);
            _neuron.nn.OnUpdateStatus += new NeuralNetwork.StatusUpdateHandler(Nn_OnUpdateStatus);
            _neuron.Train();
        }

        private void ChosedNumberForTraining(int selectedIndex,out int[] numbers)
        {
            numbers = new int[selectedIndex == 0 ? HelpFunctions.NeuralNetworkConfig.lotoLastNumber : selectedIndex];

            if (selectedIndex == 0) for (int i = 1; i <= HelpFunctions.NeuralNetworkConfig.lotoLastNumber; i++) numbers[i - 1] = i;
            else numbers[0] = 1;
        }

        #region "Events"
        private void Nn_OnUpdateStatus(object sender, HelpFunctions.ProgressEventArgs e)
        {
            WriteToConsole(e.Status);
        }

        private void textBoxValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            int result;
            if (!(int.TryParse(e.Text, out result)) || (e.Text==".") || (int.Parse( ((TextBox)sender).Text+e.Text)>49))
            {
                e.Handled = true;
            }
        }

        private void textBoxValue_PreviewTextInputNumeric(object sender, TextCompositionEventArgs e)
        {
            double result;
            if  ((e.Text != ".") && (!(double.TryParse(e.Text, out result)) || (double.Parse(((TextBox)sender).Text + e.Text) < 0)))
            {
                e.Handled = true;
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "CSV Files (*.csv)|*.csv" };
            var result = ofd.ShowDialog();
            if (result == false) return;
            LblFile.Text = ofd.FileName;
            _fileName= ofd.FileName;
            btnLoad.IsEnabled = true;
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            GridTrain.IsEnabled = false;
            GridTest.IsEnabled = false;
            GridConfig.IsEnabled = true;
            _bw = new System.ComponentModel.BackgroundWorker();
            _bw.WorkerSupportsCancellation = true;
            _bw.DoWork += _bw_DoWork;
            _bw.RunWorkerCompleted += _bw_RunWorkerCompleted;
            _action = LoadData;
            _bw.RunWorkerAsync(); 
        }

        private void btnTrain_Click(object sender, RoutedEventArgs e)
        {
            if (_bw.IsBusy) return;
            GridTrain.IsEnabled = false;
            GridConfig.IsEnabled = false;
            GetConfigurations();
            _action = Train;        
            _bw.RunWorkerAsync();
        }

        private void GetConfigurations()
        {
            if (!int.TryParse(txtMaxLotoNumber.Text, out HelpFunctions.NeuralNetworkConfig.lotoLastNumber))
                HelpFunctions.NeuralNetworkConfig.lotoLastNumber = 49;
            if(!int.TryParse(txtMaxEpochs.Text, out HelpFunctions.NeuralNetworkConfig.maxEpochs))
                HelpFunctions.NeuralNetworkConfig.lotoLastNumber = 100;
            if(!int.TryParse(txtBoxSeed.Text, out HelpFunctions.NeuralNetworkConfig.seed))
                HelpFunctions.NeuralNetworkConfig.seed = 1;
            HelpFunctions.NeuralNetworkConfig.learnRate = double.Parse(txtBoxLearnRate.Text);
            HelpFunctions.NeuralNetworkConfig.momentum = double.Parse(txtBoxMomentum.Text);

        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            if (_bw.IsBusy) return;           
            _action = Test;
            _bw.RunWorkerAsync();
        }

        private void Test()
        {
            List<int> testNumbersList = new List<int>();
            foreach (TextBox numberTxtBox in testNumbersDic.Values)
            {
                string number="";
                numberTxtBox.Dispatcher.Invoke(delegate { number = numberTxtBox.Text; });
                if(number != "") testNumbersList.Add(int.Parse(number));
            }

            int[] testNumbers = testNumbersList.ToArray();
            DateTime date= DateTime.MinValue;
            datePicker.Dispatcher.Invoke(delegate { if (datePicker.SelectedDate.HasValue) date = datePicker.SelectedDate.Value; });
            if (date == DateTime.MinValue) return;

            List<double> data = _neuron.NormalizeData(date, testNumbers).ToList();
            for (int i = 0; i < 2; i++)
                 data.RemoveAt(data.Count() - 1);
            
            double[] result=_neuron.nn.ComputeOutputs(data.ToArray());
        }

        private void _bw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (_action == null) return;
            //IAsyncResult _asyncResult;
            //_asyncResult = _action.BeginInvoke(_action.EndInvoke, null);
            _action.Invoke();
            //_asyncResult.AsyncWaitHandle.WaitOne();
        }

        private void _bw_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            _action = null;
            if (_neuron != null)
            {
                GridTrain.IsEnabled = false;
                GridTest.IsEnabled = true;
            }
            else if (_data != null)
            {
                GridTrain.IsEnabled = true;
            }
        }

        #endregion
    }

    public class GridExpanderSizeBehavior
    {
        public static DependencyProperty SizeRowsToExpanderStateProperty =
            DependencyProperty.RegisterAttached("SizeRowsToExpanderState",
                                                typeof(bool),
                                                typeof(GridExpanderSizeBehavior),
                                                new FrameworkPropertyMetadata(false, SizeRowsToExpanderStateChanged));
        public static void SetSizeRowsToExpanderState(Grid grid, bool value)
        {
            grid.SetValue(SizeRowsToExpanderStateProperty, value);
        }
        private static void SizeRowsToExpanderStateChanged(object target, DependencyPropertyChangedEventArgs e)
        {
            Grid grid = target as Grid;
            if (grid != null)
            {
                if ((bool)e.NewValue == true)
                {
                    grid.AddHandler(Expander.ExpandedEvent, new RoutedEventHandler(Expander_Expanded));
                    grid.AddHandler(Expander.CollapsedEvent, new RoutedEventHandler(Expander_Collapsed));
                }
                else if ((bool)e.OldValue == true)
                {
                    grid.RemoveHandler(Expander.ExpandedEvent, new RoutedEventHandler(Expander_Expanded));
                    grid.RemoveHandler(Expander.CollapsedEvent, new RoutedEventHandler(Expander_Collapsed));
                }
            }
        }
        private static void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            Grid grid = sender as Grid;
            Expander expander = e.OriginalSource as Expander;
            int row = Grid.GetRow(expander);
            if (row <= grid.RowDefinitions.Count)
            {
                grid.RowDefinitions[row].Height = new GridLength(1.0, GridUnitType.Star);
            }
        }
        private static void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid grid = sender as Grid;
            Expander expander = e.OriginalSource as Expander;
            int row = Grid.GetRow(expander);
            if (row <= grid.RowDefinitions.Count)
            {
                grid.RowDefinitions[row].Height = new GridLength(1.0, GridUnitType.Auto);
            }
        }
    }

}
