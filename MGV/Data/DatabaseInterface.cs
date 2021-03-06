﻿using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace MGV.Data
{
    public class DatabaseInterface
    {
        #region Private Fields

        private static DatabaseInterface _instance;
        private readonly ILogger _logger;

        #endregion Private Fields

        #region Private Constructors

        private DatabaseInterface(ILogger logger)
        {
            _logger = logger;
        }

        #endregion Private Constructors

        #region Public Methods

        public static DatabaseInterface GetInstance(ILogger logger)
        {
            if (_instance == null)
            {
                _instance = new DatabaseInterface(logger);
            }

            return _instance;
        }

        public IEnumerable<ReturnType> ExecuteCustomQuery<ReturnType>(string query, SqliteConnection connection, params SqliteParameter[] parameters) where ReturnType : new()
        {
            try
            {
                var result = new List<ReturnType>();
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.AddRange(parameters);
                    using (var reader = command.ExecuteReader())
                    {
                        result = GetDataListFromDatabase<ReturnType>(reader).ToList();
                        reader.Close();
                    }
                }
                connection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Executing Custom Query with return type {typeof(ReturnType).ToString()}");
                connection.Close();
                return null;
            }
        }

        public bool ExecuteCustomQuery(string query, SqliteConnection connection, params SqliteParameter[] parameters)
        {
            try
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.AddRange(parameters);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                connection.Close();
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Executing Custom Query");
                connection.Close();
                return false;
            }
        }

        public IEnumerable<T> GetSimpleDataList<T>(string query, string fieldName, SqliteConnection connection, params SqliteParameter[] parameters) where T : struct
        {
            var result = new List<T>();

            try
            {
                connection.Open();
                using (var command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters);
                    var reader = command.ExecuteReader();
                    result = (from IDataRecord rec in reader
                              select (T)rec[fieldName]).ToList();
                    reader.Close();
                }
                connection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Executing Custom Query with simple data");
                connection.Close();
                return null;
            }
        }

        #endregion Public Methods

        #region Private Methods

        private IEnumerable<T> GetDataListFromDatabase<T>(SqliteDataReader reader) where T : new()
        {
            var result = new List<T>();
            while (reader.Read())
            {
                T element = new T();

                for (int inc = 0; inc < reader.FieldCount; inc++)
                {
                    var propertyName = reader.GetName(inc);
                    Type type = element.GetType();
                    PropertyInfo property = type.GetProperty(propertyName);

                    if (property != null)
                    {
                        var normalType = property.PropertyType;
                        if (normalType.IsGenericType && normalType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        {
                            normalType = Nullable.GetUnderlyingType(normalType);
                        }
                        if (reader.GetValue(inc).GetType() != typeof(DBNull))
                        {
                            object value = reader.GetValue(inc);

                            if (property.PropertyType.IsEnum)
                            {
                                value = Enum.ToObject(property.PropertyType, value);
                            }

                            property.SetValue(element,  Convert.ChangeType(value, normalType != property.PropertyType ? normalType : property.PropertyType), null);
                        }
                    }
                }
                result.Add(element);
            }
            return result;
        }

        #endregion Private Methods
    }
}