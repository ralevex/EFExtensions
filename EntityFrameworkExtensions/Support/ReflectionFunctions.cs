using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reflection;
using System.Text;
using Ralevex.EF.Extensions;

namespace Ralevex.EF.Support
{
    internal static class ReflectionFunctions
    {
        internal static IEnumerable<MemberInfo> GetNonKeyColumnsForType(IReflect sourceType)
            {
            return GetColumnsForType(sourceType).Where(c => c.CustomAttributes.All(a => a.AttributeType.Name != "KeyAttribute"));
            }

        internal static IEnumerable<MemberInfo> GetKeyColumnsForType(IReflect sourceType)
            {
            return GetColumnsForType(sourceType).Where(p => p.CustomAttributes.Any(a => a.AttributeType.Name == "KeyAttribute"));
            }

        private static IEnumerable<MemberInfo> GetColumnsForType(IReflect sourceType)
        {
            return sourceType.GetMembers(BindingFlags.GetField |
                                         BindingFlags.GetProperty |
                                         BindingFlags.Instance |
                                         BindingFlags.Public)
                .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                .Where(p => p.CustomAttributes.All(a => a.AttributeType.Name != "NotMappedAttribute"));
        }

        internal static object GetReflectedMember(object obj, string name)
            {
            Type parentType = obj.GetType();

            MemberInfo member = parentType.GetMembers(BindingFlags.GetField |
                                                      BindingFlags.GetProperty |
                                                      BindingFlags.Instance |
                                                      BindingFlags.NonPublic |
                                                      BindingFlags.Public)
                .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                .FirstOrDefault(o => o.Name == name);

            if (member == null)
                return null;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (member.MemberType)
                {
                case MemberTypes.Field:
                    return ((FieldInfo)member).GetValue(obj);
                case MemberTypes.Property:
                    return ((PropertyInfo)member).GetValue(obj, null);
                    default:
                    return null;
                }

            }

        /// <summary>
        /// For an Entity Framework IQueryable, returns the SQL and Parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static string ToTraceString<T>(this IQueryable<T> query)
            {
            ObjectQuery<T> objectQuery = query.GetObjectQuery();

            var traceString = new StringBuilder();

            traceString.AppendLine(objectQuery.ToTraceString());
            traceString.AppendLine();

            foreach (ObjectParameter parameter in objectQuery.Parameters)
                {
                traceString.AppendLine(parameter.Name + " [" + parameter.ParameterType.FullName + "] = " + parameter.Value);
                }

            return traceString.ToString();
            }

        }
}