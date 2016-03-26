using System;
using System.Diagnostics;
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
        float[,] f;
        private int i;
        private int count;
        StorageFile file;

        public FilteredSensor(int count, StorageFile file)
        {
            f = new float[count, 3];
            i = 0;
            this.file = file;
            this.count = count;
        }

        private async void CreatingCsvFiles(float[] f)
        {
            // Clear previous returned folder name, if it exists, between iterations of this scenario
            CachedFileManager.DeferUpdates(file);
            StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFileToken", file);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (DataWriter writer = new DataWriter(stream))
                {
                    string delimiter = ",";
                    string[][] output = new string[][]{
                    new string[] {string.Format("{0}", f[0]), string.Format("{0}", f[1]), string.Format("{0}", f[2])} /*add the values that you want 
                                    inside a csv file. Mostly this function can be used in a foreach loop.*/
                };
                    int length = output.GetLength(0);
                    StringBuilder sb = new StringBuilder();
                    for (int index = 0; index < length; index++)
                        sb.AppendLine(string.Join(delimiter, output[index]));
                    writer.WriteString(sb.ToString());
                }
            }
            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
            if (status == FileUpdateStatus.Complete)
            {
                Debug.WriteLine("File " + file.Name + " was saved.");
            }
            else
            {
                Debug.WriteLine("File " + file.Name + " couldn't be saved.");
            }
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
    }
}
