using System;
using System.Collections.Generic;
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

        public FilteredSensor(int count)
        {
            f = new float[count,3];
            i = 0;
            this.count = count;
        }

        public void add(float x, float y, float z)
        {
            f[i % count, 0] = x;
            f[i % count, 1] = y;
            f[i % count, 2] = z;
            i++;
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
            return avg;
        }
    }
}
