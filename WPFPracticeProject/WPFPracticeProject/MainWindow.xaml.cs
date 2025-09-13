using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace WPFPracticeProject
{
    public partial class MainWindow : Window
    {
        private object[] _array;
        private bool _isFirstElementAdded = false;
        private readonly List<AppFile> _appFiles = new List<AppFile>();
        private Type _currentDataType = typeof(int);

        public MainWindow()
        {
            InitializeComponent();
            SubscribeToEvents();
            InitializeApplication();
        }

        /// <summary>
        /// Инициализация приложения
        /// </summary>
        private void InitializeApplication()
        {
            try
            {
                // Устанавливаем обработчики и начальные значения
                arraySizeSlider.Value = 5;
                intRadioButton.IsChecked = true;
                UpdateHelpText("Выберите тип данных для работы с массивом");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка инициализации приложения: {ex.Message}");
            }
        }

        /// <summary>
        /// Подписка на события элементов управления
        /// </summary>
        private void SubscribeToEvents()
        {
            try
            {
                intRadioButton.Checked += OnIntRadioButtonChecked;
                floatRadioButton.Checked += OnFloatRadioButtonChecked;
                dateRadioButton.Checked += OnDateRadioButtonChecked;
                arraySizeSlider.ValueChanged += OnArraySizeSliderValueChanged;

                arraySizeSlider.ValueChanged += (s, e) =>
                {
                    arraySizeText.Text = $"Размер: {(int)arraySizeSlider.Value}";
                };
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка подписки на события: {ex.Message}");
            }
        }

        #region Обработчики меню и тулбара

        /// <summary>
        /// Обработчик изменения вкладки
        /// </summary>
        private void OnTabChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.Source is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
                {
                    toolbarTitle.Text = selectedTab.Header.ToString();
                    UpdateHelpTextBasedOnTab(selectedTab.Header.ToString());
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при переключении вкладки: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновление текста подсказки в зависимости от вкладки
        /// </summary>
        private void UpdateHelpTextBasedOnTab(string tabName)
        {
            switch (tabName)
            {
                case "Тип данных":
                    UpdateHelpText("Выберите тип данных для работы с массивом");
                    break;
                case "Массив":
                    UpdateHelpText("Задайте размер массива и заполните элементы. Тип данных: " + _currentDataType.Name);
                    break;
                case "Сортировки":
                    UpdateHelpText("Выберите алгоритм сортировки и нажмите кнопку выполнения");
                    break;
                case "Файлы":
                    UpdateHelpText("Управление файлами проекта. Используйте контекстное меню для действий с файлами");
                    break;
            }
        }

        /// <summary>
        /// Показать подсказку
        /// </summary>
        private void OnHelpClick(object sender, RoutedEventArgs e)
        {
            try
            {
                helpPopup.IsOpen = true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка отображения подсказки: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновление текста подсказки
        /// </summary>
        private void UpdateHelpText(string text)
        {
            helpText.Text = text;
        }

        /// <summary>
        /// Сохранение через меню
        /// </summary>
        private void OnMenuSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    Title = "Сохранить данные массива"
                };

                if (dialog.ShowDialog() == true)
                {
                    SaveArrayToFile(dialog.FileName);
                    AddFileToTree(dialog.FileName);
                    ShowInformationMessage($"Файл успешно сохранен: {Path.GetFileName(dialog.FileName)}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowErrorMessage("Нет прав для сохранения файла в выбранной директории");
            }
            catch (IOException ioEx)
            {
                ShowErrorMessage($"Ошибка ввода-вывода: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка сохранения: {ex.Message}");
            }
        }

        /// <summary>
        /// Загрузка через меню
        /// </summary>
        private void OnMenuLoadClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    Title = "Загрузить данные массива"
                };

                if (dialog.ShowDialog() == true)
                {
                    LoadArrayFromFile(dialog.FileName);
                    AddFileToTree(dialog.FileName);
                    ShowInformationMessage($"Файл успешно загружен: {Path.GetFileName(dialog.FileName)}");
                }
            }
            catch (FileNotFoundException)
            {
                ShowErrorMessage("Файл не найден");
            }
            catch (IOException ioEx)
            {
                ShowErrorMessage($"Ошибка чтения файла: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Мастер работы с массивами\nВерсия 1.0", "О программе");
        }

        #endregion

        #region Типы данных

        private void OnIntRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _currentDataType = typeof(int);
            UpdateHelpText("Выбран целочисленный тип данных (int)");
        }

        private void OnFloatRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _currentDataType = typeof(float);
            UpdateHelpText("Выбран дробный тип данных (float)");
        }

        private void OnDateRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _currentDataType = typeof(DateTime);
            UpdateHelpText("Выбран тип данных Дата");
        }

        #endregion

        #region Работа с массивами

        private void OnArraySizeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int size = (int)arraySizeSlider.Value;
                _array = new object[size];

                arrayInputItems.Items.Clear();
                for (int i = 0; i < size; i++)
                {
                    arrayInputItems.Items.Add(new { Index = i });
                }

                UpdateArrayDisplay();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка изменения размера массива: {ex.Message}");
            }
        }

        private void OnArrayElementTextChanged(object sender, TextChangedEventArgs e)
        {
            try
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
            catch (FormatException)
            {
                ShowErrorMessage("Неверный формат данных. Проверьте ввод.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка обработки ввода: {ex.Message}");
            }
        }

        private void UpdateArrayElementValue(string text, int index)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _array[index] = null;
                return;
            }

            try
            {
                if (_currentDataType == typeof(int) && int.TryParse(text, out int intValue))
                {
                    _array[index] = intValue;
                }
                else if (_currentDataType == typeof(float) && float.TryParse(text, out float floatValue))
                {
                    _array[index] = floatValue;
                }
                else if (_currentDataType == typeof(DateTime) && DateTime.TryParse(text, out DateTime dateValue))
                {
                    _array[index] = dateValue;
                }
                else
                {
                    throw new FormatException("Неверный формат данных для выбранного типа");
                }
            }
            catch (FormatException ex)
            {
                ShowErrorMessage($"Ошибка формата: {ex.Message}");
                _array[index] = null;
            }
        }

        private void CheckFirstElementAddition()
        {
            if (!_isFirstElementAdded && _array.Any(item => item != null))
            {
                _isFirstElementAdded = true;
            }
        }

        private void UpdateArrayDisplay()
        {
            try
            {
                if (_array == null) return;

                arrayDisplay.Text = string.Join(Environment.NewLine,
                    _array.Select((item, index) => $"array[{index}] = {item ?? "null"}"));
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка отображения массива: {ex.Message}");
            }
        }

        private void CheckArrayCompletion()
        {
            try
            {
                bool isArrayFull = _array != null && _array.All(item => item != null);
                sortTab.IsEnabled = isArrayFull;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка проверки заполнения массива: {ex.Message}");
            }
        }

        #endregion

        #region Сортировки

        private void OnSortButtonClick(object sender, RoutedEventArgs e)
        {
            if (_array == null || _array.Length == 0) return;

            try
            {
                object[] sortedArray = (object[])_array.Clone();
                PerformSorting(sortedArray);
                DisplaySortedArray(sortedArray);
            }
            catch (InvalidOperationException ex)
            {
                ShowErrorMessage($"Ошибка сортировки: {ex.Message}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Неожиданная ошибка при сортировке: {ex.Message}");
            }
        }

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
            else
            {
                throw new InvalidOperationException("Не выбран алгоритм сортировки");
            }
        }

        private void DisplaySortedArray(object[] sortedArray)
        {
            sortedArrayDisplay.Text = string.Join(Environment.NewLine,
                sortedArray.Select((item, index) => $"sorted[{index}] = {item}"));
        }

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
                    Swap(arr, i, j);
                }
            }
            Swap(arr, i + 1, right);
            return i + 1;
        }

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

        private void Swap(object[] arr, int index1, int index2)
        {
            (arr[index1], arr[index2]) = (arr[index2], arr[index1]);
        }

        private int CompareObjects(object a, object b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            return ((IComparable)a).CompareTo(b);
        }

        #endregion

        #region Работа с файлами

        private void OnAddFileButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    Title = "Создать новый файл"
                };

                if (dialog.ShowDialog() == true)
                {
                    AddNewFile(dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при создании файла: {ex.Message}");
            }
        }

        private void AddNewFile(string filePath)
        {
            try
            {
                var newFile = new AppFile
                {
                    Name = Path.GetFileName(filePath),
                    Content = "Содержимое файла будет здесь...",
                    FilePath = filePath
                };

                _appFiles.Add(newFile);
                UpdateFilesTree();
                ShowInformationMessage($"Файл добавлен: {newFile.Name}");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка добавления файла: {ex.Message}");
            }
        }

        private void AddFileToTree(string filePath)
        {
            try
            {
                var fileName = Path.GetFileName(filePath);
                if (!_appFiles.Any(f => f.Name == fileName))
                {
                    var newFile = new AppFile
                    {
                        Name = fileName,
                        Content = File.ReadAllText(filePath),
                        FilePath = filePath
                    };
                    _appFiles.Add(newFile);
                    UpdateFilesTree();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка добавления файла в дерево: {ex.Message}");
            }
        }

        private void UpdateFilesTree()
        {
            try
            {
                filesTreeView.Items.Clear();

                var rootNode = new FileNode
                {
                    Name = "Файлы проекта",
                    IsDirectory = true
                };

                foreach (var file in _appFiles)
                {
                    rootNode.Children.Add(new FileNode
                    {
                        Name = file.Name,
                        Path = file.FilePath,
                        IsDirectory = false
                    });
                }

                filesTreeView.Items.Add(rootNode);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка обновления дерева файлов: {ex.Message}");
            }
        }

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

        private void LoadFileContent(string fileName)
        {
            try
            {
                var file = _appFiles.FirstOrDefault(f => f.Name == fileName);
                if (file != null)
                {
                    fileContentTextBox.Text = file.Content;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка загрузки содержимого файла: {ex.Message}");
            }
        }

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

        private void OnDeleteFileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedNode = filesTreeView.SelectedItem as FileNode;
                if (selectedNode == null || selectedNode.IsDirectory) return;

                DeleteFileWithConfirmation(selectedNode.Name);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при удалении файла: {ex.Message}");
            }
        }

        private void DeleteFileWithConfirmation(string fileName)
        {
            try
            {
                var result = MessageBox.Show($"Удалить файл {fileName}?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _appFiles.RemoveAll(f => f.Name == fileName);
                    UpdateFilesTree();
                    fileContentTextBox.Text = string.Empty;
                    ShowInformationMessage($"Файл {fileName} удален");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка удаления файла: {ex.Message}");
            }
        }

        private void SaveArrayToFile(string filePath)
        {
            try
            {
                var content = string.Join(Environment.NewLine, _array.Select((item, index) =>
                    $"{index}: {item ?? "null"}"));
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сохранения массива: {ex.Message}", ex);
            }
        }

        private void LoadArrayFromFile(string filePath)
        {
            try
            {
                var content = File.ReadAllText(filePath);
                // Здесь должна быть логика парсинга файла и загрузки в массив
                ShowInformationMessage("Функция загрузки массива из файла в разработке");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки массива: {ex.Message}", ex);
            }
        }

        #endregion

        #region Вспомогательные методы

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

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
    /// Класс файла приложения
    /// </summary>
    public class AppFile
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public string FilePath { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}