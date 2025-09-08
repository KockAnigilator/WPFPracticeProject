using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WPFPracticeProject
{
    /// <summary>
    /// Главное окно приложения для работы с массивами различных типов данных,
    /// сортировками и файловой системой
    /// </summary>
    public partial class MainWindow : Window
    {
        private object[] _array;
        private bool _isFirstElementAdded = false;
        private readonly List<AppFile> _appFiles = new List<AppFile>();

        public MainWindow()
        {
            InitializeComponent();
            SubscribeToEvents();
        }

        /// <summary>
        /// Подписка на все необходимые события элементов управления
        /// </summary>
        private void SubscribeToEvents()
        {
            intRadioButton.Checked += OnIntRadioButtonChecked;
            floatRadioButton.Checked += OnFloatRadioButtonChecked;
            dateRadioButton.Checked += OnDateRadioButtonChecked;
            arraySizeSlider.ValueChanged += OnArraySizeSliderValueChanged;

            // Обновление текстового поля при изменении размера массива
            arraySizeSlider.ValueChanged += (s, e) =>
            {
                arraySizeText.Text = ((int)arraySizeSlider.Value).ToString();
            };
        }

        #region Типы данных
        /// <summary>
        /// Обработчик выбора целочисленного типа данных
        /// </summary>
        private void OnIntRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            intSlider.IsEnabled = true;
            DatePickerControl.IsEnabled = false;
        }

        /// <summary>
        /// Обработчик выбора дробного типа данных
        /// </summary>
        private void OnFloatRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            intSlider.IsEnabled = false;
            DatePickerControl.IsEnabled = false;
        }

        /// <summary>
        /// Обработчик выбора типа данных "Дата"
        /// </summary>
        private void OnDateRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            intSlider.IsEnabled = false;
            DatePickerControl.IsEnabled = true;
        }
        #endregion

        #region Работа с массивами
        /// <summary>
        /// Обработчик изменения размера массива через слайдер
        /// </summary>
        private void OnArraySizeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int size = (int)arraySizeSlider.Value;
            _array = new object[size];

            // Очищаем и создаем новые поля для ввода элементов
            arrayInputItems.Items.Clear();
            for (int i = 0; i < size; i++)
            {
                arrayInputItems.Items.Add(new { Index = i });
            }

            UpdateArrayDisplay();
        }

        /// <summary>
        /// Обработчик изменения текста в элементах массива
        /// </summary>
        private void OnArrayElementTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var stackPanel = (StackPanel)textBox.Parent;
            var indexText = (TextBlock)stackPanel.Children[1];
            int index = int.Parse(indexText.Text);

            UpdateArrayElementValue(textBox.Text, index);
            CheckFirstElementAddition();
            CheckArrayCompletion();
            UpdateArrayDisplay();
        }

        /// <summary>
        /// Обновление значения элемента массива в соответствии с выбранным типом данных
        /// </summary>
        private void UpdateArrayElementValue(string text, int index)
        {
            if (intRadioButton.IsChecked == true && int.TryParse(text, out int intValue))
            {
                _array[index] = intValue;
            }
            else if (floatRadioButton.IsChecked == true && float.TryParse(text, out float floatValue))
            {
                _array[index] = floatValue;
            }
            else if (dateRadioButton.IsChecked == true && DateTime.TryParse(text, out DateTime dateValue))
            {
                _array[index] = dateValue;
            }
        }

        /// <summary>
        /// Проверка добавления первого элемента для блокировки изменения типа данных
        /// </summary>
        private void CheckFirstElementAddition()
        {
            if (!_isFirstElementAdded && _array.Any(item => item != null))
            {
                _isFirstElementAdded = true;
            }
        }

        /// <summary>
        /// Обновление отображения массива в текстовом поле
        /// </summary>
        private void UpdateArrayDisplay()
        {
            if (_array == null) return;

            arrayDisplay.Text = string.Join(Environment.NewLine,
                _array.Select((item, index) => $"array[{index}] = {item ?? "null"}"));
        }

        /// <summary>
        /// Проверка полного заполнения массива для активации вкладки сортировок
        /// </summary>
        private void CheckArrayCompletion()
        {
            bool isArrayFull = _array != null && _array.All(item => item != null);
            sortTab.IsEnabled = isArrayFull;
        }
        #endregion

        #region Сортировки
        /// <summary>
        /// Обработчик нажатия кнопки выполнения сортировки
        /// </summary>
        private void OnSortButtonClick(object sender, RoutedEventArgs e)
        {
            if (_array == null || _array.Length == 0) return;

            try
            {
                object[] sortedArray = (object[])_array.Clone();
                PerformSorting(sortedArray);
                DisplaySortedArray(sortedArray);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при сортировке: {ex.Message}");
            }
        }

        /// <summary>
        /// Выполнение выбранного алгоритма сортировки
        /// </summary>
        private void PerformSorting(object[] arrayToSort)
        {
            if (bubbleSortRadio.IsChecked == true)
            {
                BubbleSort(arrayToSort);
            }
            else if (selectionSortRadio.IsChecked == true)
            {
                SelectionSort(arrayToSort);
            }
            else if (quickSortRadio.IsChecked == true)
            {
                QuickSort(arrayToSort, 0, arrayToSort.Length - 1);
            }
        }

        /// <summary>
        /// Отображение отсортированного массива в интерфейсе
        /// </summary>
        private void DisplaySortedArray(object[] sortedArray)
        {
            sortedArrayDisplay.Text = string.Join(Environment.NewLine,
                sortedArray.Select((item, index) => $"sorted[{index}] = {item}"));
        }

        /// <summary>
        /// Алгоритм сортировки выбором
        /// </summary>
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
                Swap(arr, i, minIndex);
            }
        }

        /// <summary>
        /// Алгоритм быстрой сортировки (рекурсивный)
        /// </summary>
        private void QuickSort(object[] arr, int left, int right)
        {
            if (left < right)
            {
                int pivotIndex = Partition(arr, left, right);
                QuickSort(arr, left, pivotIndex - 1);
                QuickSort(arr, pivotIndex + 1, right);
            }
        }

        /// <summary>
        /// Вспомогательный метод для быстрой сортировки - разделение массива
        /// </summary>
        private int Partition(object[] arr, int left, int right)
        {
            object pivot = arr[right];
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                if (CompareObjects(arr[j], pivot) <= 0)
                {
                    i++;
                    Swap(arr, i, j);
                }
            }
            Swap(arr, i + 1, right);
            return i + 1;
        }

        /// <summary>
        /// Алгоритм пузырьковой сортировки
        /// </summary>
        private void BubbleSort(object[] arr)
        {
            for (int i = 0; i < arr.Length - 1; i++)
            {
                for (int j = 0; j < arr.Length - 1 - i; j++)
                {
                    if (CompareObjects(arr[j], arr[j + 1]) > 0)
                    {
                        Swap(arr, j, j + 1);
                    }
                }
            }
        }

        /// <summary>
        /// Обмен элементов массива местами
        /// </summary>
        private void Swap(object[] arr, int index1, int index2)
        {
            (arr[index1], arr[index2]) = (arr[index2], arr[index1]);
        }

        /// <summary>
        /// Сравнение двух объектов для сортировки
        /// </summary>
        private int CompareObjects(object a, object b)
        {
            return ((IComparable)a).CompareTo(b);
        }
        #endregion

        #region Работа с файлами
        /// <summary>
        /// Обработчик добавления нового файла в дерево
        /// </summary>
        private void OnAddFileButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
                };

                if (dialog.ShowDialog() == true)
                {
                    AddNewFile(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при добавлении файла: {ex.Message}");
            }
        }

        /// <summary>
        /// Добавление нового файла в коллекцию приложения
        /// </summary>
        private void AddNewFile(string filePath)
        {
            var newFile = new AppFile
            {
                Name = System.IO.Path.GetFileName(filePath),
                Content = "Содержимое файла будет здесь..."
            };

            _appFiles.Add(newFile);
            UpdateFilesTree();
        }

        /// <summary>
        /// Обновление дерева файлов в интерфейсе
        /// </summary>
        private void UpdateFilesTree()
        {
            filesTreeView.Items.Clear();

            var rootNode = new FileNode
            {
                Name = "Файлы приложения",
                IsDirectory = true
            };

            foreach (var file in _appFiles)
            {
                rootNode.Children.Add(new FileNode
                {
                    Name = file.Name,
                    Path = file.Name,
                    IsDirectory = false
                });
            }

            filesTreeView.Items.Add(rootNode);
        }

        /// <summary>
        /// Обработчик загрузки содержимого выбранного файла
        /// </summary>
        private void OnLoadFileContentClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedNode = filesTreeView.SelectedItem as FileNode;
                if (selectedNode == null || selectedNode.IsDirectory) return;

                LoadFileContent(selectedNode.Name);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при загрузке содержимого: {ex.Message}");
            }
        }

        /// <summary>
        /// Загрузка и отображение содержимого файла
        /// </summary>
        private void LoadFileContent(string fileName)
        {
            var file = _appFiles.FirstOrDefault(f => f.Name == fileName);
            if (file != null)
            {
                fileContentTextBox.Text = file.Content;
            }
        }

        /// <summary>
        /// Обработчик обновления дерева файлов
        /// </summary>
        private void OnRefreshTreeButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateFilesTree();
                ShowInformationMessage("Дерево файлов обновлено");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при обновлении дерева: {ex.Message}");
            }
        }

        /// <summary>
        /// Обработчик удаления файла из дерева
        /// </summary>
        private void OnDeleteFileClick(object sender, RoutedEventArgs e)
        {
            var selectedNode = filesTreeView.SelectedItem as FileNode;
            if (selectedNode == null || selectedNode.IsDirectory) return;

            DeleteFileWithConfirmation(selectedNode.Name);
        }

        /// <summary>
        /// Удаление файла с подтверждением действия
        /// </summary>
        private void DeleteFileWithConfirmation(string fileName)
        {
            var result = MessageBox.Show($"Удалить файл {fileName}?",
                "Подтверждение", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                _appFiles.RemoveAll(f => f.Name == fileName);
                UpdateFilesTree();
                fileContentTextBox.Text = string.Empty;
            }
        }

        /// <summary>
        /// Показать сообщение об ошибке
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Показать информационное сообщение
        /// </summary>
        private void ShowInformationMessage(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion
    }

    /// <summary>
    /// Класс узла дерева файлов
    /// </summary>
    public class FileNode
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsDirectory { get; set; }
        public List<FileNode> Children { get; set; } = new List<FileNode>();
    }

    /// <summary>
    /// Класс файла приложения для хранения метаинформации и содержимого
    /// </summary>
    public class AppFile
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}