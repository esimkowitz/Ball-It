using RobotKit;
using System;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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

        private string SpheroName_;
        public string SpheroName
        {
            get { return SpheroName_; }
            set
            {
                SpheroName_ = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SpheroName"));
                }
            }
        }

        private const string kNoSpheroConnected = "No Sphero Connected";

        //! @brief  the default string to show when connecting to a sphero ({0})
        private const string kConnectingToSphero = "Connecting to {0}";

        //! @brief  the default string to show when connected to a sphero ({0})
        private const string kSpheroConnected = "Connected to {0}";

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary> 
        /// Invoked when this page is about to be displayed in a Frame. 
        /// </summary> 
        /// <param name="e">Event data that describes how this page was reached.  The Parameter 
        /// property is typically used to configure the page.</param> 
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Application app = Application.Current;
            app.Suspending += OnSuspending;
        }

        //! @brief  handle the application entering the background 
        private void OnSuspending(object sender, SuspendingEventArgs args)
        {
            ShutdownRobotConnection();
            ConnectionToggle.IsOn = false;
        }

        /*! 
          * @brief   handle the user launching this page in the application 
          *  
          *  connects to sphero and sets up the ui 
          */
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ShutdownRobotConnection();
            Application app = Application.Current;
            app.Suspending -= OnSuspending;
            ConnectionToggle.IsOn = false;
        }

        public void SetupRobotConnection()
        {
            RobotProvider provider = RobotProvider.GetSharedProvider();
            provider.DiscoveredRobotEvent += OnRobotDiscovered;
            provider.NoRobotsEvent += OnNoRobotsEvent;
            provider.ConnectedRobotEvent += OnRobotConnected;
            provider.FindRobots();
        }

        //! @brief  when a robot is discovered, connect! 
        private void OnRobotDiscovered(object sender, Robot robot)
        {
            MessageDialog dialog = new MessageDialog(
                string.Format(kConnectingToSphero, robot.BluetoothName));

            if (m_robot == null)
            {
                RobotProvider provider = RobotProvider.GetSharedProvider();
                provider.ConnectRobot(robot);
                m_robot = (Sphero)robot;
            }
        }

        private bool SpheroConnected_ = false;
        public bool SpheroConnected
        {
            get { return SpheroConnected_; }
            set
            {
                SpheroConnected_ = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SpheroConnected"));
                }
            }
        }

        private void OnNoRobotsEvent(object sender, EventArgs e)
        {
            MessageDialog dialog = new MessageDialog(kNoSpheroConnected);
            ConnectionToggle.IsOn = false;
            ConnectionToggle.OffContent = "Failed";
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
        }

        //! @brief  when a robot is connected, get ready to drive!
        private void OnRobotConnected(object sender, Robot robot)
        {
            MessageDialog dialog = new MessageDialog(string.Format(kSpheroConnected, robot));
            ConnectionToggle.IsOn = true;
            ConnectionToggle.OnContent = "Connected";
            SpheroConnected = true;
            m_robot.SetRGBLED(255, 255, 255);
            SpheroName = string.Format(kSpheroConnected, robot.BluetoothName);

            m_robot.SensorControl.Hz = 40;
            m_robot.SensorControl.GyrometerUpdatedEvent += OnGyrometerUpdated;
            m_robot.SensorControl.AccelerometerUpdatedEvent += OnAccelerometerUpdated;
            //m_robot.CollisionControl.StartDetectionForWallCollisions();
            //m_robot.CollisionControl.CollisionDetectedEvent += OnCollisionDetected;
        }

        private void OnAccelerometerUpdated(object sender, AccelerometerReading reading)
        {
            // expects AccelerometerX,Y,Z to be defined as fields
            AccelerometerX.Text = "" + reading.X;
            AccelerometerY.Text = "" + reading.Y;
            AccelerometerZ.Text = "" + reading.Z;
        }

        private void OnGyrometerUpdated(object sender, GyrometerReading reading)
        {
            // expects GyrometerX,Y,Z to be defined as fields
            GyrometerX.Text = "" + reading.X;
            GyrometerY.Text = "" + reading.Y;
            GyrometerZ.Text = "" + reading.Z;
        }

        public void ShutdownRobotConnection()
        {
            if (m_robot != null)
            {
                try
                {
                    m_robot.SensorControl.StopAll();
                    m_robot.Sleep();
                    m_robot.Disconnect();

                    m_robot.SensorControl.AccelerometerUpdatedEvent -= OnAccelerometerUpdated;
                    m_robot.SensorControl.GyrometerUpdatedEvent -= OnGyrometerUpdated;

                    RobotProvider provider = RobotProvider.GetSharedProvider();
                    provider.DiscoveredRobotEvent -= OnRobotDiscovered;
                    provider.NoRobotsEvent -= OnNoRobotsEvent;
                    provider.ConnectedRobotEvent -= OnRobotConnected;
                }
                catch (Exception)
                {

                    throw;
                }

            }
        }

        private void ConnectionToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (ConnectionToggle.IsOn)
            {
                if (m_robot == null)
                {
                    ConnectionToggle.OnContent = "Connecting...";
                    SetupRobotConnection();
                    grdControls.Visibility = Visibility.Visible;

                }
            }
            else
            {
                ShutdownRobotConnection();
                ConnectionToggle.OffContent = "Disconnected";
                grdControls.Visibility = Visibility.Collapsed;
            }
        }
    }
}
