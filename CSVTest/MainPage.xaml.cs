using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SpheroDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            button.Content = "Click to Start";
        }

        private static Timer aTimer;

        private bool isOn = false;

        private FilteredSensor filter;
        private const int FILTER_COUNTS = 6;

        private float RandomFloat()
        {
            Random random = new Random();
            return (float)random.NextDouble();
        }

        private void getFilter(object sender)
        {
            //expects GyrometerX, Y, Z to be defined as fields
            float[] f = new float[3];
            for (int i = 0; i < 3; i++)
            {
                f[i] = RandomFloat();
            }
            filter.add(f[0], f[1], f[2]);
            float[] filteredAvg = filter.getFilteredRounded();
            //var properties = new Dictionary<string, string>
            //    {{"name", m_robot.Name}};
            //var results = new Dictionary<string, double>
            //    { { "X", filteredAvg[0]}, { "Y", filteredAvg[1]}, {"Z", filteredAvg[2] }};
            //insights.TrackEvent("Gyrometer Update", properties, results);
            Debug.WriteLine("New CSV Write");
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (!isOn)
            {
                isOn = true;
                string file = "CSVData";
                filter = new FilteredSensor(FILTER_COUNTS, file);

                //            // Create an inferred delegate that invokes methods for the timer.
                AutoResetEvent autoEvent = new AutoResetEvent(false);
                TimerCallback tcb = getFilter;
                aTimer = new Timer(tcb, autoEvent, 1000, 250);
                button.Content = "Click to Stop";
                //if (EnsureUnsnapped())
                //{
                //    FileSavePicker savePicker = new FileSavePicker();
                //    savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                //    // Dropdown of file types the user can save the file as
                //    savePicker.FileTypeChoices.Add("Comma-Separated-Values", new List<string>() { ".csv" });

                //    // Default file name if the user does not type one in or select a file to replace
                //    savePicker.SuggestedFileName = "CSVData";
                //    StorageFile file = await savePicker.PickSaveFileAsync();
                //    if (file != null)
                //    {
                //        // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                //        CachedFileManager.DeferUpdates(file);
                //        // write to file
                //        // Application now has read/write access to all contents in the picked folder (including other sub-folder contents)
                //        StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFileToken", file);
                //        Debug.WriteLine("file: " + file.Name);

                //        // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                //        // Completing updates may require Windows to ask for user input.
                //        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                //        if (status == FileUpdateStatus.Complete)
                //        {
                //            Debug.WriteLine("File " + file.Name + " was saved.");
                //            filter = new FilteredSensor(FILTER_COUNTS, file);

                //            // Create an inferred delegate that invokes methods for the timer.
                //            AutoResetEvent autoEvent = new AutoResetEvent(false);
                //            TimerCallback tcb = getFilter;
                //            aTimer = new Timer(tcb, autoEvent, 1000, 250);
                //            button.Content = "Click to Stop";
                //        }
                //        else
                //        {
                //            Debug.WriteLine("File " + file.Name + " couldn't be saved.");
                //        }
                //    }
                //    else
                //    {
                //        Debug.WriteLine("Operation cancelled.");
                //    }
                //}
            }
            else
            {
                try
                {
                    aTimer.Dispose();
                }
                catch (Exception)
                {

                    throw;
                }
                if (filter.close())
                {
                    Debug.WriteLine("Stream successfully closed.");
                    button.Content = "Click to Start";
                    isOn = false;
                } else
                {
                    Debug.WriteLine("Unable to close stream,/n" +
                        "please close application and manually delete file://" + filter.filePath);
                }
            }
        }
    }
}
