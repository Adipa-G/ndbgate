using System;
using System.Collections.Generic;
using System.Data;
using DbGate.Exceptions.Common;

namespace DbGate
{
    public class ColumnTypeMapping
    {
        private static readonly Dictionary<Type,ColumnType> NetType2ColumnType = new Dictionary<Type, ColumnType>();
        private static readonly Dictionary<ColumnType,Type> ColumnType2NetType = new Dictionary<ColumnType, Type>();
        private static readonly Dictionary<ColumnType,DbType> ColumnType2DbType = new Dictionary<ColumnType, DbType>();
        private static bool _initialized = false;

        private static void Init()
        {
            if (!_initialized)
            {
                lock (NetType2ColumnType)
                {
                    if (_initialized)
                        return;

                    AddNetTypeAndColumnTypeRelation(typeof(Guid), ColumnType.Guid);
                    AddNetTypeAndColumnTypeRelation(typeof(long),ColumnType.Long);                       
                    AddNetTypeAndColumnTypeRelation(typeof(bool),ColumnType.Boolean);                       
                    AddNetTypeAndColumnTypeRelation(typeof(char),ColumnType.Char);                       
                    AddNetTypeAndColumnTypeRelation(typeof(int),ColumnType.Integer);                       
                    AddNetTypeAndColumnTypeRelation(typeof(DateTime),ColumnType.Date);                       
                    AddNetTypeAndColumnTypeRelation(typeof(double),ColumnType.Double);                       
                    AddNetTypeAndColumnTypeRelation(typeof(float),ColumnType.Float);                       
                    AddNetTypeAndColumnTypeRelation(typeof(DateTime),ColumnType.Timestamp);                       
                    AddNetTypeAndColumnTypeRelation(typeof(string),ColumnType.Varchar);                       
                    AddNetTypeAndColumnTypeRelation(typeof(String),ColumnType.Varchar);                       
                    AddNetTypeAndColumnTypeRelation(typeof(int),ColumnType.Version);
                    
                    AddDbTypeAndColumnTypeRelation(DbType.Guid, ColumnType.Guid);
                    AddDbTypeAndColumnTypeRelation(DbType.Int32, ColumnType.Integer);
                    AddDbTypeAndColumnTypeRelation(DbType.Int64, ColumnType.Long);
                    AddDbTypeAndColumnTypeRelation(DbType.Boolean, ColumnType.Boolean);
                    AddDbTypeAndColumnTypeRelation(DbType.String, ColumnType.Varchar);
                    AddDbTypeAndColumnTypeRelation(DbType.String, ColumnType.Char);
                    AddDbTypeAndColumnTypeRelation(DbType.Date, ColumnType.Date);
                    AddDbTypeAndColumnTypeRelation(DbType.Double, ColumnType.Double);
                    AddDbTypeAndColumnTypeRelation(DbType.Decimal, ColumnType.Float);
                    AddDbTypeAndColumnTypeRelation(DbType.DateTime, ColumnType.Timestamp);
                    AddDbTypeAndColumnTypeRelation(DbType.Int32, ColumnType.Version);
                }
                _initialized = true;
            }
        }

        private static void AddNetTypeAndColumnTypeRelation(Type type,ColumnType columnType)
        {
            if (!NetType2ColumnType.ContainsKey(type))
                NetType2ColumnType.Add(type,columnType);
            if (!ColumnType2NetType.ContainsKey(columnType))
                ColumnType2NetType.Add(columnType,type);
        }

        private static void AddDbTypeAndColumnTypeRelation(DbType dbType,ColumnType columnType)
        {
            if (!ColumnType2DbType.ContainsKey(columnType))
                ColumnType2DbType.Add(columnType, dbType);
        }

        public static ColumnType GetColumnType(Type type)
        {
            Init();

            if (!NetType2ColumnType.ContainsKey(type))
                throw new InvalidDataTypeException(string.Format("Unable to find column type for {0}", type.FullName));

            return NetType2ColumnType[type];
        }

        public static Type GetNetType(ColumnType columnType)
        {
            Init();
            if (!ColumnType2NetType.ContainsKey(columnType))
                throw new InvalidDataTypeException(string.Format("Unable to find type for column type {0}", columnType));


            return ColumnType2NetType[columnType];
        }

        public static DbType GetSqlType(ColumnType columnType)
        {
            Init();
            if (!ColumnType2DbType.ContainsKey(columnType))
                throw new InvalidDataTypeException(string.Format("Unable to find db type for column type {0}", columnType));

            return ColumnType2DbType[columnType];
        }
    }
}