using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;

namespace Ralevex.EF
{
    public static class TypeMapper
        {
        private static readonly Dictionary<Type, DbType> TypeMap = new Dictionary<Type, DbType>
                {
                    [typeof(byte)]              = DbType.Byte,
                    [typeof(sbyte)]             = DbType.SByte,
                    [typeof(short)]             = DbType.Int16,
                    [typeof(ushort)]            = DbType.UInt16,
                    [typeof(int)]               = DbType.Int32,
                    [typeof(uint)]              = DbType.UInt32,
                    [typeof(long)]              = DbType.Int64,
                    [typeof(ulong)]             = DbType.UInt64,
                    [typeof(float)]             = DbType.Single,
                    [typeof(double)]            = DbType.Double,
                    [typeof(decimal)]           = DbType.Decimal,
                    [typeof(bool)]              = DbType.Boolean,
                    [typeof(string)]            = DbType.String,
                    [typeof(char)]              = DbType.StringFixedLength,
                    [typeof(Guid)]              = DbType.Guid,
                    [typeof(DateTime)]          = DbType.DateTime,
                    [typeof(DateTimeOffset)]    = DbType.DateTimeOffset,
                    [typeof(byte[])]            = DbType.Binary,
                    [typeof(byte?)]             = DbType.Byte,
                    [typeof(sbyte?)]            = DbType.SByte,
                    [typeof(short?)]            = DbType.Int16,
                    [typeof(ushort?)]           = DbType.UInt16,
                    [typeof(int?)]              = DbType.Int32,
                    [typeof(uint?)]             = DbType.UInt32,
                    [typeof(long?)]             = DbType.Int64,
                    [typeof(ulong?)]            = DbType.UInt64,
                    [typeof(float?)]            = DbType.Single,
                    [typeof(double?)]           = DbType.Double,
                    [typeof(decimal?)]          = DbType.Decimal,
                    [typeof(bool?)]             = DbType.Boolean,
                    [typeof(char?)]             = DbType.StringFixedLength,
                    [typeof(Guid?)]             = DbType.Guid,
                    [typeof(DateTime?)]         = DbType.DateTime,
                    [typeof(DateTimeOffset?)]   = DbType.DateTimeOffset,
                };

        internal static string DeclareVariable(ObjectParameter parameter)
            {
            var t = parameter.ParameterType.ToDbType();
            string declarePreffix = $"DECLARE @{parameter.Name}";
            switch (t)
                {
                case DbType.Byte:
                case DbType.SByte:
                case DbType.Int16:
                case DbType.UInt16:
                case DbType.Int32:
                case DbType.UInt32:
                    return $"{declarePreffix} INT = {parameter.Value ?? "NULL"};";

                case DbType.Int64:
                case DbType.UInt64:
                    return $"{declarePreffix}  BIGINT = {parameter.Value ?? "NULL"};";

                case DbType.Single:
                case DbType.Double:
                    return $"{declarePreffix}  FLOAT = {parameter.Value ?? "NULL"};";

                case DbType.Decimal:
                    var number = (decimal?)parameter.Value;
                    if (number.HasValue)
                        {
                        var precision = (decimal.GetBits(number.Value)[3] >> 16) & 0x000000FF;
                        return $"{declarePreffix}  DECIMAL(38,{precision}) = {number};";
                        }
                    else
                        {
                        return $"{declarePreffix}  DECIMAL = NULL;";
                        }

                case DbType.Boolean:
                        int i = (bool)parameter.Value ? 1 : 0;
                        return $"{declarePreffix}  BIT = {i};";

                    case DbType.String:
                    case DbType.StringFixedLength:
                        string s = (string)parameter.Value;
                        return s == null ? $"{declarePreffix}  NVARCHAR(1) = NULL;" 
                                         : $"{declarePreffix}  NVARCHAR({s.Length}) = '{s}';";

                case DbType.Guid:
                    var g = (Guid?)parameter.Value;
                    return !g.HasValue ? $"{declarePreffix}  UNIQUEIDENTIFIER = NULL;"
                                       : $"{declarePreffix}  UNIQUEIDENTIFIER = '{g}';";

                case DbType.DateTime:
                case DbType.DateTimeOffset:


                case DbType.Binary:
                        break;
                }
            }
        public static DbType ToDbType(this Type type)
            {
            return TypeMap.ContainsKey(type) ? TypeMap[type] : DbType.Object;
            }
        }
    }
