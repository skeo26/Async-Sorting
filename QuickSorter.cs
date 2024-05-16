using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncMergeSortWPF
{
    public class QuickSorter
    {
        public void QuickSort(int[] data, int left, int right)
        {
            if (left >= right) return;
            int pivot = Partition(data, left, right);
            QuickSort(data, left, pivot - 1);
            QuickSort(data, pivot + 1, right);
        }

        private int Partition(int[] data, int left, int right)
        {
            int pivot = data[right];
            int i = left;

            for (int j = left; j < right; j++)
            {
                if (data[j] < pivot)
                {
                    Swap(data, i, j);
                    i++;
                }
            }
            Swap(data, i, right);
            return i;
        }

        private void Swap(int[] data, int i, int j)
        {
            int temp = data[i];
            data[i] = data[j];
            data[j] = temp;
        }
    }
}
