using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using Ralevex.EF.Support;

namespace Ralevex.EF.Extensions
    {
    public static partial class QueryableExtensions
        {
        private static DbContext GetDbContext(this IQueryable queryable)
            {
            DbContext dbContext;
            try
                {
                object internalQuery = ReflectionFunctions.GetReflectedMember(queryable, "InternalQuery");
                object internalContext = ReflectionFunctions.GetReflectedMember(internalQuery, "_internalContext");
                dbContext = (DbContext)ReflectionFunctions.GetReflectedMember(internalContext, "Owner");
                }
            catch (Exception e)
                {
                throw new ApplicationException("Unable to retreive db context from IQueryable", e);
                }
            return dbContext;
            }

        internal static ObjectQuery GetObjectQuery(this IQueryable query)
            {
            object internalQueryField = ReflectionFunctions.GetReflectedMember(query, "_internalQuery");
            return ReflectionFunctions.GetReflectedMember(internalQueryField, "_objectQuery") as ObjectQuery;
            }

        private static int ExecuteSql(IQueryable queryable, SqlCommand command)
            {
            ObjectQuery objectQuery = queryable.GetObjectQuery();

            //Retrieving parameters of internal Object Query
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

            var rs = dbContext.Database.ExecuteSqlCommand(command.CommandText, parametersArray);

            //If the result is being tracked in dataContexts we need to tell DbContext to reload data for updated entries
            if (objectQuery.MergeOption != MergeOption.NoTracking)
                {
                ObjectContext objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;
                objectContext.Refresh(RefreshMode.StoreWins, queryable);
                }
            return rs;
            }
        }
    }
