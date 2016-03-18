using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.ApplicationModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Ball_It
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        SpheroManager sp;
        int lastHeading = 0;
        public MainPage()
        {
            sp = new SpheroManager();
            DataContext = sp;

            this.InitializeComponent();

            CoreWindow.GetForCurrentThread().KeyDown += MainPage_KeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += MainPage_KeyUp;
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
            sp.ShutdownRobotConnection();
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
            sp.ShutdownRobotConnection();
            Application app = Application.Current;
            app.Suspending -= OnSuspending;
            ConnectionToggle.IsOn = false;
        }


        private void MainPage_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (sp.m_robot != null)
            {

                sp.m_robot.Roll(lastHeading, 0f);
            }
        }

        private void MainPage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (sp.m_robot != null)
            {
                switch (args.VirtualKey)
                {
                    case Windows.System.VirtualKey.Up:
                        //roll forward
                        sp.m_robot.Roll(0, (float)1);
                        lastHeading = 0;
                        break;
                    case Windows.System.VirtualKey.Down:
                        sp.m_robot.Roll(180, (float)1);
                        lastHeading = 180;
                        break;
                    case Windows.System.VirtualKey.Left:
                        sp.m_robot.Roll(270, (float)1);
                        lastHeading = 270;
                        break;
                    case Windows.System.VirtualKey.Right:
                        sp.m_robot.Roll(90, (float)1);
                        lastHeading = 90;
                        break;
                }
            }
        }
        private void ConnectionToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (ConnectionToggle.IsOn)
            {
                if (sp.m_robot == null)
                {
                    ConnectionToggle.OnContent = "Connecting...";
                    sp.SetupRobotConnection();
                    grdControls.Visibility = Visibility.Visible;

                }
            }
            else
            {
                sp.ShutdownRobotConnection();
                ConnectionToggle.OffContent = "Disconnected";
                grdControls.Visibility = Visibility.Collapsed;
            }
        }
    }
}
