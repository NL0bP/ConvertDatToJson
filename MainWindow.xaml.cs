using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;

using Newtonsoft.Json;

namespace ConvertDatToJson
{
    public class AppConfig
    {
        public string LastSelectedFolder { get; set; }
    }

    public partial class MainWindow : Window
    {
        // Путь к файлу конфигурации
        private readonly string _configFilePath;
        // Переменная для хранения пути к папке
        private string _selectedFolderPath;

        public MainWindow()
        {
            InitializeComponent();

            // Определяем путь к файлу конфигурации
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

            // Загружаем конфигурацию при запуске
            LoadConfig();
        }

        private void LoadConfig()
        {
            if (File.Exists(_configFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_configFilePath);
                    var config = JsonConvert.DeserializeObject<AppConfig>(json);

                    _selectedFolderPath = config.LastSelectedFolder;
                    OutputTextBox.Text = $"Загружен последний выбранный путь: {_selectedFolderPath}\n";
                }
                catch (Exception ex)
                {
                    OutputTextBox.Text += $"Ошибка при загрузке конфигурации: {ex.Message}\n";
                }
            }
            else
            {
                try
                {
                    var config = new AppConfig
                    {
                        LastSelectedFolder = string.Empty
                    };

                    string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                    File.WriteAllText(_configFilePath, json);

                    OutputTextBox.Text += "Файл конфигурации создан.\n";
                }
                catch (Exception ex)
                {
                    OutputTextBox.Text += $"Ошибка при создании файла конфигурации: {ex.Message}\n";
                }
            }
        }

        private void SaveConfig()
        {
            try
            {
                // Создаем объект конфигурации
                var config = new AppConfig
                {
                    LastSelectedFolder = _selectedFolderPath
                };

                // Сериализуем в JSON и сохраняем в файл
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(_configFilePath, json);

                OutputTextBox.Text += $"Конфигурация сохранена: {_configFilePath}\n";
            }
            catch (Exception ex)
            {
                OutputTextBox.Text += $"Ошибка при сохранении конфигурации: {ex.Message}\n";
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

            // Обрабатываем все вложенные папки
            foreach (var subFolder in Directory.GetDirectories(folderPath))
            {
                string folderName = Path.GetFileName(subFolder); // Имя папки (например, doodad_map или npc_map)
                string jsonOutputFolder = Path.Combine(jsonOutputBaseFolder, folderName); // Папка для сохранения JSON

                // Создаем папку для JSON, если она не существует
                if (!Directory.Exists(jsonOutputFolder))
                {
                    Directory.CreateDirectory(jsonOutputFolder);
                    OutputTextBox.Text += $"Создана папка для JSON файлов: {jsonOutputFolder}\n";
                }

                // Определяем префикс для файлов в зависимости от папки
                string filePrefix = "";
                if (folderName.Equals("doodad_map", StringComparison.OrdinalIgnoreCase))
                {
                    filePrefix = "doodad_spawns_main_";
                }
                else if (folderName.Equals("npc_map", StringComparison.OrdinalIgnoreCase))
                {
                    filePrefix = "npc_spawns_main_";
                }

                // Обрабатываем все .dat файлы в текущей папке
                var datFiles = Directory.GetFiles(subFolder, "*.dat");
                foreach (var file in datFiles)
                {
                    try
                    {
                        // Парсим файл
                        var entities = ParseDatFile(file);

                        // Формируем имя JSON файла с префиксом
                        string jsonFileName = filePrefix + Path.GetFileNameWithoutExtension(file) + ".json";
                        string jsonFilePath = Path.Combine(jsonOutputFolder, jsonFileName);

                        // Сериализуем данные в JSON и записываем на диск
                        string json = JsonConvert.SerializeObject(entities, Formatting.Indented);
                        File.WriteAllText(jsonFilePath, json);

                        // Выводим информацию в интерфейс
                        OutputTextBox.Text += $"Файл {file} успешно обработан. JSON сохранен в {jsonFilePath}\n";
                    }
                    catch (Exception ex)
                    {
                        OutputTextBox.Text += $"Ошибка при обработке файла {file}: {ex.Message}\n";
                    }
                }
            }
        }

        private List<EntityData> ParseDatFile(string filePath)
        {
            var entities = new List<EntityData>();
            using (var stream = File.OpenRead(filePath))
            using (var reader = new BinaryReader(stream))
            {
                // Размер одной записи в байтах (без учета Scale и поворотов)
                int recordSize = 16; // 4 (uint) + 4*3 (float x, y, z)

                while (stream.Position < stream.Length)
                {
                    // Проверяем, что осталось достаточно байт для чтения
                    if (stream.Length - stream.Position < recordSize)
                    {
                        OutputTextBox.Text += $"Файл {filePath} поврежден или имеет неполные данные.\n";
                        break;
                    }

                    var entity = new EntityData
                    {
                        UnitId = reader.ReadUInt32(), // Читаем UnitId
                        Position = new Position
                        {
                            X = reader.ReadSingle(), // Читаем X
                            Y = reader.ReadSingle(), // Читаем Y
                            Z = reader.ReadSingle(), // Читаем Z
                            Roll = 0.0f,  // Фиксированное значение
                            Pitch = 0.0f, // Фиксированное значение
                            Yaw = 0.0f    // Фиксированное значение
                        },
                        Scale = 1.0f // Фиксированное значение
                    };

                    entities.Add(entity);
                }
            }
            return entities;
        }
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