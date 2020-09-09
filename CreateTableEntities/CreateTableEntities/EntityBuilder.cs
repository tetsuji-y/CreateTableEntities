using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CreateTableEntities
{
    /// <summary>
    /// Entity（Dto）用テキスト生成ビルダークラス
    /// </summary>
    public class EntityBuilder
    {
        public string TableName { get; set; }

        private List<(string DataType, string Field)> Fields { get; } = new List<(string, string)>();

        public void AddTable(string tableName)
        {
            var master = string.Empty;

            if ("m".Equals(tableName.Substring(0, 1)))
                master = "Master";
 
           this.TableName = $"{Regex.Replace(tableName, @"^[mt]_", string.Empty).ToPascalCase()}{master}";
        }

        public void AddField(string dataType, string fieldName)
            => this.Fields.Add((DataType:dataType, Field:fieldName));

        public string Build()
        {
            return
$@"using System;

namespace XXXX //「XXXX」部分については機能毎の名称をセットしてください
{{
    public class {this.TableName} 
    {{
{BuildFields()}
    }}
}}";
        }

        private string BuildFields()
        {
            var sb = new StringBuilder();

            foreach(var f in this.Fields)
            {
                sb.AppendLine($"        public {ToCSharpDataType(f.DataType)} {f.Field.ToPascalCase()} {{ get; set; }}");
            }

            return sb.ToString();
        }

        private string ToCSharpDataType(string dataType)
        {
            //参照： https://docs.microsoft.com/ja-jp/dotnet/framework/data/adonet/sql-server-data-type-mappings

            switch (dataType)
            {
                case "smallint":
                case "int":
                    return "int";
                case "bigint":
                    return "long";
                case "nchar":
                case "varchar":
                case "nvarchar":
                    return "string";
                case "date":
                case "datetime":
                case "datetime2":
                    return nameof(DateTime);
                case "decimal":
                    return "decimal";
                case "float":
                    return "double";
                default:
                    return "object";
            }
        }
    }

    internal static class TableNameExtension
    {
        public static string ToPascalCase(this string s)
        {
            var sb = new StringBuilder();
            var chars = s.ToCharArray();
            var IsUppder = false;

            sb.Append(chars[0].ToString().ToUpper());
            
            for (int i = 1; i < chars.Length; i++)
            {
                switch (chars[i])
                {
                    case '_':
                        IsUppder = true;
                        break;
                    default:
                        sb.Append(IsUppder ? chars[i].ToString().ToUpper() : chars[i].ToString().ToLower());
                        IsUppder = false;
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
