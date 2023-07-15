using Loxifi.Extensions;
using System.Data.SqlClient;

namespace Loxifi
{
    public class StatelessCommand : IDisposable
    {
        private readonly SqlConnection _connection;

        private readonly bool _requiresClose;

        private bool _disposedValue;

        public StatelessCommand(SqlConnection connection)
        {
            this._connection = connection;
            this._requiresClose = connection.TryOpen();
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Execute(string query, int? commandTimeout = null)
        {
            using SqlCommand command = new(query, this._connection);

            if (commandTimeout.HasValue)
            {
                command.CommandTimeout = commandTimeout.Value;
            }

            command.ExecuteNonQuery();
        }

        public int ExecuteNonQuery(string query, int? commandTimeout = null)
        {
            using SqlCommand command = new(query, this._connection);

            if (commandTimeout.HasValue)
            {
                command.CommandTimeout = commandTimeout.Value;
            }

            return command.ExecuteNonQuery();
        }

        public T ExecuteScalar<T>(string query, int? commandTimeout)
        {
            using SqlCommand command = new(query, this._connection);
            if (commandTimeout.HasValue)
            {
                command.CommandTimeout = commandTimeout.Value;
            }

            object result = command.ExecuteScalar();
            return (T)result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    if (this._requiresClose)
                    {
                        this._connection.TryClose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this._disposedValue = true;
            }
        }
    }
}