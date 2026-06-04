using BisectionApp.Models;
using Npgsql;
using System;
using System.Collections.Generic;

namespace BisectionApp.Database
{
    /// <summary>
    /// Класс для работы с базой данных PostgreSQL через Npgsql
    /// </summary>
    public static class DatabaseHelper
    {
        // Строка подключения к PostgreSQL
        // ЗАМЕНИТЕ Password на свой пароль!
        private static readonly string ConnectionString =
            "Host=localhost;Port=5432;Database=bisection_db;Username=postgres;Password=password";

        /// <summary>
        /// Создание таблицы, если она не существует
        /// </summary>
        public static void CreateTable()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS calculation_results (
                        id SERIAL PRIMARY KEY,
                        left_bound DOUBLE PRECISION NOT NULL,
                        right_bound DOUBLE PRECISION NOT NULL,
                        root DOUBLE PRECISION NOT NULL,
                        function_value DOUBLE PRECISION NOT NULL,
                        iterations INTEGER NOT NULL,
                        accuracy DOUBLE PRECISION NOT NULL,
                        calculation_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
                    )";

                using (var command = new NpgsqlCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Проверка подключения к базе данных
        /// </summary>
        public static bool CheckConnection()
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Сохранение результата вычисления в базу данных
        /// </summary>
        public static void SaveResult(CalculationResult result)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                string insertQuery = @"
                    INSERT INTO calculation_results 
                    (left_bound, right_bound, root, function_value, iterations, accuracy, calculation_date)
                    VALUES 
                    (@LeftBound, @RightBound, @Root, @FunctionValue, @Iterations, @Accuracy, @CalculationDate)
                    RETURNING id";

                using (var command = new NpgsqlCommand(insertQuery, connection))
                {
                    // Добавляем параметры для защиты от SQL-инъекций
                    command.Parameters.AddWithValue("LeftBound", result.LeftBound);
                    command.Parameters.AddWithValue("RightBound", result.RightBound);
                    command.Parameters.AddWithValue("Root", result.Root);
                    command.Parameters.AddWithValue("FunctionValue", result.FunctionValue);
                    command.Parameters.AddWithValue("Iterations", result.Iterations);
                    command.Parameters.AddWithValue("Accuracy", result.Accuracy);
                    command.Parameters.AddWithValue("CalculationDate", result.CalculationDate);

                    // Получаем сгенерированный ID
                    result.Id = Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        /// <summary>
        /// Загрузка всех результатов из базы данных
        /// </summary>
        public static List<CalculationResult> LoadAllResults()
        {
            var results = new List<CalculationResult>();

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                string selectQuery = @"
                    SELECT id, left_bound, right_bound, root, function_value, 
                           iterations, accuracy, calculation_date
                    FROM calculation_results
                    ORDER BY calculation_date DESC
                    LIMIT 100";

                using (var command = new NpgsqlCommand(selectQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var result = new CalculationResult
                        {
                            Id = reader.GetInt32(0),
                            LeftBound = reader.GetDouble(1),
                            RightBound = reader.GetDouble(2),
                            Root = reader.GetDouble(3),
                            FunctionValue = reader.GetDouble(4),
                            Iterations = reader.GetInt32(5),
                            Accuracy = reader.GetDouble(6),
                            CalculationDate = reader.GetDateTime(7)
                        };

                        results.Add(result);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Удаление всех записей из базы данных
        /// </summary>
        public static void ClearAllResults()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                string deleteQuery = "DELETE FROM calculation_results";

                using (var command = new NpgsqlCommand(deleteQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}