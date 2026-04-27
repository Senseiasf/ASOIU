using System;
using System.Collections.Generic;
using System.Text;

namespace HW2_Variant25 
{
    /// <summary>
    /// Создатель текстовых отчетов на основе паттерна Fluent Interface
    /// </summary>
    public class ReportBuilder
    {
        private DatabaseManager _db;
        private string _sql = "";
        private string _title = "";
        private string[] _headers = Array.Empty<string>();
        private int[] _widths = Array.Empty<int>();
        private bool _numbered = false;
        private string _footer = "";

        public ReportBuilder(DatabaseManager db) { _db = db; }
        /// <summary>
        /// Устанавливает SQL-запрос для получения данных отчета
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public ReportBuilder Query(string sql) { _sql = sql; return this; }
        /// <summary>
        /// Задает заголовок формируемого отчета
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public ReportBuilder Title(string title) { _title = title; return this; }
        /// <summary>
        /// Устанавливает пользовательские названия колонок таблицы
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public ReportBuilder Header(params string[] columns) { _headers = columns; return this; }
        /// <summary>
        /// Задает ширину колонок отчета в символах
        /// </summary>
        /// <param name="widths"></param>
        /// <returns></returns>
        public ReportBuilder ColumnWidths(params int[] widths) { _widths = widths; return this; }
        /// <summary>
        /// Задаёт сообщение для вывода в конце списка
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public ReportBuilder Footer (string label) { _footer = label; return this;  }
        /// <summary>
        /// Включает отображение порядковых номеров строк слева
        /// </summary>
        /// <returns></returns>
        public ReportBuilder Numbered() { _numbered = true; return this; }
        /// <summary>
        /// Выполняет SQL-запрос и формирует итоговую текстовую строку отчета
        /// </summary>
        /// <returns></returns>
        public string Build()
        {
            var (columns, rows) = _db.ExecuteQuery(_sql);
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(_title))
            {
                sb.AppendLine();
                sb.AppendLine($"=== {_title} ===");
            }
            string[] displayHeaders = _headers.Length > 0 ? _headers : columns;
            int colCount = displayHeaders.Length;
            int numWidth = _numbered ? 5 : 0;

            if (_numbered) sb.Append("№".PadRight(numWidth));
            for (int i = 0; i < colCount; i++)
                sb.Append(displayHeaders[i].PadRight(_widths.Length > i ? _widths[i] : 20));

            sb.AppendLine();

            int totalWidth = numWidth;
            for (int i = 0; i < colCount; i++)
                totalWidth += (_widths.Length > i ? _widths[i] : 20);
            sb.AppendLine(new string('-', totalWidth));

            for (int r = 0; r < rows.Count; r++)
            {
                if (_numbered) sb.Append((r + 1).ToString().PadRight(numWidth));
                for (int c = 0; c < rows[r].Length && c < colCount; c++)
                    sb.Append(rows[r][c].PadRight(_widths.Length > c ? _widths[c] : 20));
                sb.AppendLine();
            }
            if (!string.IsNullOrEmpty(_footer))
            {
                sb.AppendLine(new string('-', totalWidth));
                sb.AppendLine($"{_footer} {rows.Count}");
            }
            return sb.ToString();
        }
        /// <summary>
        /// Выводит отчёт в консоль
        /// </summary>
        public void Print() { Console.Write(Build()); }
    }
}
