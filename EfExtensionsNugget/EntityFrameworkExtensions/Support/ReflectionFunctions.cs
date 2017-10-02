using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reflection;
using System.Text;
using Ralevex.EF.Extensions;
using System.ComponentModel.DataAnnotations.Schema;
namespace Ralevex.EF.Support
{
    internal static class ReflectionFunctions
    {
        internal static IEnumerable<MemberInfo> GetNonKeyColumnsForType(this IReflect sourceType)
            {
            return sourceType.GetAllColumnsForType().Where(c => c.CustomAttributes.All(a => a.AttributeType.Name != "KeyAttribute"));
            }

        internal static IEnumerable<MemberInfo> GetKeyColumnsForType(this IReflect sourceType)
            {
            return sourceType.GetAllColumnsForType().Where(p => p.CustomAttributes.Any(a => a.AttributeType.Name == "KeyAttribute"));
            }

        private static IEnumerable<MemberInfo> GetAllColumnsForType(this IReflect sourceType)
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



        public static IEnumerable<string> GetFieldNames(this Type elementType)
        {
            return GetKeyColumnsForType(elementType)
                    .Select(oneFieldType => (oneFieldType.GetCustomAttribute<ColumnAttribute>()?.Name) 
                                         ?? oneFieldType.Name);
        }
    }
}