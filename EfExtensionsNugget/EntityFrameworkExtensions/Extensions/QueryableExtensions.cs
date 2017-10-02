using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using Ralevex.EF.Support;

namespace Ralevex.EF.Extensions
    {
    public static partial class QueryableExtensions
        {

        /// <summary>
        /// Construct and execute UPDATE on IQueryable object, with values specified via anonymous type.
        /// </summary>
        /// <param name="queryable"><see cref="T:System.Linq.IQueryable`1" /></param>
        /// <param name="parameters">Anonymous Type with values for update</param>
        /// <returns>number of affected rows</returns>
        public static int Update(this IQueryable queryable, object parameters)
            {
            Type elementType = queryable.ElementType;

            if (elementType.IsConstructedGenericType)
                throw new NotSupportedException("Provided query is not updatable");

            //Identify Entity target table name 
            var tableAttribute = elementType.GetCustomAttribute<TableAttribute>();
            string tableName = tableAttribute?.Name ?? elementType.Name;

            //retreive list of fields to be updated from anonymous class
            List<PropertyInfo> fieldListInfo = parameters.GetType().GetProperties().ToList();

            //retrieve non key columns from Entity class
            List<MemberInfo> updatableColumns = queryable.ElementType.GetNonKeyColumnsForType()
                                                .Where(p => fieldListInfo
                                                            .Any(pi => pi.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase))
                                                      )
                                                .ToList();

            List<string> invalidFields = fieldListInfo
                                    .Select(p => p.Name)
                                    .Except(updatableColumns.Select(o => o.Name), StringComparer.OrdinalIgnoreCase)
                                    .ToList();

            //validate that all requested fields present 
            if (invalidFields.Count > 0)
                {
                throw new ArgumentException($"The following fields do not exist in table <{tableName}>:  {string.Join(", ", invalidFields)}", nameof(parameters));
                }

            //build update query using selection of IQueryable
            var queryBuilder = new StringBuilder();
            queryBuilder.Append("WITH Query AS (");
            queryBuilder.Append(queryable);  // <-  this casted to string returns SQL from 
            queryBuilder.AppendLine(") ");
            queryBuilder.Append("UPDATE Base ");

            foreach (string fieldName in updatableColumns.Select(mi => (mi.GetCustomAttribute<ColumnAttribute>()?.Name) ?? mi.Name))
                queryBuilder.AppendLine($"SET Base.{fieldName} = @{fieldName}");

            queryBuilder.AppendLine($"FROM {tableName} Base ");
            queryBuilder.AppendLine("INNER JOIN Query ");

            var preffix = "ON";
            foreach (string fieldName in elementType.GetKeyColumnsForType()
                .Select(mi => (mi.GetCustomAttribute<ColumnAttribute>()?.Name) ?? mi.Name))
                {
                queryBuilder.AppendLine($"{preffix} \tBase.{fieldName} = Query.{fieldName}");
                preffix = "AND";
                }


            var command = new SqlCommand(queryBuilder.ToString());

            //Set SqlParameter(s) for update
            foreach (PropertyInfo propertyInfo in fieldListInfo)
                {
                object val = propertyInfo.GetValue(parameters, null) ?? DBNull.Value;
                if (propertyInfo.PropertyType == typeof(byte[]))
                    {
                    var param = new SqlParameter(propertyInfo.Name, SqlDbType.VarBinary) { Value = val };
                    command.Parameters.Add(param);
                    }
                else
                    command.Parameters.AddWithValue(propertyInfo.Name, val);
                }

            return ExecuteSql(queryable, command);

            }

        /// <summary>
        /// Construct and execute DELETE on IQueryable object.
        /// </summary>
        /// <param name="queryable"><see cref="T:System.Linq.IQueryable`1" /></param>
        /// <returns>number of affected rows</returns>
        public static int Delete(this IQueryable queryable)
            {
            Type elementType = queryable.ElementType;

            if (elementType.IsConstructedGenericType)
                throw new NotSupportedException("Provided query is not updatable");

            //Identify Entity target table name 
            var tableAttribute = elementType.GetCustomAttribute<TableAttribute>();
            string tableName = tableAttribute?.Name ?? elementType.Name;

            //build update query using selection of IQueryable
            var queryBuilder = new StringBuilder();
            queryBuilder.Append("WITH Query AS (");
            queryBuilder.Append(queryable);  // <-  this casted to string returns SQL from 
            queryBuilder.AppendLine(") ");
            queryBuilder.Append("DELETE Base ");
            queryBuilder.AppendLine($"FROM {tableName} Base ");
            queryBuilder.AppendLine("INNER JOIN Query ");


            var preffix = "ON";
            foreach (string fieldName in elementType.GetKeyColumnsForType()
                                                    .Select(mi => (mi.GetCustomAttribute<ColumnAttribute>()?.Name) ?? mi.Name))
                {
                queryBuilder.AppendLine($"{preffix} \tBase.{fieldName} = Query.{fieldName}");
                preffix = "AND";
                }

            var command = new SqlCommand(queryBuilder.ToString());

            return ExecuteSql(queryable, command);

            }

        /// <summary>
        /// For an Entity Framework IQueryable, returns the SQL and Parameters.
        /// </summary>
        /// <param name="query">IQueryable object</param>
        /// <returns>printable SQL string with parameters</returns>
        public static string ToTraceString(this IQueryable query)
            {
            ObjectQuery objectQuery = query.GetObjectQuery();

            //unless we call ToTraceString() on objectQuery first
            //objectQuery.Parameters is empty
            string queryText = objectQuery.ToTraceString();

            var traceString = new StringBuilder();

            foreach (ObjectParameter parameter in objectQuery.Parameters)
                traceString.AppendLine(parameter.DeclareVariable());

            traceString.AppendLine();
            traceString.AppendLine(queryText);
            traceString.AppendLine();

            return traceString.ToString();
            }

        }
    }