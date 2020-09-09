using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CreateTableEntities
{
    /// <summary>
    /// Dtoクラス生成アプリケーション
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var list = GetSchema().GroupBy(row => row["TableName"]);

            foreach(var table in list)
            {
                var tableName = table.Key.ToString();
                var builder = new EntityBuilder();

                builder.AddTable(tableName);

                foreach (var row in table)
                {
                    builder.AddField(
                        row["DataType"].ToString(),
                        row["ColumnName"].ToString()
                        );
                }

                _ = WriteFileAsync(
                    builder.TableName, 
                    builder.Build()
                    );
            }
        }

        private static IEnumerable<DataRow> GetSchema()
        {
            var conStr = ConfigurationManager.ConnectionStrings["Wakabadai"].ConnectionString;
            var dt = new DataTable();

            using (var connection = new SqlConnection(conStr))
            {
                connection.Open();

                new SqlDataAdapter(@"
                                    SELECT
                                         t.name                  AS TableName
                                        ,c.name                  AS ColumnName
                                        ,type_name(user_type_id) AS DataType
                                        ,max_length              AS Length
                                        ,is_nullable             AS Nullable
                                    FROM
                                        sys.objects t
                                        INNER JOIN sys.columns c ON t.object_id = c.object_id
                                    WHERE
                                        t.type = 'U'
                                    ORDER BY
                                        t.name",
                                        connection)
                    .Fill(dt);
            }

            return dt.AsEnumerable();
        }

        private async static Task WriteFileAsync(string tableName, string text)
        {
            var folderPath = ConfigurationManager.AppSettings["FolderPath"];

            var fullPath = Path.Combine(
                string.IsNullOrEmpty(folderPath) 
                    ? Environment.CurrentDirectory 
                    : folderPath,
                $"{tableName}.cs"
                );

            using (var sw = new StreamWriter(fullPath, false))
                await sw.WriteAsync(text).ConfigureAwait(false);
        }
    }
}
