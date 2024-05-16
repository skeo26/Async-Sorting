using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncMergeSortWPF
{
    public class InsertionSorter
    {
        public void InsertionSort(int[] data)
        {
            int n = data.Length;
            for (int i = 1; i < n; ++i)
            {
                int key = data[i];
                int j = i - 1;
                while (j >= 0 && data[j] > key)
                {
                    data[j + 1] = data[j];
                    j = j - 1;
                }
                data[j + 1] = key;
            }
        }
    }
}
