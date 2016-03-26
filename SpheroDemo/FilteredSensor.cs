using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpheroDemo
{
    class FilteredSensor
    {
        float[,] f;
        private int i;
        private int count;
        private string filename;

        public FilteredSensor(int count, string name)
        {
            f = new float[count,3];
            i = 0;
            DateTime timeCreated = DateTime.UtcNow;
            filename = string.Format(name + "-{0}{1}{2}-{3}{4}.csv",
                timeCreated.Year, timeCreated.Month, timeCreated.Day, timeCreated.Hour, timeCreated.Minute);
            this.count = count;
        }

        private void CreatingCsvFiles(float[] f)
        {
            string filePath = "E:\\" + filename;
            var t = Task.Run(() => {
                File.OpenWrite(filePath);
            });
            t.Wait();
            
            string delimiter = ",";
            string[][] output = new string[][]{
            new string[] {string.Format("{0}", f[0]), string.Format("{0}", f[1]), string.Format("{0}", f[2])} /*add the values that you want 
                                    inside a csv file. Mostly this function can be used in a foreach loop.*/
            };
            int length = output.GetLength(0);
            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < length; index++)
                sb.AppendLine(string.Join(delimiter, output[index]));
            File.AppendAllText(filePath, sb.ToString());
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
            //CreatingCsvFiles(avg);
            return avg;
        }

        public float[] getFilteredRounded()
        {
            float[] avg = getFiltered();

            for (int j = 0; j < 3; j++)
            {
                avg[j] = (float)Math.Round((decimal)avg[j], 3, MidpointRounding.AwayFromZero);
            }
            //CreatingCsvFiles(avg);
            return avg;
        }
    }
}
