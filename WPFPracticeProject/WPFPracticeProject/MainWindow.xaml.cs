using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace WPFPracticeProject
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private object[] array;
        private bool isFirstElementAdded = false;

        public MainWindow()
        {
            InitializeComponent();

            intRadioButton.Checked += IntRadioButton_Checked;
            floatRadioButton.Checked += FloatRadioButton_Checked;
            dateRadioButton.Checked += DateRadioButton_Checked;
            arraySizeSlider.ValueChanged += ArraySizeSlider_ValueChanged;

            arraySizeSlider.ValueChanged += (s, e) => {
                arraySizeText.Text = ((int)arraySizeSlider.Value).ToString();
            };
        }
        private void IntRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            intSlider.IsEnabled = true;
            DatePickerControl.IsEnabled = false;
        }

        private void FloatRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            intSlider.IsEnabled = false;
            DatePickerControl.IsEnabled = false;
        }

        private void DateRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            intSlider.IsEnabled = false;
            DatePickerControl.IsEnabled = true;
        }

        private void ArraySizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int size = (int)arraySizeSlider.Value;
            array = new object[size];

            arrayInputItems.Items.Clear();
            for (int i = 0; i < size; i++)
            {
                arrayInputItems.Items.Add(new { Index = i });
            }

            UpdateArrayDisplay();
        }

        private void ArrayElement_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var item = (StackPanel)textBox.Parent; 
            var indexText = (TextBlock)item.Children[1];
            int index = int.Parse(indexText.Text);

            if (intRadioButton.IsChecked == true)
            {
                if (int.TryParse(textBox.Text, out int value))
                    array[index] = value;
            }
            else if (floatRadioButton.IsChecked == true)
            {
                if (float.TryParse(textBox.Text, out float value))
                    array[index] = value;
            }
            else if (dateRadioButton.IsChecked == true)
            {
                if (DateTime.TryParse(textBox.Text, out DateTime value))
                    array[index] = value;
            }

            if (!isFirstElementAdded && array.Any(arrayItem => arrayItem != null))
            {
                isFirstElementAdded = true;
            }

            CheckArrayCompletion();
            UpdateArrayDisplay();

        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            if (array == null || array.Length == 0) return;

            try
            {
                object[] sortedArray = (object[])array.Clone();

                if (bubbleSortRadio.IsChecked == true)
                {
                    BubbleSort(sortedArray);
                }
                else if (selectionSortRadio.IsChecked == true)
                {
                    SelectionSort(sortedArray);
                }
                else if (quickSortRadio.IsChecked == true)
                {
                    QuickSort(sortedArray, 0, sortedArray.Length - 1);
                }

                sortedArrayDisplay.Text = string.Join(Environment.NewLine,
                    sortedArray.Select((item, index) => $"sorted[{index}] = {item}"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сортировке: {ex.Message}");
            }
        }

        // Сортировка выбором
        private void SelectionSort(object[] arr)
        {
            for (int i = 0; i < arr.Length - 1; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j < arr.Length; j++)
                {
                    if (CompareObjects(arr[j], arr[minIndex]) < 0)
                    {
                        minIndex = j;
                    }
                }
                (arr[i], arr[minIndex]) = (arr[minIndex], arr[i]);
            }
        }

        // Быстрая сортировка
        private void QuickSort(object[] arr, int left, int right)
        {
            if (left < right)
            {
                int pivotIndex = Partition(arr, left, right);
                QuickSort(arr, left, pivotIndex - 1);
                QuickSort(arr, pivotIndex + 1, right);
            }
        }

        private int Partition(object[] arr, int left, int right)
        {
            object pivot = arr[right];
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                if (CompareObjects(arr[j], pivot) <= 0)
                {
                    i++;
                    (arr[i], arr[j]) = (arr[j], arr[i]);
                }
            }
            (arr[i + 1], arr[right]) = (arr[right], arr[i + 1]);
            return i + 1;
        }

        // Пузырьковая сортировка
        private void BubbleSort(object[] arr)
        {
            for (int i = 0; i < arr.Length - 1; i++)
            {
                for (int j = 0; j < arr.Length - 1 - i; j++)
                {
                    if (CompareObjects(arr[j], arr[j + 1]) > 0)
                    {
                        (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                    }
                }
            }
        }

        // Вспомогательный метод для сравнения
        private int CompareObjects(object a, object b)
        {
            return ((IComparable)a).CompareTo(b);
        }

        private void UpdateArrayDisplay()
        {
            arrayDisplay.Text = string.Join(Environment.NewLine,
                array.Select((item, index) => $"array[{index}] = {item ?? "null"}"));
        }
        private void CheckArrayCompletion()
        {
            bool isArrayFull = array != null && array.All(item => item != null);
            sortTab.IsEnabled = isArrayFull; // Включаем/выключаем вкладку сортировок
        }


    }
}
