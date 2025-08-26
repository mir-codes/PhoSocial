using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace PhoSocial.API.Repositories
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    public class SqlDbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _config;
        public SqlDbConnectionFactory(IConfiguration config) { _config = config; }
        public IDbConnection CreateConnection()
        {
            var cs = _config.GetConnectionString("DefaultConnection");
            return new SqlConnection(cs);
        }
    }
}
