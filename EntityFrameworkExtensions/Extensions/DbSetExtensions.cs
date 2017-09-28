using System;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Ralevex.EF.Support;

namespace Ralevex.EF.Extensions
{
    internal static class DbSetExtensions
    {
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public static DbContext GetDbContext<TEntity>(this DbSet<TEntity> dbSet) where TEntity : class
        {
            try
            {
                object internalSet     = ReflectionFunctions.GetReflectedMember(dbSet, "_internalSet");
                object internalContext = internalSet
                    .GetType()
                    .BaseType
                    .GetField("_internalContext", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(internalSet);


                return (DbContext) ReflectionFunctions.GetReflectedMember(internalContext, "Owner");
            }
            catch (Exception e)
            {
                throw new ApplicationException("Unable to retreive db context from DbSet", e);
            }

        }
    }
}