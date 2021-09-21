using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace WDE.SqlQueryGenerator
{
    public static class Extensions
    {
        public static IQuery InsertIgnore(this ITable table, Dictionary<string, object?> obj)
        {
            return table.Insert(obj, true);
        }
        
        public static IQuery InsertIgnore(this ITable table, object obj)
        {
            return table.Insert(obj, true);
        }
        
        public static IQuery Insert(this ITable table, Dictionary<string, object?> obj, bool insertIgnore = false)
        {
            return table.BulkInsert(new[] { obj }, insertIgnore);
        }
        
        public static IQuery Insert(this ITable table, object obj, bool insertIgnore = false)
        {
            return table.BulkInsert(new[] { obj }, insertIgnore);
        }
        
        public static IQuery BulkInsert(this ITable table, IEnumerable<Dictionary<string, object?>> objects, bool insertIgnore = false)
        {
            bool first = true;
            IList<string> properties = null!;
            var sb = new StringBuilder();
            var lines = new List<string>();
            foreach (var o in objects)
            {
                if (first)
                {
                    properties = o.Keys.ToList();
                    var cols = string.Join(", ", properties.Select(c => $"`{c}`"));
                    var ignore = insertIgnore ? " IGNORE" : "";
                    sb.AppendLine($"INSERT{ignore} INTO `{table.TableName}` ({cols}) VALUES");
                    first = false;
                }

                var row = string.Join(", ", properties.Select(p => o[p].ToSql()));
                lines.Add($"({row})");
            }

            if (first)
                return new Query(table, "");

            sb.Append(string.Join(",\n", lines));
            sb.Append(';');
            return new Query(table, sb.ToString());
        }
        
        public static IQuery BulkInsert(this ITable table, IEnumerable<object> objects, bool insertIgnore = false)
        {
            bool first = true;
            PropertyInfo[] properties = null!;
            var sb = new StringBuilder();
            var lines = new List<string>();
            foreach (var o in objects)
            {
                if (first)
                {
                    var type = o.GetType();
                    properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    var cols = string.Join(", ", properties.Select(c => $"`{c.Name}`"));
                    var ignore = insertIgnore ? " IGNORE" : "";
                    sb.AppendLine($"INSERT{ignore} INTO `{table.TableName}` ({cols}) VALUES");
                    first = false;
                }

                var row = string.Join(", ", properties.Select(p => p.GetValue(o).ToSql()));
                lines.Add($"({row})");
            }

            if (first)
                return new Query(table, "");

            sb.Append(string.Join(",\n", lines));
            sb.Append(';');
            return new Query(table, sb.ToString());
        }
        
        public static IWhere Where(this ITable table, Expression<Func<IRow, bool>> predicate)
        {
            var condition = new ToSqlExpression().Visit(new SimplifyExpression().Visit(predicate.Body));
            if (condition is ConstantExpression c && c.Value is string s)
                return new Where(table, s);
            throw new Exception();
        }

        public static IWhere WhereIn<T>(this ITable table, string columnName, IEnumerable<T> values)
        {
            var str = string.Join(", ", values);
            return new Where(table, $"`{columnName}` IN ({str})");
        }

        public static IWhere WhereIn<T>(this IWhere where, string columnName, IEnumerable<T> values)
        {
            var str = string.Join(", ", values);
            return new Where(where.Table, $"({where.Condition}) AND (`{columnName}` IN ({str}))");
        }
        
        public static IQuery Delete(this IWhere query)
        {
            if (query.Condition == "1")
                return new Query(query.Table, $"DELETE FROM `{query.Table.TableName}`;");
            return new Query(query.Table, $"DELETE FROM `{query.Table.TableName}` WHERE {query.Condition};");
        }
        
        public static IUpdateQuery Set(this IWhere query, string key, object? value)
        {
            return new UpdateQuery(query, key, value.ToSql());
        }
        
        public static IUpdateQuery Set(this IUpdateQuery query, string key, object? value)
        {
            return new UpdateQuery(query, key, value.ToSql());
        }

        public static IQuery Update(this IUpdateQuery query)
        {
            var upd = string.Join(", ", query.Updates.Select(pair => $"`{pair.Item1}` = {pair.Item2}"));
            string where = "";
            if (query.Condition.Condition != "1")
                where = $" WHERE {query.Condition.Condition}";
            return new Query(query.Condition.Table, $"UPDATE `{query.Condition.Table.TableName}` SET {upd}{where};");
        }

        public static IQuery Comment(this IMultiQuery query, string comment)
        {
            return new Query(query, $" -- {comment}");
        }

        public static IQuery DefineVariable(this IMultiQuery query, string variableName, object? value)
        {
            return new Query(query, $"SET @{variableName} := {value.ToSql()};");
        }

        public static IQuery BlankLine(this IMultiQuery query)
        {
            return new Query(query, "\n");
        }

        public static IVariable Variable(this IMultiQuery query, string name)
        {
            return new Variable(name);
        }
        
        internal static string ToSql(this object? o)
        {
            if (o is null)
                return "NULL";
            if (o is string s)
                return s.ToSqlEscapeString();
            if (o is float f)
                return f.ToString(CultureInfo.InvariantCulture);
            if (o is double d)
                return d.ToString(CultureInfo.InvariantCulture);
            if (o is bool b)
                return b ? "1" : "0";
            return o.ToString() ?? "[INVALID TYPE]";
        }
        
        internal static string ToSqlEscapeString(this string str)
        {
            return "\"" +  str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n") + "\"";
        }
    }
}