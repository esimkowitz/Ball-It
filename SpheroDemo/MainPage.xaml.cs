using RobotKit;
using System;
using System.ComponentModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Media;
using System.Threading;

// The Blank Page item template is documented at 
// http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SpheroDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public Sphero m_robot = null;

        private static string SpheroName_;
        public string SpheroName
        {
            get { return SpheroName_; }
            set
            {
                SpheroName_ = value;
                Debug.WriteLine("SpheroName set to \"" + value + "\"");
                MessageBar.Text = value;
            }
        }

        private static SolidColorBrush scb = new SolidColorBrush(color: Windows.UI.Colors.CornflowerBlue);

        private const string kNoSpheroConnected = "No Sphero Connected";

        //! @brief  the default string to show when connecting to a sphero ({0})
        private const string kConnectingToSphero = "Connecting to {0}";

        //! @brief  the default string to show when connected to a sphero ({0})
        private const string kSpheroConnected = "Connected to {0}";

        public MainPage()
        {
            this.InitializeComponent();
            grdControls.Visibility = Visibility.Collapsed;
            MessageBar.Foreground = scb;
            Debug.WriteLine("MessageBar.Foreground set");
            SpheroName = kNoSpheroConnected;
            InitializeSensorReading.IsEnabled = false;
            SpheroConnected = false;
            Debug.WriteLine("Made it through MainPage initialization");
        }

        ///// <summary> 
        ///// Invoked when this page is about to be displayed in a Frame. 
        ///// </summary> 
        ///// <param name="e">Event data that describes how this page was reached.  The Parameter 
        ///// property is typically used to configure the page.</param> 
        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    base.OnNavigatedTo(e);
        //    Application app = Application.Current;
        //    app.Suspending += OnSuspending;
        //}

        ////! @brief  handle the application entering the background 
        //private void OnSuspending(object sender, SuspendingEventArgs args)
        //{
        //    ShutdownRobotConnection();
        //    ConnectionToggle.IsOn = false;
        //}

        ///*! 
        //  * @brief   handle the user launching this page in the application 
        //  *  
        //  *  connects to sphero and sets up the ui 
        //  */
        //protected override void OnNavigatedFrom(NavigationEventArgs e)
        //{
        //    base.OnNavigatedFrom(e);
        //    ShutdownRobotConnection();
        //    Application app = Application.Current;
        //    app.Suspending -= OnSuspending;
        //    ConnectionToggle.IsOn = false;
        //}

        public void SetupRobotConnection()
        {
            RobotProvider provider = RobotProvider.GetSharedProvider();
            Debug.WriteLine("GetSharedProvider()");
            provider.DiscoveredRobotEvent += OnRobotDiscovered;
            Debug.WriteLine("DiscoveredRobotEvent");
            provider.NoRobotsEvent += OnNoRobotsEvent;
            Debug.WriteLine("NoRobotsEvent");
            provider.ConnectedRobotEvent += OnRobotConnected;
            Debug.WriteLine("ConnectedRobotEvent");
            provider.FindRobots();
            Debug.WriteLine("FindRobots()");
        }

        //! @brief  when a robot is discovered, connect! 
        private void OnRobotDiscovered(object sender, Robot robot)
        {
            if (!SpheroConnected)
            {
                SpheroName = string.Format(kConnectingToSphero, robot.BluetoothName);
                RobotProvider provider = RobotProvider.GetSharedProvider();

                if (m_robot == null)
                {
                    provider.ConnectRobot(robot);
                }
            }
        }

        private static bool SpheroConnected_ = false;
        public bool SpheroConnected
        {
            get { return SpheroConnected_; }
            set
            {
                SpheroConnected_ = value;
            }
        }

        private void OnNoRobotsEvent(object sender, EventArgs e)
        {
            Debug.WriteLine(kNoSpheroConnected);
            SpheroName = kNoSpheroConnected;
        }

        //! @brief  when a robot is connected, get ready to drive!
        private void OnRobotConnected(object sender, Robot robot)
        {
            SpheroName = string.Format(kSpheroConnected, robot.BluetoothName);
            Debug.WriteLine(SpheroName);
            m_robot = (Sphero)robot;
            ConnectionToggle.IsOn = true;
            ConnectionToggle.OnContent = "Connected";
            SpheroConnected = true;
            m_robot.SetRGBLED(255, 255, 255);
            InitializeSensorReading.IsEnabled = true;

            m_robot.SensorControl.Hz = 15;
            Debug.WriteLine("SensorControl Hz Set");
            //m_robot.CollisionControl.StartDetectionForWallCollisions();
            //m_robot.CollisionControl.CollisionDetectedEvent += OnCollisionDetected;
        }
        
        private const int FILTER_COUNTS = 10;

        private FilteredSensor AccelerometerFiltered;

        private void OnAccelerometerUpdated(object sender, AccelerometerReading reading)
        {
            // expects AccelerometerX,Y,Z to be defined as fields
            AccelerometerFiltered.add(reading.X, reading.Y, reading.Z);
            float[] filteredAvg = AccelerometerFiltered.getFilteredRounded();
            AccelerometerX.Text = "" + filteredAvg[0];
            AccelerometerY.Text = "" + filteredAvg[1];
            AccelerometerZ.Text = "" + filteredAvg[2];
            //var properties = new Dictionary<string, string>
            //    {{"name", m_robot.Name}};
            //var results = new Dictionary<string, double>
            //    { { "X", filteredAvg[0]}, { "Y", filteredAvg[1]}, {"Z", filteredAvg[2] }};
            //insights.TrackEvent("Accelerometer Update", properties, results);
            Debug.WriteLine(string.Format("Accelerometer" + Environment.NewLine + "X: " +
                filteredAvg[0] + ", Y: " + filteredAvg[1] + ", Z: " + filteredAvg[2] + Environment.NewLine));
        }

        private FilteredSensor GyrometerFiltered;

        private void OnGyrometerUpdated(object sender, GyrometerReading reading)
        {
            //expects GyrometerX, Y, Z to be defined as fields
            GyrometerFiltered.add(reading.X, reading.Y, reading.Z);
            float[] filteredAvg = GyrometerFiltered.getFilteredRounded();
            GyrometerX.Text = "" + filteredAvg[0];
            GyrometerY.Text = "" + filteredAvg[1];
            GyrometerZ.Text = "" + filteredAvg[2];
            //var properties = new Dictionary<string, string>
            //    {{"name", m_robot.Name}};
            //var results = new Dictionary<string, double>
            //    { { "X", filteredAvg[0]}, { "Y", filteredAvg[1]}, {"Z", filteredAvg[2] }};
            //insights.TrackEvent("Gyrometer Update", properties, results);
            Debug.WriteLine(string.Format("Accelerometer" + Environment.NewLine + "X: " +
                filteredAvg[0] + ", Y: " + filteredAvg[1] + ", Z: " + filteredAvg[2] + Environment.NewLine));
        }

        public void ShutdownRobotConnection()
        {
            if (m_robot != null)
            {
                if (SpheroConnected)
                {
                    try
                    {
                        if (!AccelerometerFiltered.close())
                        {
                            SpheroName = "ERROR: Consult debug.";
                            Debug.WriteLine("Unable to close stream,/n" +
                        "please close application and manually delete file://" + AccelerometerFiltered.filePath);
                        }
                        if (!GyrometerFiltered.close())
                        {
                            SpheroName = "ERROR: Consult debug.";
                            Debug.WriteLine("Unable to close stream,/n" +
                        "please close application and manually delete file://" + GyrometerFiltered.filePath);
                        }

                        m_robot.SensorControl.StopAll();
                        m_robot.SensorControl.AccelerometerUpdatedEvent -= OnAccelerometerUpdated;
                        m_robot.SensorControl.GyrometerUpdatedEvent -= OnGyrometerUpdated;

                        m_robot.Sleep();
                        m_robot.Disconnect();
                        Debug.WriteLine("Sphero Disconnected");
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                m_robot = null;
            }
            SpheroConnected = false;
            InitializeSensorReading.IsEnabled = false;
            SpheroName = kNoSpheroConnected;

            RobotProvider provider = RobotProvider.GetSharedProvider();
            provider.DiscoveredRobotEvent -= OnRobotDiscovered;
            provider.NoRobotsEvent -= OnNoRobotsEvent;
            provider.ConnectedRobotEvent -= OnRobotConnected;
        }

        private void ConnectionToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (ConnectionToggle.IsOn)
            {
                if (m_robot == null)
                {
                    ConnectionToggle.OnContent = "Connecting...";
                    Debug.WriteLine("ConnectionToggle Toggled On");
                    SetupRobotConnection();
                    grdControls.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ShutdownRobotConnection();
                ConnectionToggle.OffContent = "Disconnected";
                Debug.WriteLine("ConnectionToggle Toggled Off");
                grdControls.Visibility = Visibility.Collapsed;
            }
        }

        private void InitializeSensorReading_Click(object sender, RoutedEventArgs e)
        {
            if (SpheroConnected)
            {

                const string accelFile = "AccelerometerData";
                const string gyroFile = "GyrometerData";
                AccelerometerFiltered = new FilteredSensor(FILTER_COUNTS, accelFile);
                GyrometerFiltered = new FilteredSensor(FILTER_COUNTS, gyroFile);
                m_robot.SensorControl.AccelerometerUpdatedEvent += OnAccelerometerUpdated;
                m_robot.SensorControl.GyrometerUpdatedEvent += OnGyrometerUpdated;
            }
        }
    }
}
