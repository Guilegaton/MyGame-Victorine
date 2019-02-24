using Dapper;
using MGV.Data;
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

        protected IDbConnection _connectionString { get { return _transaction.Connection; } }
        protected readonly ILogger _logger;
        protected readonly IDbTransaction _transaction;
        protected readonly string _tableName;

        #endregion Protected Fields

        #region Private Fields

        private bool _isDisposed = false;

        #endregion Private Fields

        #region Public Constructors

        public GenericRepository(IDbTransaction transaction, ILogger logger)
        {
            _logger = logger;
            _tableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name;
            _transaction = transaction;
        }

        #endregion Public Constructors

        #region Public Methods

        public virtual void Create(T item)
        {
            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties().Where(prop => (prop.PropertyType.IsValueType || prop.PropertyType == typeof(string)) && prop.Name != "Id");
            IEnumerable<string> propertysNames = properties.Select(prop => prop.Name);

            string columnNames = $"({propertysNames.Aggregate((cur, next) => $"{cur}, {next}")})";
            string parametersNames = $"Values({propertysNames.Select(prop => $"@{prop}").Aggregate((cur, next) => $"{cur}, {next}")})";

            var sql = $"INSERT INTO {_tableName} {columnNames} {parametersNames}";

            DynamicParameters dynamicParameters = new DynamicParameters();
            properties.Select(prop =>
            {
                dynamicParameters.Add($"{prop.Name}", prop.GetValue(item));
                return prop;
            }).Count();

            int result = _connectionString.Execute(sql, dynamicParameters, _transaction);

            if (result <= 0)
            {
                _logger.LogError($"{typeof(T).Name} not created: {item.Name}");
            }

            if (item.Files != null && item.Files.Count() > 0)
                using (var fileObjRepository = new FileObjectRepository(_transaction, _logger))
                {
                    foreach (var file in item.Files)
                    {
                        fileObjRepository.AddFileToObject(item, file.FileId);
                    }
                }
        }

        public virtual void Delete(int id)
        {
            var sql = $"Delete From {_tableName} where Id=@id";

            int result = _connectionString.Execute(sql, new { id }, _transaction);
            if (result <= 0)
            {
                _logger.LogError($"{typeof(T).Name} not deleted: {id}");
            }

            using (var fileObjRepository = new FileObjectRepository(_transaction, _logger))
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
            var sql = $"Select {_tableName}.*, Files.* From {_tableName} " +
                $"INNER JOIN Files_Objects ON {_tableName}.Id = Files_Objects.ObjectId " +
                $"INNER JOIN Files ON Files.FileId = Files_Objects.FileId " +
                $"WHERE Files_Objects.ObjectType = @objectType AND {_tableName}.Id = @id";
            try
            {
                result = _connectionString.Query<T, File, T>(sql,
                                                             (element, file) =>
                                                             {
                                                                 element.Files = element.Files.Append(file);
                                                                 return element;
                                                             }
                                                             , param: new { objectType = (int)ObjectTypeProvider.For(typeof(T)), id },
                                                             splitOn: "Id,FileId")
                                          .Single();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return null;
            }

            using (var fileObjRepo = new FileObjectRepository(_transaction, _logger))
            {
                result.Files = fileObjRepo.GetAllFilesForObject(result);
            }

            return result;
        }

        public virtual T Get(string name)
        {
            T result;
            var sql = $"Select {_tableName}.*, Files.* From {_tableName} " +
                $"INNER JOIN Files_Objects ON {_tableName}.Id = Files_Objects.ObjectId" +
                $"INNER JOIN Files ON Files.FileId = Files_Objects.FileId" +
                $"WHERE Files_Objects.ObjectType = @objectType AND {_tableName}.Name = @name";

            try
            {
                result = _connectionString.Query<T, File, T>(sql,
                                                             (element, file) =>
                                                             {
                                                                 element.Files = element.Files.Append(file);
                                                                 return element;
                                                             }
                                                             , new { objectType = ObjectTypeProvider.For(typeof(T)), name })
                                          .Single();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return null;
            }

            using (var fileObjRepo = new FileObjectRepository(_transaction, _logger))
            {
                result.Files = fileObjRepo.GetAllFilesForObject(result);
            }

            return result;
        }

        public virtual IEnumerable<T> GetAll()
        {
            IEnumerable<T> result = Enumerable.Empty<T>();
            var sql = $"Select {_tableName}.*, Files.* From {_tableName} " +
                $"INNER JOIN Files_Objects ON {_tableName}.Id = Files_Objects.ObjectId" +
                $"INNER JOIN Files ON Files.FileId = Files_Objects.FileId" +
                $"WHERE Files_Objects.ObjectType = @objectType";

            result = _connectionString.Query<T, File, T>(sql,
                                                         (elemet, file) =>
                                                         {
                                                             elemet.Files = elemet.Files.Append(file);
                                                             return elemet;
                                                         }
                                                         , new { objectType = ObjectTypeProvider.For(typeof(T)) });
            if (result == null)
            {
                _logger.LogError($"{_tableName} not find");
            }

            return result;
        }

        public virtual void Update(T item)
        {
            T oldItem = Get(item.Id);

            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties().Where(prop => (prop.PropertyType.IsValueType || prop.PropertyType == typeof(string)) && prop.Name != "Id");

            string changeValues = $"Set {properties.Select(prop => $"{prop.Name}=@{prop.Name}").Aggregate((cur, next) => $"{cur}, {next}")}";

            IEnumerable<SqliteParameter> parameters = properties.Select(prop => new SqliteParameter($"{prop.Name}", prop.GetValue(item)));

            var sql = $"Update {_tableName} {changeValues} " +
                    "Where Id = @id";

            var dynamicParameters = new DynamicParameters();
            properties.Select(prop =>
            {
                dynamicParameters.Add($"{prop.Name}", prop.GetValue(item));
                return prop;
            }).Count();

            int result = _connectionString.Execute(sql, dynamicParameters, _transaction);
            if (result <= 0)
            {
                _logger.LogError($"{typeof(T).Name} not updated: {item.Id}, {item.Name}");
            }

            using (var fileObjRepo = new FileObjectRepository(_transaction, _logger))
            {
                fileObjRepo.UpdateFilesForObject(item, oldItem);
            }
        }

        #endregion Public Methods
    }
}