using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WPFPracticeProject
{
    /// <summary>
    /// Главное окно приложения "Мастер работы с массивами".
    /// Реализует функциональность: выбор типа данных, ввод массива, сортировку и управление файлами.
    /// </summary>
    public partial class MainWindow : Window
    {
        private object[] _array;
        private bool _isFirstElementAdded = false;
        private readonly List<AppFile> _appFiles = new List<AppFile>();
        private Type _currentDataType = typeof(int);

        /// <summary>
        /// Конструктор главного окна.
        /// Инициализирует компоненты, подписывается на события и настраивает начальное состояние.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            SubscribeToEvents();
            InitializeApplication();
        }

        /// <summary>
        /// Инициализирует начальные значения элементов управления.
        /// Устанавливает размер массива по умолчанию и выбирает целочисленный тип.
        /// </summary>
        private void InitializeApplication()
        {
            arraySizeSlider.Value = 5;
            intRadioButton.IsChecked = true;
            UpdateHelpText("Выберите тип данных для работы с массивом");
        }

        /// <summary>
        /// Подписывает обработчики событий на элементы интерфейса.
        /// Обеспечивает реакцию на изменения выбора типа данных, размера массива и других действий.
        /// </summary>
        private void SubscribeToEvents()
        {
            intRadioButton.Checked += OnIntRadioButtonChecked;
            floatRadioButton.Checked += OnFloatRadioButtonChecked;
            dateRadioButton.Checked += OnDateRadioButtonChecked;
            arraySizeSlider.ValueChanged += OnArraySizeSliderValueChanged;

            // Обновление текстового отображения размера массива
            arraySizeSlider.ValueChanged += (s, e) =>
            {
                arraySizeText.Text = $"Размер: {(int)arraySizeSlider.Value}";
            };
        }

        // =============================================================================================================
        // === ОБРАБОТЧИКИ ПЕРЕКЛЮЧЕНИЯ ВКЛАДОК И ПОДСКАЗКИ ========================================================
        // =============================================================================================================

        /// <summary>
        /// Обработчик изменения выбранной вкладки.
        /// Обновляет заголовок панели инструментов и текст подсказки.
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Аргументы события изменения выбора</param>
        private void OnTabChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tab && tab.SelectedItem is TabItem item)
            {
                toolbarTitle.Text = item.Header.ToString();
                UpdateHelpTextBasedOnTab(item.Header.ToString());
            }
        }

        /// <summary>
        /// Обновляет текст подсказки в зависимости от активной вкладки.
        /// </summary>
        /// <param name="tabName">Название текущей вкладки</param>
        private void UpdateHelpTextBasedOnTab(string tabName)
        {
            switch (tabName)
            {
                case "Тип данных":
                    UpdateHelpText("Выберите тип данных для работы с массивом");
                    break;
                case "Массив":
                    UpdateHelpText($"Задайте размер и заполните массив ({_currentDataType.Name})");
                    break;
                case "Сортировки":
                    UpdateHelpText("Выберите алгоритм и нажмите 'Выполнить'");
                    break;
                case "Файлы":
                    UpdateHelpText("Управление файлами через контекстное меню");
                    break;
            }
        }

        /// <summary>
        /// Показывает всплывающую подсказку при нажатии кнопки помощи.
        /// </summary>
        /// <param name="sender">Кнопка помощи</param>
        /// <param name="e">Событие клика</param>
        private void OnHelpClick(object sender, RoutedEventArgs e)
        {
            helpPopup.IsOpen = true;
        }

        /// <summary>
        /// Обновляет текст подсказки.
        /// </summary>
        /// <param name="text">Новый текст подсказки</param>
        private void UpdateHelpText(string text)
        {
            helpText.Text = text;
        }

        // =============================================================================================================
        // === МЕНЮ И СТАНДАРТНЫЕ КОМАНДЫ ============================================================================
        // =============================================================================================================

        /// <summary>
        /// Обработчик команды "Сохранить" из меню.
        /// Открывает диалог сохранения и записывает массив в файл.
        /// </summary>
        /// <param name="sender">Элемент меню</param>
        /// <param name="e">Событие клика</param>
        private void OnMenuSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    Title = "Сохранить массив"
                };

                if (dialog.ShowDialog() == true)
                {
                    SaveArrayToFile(dialog.FileName);
                    AddFileToTree(dialog.FileName);
                    ShowInformationMessage($"Файл сохранён: {Path.GetFileName(dialog.FileName)}");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка сохранения: {ex.Message}");
            }
        }

        /// <summary>
        /// Обработчик команды "Загрузить" из меню.
        /// Заглушка — в данной версии не реализована.
        /// </summary>
        /// <param name="sender">Элемент меню</param>
        /// <param name="e">Событие клика</param>
        private void OnMenuLoadClick(object sender, RoutedEventArgs e)
        {
            ShowInformationMessage("Загрузка из файла — заглушка. Реализуется позже.");
        }

        /// <summary>
        /// Закрывает приложение.
        /// </summary>
        /// <param name="sender">Пункт меню "Выход"</param>
        /// <param name="e">Событие клика</param>
        private void OnExitClick(object sender, RoutedEventArgs e) => Close();

        /// <summary>
        /// Показывает информацию о программе.
        /// </summary>
        /// <param name="sender">Пункт меню "О программе"</param>
        /// <param name="e">Событие клика</param>
        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Мастер работы с массивами\nЛабораторная работа №1\nv1.0", "О программе");
        }

        // =============================================================================================================
        // === ВЫБОР ТИПА ДАННЫХ ======================================================================================
        // =============================================================================================================

        /// <summary>
        /// Обработчик выбора целочисленного типа данных (int).
        /// Обновляет текущий тип и подсказку.
        /// </summary>
        /// <param name="sender">RadioButton для int</param>
        /// <param name="e">Событие выбора</param>
        private void OnIntRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _currentDataType = typeof(int);
            UpdateHelpText("Выбран тип: int");
        }

        /// <summary>
        /// Обработчик выбора дробного типа данных (float).
        /// Обновляет текущий тип и подсказку.
        /// </summary>
        /// <param name="sender">RadioButton для float</param>
        /// <param name="e">Событие выбора</param>
        private void OnFloatRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _currentDataType = typeof(float);
            UpdateHelpText("Выбран тип: float");
        }

        /// <summary>
        /// Обработчик выбора типа данных "Дата" (DateTime).
        /// Обновляет текущий тип и подсказку.
        /// </summary>
        /// <param name="sender">RadioButton для DateTime</param>
        /// <param name="e">Событие выбора</param>
        private void OnDateRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _currentDataType = typeof(DateTime);
            UpdateHelpText("Выбран тип: DateTime");
        }

        // =============================================================================================================
        // === РАБОТА С МАССИВОМ ======================================================================================
        // =============================================================================================================

        /// <summary>
        /// Обработчик изменения размера массива через слайдер.
        /// Создаёт новый массив и обновляет интерфейс ввода.
        /// </summary>
        /// <param name="sender">Слайдер размера массива</param>
        /// <param name="e">Событие изменения значения</param>
        private void OnArraySizeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
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

        /// <summary>
        /// Обработчик ввода значения в поле элемента массива.
        /// Парсит значение в соответствии с выбранным типом данных.
        /// </summary>
        /// <param name="sender">TextBox поля ввода</param>
        /// <param name="e">Событие изменения текста</param>
        private void OnArrayElementTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var stackPanel = (StackPanel)textBox.Parent;
            var indexText = (TextBlock)stackPanel.Children[1];
            int index = int.Parse(indexText.Text);

            string text = textBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                _array[index] = null;
            }
            else if (_currentDataType == typeof(int) && int.TryParse(text, out int intValue))
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
                _array[index] = null;
            }

            CheckFirstElementAddition();
            CheckArrayCompletion();
            UpdateArrayDisplay();
        }

        /// <summary>
        /// Проверяет, был ли добавлен первый элемент массива.
        /// Используется для логики перехода между этапами.
        /// </summary>
        private void CheckFirstElementAddition()
        {
            if (!_isFirstElementAdded && _array.Any(x => x != null))
                _isFirstElementAdded = true;
        }

        /// <summary>
        /// Обновляет текстовое отображение текущего состояния массива.
        /// </summary>
        private void UpdateArrayDisplay()
        {
            arrayDisplay.Text = string.Join(Environment.NewLine,
                _array.Select((item, i) => $"array[{i}] = {item ?? "null"}"));
        }

        /// <summary>
        /// Проверяет, заполнен ли массив полностью.
        /// Если да — активирует вкладку сортировки.
        /// </summary>
        private void CheckArrayCompletion()
        {
            sortTab.IsEnabled = _array.All(item => item != null);
        }

        // =============================================================================================================
        // === АЛГОРИТМЫ СОРТИРОВКИ ===================================================================================
        // =============================================================================================================

        /// <summary>
        /// Обработчик кнопки "Выполнить сортировку".
        /// Клонирует массив, применяет выбранный алгоритм и отображает результат.
        /// </summary>
        /// <param name="sender">Кнопка сортировки</param>
        /// <param name="e">Событие клика</param>
        private void OnSortButtonClick(object sender, RoutedEventArgs e)
        {
            if (_array == null || _array.Length == 0) return;

            object[] result = (object[])_array.Clone();
            if (bubbleSortRadio.IsChecked == true)
                BubbleSort(result);
            else if (selectionSortRadio.IsChecked == true)
                SelectionSort(result);
            else if (quickSortRadio.IsChecked == true)
                QuickSort(result, 0, result.Length - 1);

            sortedArrayDisplay.Text = string.Join(Environment.NewLine,
                result.Select((item, i) => $"sorted[{i}] = {item}"));
        }

        /// <summary>
        /// Выполняет пузырьковую сортировку массива.
        /// </summary>
        /// <param name="arr">Массив для сортировки</param>
        private void BubbleSort(object[] arr)
        {
            for (int i = 0; i < arr.Length - 1; i++)
                for (int j = 0; j < arr.Length - 1 - i; j++)
                    if (Compare(arr[j], arr[j + 1]) > 0)
                        Swap(arr, j, j + 1);
        }

        /// <summary>
        /// Выполняет сортировку выбором.
        /// </summary>
        /// <param name="arr">Массив для сортировки</param>
        private void SelectionSort(object[] arr)
        {
            for (int i = 0; i < arr.Length - 1; i++)
            {
                int min = i;
                for (int j = i + 1; j < arr.Length; j++)
                    if (Compare(arr[j], arr[min]) < 0)
                        min = j;
                Swap(arr, i, min);
            }
        }

        /// <summary>
        /// Выполняет быструю сортировку (QuickSort) рекурсивно.
        /// </summary>
        /// <param name="arr">Массив</param>
        /// <param name="left">Левая граница</param>
        /// <param name="right">Правая граница</param>
        private void QuickSort(object[] arr, int left, int right)
        {
            if (left < right)
            {
                int pivot = Partition(arr, left, right);
                QuickSort(arr, left, pivot - 1);
                QuickSort(arr, pivot + 1, right);
            }
        }

        /// <summary>
        /// Разделяет массив на части относительно опорного элемента.
        /// </summary>
        /// <param name="arr">Массив</param>
        /// <param name="left">Левая граница</param>
        /// <param name="right">Правая граница</param>
        /// <returns>Индекс опорного элемента</returns>
        private int Partition(object[] arr, int left, int right)
        {
            object p = arr[right];
            int i = left - 1;
            for (int j = left; j < right; j++)
            {
                if (Compare(arr[j], p) <= 0)
                {
                    i++;
                    Swap(arr, i, j);
                }
            }
            Swap(arr, i + 1, right);
            return i + 1;
        }

        /// <summary>
        /// Сравнивает два объекта, реализующих IComparable.
        /// Учитывает null-значения.
        /// </summary>
        /// <param name="a">Первый объект</param>
        /// <param name="b">Второй объект</param>
        /// <returns>-1, 0 или 1</returns>
        private int Compare(object a, object b)
        {
            if (a == null) return b == null ? 0 : -1;
            if (b == null) return 1;
            return ((IComparable)a).CompareTo(b);
        }

        /// <summary>
        /// Меняет местами два элемента массива.
        /// </summary>
        /// <param name="arr">Массив</param>
        /// <param name="i">Индекс первого элемента</param>
        /// <param name="j">Индекс второго элемента</param>
        private void Swap(object[] arr, int i, int j)
        {
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }

        // =============================================================================================================
        // === РАБОТА С ФАЙЛАМИ =======================================================================================
        // =============================================================================================================

        /// <summary>
        /// Обработчик кнопки "Добавить файл".
        /// Открывает диалог создания нового файла и добавляет его в список.
        /// </summary>
        /// <param name="sender">Кнопка добавления</param>
        /// <param name="e">Событие клика</param>
        private void OnAddFileButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                var file = new AppFile
                {
                    Name = Path.GetFileName(dialog.FileName),
                    Content = "Пример содержимого...",
                    FilePath = dialog.FileName
                };
                _appFiles.Add(file);
                UpdateFilesTree();
            }
        }

        /// <summary>
        /// Обработчик кнопки "Обновить дерево".
        /// Перестраивает дерево файлов на основе текущего списка.
        /// </summary>
        /// <param name="sender">Кнопка обновления</param>
        /// <param name="e">Событие клика</param>
        private void OnRefreshTreeButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateFilesTree();
        }

        /// <summary>
        /// Обработчик контекстного меню: загрузить содержимое файла.
        /// Отображает содержимое выбранного файла в текстовом поле.
        /// </summary>
        /// <param name="sender">Элемент меню</param>
        /// <param name="e">Событие клика</param>
        private void OnLoadFileContentClick(object sender, RoutedEventArgs e)
        {
            if (filesTreeView.SelectedItem is FileNode node && !node.IsDirectory)
            {
                var file = _appFiles.FirstOrDefault(f => f.Name == node.Name);
                fileContentTextBox.Text = file?.Content ?? "Файл не найден";
            }
        }

        /// <summary>
        /// Обработчик удаления файла через контекстное меню.
        /// Запрашивает подтверждение и удаляет файл из списка.
        /// </summary>
        /// <param name="sender">Элемент меню</param>
        /// <param name="e">Событие клика</param>
        private void OnDeleteFileClick(object sender, RoutedEventArgs e)
        {
            if (filesTreeView.SelectedItem is FileNode node && !node.IsDirectory)
            {
                if (MessageBox.Show($"Удалить файл {node.Name}?", "Подтвердите", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _appFiles.RemoveAll(f => f.Name == node.Name);
                    UpdateFilesTree();
                    fileContentTextBox.Text = "";
                }
            }
        }

        /// <summary>
        /// Обновляет дерево файлов в TreeView.
        /// Создаёт корневой узел и добавляет все файлы.
        /// </summary>
        private void UpdateFilesTree()
        {
            filesTreeView.Items.Clear();
            var root = new FileNode { Name = "Проект", IsDirectory = true };
            foreach (var f in _appFiles)
            {
                root.Children.Add(new FileNode { Name = f.Name, Path = f.FilePath, IsDirectory = false });
            }
            filesTreeView.Items.Add(root);
        }

        /// <summary>
        /// Сохраняет текущий массив в указанный файл.
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        private void SaveArrayToFile(string path)
        {
            string content = string.Join(Environment.NewLine, _array.Select((x, i) => $"{i}: {x}"));
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Добавляет файл в дерево, если он ещё не добавлен.
        /// </summary>
        /// <param name="filePath">Путь к файлу</param>
        private void AddFileToTree(string filePath)
        {
            string name = Path.GetFileName(filePath);
            if (!_appFiles.Any(f => f.Name == name))
            {
                _appFiles.Add(new AppFile { Name = name, Content = "Новое содержимое", FilePath = filePath });
                UpdateFilesTree();
            }
        }

        // =============================================================================================================
        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =================================================================================
        // =============================================================================================================

        /// <summary>
        /// Показывает сообщение об ошибке.
        /// </summary>
        /// <param name="msg">Текст ошибки</param>
        private void ShowErrorMessage(string msg) =>
            MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

        /// <summary>
        /// Показывает информационное сообщение.
        /// </summary>
        /// <param name="msg">Текст сообщения</param>
        private void ShowInformationMessage(string msg) =>
            MessageBox.Show(msg, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // =============================================================================================================
    // === МОДЕЛИ ДАННЫХ ===========================================================================================
    // =============================================================================================================

    /// <summary>
    /// Представляет узел в древовидной структуре файлов.
    /// Может быть директорией или файлом.
    /// </summary>
    public class FileNode
    {
        /// <summary>
        /// Название узла (файла или папки).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Путь к файлу (если это файл).
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Признак того, что узел является директорией.
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// Дочерние узлы (для директорий).
        /// </summary>
        public List<FileNode> Children { get; set; } = new List<FileNode>();
    }

    /// <summary>
    /// Представляет файл внутри приложения.
    /// Хранит имя, содержимое и метаданные.
    /// </summary>
    public class AppFile
    {
        /// <summary>
        /// Имя файла.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Содержимое файла.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Полный путь к файлу на диске.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Дата создания файла.
        /// </summary>
        public DateTime Created { get; set; } = DateTime.Now;
    }
}