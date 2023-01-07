using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace MachineSystem
{
    public partial class MainWindow : Window
    {
        // INITIALISING THE GLOBAL VARS 
        public List<double> P2 = new List<double>();
        public string DB_PATH_NAME;
        private string FirstButtonPath, SecondButtonPath;
        public List<int> SizeX, SizeY, SizeZ;
        public double movementSpeed;
        public List<double> CurrentPoint = new List<double>()
            {
                0,0,0
            };


        // GENERATING RANDOM NUMBERS FOR COORDINATE OF P2
        private List<double> GenerateP2(List<int> XParam, List<int> YParam, List<int> ZParam)
        {
            Random random = new Random();
            List<double> P2 = new List<double>()
            {
                random.Next(XParam[0], XParam[1]), random.Next(YParam[0], YParam[1]), random.Next(ZParam[0], ZParam[1])
            };
            return P2;
        }

        // CALCULATING THE THISTANCE BETWEEN P1 & P2
        private double CalcDistance(List<double> point1, List<double> point2)
        {
            double distance = Math.Sqrt(Math.Pow(point2[0] - point1[0], 2) +
                                        Math.Pow(point2[1] - point1[1], 2) +
                                        Math.Pow(point2[2] - point1[2], 2));
            return distance;
        }
        // UPDATING THE CURRENT LOCATION OF WHERE THE MACHINE IS
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
            // READING THE CONFIG FILE
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sFile = Path.Combine(sCurrentDirectory, @"..\..\..\CONFIGURATION.txt");
            string FilePath = Path.GetFullPath(sFile);
            string[] ConfigLines = File.ReadAllLines(FilePath);

            // GET THE MOVEMENT SPEED
            int charPos = ConfigLines[0].IndexOf("=") + 1;
            movementSpeed = double.Parse(ConfigLines[0].Substring(charPos), System.Globalization.CultureInfo.InvariantCulture) * 0.1;

            // GET EACH AXIS SIZE (X,Y,Z)
            //X
            charPos = ConfigLines[1].IndexOf("=") + 1;
            // Remove the square brackets and Split the string into an array of strings
            string[] tmp_array = ConfigLines[1].Substring(charPos).Trim('[', ']').Split(',');
            // Convert the array of strings to an array of int & Convert the array of int to a List<int>
            SizeX = Array.ConvertAll(tmp_array, int.Parse).ToList();
            //Y
            charPos = ConfigLines[2].IndexOf("=") + 1;
            tmp_array = ConfigLines[2].Substring(charPos).Trim('[', ']').Split(',');
            SizeY = Array.ConvertAll(tmp_array, int.Parse).ToList();
            //Z
            charPos = ConfigLines[3].IndexOf("=") + 1;
            tmp_array = ConfigLines[3].Substring(charPos).Trim('[', ']').Split(',');
            SizeZ = Array.ConvertAll(tmp_array, int.Parse).ToList();

            //THE TIME INTERVAL TO SAVE THE DATA IN A TEXT FILE
            charPos = ConfigLines[4].IndexOf("=") + 1;
            double saveTimeInterval = double.Parse(ConfigLines[4].Substring(charPos), System.Globalization.CultureInfo.InvariantCulture);

            // GET THE PATH AND THE FILE NAME FOR First Button TO SAVE
            charPos = ConfigLines[5].IndexOf("=") + 1;
            FirstButtonPath = ConfigLines[5].Substring(charPos);

            // GET THE PATH AND THE FILE NAME FOR Second Button TO SAVE
            charPos = ConfigLines[6].IndexOf("=") + 1;
            SecondButtonPath = ConfigLines[6].Substring(charPos);

            //// GET THE PATH AND THE FILE NAME FOR DB FILE TO SAVE
            charPos = ConfigLines[7].IndexOf("=") + 1;
            DB_PATH_NAME = ConfigLines[7].Substring(charPos);
            //READING THE CONFIG FILE FINISHED



            P2 = GenerateP2(SizeX, SizeY, SizeZ);
            P2coordinate.Text = "P2: (" + string.Join(", ", P2) + ")";

            // CREATING TIMER TO UPDATE DISPLAY
            DispatcherTimer UpdateDisplayTimer = new DispatcherTimer();
            UpdateDisplayTimer.Interval = TimeSpan.FromMilliseconds(250);
            UpdateDisplayTimer.Tick += new EventHandler(Timer_Tick);
            UpdateDisplayTimer.Start();

            //CREATING TIMER TO SAVE THE CURRENT PPOINT IN DB
            DispatcherTimer SaveFileTimer = new DispatcherTimer();
            SaveFileTimer.Interval = TimeSpan.FromSeconds(saveTimeInterval);
            SaveFileTimer.Tick += new EventHandler(SaveInDB);
            SaveFileTimer.Start();

            //
            DispatcherTimer MoveTimer = new DispatcherTimer();
            MoveTimer.Interval = TimeSpan.FromSeconds(1);
            MoveTimer.Tick += new EventHandler(MoveIt);
            MoveTimer.Start();
        }
        private void MoveIt(object sender, EventArgs e)
        {
            CurrentPoint = UpdateCurrentPoint(P2, CurrentPoint, movementSpeed);
            // Check if the object has reached the target
            if (CalcDistance(CurrentPoint, P2) == 0)
            {
                P2 = GenerateP2(SizeX, SizeY, SizeZ);
                P2coordinate.Text = "P2: (" + string.Join(", ", P2) + ")";
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {

            // Display the coordinates on the screen
            coordinate_x.Text = CurrentPoint[0].ToString();
            coordinate_y.Text = CurrentPoint[1].ToString();
            coordinate_z.Text = CurrentPoint[2].ToString();


        }
        private void SaveInDB(object sender, EventArgs e)
        {
            using (var db = new CoordinateContext(DB_PATH_NAME))
            {
                db.Database.Migrate();
                var point = new PointCoordinate();
                point.x = CurrentPoint[0];
                point.y = CurrentPoint[1];
                point.z = CurrentPoint[2];
                point.date = DateTime.Now.Date;
                point.time = DateTime.Now.TimeOfDay;
                db.PointCoordinates.Add(point);
                db.SaveChanges();
            }
        }
        private void FirstButton(object sender, RoutedEventArgs e)
        {
            using (StreamWriter sw = File.AppendText(FirstButtonPath))
            {
                sw.WriteLine(string.Join(", ", CurrentPoint));
            }
        }
        private void SecondButton(object sender, RoutedEventArgs e)
        {
            using (StreamWriter sw = File.AppendText(SecondButtonPath))
            {
                using (var db = new CoordinateContext(DB_PATH_NAME))
                {
                    foreach (var point in db.PointCoordinates)
                    {
                        string tmp = point.x.ToString() + ", " + point.y.ToString() + ", " + point.z.ToString();
                        sw.WriteLine(tmp);
                    }
                }
            }
        }
    }
}
