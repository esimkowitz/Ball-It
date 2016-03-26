using RobotKit;
using System;
using System.ComponentModel;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.IO;
using System.Text;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.ViewManagement;
using Windows.Storage.Provider;

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

        TelemetryClient insights = new TelemetryClient();

        public MainPage()
        {
            this.InitializeComponent();
            grdControls.Visibility = Visibility.Collapsed;
            InitializeSensorReading.IsEnabled = false;
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
            provider.DiscoveredRobotEvent += OnRobotDiscovered;
            provider.NoRobotsEvent += OnNoRobotsEvent;
            provider.ConnectedRobotEvent += OnRobotConnected;
            provider.FindRobots();
        }

        //! @brief  when a robot is discovered, connect! 
        private void OnRobotDiscovered(object sender, Robot robot)
        {
            if (!SpheroConnected)
            {
                Debug.WriteLine(string.Format(kConnectingToSphero, robot.BluetoothName));
                RobotProvider provider = RobotProvider.GetSharedProvider();

                if (m_robot == null)
                {
                    provider.ConnectRobot(robot);
                }
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
            Debug.WriteLine(kNoSpheroConnected);
        }

        //! @brief  when a robot is connected, get ready to drive!
        private void OnRobotConnected(object sender, Robot robot)
        {
            Debug.WriteLine(string.Format(kSpheroConnected, robot));
            m_robot = (Sphero)robot;
            ConnectionToggle.IsOn = true;
            ConnectionToggle.OnContent = "Connected";
            SpheroConnected = true;
            m_robot.SetRGBLED(255, 255, 255);
            SpheroName = string.Format(kSpheroConnected, robot.BluetoothName);
            InitializeSensorReading.IsEnabled = true;

            m_robot.SensorControl.Hz = 15;
            Debug.WriteLine("SensorControl");
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
            Debug.WriteLine(string.Format("Gyrometer" + Environment.NewLine + "X: " +
            reading.X + ", Y: " + reading.Y + ", Z: " + reading.Z + Environment.NewLine));
        }

        public void ShutdownRobotConnection()
        {
            if (m_robot != null)
            {
                if (SpheroConnected)
                {
                    try
                    {
                        m_robot.SensorControl.StopAll();
                        m_robot.Sleep();
                        m_robot.Disconnect();
                        Debug.WriteLine("Sphero Disconnected");

                        m_robot.SensorControl.AccelerometerUpdatedEvent -= OnAccelerometerUpdated;
                        m_robot.SensorControl.GyrometerUpdatedEvent -= OnGyrometerUpdated;

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

        private async void InitializeSensorReading_Click(object sender, RoutedEventArgs e)
        {
            if (SpheroConnected)
            {
                if (EnsureUnsnapped())
                {
                    FileSavePicker savePicker = new FileSavePicker();
                    savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                    // Dropdown of file types the user can save the file as
                    savePicker.FileTypeChoices.Add("Comma-Separated-Values", new List<string>() { ".csv" });
                    
                    // Default file name if the user does not type one in or select a file to replace
                    savePicker.SuggestedFileName = "AccelerometerData";
                    StorageFile AccelerometerFile = await savePicker.PickSaveFileAsync();
                    savePicker.SuggestedFileName = "GyrometerData";
                    StorageFile GyrometerFile = await savePicker.PickSaveFileAsync();
                    if (AccelerometerFile != null && GyrometerFile != null)
                    {
                        // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(AccelerometerFile);
                        CachedFileManager.DeferUpdates(GyrometerFile);
                        // write to file
                        // Application now has read/write access to all contents in the picked folder (including other sub-folder contents)
                        StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFileToken", AccelerometerFile);
                        StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFileToken", GyrometerFile);
                        Debug.WriteLine("Gyrometer file: " + GyrometerFile.Name);
                        Debug.WriteLine("Accelerometer file: " + AccelerometerFile.Name);

                        AccelerometerFiltered = new FilteredSensor(FILTER_COUNTS, AccelerometerFile);
                        GyrometerFiltered = new FilteredSensor(FILTER_COUNTS, GyrometerFile);
                        
                        // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                        // Completing updates may require Windows to ask for user input.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(GyrometerFile);
                        FileUpdateStatus status1 = await CachedFileManager.CompleteUpdatesAsync(AccelerometerFile);
                        if (status == FileUpdateStatus.Complete && status1 == FileUpdateStatus.Complete)
                        {
                            Debug.WriteLine("File " + GyrometerFile.Name + " was saved.");
                            Debug.WriteLine("File " + AccelerometerFile.Name + " was saved.");

                            InitializeSensorReading.IsEnabled = false;
                            Debug.WriteLine("GyrometerEvent");
                            m_robot.SensorControl.GyrometerUpdatedEvent += OnGyrometerUpdated;
                            Debug.WriteLine("AccelerometerEvent");
                            m_robot.SensorControl.AccelerometerUpdatedEvent += OnAccelerometerUpdated;
                        }
                        else
                        {
                            Debug.WriteLine("File " + GyrometerFile.Name + " couldn't be saved.");
                            Debug.WriteLine("File " + AccelerometerFile.Name + " couldn't be saved.");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Operation cancelled.");
                    }
                }
            }
        }

        internal bool EnsureUnsnapped()
        {
            // FilePicker APIs will not work if the application is in a snapped state.
            // If an app wants to show a FilePicker while snapped, it must attempt to unsnap first
            bool unsnapped = ((ApplicationView.Value != ApplicationViewState.Snapped) || ApplicationView.TryUnsnap());
            if (!unsnapped)
            {
                //NotifyUser("Cannot unsnap the sample.", NotifyType.StatusMessage);
            }

            return unsnapped;
        }
    }
}
