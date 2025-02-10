using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Newtonsoft.Json;
using NLog;
using MessageBox = System.Windows.MessageBox;

namespace ConvertDatToJson
{
    public partial class MainWindow : Window
    {
        // Логгер NLog
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Переменная для хранения пути к папке
        private string _selectedFolderPath;

        public MainWindow()
        {
            InitializeComponent();

            // Создаем папку для логов, если она не существует
            string logsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(logsFolder))
            {
                Directory.CreateDirectory(logsFolder);
            }

            // Загружаем конфигурацию
            LoadConfig();
        }

        private void LoadConfig()
        {
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (File.Exists(configFilePath))
            {
                try
                {
                    string json = File.ReadAllText(configFilePath);
                    var config = JsonConvert.DeserializeObject<AppConfig>(json);

                    // Устанавливаем последний выбранный путь
                    _selectedFolderPath = config.LastSelectedFolder;
                    OutputTextBox.Text = $"Загружен последний выбранный путь: {_selectedFolderPath}\n";
                    Logger.Info($"Загружен последний выбранный путь: {_selectedFolderPath}");
                }
                catch (Exception ex)
                {
                    OutputTextBox.Text += $"Ошибка при загрузке конфигурации: {ex.Message}\n";
                    Logger.Error(ex, "Ошибка при загрузке конфигурации");
                }
            }
            else
            {
                Logger.Info("Файл конфигурации не найден. Будет создан новый.");
            }
        }

        private void SaveConfig()
        {
            try
            {
                string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                var config = new AppConfig
                {
                    LastSelectedFolder = _selectedFolderPath
                };

                // Сериализуем конфигурацию в JSON и сохраняем в файл
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, json);

                Logger.Info($"Конфигурация сохранена: {configFilePath}");
            }
            catch (Exception ex)
            {
                OutputTextBox.Text += $"Ошибка при сохранении конфигурации: {ex.Message}\n";
                Logger.Error(ex, "Ошибка при сохранении конфигурации");
            }
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new FolderBrowserDialog();

            // Устанавливаем начальный путь, если он был загружен из конфигурации
            if (!string.IsNullOrEmpty(_selectedFolderPath))
            {
                folderDialog.SelectedPath = _selectedFolderPath;
            }

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Запоминаем путь к папке
                _selectedFolderPath = folderDialog.SelectedPath;
                OutputTextBox.Text = $"Выбрана папка: {_selectedFolderPath}\n";
                Logger.Info($"Выбрана папка: {_selectedFolderPath}");

                // Сохраняем конфигурацию
                SaveConfig();

                // Обрабатываем файлы в выбранной папке
                ProcessFolder(_selectedFolderPath);
            }
        }

        private void ProcessFolder(string folderPath)
        {
            // Путь для сохранения JSON файлов (в папке GameData\map_data)
            string jsonOutputBaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GameData", "map_data");

            // Списки для хранения информации о файлах
            var processedFiles = new List<string>();
            var failedFiles = new List<string>();

            // Получаем все .dat файлы в папке и подпапках
            var datFiles = Directory.GetFiles(folderPath, "*.dat", SearchOption.AllDirectories);

            // Настраиваем прогресс-бар
            ProgressBar.Maximum = datFiles.Length;
            ProgressBar.Value = 0;

            // Обрабатываем каждый файл
            foreach (var file in datFiles)
            {
                try
                {
                    // Определяем папку для сохранения JSON
                    string folderName = Path.GetFileName(Path.GetDirectoryName(file)); // Имя папки (doodad_map или npc_map)
                    string jsonOutputFolder = Path.Combine(jsonOutputBaseFolder, folderName);

                    // Создаем папку для JSON, если она не существует
                    if (!Directory.Exists(jsonOutputFolder))
                    {
                        Directory.CreateDirectory(jsonOutputFolder);
                        Logger.Info($"Создана папка для JSON файлов: {jsonOutputFolder}");
                    }

                    // Определяем префикс для файлов в зависимости от папки
                    string filePrefix = folderName.Equals("doodad_map", StringComparison.OrdinalIgnoreCase) ? "doodad_spawns_main_" : "npc_spawns_main_";

                    // Парсим файл
                    var entities = ParseDatFile(file);

                    // Формируем имя JSON файла с префиксом
                    string jsonFileName = filePrefix + Path.GetFileNameWithoutExtension(file) + ".json";
                    string jsonFilePath = Path.Combine(jsonOutputFolder, jsonFileName);

                    // Сериализуем данные в JSON и записываем на диск
                    string json = JsonConvert.SerializeObject(entities, Formatting.Indented);
                    File.WriteAllText(jsonFilePath, json);

                    // Добавляем файл в список успешно обработанных
                    processedFiles.Add(file);
                    Logger.Info($"Файл {file} успешно обработан. JSON сохранен в {jsonFilePath}");
                }
                catch (Exception ex)
                {
                    // Добавляем файл в список необработанных
                    failedFiles.Add(file);
                    Logger.Error(ex, $"Ошибка при обработке файла {file}");
                }

                // Обновляем прогресс-бар
                ProgressBar.Value++;
            }

            // Выводим итоговое сообщение
            string message = $"Обработка завершена.\nУспешно обработано файлов: {processedFiles.Count}\nНе удалось обработать файлов: {failedFiles.Count}";
            MessageBox.Show(message, "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            Logger.Info(message);

            // Выводим список необработанных файлов
            if (failedFiles.Count > 0)
            {
                string failedFilesMessage = "Не удалось обработать следующие файлы:\n" + string.Join("\n", failedFiles);
                OutputTextBox.Text += failedFilesMessage + "\n";
                Logger.Warn(failedFilesMessage);
            }
        }

        private List<EntityData> ParseDatFile(string filePath)
        {
            var entities = new List<EntityData>();
            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(stream))
            {
                // Размер одной записи в байтах
                int recordSize = 16; // 4 (uint) + 4*3 (float x, y, z)

                while (stream.Position < stream.Length)
                {
                    // Проверяем, что осталось достаточно байт для чтения
                    if (stream.Length - stream.Position < recordSize)
                    {
                        Logger.Warn($"Файл {filePath} поврежден или имеет неполные данные.");
                        break;
                    }

                    var entity = new EntityData
                    {
                        UnitId = reader.ReadUInt32(),
                        Position = new Position
                        {
                            X = reader.ReadSingle(),
                            Y = reader.ReadSingle(),
                            Z = reader.ReadSingle(),
                            Roll = 0.0f,
                            Pitch = 0.0f,
                            Yaw = 0.0f
                        },
                        Scale = 1.0f
                    };

                    entities.Add(entity);
                }
            }
            return entities;
        }
    }

    public class AppConfig
    {
        public string LastSelectedFolder { get; set; }
    }

    public class EntityData
    {
        public uint UnitId { get; set; }
        public Position Position { get; set; }
        public float Scale { get; set; }
    }

    public class Position
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
    }
}