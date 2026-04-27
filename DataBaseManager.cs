using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HW2_Variant25 
{
    /// <summary>
    /// Взаимодействие с БД
    /// </summary>
    public class DatabaseManager
    {
        private string _connectionString;

        public DatabaseManager(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";
        }
        /// <summary>
        /// Создает таблицы и выполняет первичный импорт данных из CSV-файлов
        /// </summary>
        /// <param name="zoosCsvPath"></param>
        /// <param name="animalsCsvPath"></param>
        public void InitializeDatabase(string zoosCsvPath, string animalsCsvPath)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS zoos (
                    zoo_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    zoo_name TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS animals (
                    animal_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    zoo_id INTEGER NOT NULL,
                    animal_name TEXT NOT NULL,
                    weight_kg REAL NOT NULL,
                    FOREIGN KEY (zoo_id) REFERENCES zoos(zoo_id)
                );";
            cmd.ExecuteNonQuery();

            if (GetAllZoos().Count == 0 && File.Exists(zoosCsvPath))
            {
                ImportZoosFromCsv(zoosCsvPath);
                Console.WriteLine($"[OK] Загружены зоопарки из {Path.GetFileName(zoosCsvPath)}");
            }

            if (GetAllAnimals().Count == 0 && File.Exists(animalsCsvPath))
            {
                ImportAnimalsFromCsv(animalsCsvPath);
                Console.WriteLine($"[OK] Загружены животные из {Path.GetFileName(animalsCsvPath)}");
            }
        }
        /// <summary>
        /// Загружает записи о зоопарках из CSV-файла в базу данных
        /// </summary>
        /// <param name="path"></param>
        private void ImportZoosFromCsv(string path)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string[] lines = File.ReadAllLines(path);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(';');
                if (parts.Length < 2) continue;

                var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO zoos (zoo_id, zoo_name) VALUES (@id, @name)";
                cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
                cmd.Parameters.AddWithValue("@name", parts[1]);
                cmd.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Загружает записи о животных из CSV-файла в базу данных
        /// </summary>
        /// <param name="path"></param>
        private void ImportAnimalsFromCsv(string path)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            string[] lines = File.ReadAllLines(path);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(';');
                if (parts.Length < 4) continue;

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO animals (zoo_id, animal_name, weight_kg) 
                                    VALUES (@zId, @name, @w)";

                cmd.Parameters.AddWithValue("@zId", int.Parse(parts[0]));
                cmd.Parameters.AddWithValue("@name", parts[2]);

                string weightStr = parts[3].Replace(',', '.');
                cmd.Parameters.AddWithValue("@w", double.Parse(weightStr, CultureInfo.InvariantCulture));

                cmd.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Возвращает полный список зоопарков из базы данных
        /// </summary>
        /// <returns></returns>
        public List<Zoo> GetAllZoos()
        {
            var result = new List<Zoo>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT zoo_id, zoo_name FROM zoos ORDER BY zoo_id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(new Zoo(reader.GetInt32(0), reader.GetString(1)));
            return result;
        }
        /// <summary>
        /// Возвращает полный список животных из базы данных
        /// </summary>
        /// <returns></returns>
        public List<Animal> GetAllAnimals()
        {
            var result = new List<Animal>();
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT animal_id, zoo_id, animal_name, weight_kg FROM animals ORDER BY animal_id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(new Animal(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetDouble(3)));
            return result;
        }
        /// <summary>
        /// Добавляет новую запись о животном в базу данных
        /// </summary>
        /// <param name="animal"></param>
        public void AddAnimal(Animal animal)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO animals (zoo_id, animal_name, weight_kg) VALUES (@zId, @name, @w)";
            cmd.Parameters.AddWithValue("@zId", animal.ZooId);
            cmd.Parameters.AddWithValue("@name", animal.Name);
            cmd.Parameters.AddWithValue("@w", animal.Weight);
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// Обновляет существующую запись о животном по идентификатору
        /// </summary>
        /// <param name="animal"></param>
        public void UpdateAnimal(Animal animal)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE animals SET zoo_id=@zId, animal_name=@name, weight_kg=@w WHERE animal_id=@id";
            cmd.Parameters.AddWithValue("@id", animal.Id);
            cmd.Parameters.AddWithValue("@zId", animal.ZooId);
            cmd.Parameters.AddWithValue("@name", animal.Name);
            cmd.Parameters.AddWithValue("@w", animal.Weight);
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// Удаляет запись о животном из базы данных по идентификатору
        /// </summary>
        /// <param name="id"></param>
        public void DeleteAnimal(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM animals WHERE animal_id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// Ищет и возвращает запись о животном по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Animal GetAnimalById(int id)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT animal_id, zoo_id, animal_name, weight_kg FROM animals WHERE animal_id=@id";
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return new Animal(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetDouble(3));
            return null;
        }
        /// <summary>
        /// Выполняет запрос в БД и возвращает названия столбцов и строки данных для отчетов
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var reader = cmd.ExecuteReader();

            string[] columns = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                columns[i] = reader.GetName(i);

            var rows = new List<string[]>();
            while (reader.Read())
            {
                string[] row = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; i++)
                    row[i] = reader.GetValue(i)?.ToString() ?? "";
                rows.Add(row);
            }
            return (columns, rows);
        }
    }
}

