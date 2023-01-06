using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace MachineSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        public List<double> P2 = new List<double>();
        double movement_speed = 2.5;
        public List<double> CurrentPoint = new List<double>()
            {
                0,0,0
            };


        private List<double> GenerateP2()
        {
            Random random = new Random();
            List<double> P2 = new List<double>()
            {
                random.Next(1, 100), random.Next(1, 100), random.Next(1, 100)
            };

            return P2;

        }

        private double CalcDistance(List<double> point1, List<double> point2)
        {
            double distance = Math.Sqrt(Math.Pow(point2[0] - point1[0], 2) +
                                        Math.Pow(point2[1] - point1[1], 2) +
                                        Math.Pow(point2[2] - point1[2], 2));
            return distance;
        }
        private List<double> UpdateCurrentPoint(List<double> Target, List<double> CurrentPoint, double Speed)
        {
            for (int i = 0; i < 3; i++)
            {
                int sign = (Target[i] - CurrentPoint[i] < 0) ? -1 : 1;
                if (Target[i] - CurrentPoint[i] < Speed)
                    CurrentPoint[i] += (Target[i] - CurrentPoint[i]);
                else
                    CurrentPoint[i] += Speed * sign;
            }
            return CurrentPoint;
        }
        public MainWindow()
        {
            InitializeComponent();
            P2 = GenerateP2();
            P2coordinate.Text = "P2: (" + string.Join(", ", P2) + ")";


            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(250);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();





        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentPoint = UpdateCurrentPoint(P2, CurrentPoint, 2.5);
            
            // Display the coordinates on the screen
            coordinate_x.Text = CurrentPoint[0].ToString();
            coordinate_y.Text = CurrentPoint[1].ToString();
            coordinate_z.Text = CurrentPoint[2].ToString();

            // Check if the object has reached the target
            if (CalcDistance(CurrentPoint,P2)<1)
            {
                P2 = GenerateP2();
                P2coordinate.Text = "P2: ("+string.Join(", ", P2)+")";
            }
                


        }
    }
}
