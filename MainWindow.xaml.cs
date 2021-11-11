
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string path = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "output"; // Default file name
            dialog.DefaultExt = ".csv"; // Default file extension
            dialog.Filter = "CSV (.csv)|*.csv"; // Filter files by extension

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
                if (dialog.CheckFileExists)
                {
                    MainWindow.path = dialog.FileName;
                    Start.IsEnabled = true;
                    Path.Content = dialog.FileName;
                }
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            var dbRows = DbAdapter.GetAllValues(DbAdapter.sqlConnStr);
            var inputMDs = new List<float>();
            var inputTimestamps = new List<float>();
            var inputGRBXs = new List<float>();

            var rowCounter = 0;
            foreach (var item in dbRows)
            {
                if (rowCounter==dbRows.Count)
                {
                    break;
                }

                var timestamp = float.Parse((((object[])item)[1]).ToString());
                var MD = (float)(((object[])item)[2]);
                var GRBX = (float)(((object[])item)[3]);

                inputMDs.Add(MD);
                inputTimestamps.Add(/*(float)(Math.Round(Convert.ToDouble(*/timestamp/*), 0))*/);
                inputGRBXs.Add(GRBX);

                rowCounter++;
            }

            float[] outputTimestamps;
            float[] outputGRBX;
            outputGRBX = InterpolateValues(inputMDs.ToArray(), inputGRBXs.ToArray(), (float)0.1);
            outputTimestamps = InterpolateValues(inputMDs.ToArray(), inputTimestamps.ToArray(), (float)0.1);
            float[] MDs = GenerateMDsWithStep(inputMDs.First(), inputMDs.Last(), (float)0.1).ToArray();

            StringBuilder strBuilder = new StringBuilder();

            for (int i = 0; i < outputTimestamps.Length; i++)
            {
                var convertedTimestamp = (UnixTimeStampToDateTime((long)outputTimestamps[i])).ToString();
                strBuilder.Append(string.Format("\"{0}\",\"{1}\",\"{2}\"{3}", (UnixTimeStampToDateTime((double)outputTimestamps[i])).ToString(), MDs[i].ToString(), outputGRBX[i].ToString(), Environment.NewLine));
            }

            System.IO.File.WriteAllText(path, strBuilder.ToString());
        }

        private float InterpolateValue(float x, float x0, float x1, float fx0, float fx1)
        {
            return fx0 + ((fx1) / (x1 - x0)) * (x - x0);
        }

        private List<float> InterpolateValues(float x0, float x1, float fx0, float fx1, float step_x)
        {
            var list = new List<float>();
            var x = x0+step_x;
            while (x < x1)
            {
                x = (float)(Math.Round(Convert.ToDouble(x) , 1));
                list.Add(fx0 + ((fx1-fx0) / (x1 - x0)) * (x - x0));
                x = (float)(Math.Round(Convert.ToDouble(x + step_x), 1)) ;
            }

            return list;
        }

        private List<float> GenerateMDsWithStep(float firstValue, float lastValue, float step)
        {
            var list = new List<float>();
            var currentValue = firstValue;

            while (currentValue<lastValue)
            {
                list.Add((float)(Math.Round(Convert.ToDouble(currentValue), 1)));
                currentValue = currentValue + step;
            }

            return list;
        }

        private float[] InterpolateValues(float[] x, float[] fx, float step_x)
        {
            var listValues = new List<float>();
            float[] result = null;

            if (x.Length == fx.Length)
            {
                for (int i = 0; i < x.Length-1; i++)
                {
                    listValues.Add(fx[i]);
                    listValues.AddRange(InterpolateValues(x[i], x[i + 1], fx[i], fx[i + 1], step_x));
                }
                listValues.Add(fx[x.Length-1]);
            }

            return listValues.ToArray();
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }


    }
}
