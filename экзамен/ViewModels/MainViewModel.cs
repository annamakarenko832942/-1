using System;
using System.ComponentModel;
using System.Windows;
using BisectionApp.Models;
using BisectionApp.Database;
using System.Collections.ObjectModel;
using System.Linq;

namespace BisectionApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private double _leftBound;
        private double _rightBound;
        private double _accuracy = 0.0001;
        private CalculationResult? _lastResult;  // ✅ Добавь ? (nullable)
        private ObservableCollection<CalculationResult> _history;  // ✅ Инициализируй сразу
        private string _statusMessage;

        public MainViewModel()
        {
            _history = new ObservableCollection<CalculationResult>();  // ✅ Инициализация
            _statusMessage = string.Empty;  // ✅ Инициализация
            _lastResult = null;  // ✅ Явно указываем null

            History = _history;
            CalculateCommand = new RelayCommand(CalculateRoot, CanCalculate);
            LoadHistoryCommand = new RelayCommand(LoadHistory);
            ClearHistoryCommand = new RelayCommand(ClearHistory);

            CheckDatabaseConnection();
        }

        // ==================== СВОЙСТВА ====================

        public double LeftBound
        {
            get => _leftBound;
            set { _leftBound = value; OnPropertyChanged(); }
        }

        public double RightBound
        {
            get => _rightBound;
            set { _rightBound = value; OnPropertyChanged(); }
        }

        public double Accuracy
        {
            get => _accuracy;
            set { _accuracy = value; OnPropertyChanged(); }
        }

        public CalculationResult? LastResult  // ✅ Добавь ?
        {
            get => _lastResult;
            set { _lastResult = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CalculationResult> History
        {
            get => _history;
            set { _history = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // ==================== КОМАНДЫ ====================

        public RelayCommand CalculateCommand { get; }
        public RelayCommand LoadHistoryCommand { get; }
        public RelayCommand ClearHistoryCommand { get; }

        // ==================== ПРОВЕРКА ПОДКЛЮЧЕНИЯ ====================

        private void CheckDatabaseConnection()
        {
            if (DatabaseHelper.CheckConnection())
            {
                DatabaseHelper.CreateTable();
                StatusMessage = "✓ Подключение к PostgreSQL успешно";
            }
            else
            {
                StatusMessage = "✗ Ошибка подключения к PostgreSQL. Проверьте настройки.";
            }
        }

        // ==================== МЕТОД ПОЛОВИННОГО ДЕЛЕНИЯ ====================

        private void CalculateRoot(object parameter)
        {
            try
            {
                double a = LeftBound;
                double b = RightBound;
                double eps = Accuracy;
                int iterations = 0;

                if (Function(a) * Function(b) > 0)
                {
                    MessageBox.Show(
                        "На указанном интервале корня нет! f(a) и f(b) должны иметь разные знаки.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                while ((b - a) / 2 > eps)
                {
                    double c = (a + b) / 2;
                    iterations++;

                    if (Math.Abs(Function(c)) < 1e-15)
                        break;

                    if (Function(a) * Function(c) < 0)
                        b = c;
                    else
                        a = c;
                }

                double root = (a + b) / 2;

                LastResult = new CalculationResult
                {
                    LeftBound = LeftBound,
                    RightBound = RightBound,
                    Root = root,
                    FunctionValue = Function(root),
                    Iterations = iterations,
                    Accuracy = (b - a) / 2,
                    CalculationDate = DateTime.UtcNow
                };

                DatabaseHelper.SaveResult(LastResult);
                History.Insert(0, LastResult);

                StatusMessage = $"✓ Корень найден: {root:F6} за {iterations} итераций";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка вычисления: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanCalculate(object parameter)
        {
            return LeftBound < RightBound;
        }

        private double Function(double x)
        {
            return Math.Pow(x, 3) - x - 2;
        }

        // ==================== ЗАГРУЗКА ИСТОРИИ ====================

        private void LoadHistory(object parameter)
        {
            try
            {
                var results = DatabaseHelper.LoadAllResults();

                History.Clear();
                foreach (var result in results)
                    History.Add(result);

                StatusMessage = $"✓ Загружено {results.Count} записей из PostgreSQL";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== ОЧИСТКА ИСТОРИИ ====================

        private void ClearHistory(object parameter)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите удалить все записи из базы данных?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    DatabaseHelper.ClearAllResults();
                    History.Clear();
                    StatusMessage = "✓ История очищена";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка очистки: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ==================== INotifyPropertyChanged ====================

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}