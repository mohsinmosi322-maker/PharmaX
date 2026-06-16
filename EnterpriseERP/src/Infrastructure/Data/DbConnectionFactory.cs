using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EnterpriseERP.Infrastructure.Data
{
    /// <summary>
    /// Database connection factory
    /// </summary>
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }

    /// <summary>
    /// Connection factory interface
    /// </summary>
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    /// <summary>
    /// Database settings configuration
    /// </summary>
    public class DatabaseSettings
    {
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public bool IntegratedSecurity { get; set; } = true;
        public string? UserId { get; set; }
        public string? Password { get; set; }
        public int Timeout { get; set; } = 30;

        public string ConnectionString
        {
            get
            {
                if (IntegratedSecurity)
                {
                    return $"Server={Server};Database={Database};Integrated Security=true;TrustServerCertificate=true;";
                }
                else
                {
                    return $"Server={Server};Database={Database};User Id={UserId};Password={Password};TrustServerCertificate=true;";
                }
            }
        }
    }
}
