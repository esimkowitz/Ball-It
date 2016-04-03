using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Provider;
using Windows.Storage.Streams;

namespace SpheroDemo
{
    class FilteredSensor
    {
        private float[,] f;
        private int i;
        private int count;
        private StreamWriter writer;
        public string filePath;

        public FilteredSensor(int count, string file )
        {
            f = new float[count, 3];
            i = 0;
            this.count = count;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            DateTime current = DateTime.UtcNow;
            string filename = string.Format(file + "-{0}{1}{2}-{3}{4}.csv",
                current.Year, current.Month, current.Day, current.Hour, current.Minute);
            filePath = string.Format(storageFolder.Path + "\\" + filename);
            Debug.WriteLine("File Path: " + filePath);
            writer = File.CreateText(filePath);
        }

        private void CreatingCsvFiles(float[] f)
        {
            string output = string.Format("{0},{1},{2}\n", f[0], f[1], f[2]); //*add the values that you want 
            writer.WriteLine(output);
            Debug.WriteLine(output);
            //// Clear previous returned folder name, if it exists, between iterations of this scenario
            //CachedFileManager.DeferUpdates(file);
            //StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFileToken", file);
            //using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            //{
            //    using (DataWriter writer = new DataWriter(stream))
            //    {
            //        string output = string.Format("{0},{1},{2}\n", f[0], f[1], f[2]); //*add the values that you want 
            //        writer.WriteString(output);
            //        Debug.WriteLine("Data written");
            //    }
            //}

            //FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
            //if (status == FileUpdateStatus.Complete)
            //{
            //    Debug.WriteLine("File " + file.Name + " was saved.");
            //}
            //else
            //{
            //    Debug.WriteLine("File " + file.Name + " couldn't be saved.");
            //}
        }

        public void add(float x, float y, float z)
        {
            f[i % count, 0] = x;
            f[i % count, 1] = y;
            f[i % count, 2] = z;
            i++;
            if (i > count)
            {
                i = 0;
            }
        }

        public float[] getFiltered()
        {
            float[] avg = { 0, 0, 0 };
            for (int j = 0; j < count; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    avg[k] += f[j, k];
                }
            }

            for (int j = 0; j < 3; j++)
            {
                avg[j] = avg[j] / count;
            }
            CreatingCsvFiles(avg);
            return avg;
        }

        public float[] getFilteredRounded()
        {
            float[] avg = getFiltered();

            for (int j = 0; j < 3; j++)
            {
                avg[j] = (float)Math.Round((decimal)avg[j], 3, MidpointRounding.AwayFromZero);
            }
            CreatingCsvFiles(avg);
            return avg;
        }

        public Boolean close()
        {
            try
            {
                Debug.WriteLine("Closing stream to file://" + filePath);
                writer.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
