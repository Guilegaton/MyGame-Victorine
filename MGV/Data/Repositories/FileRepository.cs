using MGV.Models;
using MGV.Share;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGV.Data.Repositories
{
    public class FileRepository : IRepository<File>
    {
        public string _connectionString { get; private set; }

        public FileRepository(string connectionString)
        {
            _connectionString = connectionString;

        }

        public void Create(File item)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {

            }
           
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public File Get(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<File> GetAll()
        {
            throw new NotImplementedException();
        }

        public void Update(File item)
        {
            throw new NotImplementedException();
        }
    }
}
