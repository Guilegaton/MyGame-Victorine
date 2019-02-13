﻿using MGV.Data;
using MGV.Data.Repositories;
using MGV.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;

namespace MGV.Shared
{
    public abstract class GenericRepository<T> : IRepository<T> where T : BaseEntity, new()
    {
        #region Protected Fields

        protected readonly SqliteConnection _connectionString;
        protected readonly DatabaseInterface _databaseInterface;
        protected readonly ILogger _logger;
        protected readonly string _tableName;

        #endregion Protected Fields

        #region Private Fields

        private bool _isDisposed = false;

        #endregion Private Fields

        #region Public Constructors

        public GenericRepository(SqliteConnection connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
            _databaseInterface = DatabaseInterface.GetInstance(_logger);
            _tableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name;
        }

        #endregion Public Constructors

        #region Public Methods

        public virtual void Create(T item)
        {
            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties().Where(prop => (prop.PropertyType.IsValueType || prop.PropertyType == typeof(string)) && prop.Name != "Id");
            IEnumerable<string> propertysNames = properties.Select(prop => prop.Name);

            string columnNames = $"({propertysNames.Aggregate((cur, next) => $"{cur}, {next}")})";
            string parametersNames = $"Values({propertysNames.Select(prop => $"@{prop}").Aggregate((cur, next) => $"{cur}, {next}")})";

            IEnumerable<SqliteParameter> parameters = properties.Select(prop => new SqliteParameter($"@{prop.Name}", prop.GetValue(item)));

            bool result = _databaseInterface.ExecuteCustomQuery(
                    $"INSERT INTO {_tableName} {columnNames} {parametersNames}",
                    _connectionString,
                    parameters.ToArray()
                );
            if (!result)
            {
                _logger.LogError($"{typeof(T).Name} not created: {item.Name}");
            }

            if(item.Files != null && item.Files.Count() > 0)
            using (var fileObjRepository = new FileObjectRepository(_connectionString, _logger))
            {
                foreach (var file in item.Files)
                {
                    fileObjRepository.AddFileToObject(item, file.Id);
                }
            }
        }

        public virtual void Delete(int id)
        {
            bool result = _databaseInterface.ExecuteCustomQuery(
                    $"Delete From {_tableName} where Id=@id",
                    _connectionString,
                    new SqliteParameter("@id", id)
                );
            if (!result)
            {
                _logger.LogError($"{typeof(T).Name} not deleted: {id}");
            }

            using (var fileObjRepository = new FileObjectRepository(_connectionString, _logger))
            {
                fileObjRepository.DeleteAllFilesFromObject<T>(id);
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public virtual T Get(int id)
        {
            T result;
            try
            {
                result = _databaseInterface.ExecuteCustomQuery<T>(
                        $"Select * From {_tableName} Where {_tableName}.Id = @id",
                        _connectionString,
                        new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = id }
                    ).Single();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return null;
            }

            using (var fileObjRepo = new FileObjectRepository(_connectionString, _logger))
            {
                result.Files = fileObjRepo.GetFiles(result);
            }

            return result;
        }

        public virtual T Get(string name)
        {
            T result;

            try
            {
                result = _databaseInterface.ExecuteCustomQuery<T>(
                        $"Select * From {_tableName} Where {_tableName}.Name = @name",
                        _connectionString,
                        new SqliteParameter { ParameterName = "@name", DbType = DbType.String, Value = name }
                    ).Single();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return null;
            }

            using (var fileObjRepo = new FileObjectRepository(_connectionString, _logger))
            {
                result.Files = fileObjRepo.GetFiles(result);
            }

            return result;
        }

        public virtual IEnumerable<T> GetAll()
        {
            IEnumerable<T> result = Enumerable.Empty<T>();
            result = _databaseInterface.ExecuteCustomQuery<T>(
                    $"Select * From {_tableName}",
                    _connectionString
                );
            if (result == null)
            {
                _logger.LogError($"{_tableName} not find");
            }

            using (var fileObjRepo = new FileObjectRepository(_connectionString, _logger))
            {
                foreach (var item in result)
                {
                    item.Files = fileObjRepo.GetFiles(item);
                }
            }

            return result;
        }

        public virtual void Update(T item)
        {
            T oldItem = Get(item.Id);

            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties().Where(prop => (prop.PropertyType.IsValueType || prop.PropertyType == typeof(string)) && prop.Name != "Id");

            string changeValues = $"Set {properties.Select(prop => $"{prop.Name}=@{prop.Name}").Aggregate((cur, next) => $"{cur}, {next}")}";

            IEnumerable<SqliteParameter> parameters = properties.Select(prop => new SqliteParameter($"{prop.Name}", prop.GetValue(item)));

            bool result = _databaseInterface.ExecuteCustomQuery(
                    $"Update {_tableName} {changeValues} " +
                    "Where Id = @id",
                    _connectionString,
                    parameters.Append(new SqliteParameter { ParameterName = "@id", DbType = DbType.Int32, Value = item.Id }).ToArray()
                );
            if (!result)
            {
                _logger.LogError($"{typeof(T).Name} not updated: {item.Id}, {item.Name}");
            }

            using (var fileObjRepo = new FileObjectRepository(_connectionString, _logger))
            {
                fileObjRepo.UpdateFilesForObject(item, oldItem);
            }
        }

        #endregion Public Methods
    }
}