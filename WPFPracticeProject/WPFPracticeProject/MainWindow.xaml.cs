using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFPracticeProject
{
    /// <summary>
    /// Главное окно приложения "Мастер работы с массивами"
    /// 
    /// Функциональные возможности:
    /// - Выбор типа данных (int, float, DateTime)
    /// - Динамическое создание массива с возможностью ввода данных
    /// - Сортировка массива тремя алгоритмами (пузырьковая, выбором, быстрая)
    /// - Работа с файлами: сохранение, загрузка, управление через дерево файлов
    /// - Контекстные подсказки и помощь пользователю
    /// </summary>
    public partial class MainWindow : Window
    {
        // Поля класса для хранения состояния приложения
        private object[] _array;                    // Текущий рабочий массив
        private bool _isFirstElementAdded = false;  // Флаг первого добавленного элемента
        private readonly List<AppFile> _appFiles = new List<AppFile>(); // Коллекция файлов приложения
        private Type _currentDataType = typeof(int); // Текущий выбранный тип данных

        /// <summary>
        /// Конструктор главного окна приложения
        /// Инициализирует компоненты, настраивает обработчики событий и начальное состояние
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            SubscribeToEvents();
            InitializeApplication();
        }

        /// <summary>
        /// Инициализация начального состояния приложения
        /// Устанавливает значения по умолчанию для элементов управления
        /// </summary>
        private void InitializeApplication()
        {
            arraySizeSlider.Value = 5;                    // Начальный размер массива
            intRadioButton.IsChecked = true;              // Тип данных по умолчанию - целочисленный
            bubbleSortRadio.IsChecked = true;             // Алгоритм сортировки по умолчанию - пузырьковая
            UpdateHelpText("Выберите тип данных для работы с массивом");
        }

        /// <summary>
        /// Подписка на события элементов управления
        /// Настраивает реакцию приложения на действия пользователя
        /// </summary>
        private void SubscribeToEvents()
        {
            // Подписка на события выбора типа данных
            intRadioButton.Checked += OnIntRadioButtonChecked;
            floatRadioButton.Checked += OnFloatRadioButtonChecked;
            dateRadioButton.Checked += OnDateRadioButtonChecked;

            // Подписка на изменение размера массива
            arraySizeSlider.ValueChanged += OnArraySizeSliderValueChanged;

            // Обновление текстового отображения размера массива при изменении слайдера
            arraySizeSlider.ValueChanged += (s, e) =>
            {
                arraySizeText.Text = $"Размер: {(int)arraySizeSlider.Value}";
            };
        }

        // =====================================================================
        // ОБРАБОТКА ИНТЕРФЕЙСА И ПОДСКАЗОК
        // =====================================================================

        /// <summary>
        /// Обработчик изменения активной вкладки
        /// Обновляет заголовок панели инструментов и контекстные подсказки
        /// </summary>
        private void OnTabChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tab && tab.SelectedItem is TabItem item)
            {
                toolbarTitle.Text = item.Header.ToString();
                UpdateHelpTextBasedOnTab(item.Header.ToString());
            }
        }

        /// <summary>
        /// Обновление текста подсказки в зависимости от активной вкладки
        /// </summary>
        /// <param name="tabName">Название активной вкладки</param>
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
        /// Показывает всплывающую подсказку при нажатии на кнопку помощи
        /// </summary>
        private void OnHelpClick(object sender, RoutedEventArgs e)
        {
            helpPopup.IsOpen = true;
        }

        /// <summary>
        /// Обновляет текст всплывающей подсказки
        /// </summary>
        /// <param name="text">Новый текст подсказки</param>
        private void UpdateHelpText(string text)
        {
            helpText.Text = text;
        }

        // =====================================================================
        // ОБРАБОТКА МЕНЮ И КОМАНД
        // =====================================================================

        /// <summary>
        /// Сохранение текущего массива в файл
        /// Открывает диалоговое окно выбора файла и записывает данные
        /// </summary>
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
        /// Заглушка для функции загрузки массива из файла
        /// В данной версии не реализована
        /// </summary>
        private void OnMenuLoadClick(object sender, RoutedEventArgs e)
        {
            ShowInformationMessage("Загрузка из файла — заглушка. Реализуется позже.");
        }

        /// <summary>
        /// Закрытие приложения
        /// </summary>
        private void OnExitClick(object sender, RoutedEventArgs e) => Close();

        /// <summary>
        /// Показывает информацию о программе
        /// </summary>
        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Мастер работы с массивами\nЛабораторная работа №1\nv1.0", "О программе");
        }

        // =====================================================================
        // ВЫБОР ТИПА ДАННЫХ
        // =====================================================================

        /// <summary>
        /// Обработчик выбора целочисленного типа данных
        /// Устанавливает тип int и обновляет интерфейс ввода
        /// </summary>
        private void OnIntRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _currentDataType = typeof(int);
            UpdateHelpText("Выбран тип: int");
            UpdateInputControlsVisibility();
        }

        /// <summary>
        /// Обработчик выбора дробного типа данных
        /// Устанавливает тип float и обновляет интерфейс ввода
        /// </summary>
        private void OnFloatRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _currentDataType = typeof(float);
            UpdateHelpText("Выбран тип: float");
            UpdateInputControlsVisibility();
        }

        /// <summary>
        /// Обработчик выбора типа данных "Дата"
        /// Устанавливает тип DateTime и переключает на DatePicker
        /// </summary>
        private void OnDateRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _currentDataType = typeof(DateTime);
            UpdateHelpText("Выбран тип: DateTime");
            UpdateInputControlsVisibility();
        }

        // =====================================================================
        // РАБОТА С МАССИВОМ
        // =====================================================================

        /// <summary>
        /// Обработчик изменения размера массива через слайдер
        /// Создает новый массив указанного размера и обновляет интерфейс ввода
        /// </summary>
        private void OnArraySizeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int size = (int)arraySizeSlider.Value;
            _array = new object[size];
            arrayInputItems.Items.Clear();

            // Создание элементов управления для ввода каждого элемента массива
            for (int i = 0; i < size; i++)
            {
                arrayInputItems.Items.Add(new ArrayElementModel { Index = i });
            }

            UpdateInputControlsVisibility();
            UpdateArrayDisplay();
        }

        /// <summary>
        /// Обработчик изменения текста в TextBox для элементов массива
        /// Парсит введенное значение в соответствии с выбранным типом данных
        /// </summary>
        private void OnArrayElementTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var stackPanel = (StackPanel)textBox.Parent;
            var indexText = (TextBlock)stackPanel.Children[1];
            int index = int.Parse(indexText.Text);

            HandleArrayElementInput(index, textBox.Text);
        }

        /// <summary>
        /// Обработчик выбора даты в DatePicker
        /// Устанавливает значение выбранной даты в соответствующий элемент массива
        /// </summary>
        private void OnDatePickerSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var datePicker = (DatePicker)sender;
            var stackPanel = (StackPanel)datePicker.Parent;
            var indexText = (TextBlock)stackPanel.Children[1];
            int index = int.Parse(indexText.Text);

            _array[index] = datePicker.SelectedDate;
            CheckFirstElementAddition();
            CheckArrayCompletion();
            UpdateArrayDisplay();
        }

        /// <summary>
        /// Обрабатывает ввод данных для элемента массива
        /// Выполняет парсинг строки в соответствии с текущим типом данных
        /// </summary>
        /// <param name="index">Индекс элемента массива</param>
        /// <param name="text">Введенный текст</param>
        private void HandleArrayElementInput(int index, string text)
        {
            string trimmedText = text.Trim();

            if (string.IsNullOrEmpty(trimmedText))
            {
                _array[index] = null;
            }
            else if (_currentDataType == typeof(int) && int.TryParse(trimmedText, out int intValue))
            {
                _array[index] = intValue;
            }
            else if (_currentDataType == typeof(float) && float.TryParse(trimmedText, out float floatValue))
            {
                _array[index] = floatValue;
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
        /// Обновляет видимость элементов управления ввода в зависимости от типа данных
        /// Для DateTime показывает DatePicker, для остальных типов - TextBox
        /// </summary>
        private void UpdateInputControlsVisibility()
        {
            foreach (var item in arrayInputItems.Items)
            {
                if (arrayInputItems.ItemContainerGenerator.ContainerFromItem(item) is ContentPresenter container)
                {
                    var stackPanel = VisualTreeHelper.GetChild(container, 0) as StackPanel;
                    if (stackPanel != null)
                    {
                        var textInput = stackPanel.FindName("textInput") as TextBox;
                        var dateInput = stackPanel.FindName("dateInput") as DatePicker;

                        if (textInput != null && dateInput != null)
                        {
                            if (_currentDataType == typeof(DateTime))
                            {
                                textInput.Visibility = Visibility.Collapsed;
                                dateInput.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                textInput.Visibility = Visibility.Visible;
                                dateInput.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Проверяет, был ли добавлен первый элемент в массив
        /// Используется для логики активации дополнительных функций
        /// </summary>
        private void CheckFirstElementAddition()
        {
            if (!_isFirstElementAdded && _array.Any(x => x != null))
                _isFirstElementAdded = true;
        }

        /// <summary>
        /// Обновляет текстовое отображение текущего состояния массива
        /// Показывает все элементы массива с их индексами и значениями
        /// </summary>
        private void UpdateArrayDisplay()
        {
            arrayDisplay.Text = string.Join(Environment.NewLine,
                _array.Select((item, i) => $"array[{i}] = {item ?? "null"}"));
        }

        /// <summary>
        /// Проверяет, полностью ли заполнен массив
        /// Активирует вкладку сортировки, если все элементы заполнены
        /// </summary>
        private void CheckArrayCompletion()
        {
            sortTab.IsEnabled = _array.All(item => item != null);
        }

        // =====================================================================
        // АЛГОРИТМЫ СОРТИРОВКИ
        // =====================================================================

        /// <summary>
        /// Выполняет сортировку массива выбранным алгоритмом
        /// Создает копию исходного массива и применяет выбранный метод сортировки
        /// </summary>
        private void OnSortButtonClick(object sender, RoutedEventArgs e)
        {
            if (_array == null || _array.Length == 0) return;

            // Создание копии массива для сортировки
            object[] result = (object[])_array.Clone();

            // Применение выбранного алгоритма сортировки
            if (bubbleSortRadio.IsChecked == true)
                BubbleSort(result);
            else if (selectionSortRadio.IsChecked == true)
                SelectionSort(result);
            else if (quickSortRadio.IsChecked == true)
                QuickSort(result, 0, result.Length - 1);

            // Отображение результата сортировки
            sortedArrayDisplay.Text = string.Join(Environment.NewLine,
                result.Select((item, i) => $"sorted[{i}] = {item}"));
        }

        /// <summary>
        /// Алгоритм пузырьковой сортировки
        /// Попарно сравнивает и обменивает соседние элементы
        /// Сложность: O(n²)
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
        /// Алгоритм сортировки выбором
        /// Находит минимальный элемент и помещает его в начало неотсортированной части
        /// Сложность: O(n²)
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
        /// Алгоритм быстрой сортировки (QuickSort)
        /// Рекурсивно разделяет массив на части относительно опорного элемента
        /// Сложность: O(n log n) в среднем случае
        /// </summary>
        /// <param name="arr">Массив для сортировки</param>
        /// <param name="left">Левая граница диапазона</param>
        /// <param name="right">Правая граница диапазона</param>
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
        /// Вспомогательный метод для быстрой сортировки
        /// Разделяет массив на две части относительно опорного элемента
        /// </summary>
        /// <returns>Позиция опорного элемента после разделения</returns>
        private int Partition(object[] arr, int left, int right)
        {
            object p = arr[right];  // Опорный элемент
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
        /// Сравнивает два объекта с учетом null-значений
        /// Использует интерфейс IComparable для сравнения
        /// </summary>
        /// <returns>
        /// -1 если a < b, 0 если a == b, 1 если a > b
        /// Null всегда считается меньше любого значения
        /// </returns>
        private int Compare(object a, object b)
        {
            if (a == null) return b == null ? 0 : -1;
            if (b == null) return 1;
            return ((IComparable)a).CompareTo(b);
        }

        /// <summary>
        /// Обменивает местами два элемента массива
        /// Использует современный синтаксис кортежей для обмена
        /// </summary>
        private void Swap(object[] arr, int i, int j)
        {
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }

        // =====================================================================
        // РАБОТА С ФАЙЛАМИ
        // =====================================================================

        /// <summary>
        /// Добавление нового файла в проект
        /// Открывает диалоговое окно выбора места сохранения
        /// </summary>
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
        /// Обновление дерева файлов
        /// Перестраивает отображение файловой структуры
        /// </summary>
        private void OnRefreshTreeButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateFilesTree();
        }

        /// <summary>
        /// Загрузка содержимого выбранного файла в текстовое поле
        /// </summary>
        private void OnLoadFileContentClick(object sender, RoutedEventArgs e)
        {
            if (filesTreeView.SelectedItem is FileNode node && !node.IsDirectory)
            {
                var file = _appFiles.FirstOrDefault(f => f.Name == node.Name);
                fileContentTextBox.Text = file?.Content ?? "Файл не найден";
            }
        }

        /// <summary>
        /// Удаление выбранного файла из проекта
        /// Запрашивает подтверждение перед удалением
        /// </summary>
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
        /// Обновление отображения дерева файлов
        /// Создает корневой узел и добавляет все файлы проекта
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
        /// Сохранение текущего массива в указанный файл
        /// Форматирует данные в виде "индекс: значение"
        /// </summary>
        /// <param name="path">Путь к файлу для сохранения</param>
        private void SaveArrayToFile(string path)
        {
            string content = string.Join(Environment.NewLine, _array.Select((x, i) => $"{i}: {x}"));
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Добавление файла в дерево файлов проекта
        /// Проверяет, не добавлен ли файл ранее
        /// </summary>
        /// <param name="filePath">Путь к добавляемому файлу</param>
        private void AddFileToTree(string filePath)
        {
            string name = Path.GetFileName(filePath);
            if (!_appFiles.Any(f => f.Name == name))
            {
                _appFiles.Add(new AppFile { Name = name, Content = "Новое содержимое", FilePath = filePath });
                UpdateFilesTree();
            }
        }

        // =====================================================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // =====================================================================

        /// <summary>
        /// Показывает сообщение об ошибке
        /// Использует стандартный диалог Windows с иконкой ошибки
        /// </summary>
        /// <param name="msg">Текст сообщения об ошибке</param>
        private void ShowErrorMessage(string msg) =>
            MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

        /// <summary>
        /// Показывает информационное сообщение
        /// Использует стандартный диалог Windows с иконкой информации
        /// </summary>
        /// <param name="msg">Текст информационного сообщения</param>
        private void ShowInformationMessage(string msg) =>
            MessageBox.Show(msg, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // =====================================================================
    // МОДЕЛИ ДАННЫХ
    // =====================================================================

    /// <summary>
    /// Модель элемента массива для отображения в интерфейсе
    /// Содержит индекс элемента для привязки данных в XAML
    /// </summary>
    public class ArrayElementModel
    {
        /// <summary>
        /// Индекс элемента в массиве (от 0 до n-1)
        /// </summary>
        public int Index { get; set; }
    }

    /// <summary>
    /// Модель узла дерева файлов
    /// Представляет файл или папку в древовидной структуре
    /// </summary>
    public class FileNode
    {
        /// <summary>
        /// Имя файла или папки
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Полный путь к файлу в файловой системе
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Флаг, указывающий является ли узел директорией
        /// true - папка, false - файл
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// Коллекция дочерних узлов (для папок)
        /// </summary>
        public List<FileNode> Children { get; set; } = new List<FileNode>();
    }

    /// <summary>
    /// Модель файла приложения
    /// Хранит метаинформацию и содержимое файла
    /// </summary>
    public class AppFile
    {
        /// <summary>
        /// Имя файла с расширением
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Текстовое содержимое файла
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Абсолютный путь к файлу в файловой системе
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Дата и время создания файла в приложении
        /// Устанавливается автоматически при создании
        /// </summary>
        public DateTime Created { get; set; } = DateTime.Now;
    }
}