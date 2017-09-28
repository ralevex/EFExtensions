using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Ralevex.EF.Support;

namespace Ralevex.EF.Extensions
    {
    public static class QueryableExtensions
        {

        public static int Update<T>(this IQueryable<T> queryable,object parameters) where T : class
            {
            Type elementType = queryable.ElementType;
            //Identify Entity target table name 
            var tableAttribute = elementType.GetCustomAttribute<TableAttribute>();
            string tableName = tableAttribute?.Name ?? elementType.Name;

            //retreive list of fields to be updated from anonymous class
            List<PropertyInfo> fieldListInfo = parameters.GetType().GetProperties().ToList();

            //retrieve non key columns from Entity class
            List<MemberInfo> updatableColumns = ReflectionFunctions.GetNonKeyColumnsForType(queryable.ElementType)
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

            foreach (MemberInfo oneColumn in updatableColumns)
                {
                var columnAttribute = oneColumn.GetCustomAttribute<ColumnAttribute>();
                string fieldName = columnAttribute?.Name ?? oneColumn.Name;
                queryBuilder.AppendLine($"SET Base.{fieldName} = @{fieldName}");
                }

            queryBuilder.AppendLine($"FROM {tableName} Base ");
            queryBuilder.AppendLine("INNER JOIN Query ");

            var preffix = "ON";
            foreach (MemberInfo oneFieldType in ReflectionFunctions.GetKeyColumnsForType(queryable.ElementType))
                {
                var columnAttribute = oneFieldType.GetCustomAttribute<ColumnAttribute>();
                string fieldName    = columnAttribute?.Name ?? oneFieldType.Name;
                queryBuilder.AppendLine($"{preffix} \tBase.{fieldName} = Query.{fieldName}");
                preffix = "AND";
                }

            //Retrieving parameters of internal Object Query
            ObjectQuery<T> objectQuery = queryable.GetObjectQuery();
            var command = new SqlCommand(queryBuilder.ToString());

            foreach (ObjectParameter parameter in objectQuery.Parameters)
                {
                if (parameter.ParameterType == typeof(byte[]))
                    {
                    var param = new SqlParameter(parameter.Name, SqlDbType.VarBinary) { Value = parameter.Value };
                    command.Parameters.Add(param);
                    }
                else
                    command.Parameters.AddWithValue(parameter.Name, parameter.Value);
                }

            //Set SqlParameter(s) for parametarized query
            //  var command = new SqlCommand(queryBuilder.ToString());
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
            //convert sql parameters to array of objects which is expected by ExecuteSqlCommand from EF
            object[] parametersArray = command.Parameters.Cast<object>().ToArray();

            // EF will attempt to add parameters in its own collectin and parameter object do not allow multiple parents
            command.Parameters.Clear();

            //Attempt to retreive DbContext from internal properties of EF IQueryable object
            DbContext dbContext = queryable.GetDbContext();
         
            return dbContext.Database.ExecuteSqlCommand(command.CommandText, parametersArray);

            }
        public static int Delete<T>(this IQueryable<T> queryable) where T : class
            {
            Type elementType = queryable.ElementType;
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
            foreach (MemberInfo oneFieldType in ReflectionFunctions.GetKeyColumnsForType(queryable.ElementType))
                {
                var columnAttribute = oneFieldType.GetCustomAttribute<ColumnAttribute>();
                string fieldName = columnAttribute?.Name ?? oneFieldType.Name;
                queryBuilder.AppendLine($"{preffix} \tBase.{fieldName} = Query.{fieldName}");

                preffix = "AND";
                }

            //Retrieving parameters of internal Object Query
            ObjectQuery<T> objectQuery = queryable.GetObjectQuery();
            var command = new SqlCommand(queryBuilder.ToString());

            foreach (ObjectParameter parameter in objectQuery.Parameters)
                {
                if (parameter.ParameterType == typeof(byte[]))
                    {
                    var param = new SqlParameter(parameter.Name, SqlDbType.VarBinary) { Value = parameter.Value };
                    command.Parameters.Add(param);
                    }
                else
                    command.Parameters.AddWithValue(parameter.Name, parameter.Value);
                }

        
            //convert sql parameters to array of objects which is expected by ExecuteSqlCommand from EF
            object[] parametersArray = command.Parameters.Cast<object>().ToArray();

            // EF will attempt to add parameters in its own collectin and parameter object do not allow multiple parents
            command.Parameters.Clear();
            //Attempt to retreive DbContext from internal properties of EF IQueryable object
            DbContext dbContext = queryable.GetDbContext();

            return dbContext.Database.ExecuteSqlCommand(queryBuilder.ToString(), parametersArray);

            }
        private static DbContext GetDbContext<T>(this IQueryable<T> queryable) where T : class
            {
            DbContext dbContext;
            try
                {
                object internalQuery = ReflectionFunctions.GetReflectedMember(queryable, "InternalQuery");
                object internalContext = ReflectionFunctions.GetReflectedMember(internalQuery, "_internalContext");
                dbContext = (DbContext) ReflectionFunctions.GetReflectedMember(internalContext, "Owner");
                }
            catch (Exception e)
                {
                throw new ApplicationException("Unable to retreive db context from IQueryable", e);
                }
            return dbContext;
            }

        internal static ObjectQuery<T> GetObjectQuery<T>(this IQueryable<T> query)
            {
            object internalQueryField = ReflectionFunctions.GetReflectedMember(query, "_internalQuery");
            return ReflectionFunctions.GetReflectedMember(internalQueryField, "_objectQuery") as ObjectQuery<T>;
            }
        }


    }