using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;

namespace HW2_Variant25
{
    /// <summary>
    /// Основной класс приложения, реализующий интерфейс в консоли
    /// </summary>
    class Program
    {
        static string FindFile(string fileName)
        {
            string dir = AppContext.BaseDirectory;
            while (dir != null)
            {
                string path = Path.Combine(dir, fileName);
                if (File.Exists(path))
                    return path;

                dir = Directory.GetParent(dir)?.FullName;
            }
            return null;
        }
        /// <summary>
        /// Главный цикл консольного меню
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Clear();
            string zoosCsv = FindFile("zoos.csv");
            string animalsCsv = FindFile("animals.csv");

            if (zoosCsv == null || animalsCsv == null)
            {
                Console.WriteLine("ОШИБКА: Не удалось найти CSV файлы!");
                Console.WriteLine($"Папка запуска: {AppContext.BaseDirectory}");
                Console.WriteLine("Убедись, что файлы называются строго zoos.csv и animals.csv");
                Console.ReadLine();
                return;
            }

            string dbPath = "zoos_data.db";
            var db = new DatabaseManager(dbPath);
            db.InitializeDatabase(zoosCsv, animalsCsv);

            Console.WriteLine();

            string choice;
            do
            {
                Console.WriteLine("--- УПРАВЛЕНИЕ ЗООПАРКАМИ (Вариант 25) ---");
                Console.WriteLine("1. Показать все зоопарки");
                Console.WriteLine("2. Показать всех животных");
                Console.WriteLine("3. Добавить животное");
                Console.WriteLine("4. Редактировать животное");
                Console.WriteLine("5. Удалить животное");
                Console.WriteLine("6. Отчёты");
                Console.WriteLine("0. Выход");
                Console.Write("Ваш выбор: ");
                choice = Console.ReadLine()?.Trim() ?? "";

                Console.WriteLine();

                switch (choice)
                {
                    case "1": ShowZoos(db); break;
                    case "2": ShowAnimals(db); break;
                    case "3": AddAnimal(db); break;
                    case "4": EditAnimal(db); break;
                    case "5": DeleteAnimal(db); break;
                    case "6": ShowReports(db); break;
                    case "0": Console.WriteLine("До свидания!"); break;
                    default: Console.WriteLine("Неверный пункт меню."); break;
                }
                Console.WriteLine();
            } while (choice != "0");
        }
        /// <summary>
        /// Выводит в консоль список всех доступных зоопарков
        /// </summary>
        /// <param name="db"></param>
        static void ShowZoos(DatabaseManager db)
        {
            Console.WriteLine("--- Все зоопарки ---");
            var zoos = db.GetAllZoos();
            foreach (var z in zoos) Console.WriteLine("  " + z);
            Console.WriteLine($"Всего зоопарков: {zoos.Count}");
        }
        /// <summary>
        /// Выводит в консоль список всех зарегистрированных животных
        /// </summary>
        /// <param name="db"></param>
        static void ShowAnimals(DatabaseManager db)
        {
            Console.WriteLine("--- Все животные ---");
            var animals = db.GetAllAnimals();
            foreach (var a in animals) Console.WriteLine("  " + a);
            Console.WriteLine($"Всего животных: {animals.Count}");
        }
        /// <summary>
        /// Обрабатывает ввод пользователя для добавления нового животного
        /// </summary>
        /// <param name="db"></param>
        static void AddAnimal(DatabaseManager db)
        {
            Console.WriteLine("--- Добавление животного ---");
            Console.WriteLine("Доступные зоопарки:");
            foreach (var z in db.GetAllZoos()) Console.WriteLine("  " + z);

            Console.Write("ID зоопарка: ");
            if (!int.TryParse(Console.ReadLine(), out int zId))
            {
                Console.WriteLine("Ошибка: введите целое число.");
                return;
            }

            Console.Write("Имя животного: ");
            string name = Console.ReadLine()?.Trim() ?? "";
            if (name.Length == 0)
            {
                Console.WriteLine("Ошибка: имя не может быть пустым.");
                return;
            }

            Console.Write("Вес (кг): ");
            if (!double.TryParse(Console.ReadLine()?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double weight))
            {
                Console.WriteLine("Ошибка: введите число.");
                return;
            }

            try
            {
                db.AddAnimal(new Animal(0, zId, name, weight));
                Console.WriteLine("Животное успешно добавлено.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
        /// <summary>
        /// Обрабатывает ввод пользователя для редактирования данных животного
        /// </summary>
        /// <param name="db"></param>
        static void EditAnimal(DatabaseManager db)
        {
            Console.WriteLine("--- Редактирование животного ---");
            Console.Write("Введите ID животного: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите целое число.");
                return;
            }

            var animal = db.GetAnimalById(id);
            if (animal == null)
            {
                Console.WriteLine($"Животное с ID={id} не найдено.");
                return;
            }

            Console.WriteLine($"Текущие данные: {animal}");
            Console.WriteLine("(нажмите Enter, чтобы оставить значение без изменений)");

            Console.Write($"Имя [{animal.Name}]: ");
            string input = Console.ReadLine()?.Trim() ?? "";
            if (input.Length > 0) animal.Name = input;

            Console.Write($"ID зоопарка [{animal.ZooId}]: ");
            input = Console.ReadLine()?.Trim() ?? "";
            if (input.Length > 0 && int.TryParse(input, out int newZId)) animal.ZooId = newZId;

            Console.Write($"Вес [{animal.Weight}]: ");
            input = Console.ReadLine()?.Trim().Replace(',', '.') ?? "";
            if (input.Length > 0 && double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double newWeight))
            {
                try
                {
                    animal.Weight = newWeight;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    return;
                }
            }

            db.UpdateAnimal(animal);
            Console.WriteLine("Данные обновлены.");
        }
        /// <summary>
        /// Обрабатывает ввод пользователя для удаления животного
        /// (с подтверждением удаления)
        /// </summary>
        /// <param name="db"></param>
        static void DeleteAnimal(DatabaseManager db)
        {
            Console.WriteLine("--- Удаление животного ---");
            Console.Write("Введите ID животного: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Ошибка: введите целое число.");
                return;
            }

            var animal = db.GetAnimalById(id);
            if (animal == null)
            {
                Console.WriteLine($"Животное с ID={id} не найдено.");
                return;
            }

            Console.Write($"Удалить «{animal.Name}»? (да/нет): ");
            string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
            if (confirm == "да")
            {
                db.DeleteAnimal(id);
                Console.WriteLine("Животное удалено.");
            }
            else
            {
                Console.WriteLine("Удаление отменено.");
            }
        }
        /// <summary>
        /// Доп. меню для выбора и создания отчётов
        /// </summary>
        /// <param name="db"></param>
        static void ShowReports(DatabaseManager db)
        {
            string choice;
            do
            {
                Console.WriteLine("--- Отчёты ---");
                Console.WriteLine("1. Животные с названиями зоопарков");
                Console.WriteLine("2. Количество особей в зоопарках");
                Console.WriteLine("3. Средний вес животных по зоопаркам");
                Console.WriteLine("0. Назад");
                Console.Write("Ваш выбор: ");
                choice = Console.ReadLine()?.Trim() ?? "";

                var builder = new ReportBuilder(db);
                switch (choice)
                {
                    case "1":
                        builder.Query(@"
                            SELECT a.animal_name, z.zoo_name, a.weight_kg 
                            FROM animals a 
                            JOIN zoos z ON a.zoo_id = z.zoo_id 
                            ORDER BY a.animal_name")
                               .Title("Животные по зоопаркам")
                               .Header("Кличка", "Зоопарк", "Вес (кг)")
                               .ColumnWidths(25, 30, 15)
                               .Numbered()
                               .Footer("Всего животных:")
                               .Print();

                        break;
                    case "2":
                        builder.Query(@"
                            SELECT z.zoo_name, COUNT(*) 
                            FROM animals a 
                            JOIN zoos z ON a.zoo_id = z.zoo_id 
                            GROUP BY z.zoo_name
                            ORDER BY z.zoo_name")
                               .Title("Количество особей")
                               .Header("Зоопарк", "Кол-во")
                               .ColumnWidths(30, 15)
                               .Print();
                        break;
                    case "3":
                        builder.Query(@"
                            SELECT z.zoo_name, ROUND(AVG(a.weight_kg), 2) AS avg_weight
                            FROM animals a 
                            JOIN zoos z ON a.zoo_id = z.zoo_id 
                            GROUP BY z.zoo_name
                            ORDER BY avg_weight DESC")
                               .Title("Средний вес")
                               .Header("Зоопарк", "Средний вес (кг)")
                               .ColumnWidths(30, 20)
                               .Print();
                        break;
                    case "0":
                        break;
                    default:
                        Console.WriteLine("Неверный пункт.");
                        break;
                }
                Console.WriteLine();
            } while (choice != "0");
        }
    }
}