using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MergeLibrary;


namespace AsyncMergeSortWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cancellationTokenSource;
        public MainWindow()
        {
            InitializeComponent();
            sliderArraySize.ValueChanged += SliderArraySize_ValueChanged;
        }

        private async void buttonStartSort_Click(object sender, RoutedEventArgs e)
        {
            int size = (int)sliderArraySize.Value;
            int[] data = GenerateRandomData(size);

            progressBar.Visibility = Visibility.Visible;
            progressBar.IsIndeterminate = true;

            txtStatus.Text = "Статус: Сортировка...";

            Stopwatch watch = Stopwatch.StartNew();
            await Task.Run(() =>
                {
                    BaseSorter sorter = new CorrectParallelSorter();
                    sorter.Sort(data);
                }
            );

            watch.Stop();
            progressBar.IsIndeterminate = false;
            progressBar.Visibility = Visibility.Hidden;
            txtStatus.Text = $"Статус: Выполнено. Затраченное Время: {watch.ElapsedMilliseconds} мс";
        }

        private void SliderArraySize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtArraySize != null)
            {
                txtArraySize.Text = Math.Round(sliderArraySize.Value).ToString();
            }
        }

        private int[] GenerateRandomData(int size)
        {
            int[] resultArr = new int[size];

            Random rnd = new Random();

            for (int i = 0; i < resultArr.Length; i++)
            {
                resultArr[i] = rnd.Next(0, 100000);
            }

            return resultArr;
        }

        private void buttonBlockSort_Click(object sender, RoutedEventArgs e)
        {
            int size = (int)sliderArraySize.Value;
            int[] data = GenerateRandomData(size);

            progressBar.Visibility = Visibility.Visible;
            progressBar.IsIndeterminate = true;

            txtStatus.Text = "Статус: Сортировка...";

            Stopwatch watch = Stopwatch.StartNew();
            Task.Run(() =>
                {
                    BaseSorter sorter = new CorrectParallelSorter();
                    sorter.Sort(data);
                }
            ).Wait();

            watch.Stop();
            progressBar.IsIndeterminate = false;
            progressBar.Visibility = Visibility.Hidden;
            txtStatus.Text = $"Статус: Выполнено. Затраченное Время: {watch.ElapsedMilliseconds} мс";
        }

        private void buttonWait_Click(object sender, RoutedEventArgs e)
        {

            int size = (int)sliderArraySize.Value;
            int[] data1 = GenerateRandomData(size);
            int[] data2 = (int[])data1.Clone();
            int[] data3 = (int[])data1.Clone();

            QuickSorter quickSorter = new QuickSorter();
            InsertionSorter insertionSorter = new InsertionSorter();

            var task1 = new Task(() => quickSorter.QuickSort(data1, 0, data1.Length - 1));
            var task2 = new Task(() => insertionSorter.InsertionSort(data2));
            var task3 = new Task(() => new CorrectParallelSorter().Sort(data3));

            Task[] tasks = { task1, task2, task3 };

            foreach (var task in tasks)
            {
                task.Start();
            }

            txtStatus.Text += ShowTaskStatuses(tasks, "перед WhenAny");
            Task.Delay(3000).Wait();

            int firstFinished = Task.WaitAny(tasks);
            txtStatus.Text += $"Индекс первой завершившейся задачи равен {firstFinished}";
            Task.Delay(3000).Wait();

            ShowTaskStatuses(tasks, "сразу после WaitAny");
            Task.Delay(3000).Wait();
            ShowTaskStatuses(tasks, "чуть-чуть подождав");

            Task.WaitAll(tasks);
            txtStatus.Text += ShowTaskStatuses(tasks, "\nпосле WaitAll");
            
        }

        private async void buttonWhen_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            int size = (int)sliderArraySize.Value;
            int[] data1 = GenerateRandomData(size);
            int[] data2 = (int[])data1.Clone();
            int[] data3 = (int[])data1.Clone();

            QuickSorter quickSorter = new QuickSorter();
            InsertionSorter insertionSorter = new InsertionSorter();

            var task1 = Task.Run(() => quickSorter.QuickSort(data1, 0, data1.Length - 1), token);
            var task2 = Task.Run(() => insertionSorter.InsertionSort(data2), token);
            var task3 = Task.Run(() => new CorrectParallelSorter().Sort(data3), token);

            Task[] tasks = { task1, task2, task3 };

            txtStatus.Text = ShowTaskStatuses(tasks, "перед WhenAny");
            await Task.Delay(3000);

            try
            {
                token.ThrowIfCancellationRequested();

                var firstFinished = await Task.WhenAny(tasks);
                txtStatus.Text = $"\nПервая завершённая сортировка: {GetTaskLabel(firstFinished, task1, task2, task3)}";
                    
                await Task.Delay(3000);

                progressBar.Visibility = Visibility.Visible;
                progressBar.IsIndeterminate = true;

                await Task.WhenAll(tasks);

                progressBar.IsIndeterminate = false;
                progressBar.Visibility = Visibility.Hidden;
                txtStatus.Text = ShowTaskStatuses(tasks, "после WhenAll");
            }
            catch (OperationCanceledException)
            {
                txtStatus.Text = "Операция была отменена";
                progressBar.IsIndeterminate = false;
                progressBar.Visibility = Visibility.Hidden;
            }
        }

        private static string ShowTaskStatuses(Task[] tasks, string when)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(when);
            for (int i = 0; i < tasks.Length; i++)
            {
                Task task = tasks[i];
                sb.AppendLine($"статус задачи #{i} - {task.Status}");
            }

            return sb.ToString();
        }

        private string GetTaskLabel(Task completedTask, Task task1, Task task2, Task task3)
        {
            if (completedTask == task1)
                return $"Быстрая сортировка. Статус задачи - {task1.Status}";
            else if (completedTask == task2)
                return $"Сортировка вставками. Статус задачи - {task2.Status}";
            else
                return $"Cортировка слиянием. Статус задачи - {task3.Status}";
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
        }
    }
}
