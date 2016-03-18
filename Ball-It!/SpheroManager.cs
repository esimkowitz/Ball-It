using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Popups;
using RobotKit;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Navigation;

namespace Ball_It
{
    class SpheroManager : INotifyPropertyChanged
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

        public SpheroManager()
        {
            //SetupRobotConnection();
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
            Debug.WriteLine(string.Format("Discovered \"{0}\"", robot.BluetoothName));

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
        }

        //! @brief  when a robot is connected, get ready to drive!
        private void OnRobotConnected(object sender, Robot robot)
        {
            Debug.WriteLine(string.Format(kSpheroConnected, robot));
            //ConnectionToggle.IsOn = true;
            //ConnectionToggle.OnContent = "Connected";
            SpheroConnected = true;
            m_robot.SetRGBLED(255, 255, 255);
            SpheroName = string.Format(kSpheroConnected, robot.BluetoothName);

            m_robot.SensorControl.Hz = 10;
            m_robot.SensorControl.GyrometerUpdatedEvent += OnGyrometerUpdated;
            m_robot.SensorControl.AccelerometerUpdatedEvent += OnAccelerometerUpdated;
            //m_robot.CollisionControl.StartDetectionForWallCollisions();
            //m_robot.CollisionControl.CollisionDetectedEvent += OnCollisionDetected;
        }

        private void OnAccelerometerUpdated(object sender, AccelerometerReading e)
        {
            AccelReading = string.Format("X:{0}" + Environment.NewLine + "Y:{1}" + Environment.NewLine + "Z:{2}", e.X, e.Y, e.Z);
        }

        private string _AccelReading;
        public string AccelReading
        {
            get
            {
                return _AccelReading;
            }
            set
            {
                _AccelReading = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AccelReading"));
                }
            }
        }

        private void OnGyrometerUpdated(object sender, GyrometerReading e)
        {
            GyroReading = string.Format("X:{0}" + Environment.NewLine + "Y:{1}" + Environment.NewLine + "Z:{2}", e.X, e.Y, e.Z);
        }

        private string _GyroReading;
        public string GyroReading
        {
            get
            {
                return _GyroReading;
            }
            set
            {
                _GyroReading = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("GyroReading"));
                }
            }
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
    }
}