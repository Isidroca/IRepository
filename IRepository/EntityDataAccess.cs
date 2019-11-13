using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Linq;
using System.Dynamic;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.Globalization;

namespace EntityRepository {

    public class EntityDataAccess : IDisposable {

        public string ConnectionString { get; set; }
        public string CommandText { get; set; }
        public ICommandType CommandType { get; set; }
        public string InternalError = string.Empty;
        private protected string SqlText { get; private set; }
        public Exception Exception { get; private set; }
        public int CommandTimeOut = 30;
        protected int _durationTime = 0;
        public int QueryDurationTime {
            get {
                return _durationTime;
            }
        }
        public bool StatisticsEnabled { get; set; } = false;
        public bool IsValidModel { get; private set; } = true;
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public CancellationToken CancellationToken { get; set; }
        internal List<string> _infoMessage;
        public List<string> InfoMessage {
            get {
                return _infoMessage;
            }
        }
        protected internal bool _ifInserted = false;
        protected internal Type _idenType;
        private List<SqlParameter> SqlParameters = new List<SqlParameter>();
        ConcurrentDictionary<Type, Delegate> ExpressionCache = new ConcurrentDictionary<Type, Delegate>();
        ConcurrentDictionary<Type, string> QueryCache = new ConcurrentDictionary<Type, string>();
        SqlConnection SQLConnection;
        SqlTransaction SQLTransaction;

        public enum ICommandType : byte {

            Text = 1,
            StorePrecedure = 2
        }

        /// <summary>
        /// Create new intance to EntityDataAccess class
        /// </summary>
        public EntityDataAccess() { }

        /// <summary>
        /// Create new intance to EntityDataAccess class
        /// </summary>
        /// <param name="ConnectionString">Gets or sets the SqlConnection used by this instance of the SqlCommand, The default value is null.</param>
        public EntityDataAccess(string ConnectionString) {

            this.ConnectionString = ConnectionString;
       
        }

        /// <summary>
        /// Create new intance to EntityDataAccess class
        /// </summary>
        /// <param name="ConnectionString">Gets or sets the SqlConnection used by this instance of the SqlCommand, The default value is null.</param>
        /// <param name="CommandText">The Transact-SQL statement or stored procedure to execute.</param>
        public EntityDataAccess(string ConnectionString, string CommandText) {

            this.ConnectionString = ConnectionString;
            this.CommandText = CommandText;
        }

        /// <summary>
        /// Create new intance to EntityDataAccess class
        /// </summary>
        /// <param name="ConnectionString">Gets or sets the SqlConnection used by this instance of the SqlCommand, The default value is null.</param>
        /// <param name="CommandText">The Transact-SQL statement or stored procedure to execute.</param>
        /// <param name="CommandType">Gets or sets a value indicating how the CommandText property is to be interpreted, query or store procedure, text query by default</param>
        public EntityDataAccess(string ConnectionString, string CommandText, ICommandType CommandType) {

            this.ConnectionString = ConnectionString;
            this.CommandText = CommandText;
            this.CommandType = CommandType;
        }

        /// <summary>
        /// Create new intance to EntityDataAccess class
        /// </summary>
        /// <param name="ConnectionString">Gets or sets the SqlConnection used by this instance of the SqlCommand, The default value is null.</param>
        /// <param name="CommandText">The Transact-SQL statement or stored procedure to execute.</param>
        /// <param name="CommandType">Gets or sets a value indicating how the CommandText property is to be interpreted, query or store procedure, text query by default</param>
        /// <param name="CommandTimeOut">Wait time by execution query, 30 second by default</param>
        public EntityDataAccess(string ConnectionString, string CommandText, ICommandType CommandType, int CommandTimeOut) {

            this.ConnectionString = ConnectionString;
            this.CommandText = CommandText;
            this.CommandType = CommandType;
            this.CommandTimeOut = CommandTimeOut;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_commType"></param>
        /// <returns></returns>
        private CommandType SetCommandType(ICommandType _commType) {

            return _commType == ICommandType.StorePrecedure ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text;
        }

        /// <summary>
        /// Attempt to commit the transaction.
        /// </summary>
        public void Commit() {
            if (SQLTransaction != null) {
                SQLTransaction.Commit();
                DisponseConn();
            }
        }

        /// <summary>
        ///  Attempt to roll back the transaction.
        /// </summary>
        public void Rollback() {

            if (SQLTransaction != null) {
                SQLTransaction.Rollback();
                DisponseConn();
            }
        }

        /// <summary>
        /// Get one DataTable
        /// </summary>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public DataTable DataTable() {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            DataTable _dt = new DataTable();

            using (SqlConnection _strConn = new SqlConnection(ConnectionString)) {
                using (SqlCommand _sqlComand = new SqlCommand(CommandText, _strConn)) {

                    _sqlComand.CommandType = this.SetCommandType(CommandType);
                    _sqlComand.CommandTimeout = this.CommandTimeOut;

                    try {
                        if (SqlParameters.Count > 0) {
                            _sqlComand.Parameters.AddRange(SqlParameters.ToArray());
                            SqlParameters.Clear();
                        }
                        _strConn.Open();
                        _sqlComand.ExecuteNonQuery();
                        SqlDataAdapter _da = new SqlDataAdapter(_sqlComand);
                        _da.Fill(_dt);
                    } catch (SqlException ex) {
                        Exception = ex;
                        InternalError += string.Format("Error ({0}): {1}", ex.Number, ex.Message);
                        return null;
                    } catch (Exception ex) {
                        Exception = ex;
                        InternalError += ex.Message.ToString();
                    }
                }
            }
            return _dt;
        }

        /// <summary>
        /// Get one DataSet
        /// </summary>
        /// <returns></returns>
        public DataSet DataSet() {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            DataSet _dt = new DataSet();

            using (SqlConnection _strConn = new SqlConnection(ConnectionString)) {
                using (SqlCommand _sqlComand = new SqlCommand(CommandText, _strConn)) {

                    _sqlComand.CommandType = this.SetCommandType(CommandType);
                    _sqlComand.CommandTimeout = this.CommandTimeOut;

                    try {
                        if (SqlParameters.Count > 0) {
                            _sqlComand.Parameters.AddRange(SqlParameters.ToArray());
                            SqlParameters.Clear();
                        }
                        _strConn.Open();
                        _sqlComand.ExecuteNonQuery();
                        SqlDataAdapter _da = new SqlDataAdapter(_sqlComand);
                        _da.Fill(_dt);
                    } catch (SqlException ex) {
                        Exception = ex;
                        InternalError += string.Format("Error ({0}): {1}", ex.Number, ex.Message);
                        return null;
                    } catch (Exception ex) {
                        Exception = ex;
                        InternalError += ex.Message.ToString();
                    }
                }
            }
            return _dt;
        }

        /// <summary>
        /// Execute transactions on the database and return first column value
        /// </summary>
        /// <returns>Object</returns>
        public object ExecuteScalar() {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            using (SqlConnection _strConn = new SqlConnection(ConnectionString)) {
                using (SqlCommand _sqlComand = new SqlCommand(CommandText, _strConn)) {

                    _sqlComand.CommandType = this.SetCommandType(CommandType);
                    _sqlComand.CommandTimeout = this.CommandTimeOut;

                    try {
                        if (SqlParameters.Count > 0) {
                            _sqlComand.Parameters.AddRange(SqlParameters.ToArray());
                            SqlParameters.Clear();
                        }
                        _strConn.Open();
                        return _sqlComand.ExecuteScalar();

                    } catch (SqlException ex) {
                        Exception = ex;
                        InternalError += string.Format("Error ({0}): {1}", ex.Number, ex.Message);
                        return 0;
                    } catch (Exception ex) {
                        Exception = ex;
                        InternalError += ex.Message.ToString();
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// Execute transactions on the database and return first column value
        /// </summary>
        /// <returns></returns>
        public async Task<object> ExecuteScalarAsync() {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            using (SqlConnection _strConn = new SqlConnection(ConnectionString)) {
                using (SqlCommand _sqlComand = new SqlCommand(CommandText, _strConn)) {

                    _sqlComand.CommandType = this.SetCommandType(CommandType);
                    _sqlComand.CommandTimeout = this.CommandTimeOut;

                    try {
                        if (SqlParameters.Count > 0) {
                            _sqlComand.Parameters.AddRange(SqlParameters.ToArray());
                            SqlParameters.Clear();
                        }
                        _strConn.Open();
                        return await _sqlComand.ExecuteScalarAsync().ConfigureAwait(false);

                    } catch (SqlException ex) {
                        Exception = ex;
                        InternalError += string.Format("Error ({0}): {1}", ex.Number, ex.Message);
                        return 0;
                    } catch (Exception ex) {
                        Exception = ex;
                        InternalError += ex.Message.ToString();
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// Execute transactions on the database and return the number of rows effected.
        /// </summary>
        /// <returns>int, return the number of rows effected</returns>
        public object ExecuteNonQuery() {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            using (SqlConnection _strConn = new SqlConnection(ConnectionString)) {
                using (SqlCommand _sqlComand = new SqlCommand(this.CommandText, _strConn)) {

                    _sqlComand.CommandType = SetCommandType(CommandType);
                    _sqlComand.CommandTimeout = this.CommandTimeOut;

                    try {
                        if (SqlParameters != null) {
                            _sqlComand.Parameters.AddRange(SqlParameters.ToArray());
                            SqlParameters.Clear();
                        }
                        _strConn.Open();
                        if (_ifInserted) { 
                            if (_idenType == typeof(short)) {
                                return (short)_sqlComand.ExecuteScalar();
                            }
                            else if (_idenType == typeof(int)) {
                                return (int)_sqlComand.ExecuteScalar();
                            }
                            else if (_idenType == typeof(long)) {
                                return (long)_sqlComand.ExecuteScalar();
                            }
                       }
                       return _sqlComand.ExecuteNonQuery();

                    } catch (SqlException ex) {
                        Exception = ex;
                        InternalError += string.Format("Error ({0}): {1}", ex.Number, ex.Message);
                        return 0;
                    } catch (Exception ex) {
                        Exception = ex;
                        InternalError += ex.Message.ToString();
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<object> ExecuteNonQueryAsync() {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            using (SqlConnection _strConn = new SqlConnection(ConnectionString)) {
                using (SqlCommand _sqlComand = new SqlCommand(this.CommandText, _strConn)) {

                    _sqlComand.CommandType = SetCommandType(CommandType);
                    _sqlComand.CommandTimeout = this.CommandTimeOut;

                    try {
                        if (SqlParameters != null) {
                            _sqlComand.Parameters.AddRange(SqlParameters.ToArray());
                            SqlParameters.Clear();
                        }
                        _strConn.Open();
                        if (_ifInserted)
                            if (_idenType == typeof(short)) {
                                return Convert.ToInt16(await _sqlComand.ExecuteScalarAsync().ConfigureAwait(false));
                            }
                            else if (_idenType == typeof(int)) {
                                return Convert.ToInt32(await _sqlComand.ExecuteScalarAsync().ConfigureAwait(false));
                            }
                            else if (_idenType == typeof(long)) {
                                return Convert.ToInt64(await _sqlComand.ExecuteScalarAsync().ConfigureAwait(false));
                            }
                            return await _sqlComand.ExecuteNonQueryAsync().ConfigureAwait(false);

                    } catch (SqlException ex) {
                        Exception = ex;
                        InternalError += string.Format("Error ({0}): {1}", ex.Number, ex.Message);
                        return 0;
                    } catch (Exception ex) {
                        Exception = ex;
                        InternalError += ex.Message.ToString();
                        return 0;
                    }
                }
            }
        }

        public object ToExecute(bool UseTransaction) {

            if (!IsValidModel) return 0;

            if (UseTransaction) {
                return ExecuteNonQuery(true);
            }
            else {
                return ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UseTransaction"></param>
        /// <returns></returns>
        public async Task<object> ToExecuteAsync(bool UseTransaction) {

            if (!IsValidModel) return 0;

            if (UseTransaction) {
                return await ExecuteNonQueryAsync(UseTransaction);
            }
            else {
                return await ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UseTransaction"></param>
        /// <returns></returns>
        public object ExecuteNonQuery(bool UseTransaction) {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            if (SQLConnection == null) {
                SQLConnection = new SqlConnection();
                SQLConnection.ConnectionString = this.ConnectionString;
                SQLConnection.Open();
                SQLTransaction = SQLConnection.BeginTransaction();
            }

            if (SQLConnection != null & SQLConnection.State == ConnectionState.Closed) {
                SQLConnection.Open();

            }
            SqlCommand _sqlComand = SQLConnection.CreateCommand();

            try {

                _sqlComand.CommandType = this.SetCommandType(CommandType);
                _sqlComand.CommandText = this.CommandText;
                _sqlComand.CommandTimeout = this.CommandTimeOut;

                if (SqlParameters.Count > 0) {
                    _sqlComand.Parameters.AddRange(SqlParameters.ToArray());
                    SqlParameters.Clear();
                }
                _sqlComand.Connection = SQLConnection;
                _sqlComand.Transaction = SQLTransaction;
                if (_ifInserted)
                    if (_idenType == typeof(short)) {
                        return Convert.ToInt16(_sqlComand.ExecuteScalar());
                    } else if (_idenType == typeof(int)) {
                        return Convert.ToInt32(_sqlComand.ExecuteScalar());
                    } else if (_idenType == typeof(long)) {
                        return Convert.ToInt64(_sqlComand.ExecuteScalar());
                    }
                return _sqlComand.ExecuteNonQuery();
            } catch (SqlException ex) {
                Exception = ex;
                DisponseConn();
                InternalError += string.Format("Error ({0}): {1}", ex.Number, ex.Message);
                return 0;
            } catch (Exception ex) {
                Exception = ex;
                DisponseConn();
                InternalError += ex.Message.ToString();
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="UseTransaction"></param>
        /// <returns></returns>
        public async Task<object> ExecuteNonQueryAsync(bool UseTransaction) {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            if (SQLConnection == null) {
                SQLConnection = new SqlConnection();
                SQLConnection.ConnectionString = this.ConnectionString;
                await SQLConnection.OpenAsync();
                SQLTransaction = SQLConnection.BeginTransaction();
            }

            if (SQLConnection != null & SQLConnection.State == ConnectionState.Closed) {
                await SQLConnection.OpenAsync();

            }
            SqlCommand _sqlComand = SQLConnection.CreateCommand();

            try {

                _sqlComand.CommandType = this.SetCommandType(CommandType);
                _sqlComand.CommandText = this.CommandText;
                _sqlComand.CommandTimeout = this.CommandTimeOut;

                if (SqlParameters.Count > 0) {
                    _sqlComand.Parameters.AddRange(SqlParameters.ToArray());
                    SqlParameters.Clear();
                }
                _sqlComand.Connection = SQLConnection;
                _sqlComand.Transaction = SQLTransaction;

                if (_ifInserted)
                    if (_idenType == typeof(short))
                    {
                        return Convert.ToInt16(await _sqlComand.ExecuteScalarAsync().ConfigureAwait(false));
                    }
                    else if (_idenType == typeof(int))
                    {
                        return Convert.ToInt32(await _sqlComand.ExecuteScalarAsync().ConfigureAwait(false));
                    }
                    else if (_idenType == typeof(long))
                    {
                        return Convert.ToInt64(await _sqlComand.ExecuteScalarAsync().ConfigureAwait(false));
                    }
                return await _sqlComand.ExecuteNonQueryAsync().ConfigureAwait(false);
            } catch (SqlException ex) {
                Exception = ex;
                DisponseConn();
                InternalError += string.Format("Error ({0}): {1}", ex.Number, ex.Message);
                return 0;
            } catch (Exception ex) {
                Exception = ex;
                DisponseConn();
                InternalError += ex.Message.ToString();
                return 0;
            }
        }

        private void DisponseConn() {

            SQLConnection.Close();
            SQLConnection.Dispose();
            if (SQLTransaction != null) {
                SQLTransaction.Dispose();
            }
            SQLTransaction = null;

        }

        /// <summary>
        /// Execute transactions on the database and return a specified value, implementation Ej: @ID int = null output
        /// </summary>
        /// <param name="ReturnVariableName">Sql Output variable name, ej: @ID</param>
        /// <param name="Parameters">Delimited coma store procedure parameter, eje: val1, val2, ext</param>
        /// <returns></returns>
        public object ExecuteNonQuery(string ReturnVariableName, params object[] Parameters) {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            using (SqlConnection _strConn = new SqlConnection(ConnectionString)) {

                using (SqlCommand _sqlComand = new SqlCommand(this.CommandText, _strConn)) {

                    _sqlComand.CommandType = System.Data.CommandType.StoredProcedure;
                    _sqlComand.CommandTimeout = this.CommandTimeOut;
                    _strConn.Open();

                    SqlCommandBuilder.DeriveParameters(_sqlComand);

                    int _index = 0;
                    try {

                        foreach (SqlParameter _paramms in _sqlComand.Parameters) {
                            if (_paramms.Direction == ParameterDirection.Input || _paramms.Direction == ParameterDirection.Output) {
                                _paramms.Value = Parameters[_index];
                                _index += 1;
                            }
                        }
                        _sqlComand.ExecuteNonQuery();
                        return _sqlComand.Parameters[ReturnVariableName].Value;
                    } catch (SqlException ex) {
                        Exception = ex;
                        InternalError += string.Format("Error ({0}): {1}", ex.Number, ex.Message);
                        return 0;
                    } catch (Exception ex) {
                        Exception = ex;
                        InternalError += ex.Message.ToString();
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// Execute transactions on the database and return a specified value, implementation Ej: @ID int = null output
        /// </summary>
        /// <param name="ReturnVariableName">Sql Output variable name, ej: @ID</param>
        /// <param name="Parameters">Delimited coma store procedure parameter, eje: val1, val2, ext</param>
        /// <returns></returns>
        public async Task<object> ExecuteNonQueryAsync(string ReturnVariableName, params object[] Parameters) {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            using (SqlConnection _strConn = new SqlConnection(ConnectionString)) {

                using (SqlCommand _sqlComand = new SqlCommand(this.CommandText, _strConn)) {

                    _sqlComand.CommandType = System.Data.CommandType.StoredProcedure;
                    _sqlComand.CommandTimeout = this.CommandTimeOut;
                    _strConn.Open();

                    SqlCommandBuilder.DeriveParameters(_sqlComand);

                    int _index = 0;
                    try {

                        foreach (SqlParameter _paramms in _sqlComand.Parameters) {
                            if (_paramms.Direction == ParameterDirection.Input || _paramms.Direction == ParameterDirection.Output) {
                                _paramms.Value = Parameters[_index];
                                _index += 1;
                            }
                        }
                        await _sqlComand.ExecuteNonQueryAsync().ConfigureAwait(false);
                        return _sqlComand.Parameters[ReturnVariableName].Value;
                    } catch (SqlException ex) {
                        Exception = ex;
                        InternalError += string.Format("Error ({0}): {1}", ex.Number, ex.Message);
                        return 0;
                    } catch (Exception ex) {
                        Exception = ex;
                        InternalError += ex.Message.ToString();
                        return 0;
                    }
                }
            }
        }

        private bool CheckObjectIsArray(object Value) {

            bool isArray = false;

            Type[] types = {
                             typeof(byte[]),
                             typeof(short[]),
                             typeof(int[]),
                             typeof(string[]),
                             typeof(char[]),
                             typeof(ArrayList),
                             typeof(Array),
                             typeof(List<byte>),
                             typeof(List<short>),
                             typeof(List<int>),
                             typeof(List<string>),
                             typeof(List<char>),
                             typeof(IEnumerable<byte>),
                             typeof(IEnumerable<short>),
                             typeof(IEnumerable<int>),
                             typeof(IEnumerable<string>),
                             typeof(IEnumerable<char>)
                           };
            
            foreach (var t in types) {
                if (!(Value is null)) {
                    if (Value.GetType() == t) {
                        isArray = true;
                    }
                }
            }
            return isArray;
        }

        /// <summary>
        /// The parameters of the Transact-SQL statement or stored procedure. The default is an empty collection.
        /// </summary>
        /// <param name="Key">Set Parameters name</param>
        /// <param name="Value">Set Parameters value</param>
        public void AddParameter(string Key, object Value) {

           
            SqlParameter lp = new SqlParameter();
            lp.ParameterName = Key;
            lp.Value = Value == null ? DBNull.Value : Value;
            if (!SqlParameters.Where(x => x.ParameterName == Key).Any()) {
                SqlParameters.Add(lp);
            }
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="Values"></param>
        public void AddParameterList<T>(string Key, List<T> Values) {

            var parameterNames = new List<string>();
            var paramNbr = 0;

            foreach(var item in Values) {

                var paramName = string.Format(Key.StartsWith("@") ? "{0}{1}" : "@{0}{1}", Key, paramNbr++);
                parameterNames.Add(paramName);
                AddParameter(paramName, item);
            }

            CommandText = CommandText?.Replace(Key, string.Join(",", parameterNames));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SQL"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string SQL, params object[] Parameters) where T : new() {

            CommandText = SQL;

            this.DymanicParams(Parameters);
            return ExecuteReader<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SQL"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(string SQL, params object[] Parameters) where T : new() {

            CommandText = SQL;

            this.DymanicParams(Parameters);
            return await ExecuteReaderAsync<T>();
        }

        /// <summary>
        /// Map one type of SQL query or store procedure to class model in automated and easy way.
        /// EntityDataAccess relies completely on the properties from the model or class.
        /// </summary>
        /// <typeparam name="T">Entity to map</typeparam>
        /// <param name="ConnectionString">Gets or sets the SqlConnection used by this instance of the SqlCommand, The default value is null.</param>
        /// <param name="SQL">The Transact-SQL statement or stored procedure to execute.</param>
        /// <param name="CommandType">Gets or sets a value indicating how the CommandText property is to be interpreted, query or store procedure, text query by default</param>
        /// <param name="Timeout">The time in seconds to wait for the command to execute. The default is 30 seconds. A value of 0 indicates no limit (an attempt to execute a command will wait indefinitely).</param>
        /// <param name="Parameters">The parameters of the Transact-SQL statement or stored procedure. The default is an empty collection. Eje: new { p1 = 1 }, on multiple params Eje: new { p1 = 1, p2 = 5 }</param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string ConnectionString, string SQL, params object[] Parameters) where T : new() {

            this.ConnectionString = ConnectionString;
            CommandText = SQL;

            this.DymanicParams(Parameters);
            return ExecuteReader<T>();
        }

        /// <summary>
        /// Map one type of SQL query or store procedure to class model in automated and easy way.
        /// EntityDataAccess relies completely on the properties from the model or class.
        /// </summary>
        /// <typeparam name="T">Entity to map</typeparam>
        /// <param name="ConnectionString">Gets or sets the SqlConnection used by this instance of the SqlCommand, The default value is null.</param>
        /// <param name="SQL">The Transact-SQL statement or stored procedure to execute.</param>
        /// <param name="CommandType">Gets or sets a value indicating how the CommandText property is to be interpreted, query or store procedure, text query by default</param>
        /// <param name="Timeout">The time in seconds to wait for the command to execute. The default is 30 seconds. A value of 0 indicates no limit (an attempt to execute a command will wait indefinitely).</param>
        /// <param name="Parameters">The parameters of the Transact-SQL statement or stored procedure. The default is an empty collection. Eje: new { p1 = 1 }, on multiple params Eje: new { p1 = 1, p2 = 5 }</param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string ConnectionString, string SQL, ICommandType CommandType = ICommandType.Text, int Timeout = 30, params object[] Parameters) where T : new() {

            this.ConnectionString = ConnectionString;
            CommandText = SQL;
            this.CommandType = CommandType;
            CommandTimeOut = Timeout;

            this.DymanicParams(Parameters);
            return ExecuteReader<T>();
        }

        public T FirstOrDefault<T>() {

            return GetFirstOrDefault<T>();
        }

        public T FirstOrDefault<T>(string SQL) {

            return FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SQL"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public T FirstOrDefault<T>(string SQL, params object[] Parameters) {

            CommandText = SQL;

            this.DymanicParams(Parameters);
            return FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SQL"></param>
        /// <returns></returns>
        public async Task<T> FirstOrDefaultAsync<T>(string SQL) {

            CommandText = SQL;
            return await FirstOrDefaultAsync<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="SQL"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public async Task<T> FirstOrDefaultAsync<T>(string SQL, params object[] Parameters) {

            CommandText = SQL;

            this.DymanicParams(Parameters);
            return await FirstOrDefaultAsync<T>();
        }

        /// <summary>
        /// Map one type of SQL query or store procedure to class model in automated and easy way.
        /// EntityDataAccess relies completely on the properties from the model or class.
        /// </summary>
        /// <typeparam name="T">Entity to map</typeparam>
        /// <param name="ConnectionString">Gets or sets the SqlConnection used by this instance of the SqlCommand, The default value is null.</param>
        /// <param name="SQL">The Transact-SQL statement or stored procedure to execute.</param>
        /// <param name="CommandType">Gets or sets a value indicating how the CommandText property is to be interpreted, query or store procedure, text query by default</param>
        /// <param name="Timeout">The time in seconds to wait for the command to execute. The default is 30 seconds. A value of 0 indicates no limit (an attempt to execute a command will wait indefinitely).</param>
        /// <param name="Parameters">The parameters of the Transact-SQL statement or stored procedure. The default is an empty collection. Eje: new { p1 = 1 }, on multiple params Eje: new { p1 = 1, p2 = 5 }</param>
        /// <returns></returns>
        public dynamic FirstOrDefault(string ConnectionString, string SQL, params object[] Parameters) {

            this.ConnectionString = ConnectionString;
            CommandText = SQL;

            this.DymanicParams(Parameters);
            return FirstOrDefault();
        }

        /// <summary>
        /// Map one type of SQL query or store procedure to class model in automated and easy way.
        /// EntityDataAccess relies completely on the properties from the model or class.
        /// </summary>
        /// <typeparam name="T">Entity to map</typeparam>
        /// <param name="ConnectionString">Gets or sets the SqlConnection used by this instance of the SqlCommand, The default value is null.</param>
        /// <param name="SQL">The Transact-SQL statement or stored procedure to execute.</param>
        /// <param name="CommandType">Gets or sets a value indicating how the CommandText property is to be interpreted, query or store procedure, text query by default</param>
        /// <param name="Timeout">The time in seconds to wait for the command to execute. The default is 30 seconds. A value of 0 indicates no limit (an attempt to execute a command will wait indefinitely).</param>
        /// <param name="Parameters">The parameters of the Transact-SQL statement or stored procedure. The default is an empty collection. Eje: new { p1 = 1 }, on multiple params Eje: new { p1 = 1, p2 = 5 }</param>
        /// <returns></returns>
        public dynamic FirstOrDefault(string ConnectionString, string SQL, ICommandType CommandType = ICommandType.Text, int Timeout = 30, params object[] Parameters) {

            this.ConnectionString = ConnectionString;
            CommandText = SQL;
            this.CommandType = CommandType;
            CommandTimeOut = Timeout;

            this.DymanicParams(Parameters);
            return FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public dynamic FirstOrDefault(string SQL, params object[] Parameters) {

            CommandText = SQL;

            this.DymanicParams(Parameters);
            return FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Parameters"></param>
        private void DymanicParams(params object[] Parameters) {

            for (int i = 0; i < Parameters.Length; i++) {

                string[] items = Parameters[i].ToString().Split(',');

                foreach (var subItems in items) {

                    string[] keyVal = SetupParam(subItems).Split('=');
                    AddParameter(keyVal[0], keyVal[1]);
                }
            }
        }

        /// <summary>
        /// Map one type of SQL query or store procedure on dynamic model in automated and easy way. 
        /// </summary>
        /// <typeparam name="T">Entity to map</typeparam>
        /// <param name="ConnectionString">Gets or sets the SqlConnection used by this instance of the SqlCommand, The default value is null.</param>
        /// <param name="SQL">The Transact-SQL statement or stored procedure to execute.</param>
        /// <param name="Parameters">The parameters of the Transact-SQL statement or stored procedure. The default is an empty collection. Eje: new { p1 = 1 }, on multiple params Eje: new { p1 = 1, p2 = 5 }</param>
        /// <returns></returns>
        public IEnumerable<dynamic> Dynamic(string ConnectionString, string SQL, params object[] Parameters) {

            this.ConnectionString = ConnectionString;
            CommandText = SQL;

            this.DymanicParams(Parameters);
            return GetDynamic();
        }

        /// <summary>
        /// Map one type of SQL query or store procedure on dynamic model in automated and easy way.
        /// </summary>
        /// <typeparam name="T">Entity to map</typeparam>
        /// <param name="ConnectionString">Gets or sets the SqlConnection used by this instance of the SqlCommand, The default value is null.</param>
        /// <param name="SQL">The Transact-SQL statement or stored procedure to execute.</param>
        /// <param name="CommandType">Gets or sets a value indicating how the CommandText property is to be interpreted, query or store procedure, text query by default</param>
        /// <param name="Timeout">The time in seconds to wait for the command to execute. The default is 30 seconds. A value of 0 indicates no limit (an attempt to execute a command will wait indefinitely).</param>
        /// <param name="Parameters">The parameters of the Transact-SQL statement or stored procedure. The default is an empty collection. Eje: new { p1 = 1 }, on multiple params Eje: new { p1 = 1, p2 = 5 }</param>
        /// <returns></returns>
        public IEnumerable<dynamic> Dynamic(string ConnectionString, string SQL, ICommandType CommandType = ICommandType.Text, int Timeout = 30, params object[] Parameters) {

            this.ConnectionString = ConnectionString;
            CommandText = SQL;
            this.CommandType = CommandType;
            CommandTimeOut = Timeout;

            this.DymanicParams(Parameters);
            return GetDynamic();
        }

        private string SetupParam(string param) {

            return string.Concat("@", param.Replace("{", string.Empty).Replace(" ", string.Empty).Replace("}", string.Empty).Replace(" ", string.Empty));
        }

        /// <summary>
        /// Run a read-only query in the database and map a class with the return values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>List class object</returns>
        public IEnumerable<T> ExecuteReader<T>() where T : new() {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            var _list = new List<T>();

            try {
                using (SqlConnection _strConn = new SqlConnection(ConnectionString)) {
                    using (SqlCommand _conn = new SqlCommand(CommandText, _strConn)) {

                        _conn.CommandType = this.SetCommandType(CommandType);
                        _conn.CommandTimeout = this.CommandTimeOut;
                        _strConn.Open();

                        if (SqlParameters.Count > 0) {
                            _conn.Parameters.AddRange(SqlParameters.ToArray());
                            SqlParameters.Clear();
                        }
                        using (SqlDataReader _dr = _conn.ExecuteReader()) {

                            if (IsPrimitive<T>()) {

                                _list = (from IDataRecord r in _dr
                                         select (T)r[0]
                                     ).ToList();

                            }
                            else {
                                Func<SqlDataReader, T> readRow = GetReader<T>(_dr);

                                while (_dr.Read()) {
                                    _list.Add(readRow(_dr));
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) { Exception = ex; InternalError += ex.Message; }
            return _list;
        }

        /// <summary>
        /// Run a read-only query in the database and map a class with the return values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>List class object</returns>
        public async Task<IEnumerable<T>> ExecuteReaderAsync<T>() {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            var _list = new List<T>();

            try {
                using (SqlConnection _strConn = new SqlConnection(ConnectionString)) {
                    using (SqlCommand _conn = new SqlCommand(CommandText, _strConn)) {

                        _conn.CommandType = this.SetCommandType(CommandType);
                        _conn.CommandTimeout = this.CommandTimeOut;

                        if (CancellationTokenSource != null) {
                            CancellationToken = CancellationTokenSource.Token;
                        }
                        await _strConn.OpenAsync(CancellationToken);

                        if (SqlParameters.Count > 0) {
                            _conn.Parameters.AddRange(SqlParameters.ToArray());
                            SqlParameters.Clear();
                        }

                        using (SqlDataReader _dr = await _conn.ExecuteReaderAsync(CancellationToken).ConfigureAwait(false)) {

                            if (IsPrimitive<T>()) {

                                _list = (from IDataRecord r in _dr
                                         select (T)r[0]
                                        ).ToList();

                            }
                            else {
                                Func<SqlDataReader, T> readRow = GetReader<T>(_dr);

                                while (_dr.Read()) {
                                    _list.Add(readRow(_dr));
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) { Exception = ex; InternalError += ex.Message; }
            return _list;
        }

        /// <summary>
        /// Method for creating the dynamic entity for SQL query columns
        /// </summary>
        /// <returns></returns>
        public IEnumerable<dynamic> GetDynamic() {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            using (SqlConnection conn = new SqlConnection(ConnectionString)) {
                using (SqlCommand comm = new SqlCommand(CommandText, conn)) {

                    comm.CommandType = this.SetCommandType(CommandType);
                    comm.CommandTimeout = this.CommandTimeOut;
                    conn.Open();

                    if (SqlParameters.Count > 0) {
                        comm.Parameters.AddRange(SqlParameters.ToArray());
                        SqlParameters.Clear();
                    }

                    using (SqlDataReader _dr = comm.ExecuteReader()) {

                        if (_dr.Read()) {

                            do {
                                dynamic tclass = new ExpandoObject();

                                foreach (string col in this.GetColumnNames(_dr, false)) {
                                    ((IDictionary<string, object>)tclass)[col] = _dr[col];
                                }

                                yield return tclass;
                            } while (_dr.Read());
                        }
                    }
                }
            }
        }

        private T GetFirstOrDefault<T>() {

            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText))
            {
                throw new ArgumentNullException("CommandText can not be blank");
            }
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand comm = new SqlCommand(CommandText, conn))
                    {

                        comm.CommandType = this.SetCommandType(CommandType);
                        comm.CommandTimeout = this.CommandTimeOut;
                        conn.Open();

                        if (SqlParameters.Count > 0)
                        {
                            comm.Parameters.AddRange(SqlParameters.ToArray());
                            SqlParameters.Clear();
                        }

                        List<T> _list = new List<T>();

                        using (SqlDataReader _dr = comm.ExecuteReader())
                        {

                            bool isPrimitive = IsPrimitive<T>();

                            if (isPrimitive) {

                                _list = (from IDataRecord r in _dr
                                         select (T)r[0]
                                     ).ToList();

                            }
                            else {
                                Func<SqlDataReader, T> readRow = GetReader<T>(_dr);
                                bool c = false;

                                while (_dr.Read() && !c)
                                {
                                    _list.Add(readRow(_dr));
                                    c = true;
                                    break;
                                }

                            }
                        }
                        return _list.FirstOrDefault();
                    }
                }
            }
            catch (Exception ex) { Exception = ex; InternalError += ex.Message; return default(T); }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> FirstOrDefaultAsync<T>() {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }
            try {
                using (SqlConnection conn = new SqlConnection(ConnectionString)) {
                    using (SqlCommand comm = new SqlCommand(CommandText, conn)) {

                        comm.CommandType = this.SetCommandType(CommandType);
                        comm.CommandTimeout = this.CommandTimeOut;
                        await conn.OpenAsync();

                        if (SqlParameters.Count > 0) {
                            comm.Parameters.AddRange(SqlParameters.ToArray());
                            SqlParameters.Clear();
                        }

                        List<T> _list = new List<T>();

                        using (SqlDataReader _dr = await comm.ExecuteReaderAsync().ConfigureAwait(false)) {

                            bool isPrimitive = IsPrimitive<T>();

                            if (isPrimitive) {

                                _list = (from IDataRecord r in _dr
                                         select (T)r[0]
                                     ).ToList();

                            }
                            else {
                                Func<SqlDataReader, T> readRow = GetReader<T>(_dr);
                                bool c = false;

                                while (await _dr.ReadAsync() && !c) {
                                    _list.Add(readRow(_dr));
                                    c = true;
                                    break;
                                }

                            }
                        }
                        return _list.FirstOrDefault();
                    }
                }
            } catch (Exception ex) { Exception = ex; InternalError += ex.Message; return default(T); }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public dynamic FirstOrDefault() {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }
            try {
                using (SqlConnection conn = new SqlConnection(ConnectionString)) {
                    using (SqlCommand comm = new SqlCommand(CommandText, conn)) {

                        comm.CommandType = this.SetCommandType(CommandType);
                        comm.CommandTimeout = this.CommandTimeOut;
                        conn.Open();

                        if (SqlParameters.Count > 0) {
                            comm.Parameters.AddRange(SqlParameters.ToArray());
                            SqlParameters.Clear();
                        }

                        var obj = new ExpandoObject() as IDictionary<string, object>;

                        using (SqlDataReader _dr = comm.ExecuteReader()) {

                            if (_dr.Read()) {

                                foreach (string col in this.GetColumnNames(_dr, false)) {

                                    obj.Add(col, _dr[col]);
                                }
                            }
                        }
                        return obj;
                    }
                }
            } catch (Exception ex) { Exception = ex; InternalError += ex.Message; return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private List<string> GetColumnNames(SqlDataReader reader, bool UpperCompare) {

            List<string> readerColumns = new List<string>();

            for (int index = 0; index < reader.FieldCount; index++)
                if (UpperCompare) {
                    readerColumns.Add(reader.GetName(index).ToUpper());
                }
                else {
                    readerColumns.Add(reader.GetName(index));
                }
            return readerColumns;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        /// <returns></returns>
        private string GetTableName<T>(T Entity) {
            //Getting TableNAmeAtrribute from entity
            var tableNameAttr = Entity.GetType().GetCustomAttributes(typeof(TableNameAttribute), false);
            //If not found the TableNameAttribute raise an entity name
            if (tableNameAttr.Length == 0)
                return Entity.GetType().Name;
            return ((TableNameAttribute)tableNameAttr[0]).Name;
        }

        private bool CheckEntityUpperCase<T>(T Entity) {
            var isUpper = Entity.GetType().GetCustomAttributes(typeof(UpperCaseAttribute), false);
            return isUpper.Any() ? true : false;
        }

        private bool CheckEntityLowerCase<T>(T Entity) {
            var isLower = Entity.GetType().GetCustomAttributes(typeof(LowerCaseAttribute), false);
            return isLower.Any() ? true : false;
        }

        private bool IsPrimitive<T>() => IsPrimitiveType(Expression.Parameter(typeof(T)));

        private bool IsPrimitiveType(ParameterExpression parameter) {

            var types = new Type[] {
                        typeof(string),
                        typeof(char),
                        typeof(byte),
                        typeof(decimal),
                        typeof(DateTime),
                        typeof(DateTimeOffset),
                        typeof(TimeSpan),
                        typeof(short),
                        typeof(int),
                        typeof(long),
                        typeof(float),
                        typeof(double),
            };
            return types.Contains(parameter.Type);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public Func<SqlDataReader, T> GetReader<T>(SqlDataReader reader) {

            Delegate resDelegate;

            if (!ExpressionCache.TryGetValue(typeof(T), out resDelegate)) {

                // determine the information about the reader
                var readerParam = Expression.Parameter(typeof(SqlDataReader), "reader");
                var readerGetValue = typeof(SqlDataReader).GetMethod("GetValue");

                // create a Constant expression of DBNull.Value to compare values to in reader
                var dbNullExp = Expression.Field(expression: null, type: typeof(DBNull), fieldName: "Value");

                // loop through the properties and create MemberBinding expressions for each property
                List<MemberBinding> memberBindings = new List<MemberBinding>();

                foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)) {

                    try {
                        // determine the default value of the property
                        object defaultValue = null;
                        if (prop.PropertyType.IsValueType)
                            defaultValue = Activator.CreateInstance(prop.PropertyType);
                        else if (prop.PropertyType.Name.ToLower().Equals("string"))
                            defaultValue = string.Empty;

                        if (this.GetColumnNames(reader, true).Contains(prop.Name.ToUpper())) {
                            // build the Call expression to retrieve the data value from the reader
                            var indexExpression = Expression.Constant(reader.GetOrdinal(prop.Name));
                            var getValueExp = Expression.Call(readerParam, readerGetValue, new Expression[] { indexExpression });

                            // create the conditional expression to make sure the reader value != DBNull.Value
                            var testExp = Expression.NotEqual(dbNullExp, getValueExp);
                            var ifTrue = Expression.Convert(getValueExp, prop.PropertyType);
                            var ifFalse = Expression.Convert(Expression.Constant(defaultValue), prop.PropertyType);

                            // create the actual Bind expression to bind the value from the reader to the property value
                            MemberInfo mi = typeof(T).GetMember(prop.Name)[0];
                            MemberBinding mb = Expression.Bind(mi, Expression.Condition(testExp, ifTrue, ifFalse));
                            memberBindings.Add(mb);
                        }
                    } catch (Exception ex) {
                        Exception = ex;
                        InternalError += ex.Message;
                        continue;
                    }
                }
                try {
                    var newItem = Expression.New(typeof(T));
                    var memberInit = Expression.MemberInit(newItem, memberBindings);

                    var lambda = Expression.Lambda<Func<SqlDataReader, T>>(memberInit, new ParameterExpression[] { readerParam });
                    resDelegate = lambda.Compile();
                    ExpressionCache[typeof(T)] = resDelegate;
                } catch (Exception ex) { Exception = ex; InternalError += ex.Message; }
            }
            return (Func<SqlDataReader, T>)resDelegate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <returns></returns>
        public MultiQuery<A, B> ExecuteReader<A, B>() where A : new()
                                                      where B : new() {

            if (string.IsNullOrWhiteSpace(ConnectionString)) {
                throw new ArgumentNullException("Connection string can not be empty");
            }

            if (string.IsNullOrWhiteSpace(CommandText)) {
                throw new ArgumentNullException("CommandText can not be blank");
            }

            var _AList = new List<A>();
            var _BList = new List<B>();
            var mset = new MultiQuery<A, B>();

            using (SqlConnection _strConn = new SqlConnection(ConnectionString)) {
                using (SqlCommand _conn = new SqlCommand(CommandText, _strConn)) {

                    _conn.CommandType = this.SetCommandType(CommandType);
                    _conn.CommandTimeout = this.CommandTimeOut;
                    _strConn.Open();

                    if (SqlParameters.Count > 0) {
                        _conn.Parameters.AddRange(SqlParameters.ToArray());
                        SqlParameters.Clear();
                    }
                    using (SqlDataReader _dr = _conn.ExecuteReader()) {

                        if (_dr.Read()) {

                            _AList = this.MapEntityCollection<A>(_dr);
                        }
                        if (_dr.NextResult()) {

                            _BList = this.MapEntityCollection<B>(_dr);
                        }
                    }
                }
            }

            mset.Pro1 = _AList;
            mset.Pro2 = _BList;

            return mset;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="Closed"></param>
        /// <returns></returns>
        private List<T> MapEntityCollection<T>(IDataReader dr, bool Closed = true) where T : new() {

            Type entityType = typeof(T);
            List<T> entitys = new List<T>();
            Hashtable hashtable = new Hashtable();
            PropertyInfo[] properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo info in properties) {
                hashtable[info.Name.ToUpper()] = info;
            }

            while (dr.Read()) {
                T newObject = new T();
                for (int index = 0; index < dr.FieldCount; index++) {
                    try {
                        PropertyInfo info = (PropertyInfo)hashtable[dr.GetName(index).ToUpper()];

                        //bool isVirtual = info.GetGetMethod().IsVirtual;
                        //if (isVirtual) {

                        //}
                        if ((info != null) && info.CanWrite) {
                            info.SetValue(newObject, ChangeType.ChangeEntityType(dr.GetValue(index), info.PropertyType), null);
                        }
                    } catch {
                        continue;
                    }
                }
                entitys.Add(newObject);
            }
            if (Closed) {
                dr.Close();
            }
            return entitys;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        /// <param name="UseTransaction"></param>
        /// <returns></returns>
        public object Insert<T>(T Entity, bool UseTransaction = false) {

            if (CommandType == 0 || CommandType == ICommandType.Text) {

                CommandText = insertQuery(Entity);
            }
            return ToExecute(UseTransaction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        /// <param name="UseTransaction"></param>
        /// <returns></returns>
        public async Task<object> InsertAsync<T>(T Entity, bool UseTransaction = false) {

            if (CommandType == 0 || CommandType == ICommandType.Text) {

                CommandText = insertQuery(Entity);
            }
            return await ToExecuteAsync(UseTransaction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        /// <param name="UseTransaction"></param>
        /// <returns></returns>
        public int Update<T>(T Entity, bool UseTransaction = false) {

            if (CommandType == 0 || CommandType == ICommandType.Text) {

                CommandText = updateQuery(Entity);
            }
            return (int)ToExecute(UseTransaction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        /// <param name="UseTransaction"></param>
        /// <returns></returns>
        public async Task<object> UpdateAsync<T>(T Entity, bool UseTransaction = false) {

            if (CommandType == 0 || CommandType == ICommandType.Text) {

                CommandText = updateQuery(Entity);
            }
            return Convert.ToInt32(await ToExecuteAsync(UseTransaction));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Entity"></param>
        /// <param name="UseTransaction"></param>
        /// <returns></returns>
        public int Delete<T>(T Entity, bool UseTransaction = false) where T : new() {

            string query = string.Empty;
            Type entityType = typeof(T);
            string _tname = entityType.UnderlyingSystemType.Name;
            PropertyInfo[] properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            StringBuilder sbParams = new StringBuilder();
            bool isFirstTime = true;

            foreach (PropertyInfo info in properties) {

                var isPrimary = Attribute.GetCustomAttribute(info, typeof(PrimaryKeyAttribute)) as PrimaryKeyAttribute;

                if (isPrimary != null) {

                    string paramName = string.Concat("@", info.Name);
                    var value = Entity.GetType().GetProperty(info.Name).GetValue(Entity, null);

                    AddParameter(paramName, value);

                    if (isFirstTime) {
                        sbParams.AppendLine(string.Concat(paramName.Replace("@", string.Empty), " = ", value));
                    }
                    else {
                        sbParams.AppendLine(string.Concat(" AND ", paramName.Replace("@", string.Empty), " = ", value));
                    }

                    isFirstTime = false;
                }
            }

            if (string.IsNullOrWhiteSpace(sbParams.ToString())) {
                CommandText = "";
                throw new ArgumentNullException("Delete key property not found");
            }
            else {
                CommandText = string.Concat("DELETE FROM [", _tname, "] WHERE ", sbParams.ToString(), ";");
            }

            return (int)ToExecute(UseTransaction);
        }

        private List<PropertyInfo> GetInterfaceProperties(Type type) {

            return type.GetInterfaces().SelectMany(i => i.GetProperties(BindingFlags.Public | BindingFlags.Instance)).ToList();
        }

        protected string insertQuery<T>(T entity) {

            string query = string.Empty;
            _ifInserted = false;
            Type entityType = typeof(T);
            PropertyInfo[] properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var faceProd = GetInterfaceProperties(entityType);

            StringBuilder sbColumns = new StringBuilder();
            StringBuilder sbParams = new StringBuilder();
            StringBuilder sbUpdate = new StringBuilder();
            StringBuilder sbWhereParams = new StringBuilder();

            string _tname = GetTableName(entity);
            bool isUpperCase = CheckEntityUpperCase(entity);
            bool isLowerCase = CheckEntityLowerCase(entity);
            bool isFirstTime = true;
            string _inserted = " OUTPUT INSERTED.";

            foreach (PropertyInfo info in properties) {

                bool isVirtual = typeof(T).GetProperty(info.Name).GetGetMethod().IsVirtual;

                if (isVirtual & !faceProd.Any(x => x.Name == info.Name)) { continue; }

                var isNoInsertMap = Attribute.GetCustomAttribute(info, typeof(NoInsertAttribute)) as NoInsertAttribute;
                if (isNoInsertMap != null && isNoInsertMap.NoInsert) { continue; }

                var computed = Attribute.GetCustomAttribute(info, typeof(ComputedAttribute)) as ComputedAttribute;
                if (computed != null) { continue; }

                var isRequired = Attribute.GetCustomAttribute(info, typeof(RequiredAttribute)) as RequiredAttribute;
                var isPrimary = Attribute.GetCustomAttribute(info, typeof(PrimaryKeyAttribute)) as PrimaryKeyAttribute;
                var toUpperAtt = Attribute.GetCustomAttribute(info, typeof(UpperCaseAttribute)) as UpperCaseAttribute;
                var toLowerAtt = Attribute.GetCustomAttribute(info, typeof(LowerCaseAttribute)) as LowerCaseAttribute;

                string paramName = string.Concat("@", info.Name);
                object value;

                value = entity.GetType().GetProperty(info.Name).GetValue(entity, null);

                if ((toUpperAtt != null || isUpperCase) && value is string u && !IsObjectNullOrEmpty(value)) {
                    value = u.ToUpper();
                }
                if ((toLowerAtt != null || isLowerCase) && value is string l && !IsObjectNullOrEmpty(value)) {
                    value = l.ToLower();
                }

                if (isRequired != null && IsObjectNullOrEmpty(value)) {
                    IsValidModel = false;
                    InternalError += string.Concat("Property ", info.Name, " is required" + Environment.NewLine);
                    continue;
                }

                if (isPrimary == null || isPrimary.AutoIncrease == false) {

                    sbColumns.AppendLine(string.Concat(info.Name, ","));
                    AddParameter(paramName, value);
                }
                else if (isPrimary.AutoIncrease) {

                    _ifInserted = true;
                    _idenType = info.PropertyType;
                    _inserted = _inserted + info.Name;
                }

                if (isNoInsertMap != null) { continue; }

                paramName = isFirstTime ? paramName : string.Concat(",", paramName);

                if (isPrimary == null || isPrimary.AutoIncrease == false) {
                    sbParams.AppendLine(paramName);
                }

                isFirstTime = false;
            }

            if (!IsValidModel) { InternalError += "Please check all properties marked as required" + Environment.NewLine; }

            string _columns = sbColumns.ToString();
            string _param = sbParams.ToString();

            _columns = CleanEndLine(_columns);
            _param = CleanEndLine(_param);

            _columns = _columns.StartsWith(",", StringComparison.InvariantCultureIgnoreCase) ?_columns.Substring(1) : _columns;

            _columns = _columns.EndsWith(",", StringComparison.InvariantCultureIgnoreCase) ? _columns.Substring(0, _columns.Length - 1) : _columns;

            _param = _param.StartsWith(",", StringComparison.InvariantCultureIgnoreCase) ? _param.Substring(1) : _param;

            _param = _param.EndsWith(",", StringComparison.InvariantCultureIgnoreCase) ? _param.Substring(0, _param.Length - 1) : _param;

            query = string.Concat("INSERT INTO ", "[" + _tname,  "]" + " (", _columns, ")", _ifInserted ? _inserted : "", " VALUES " + "(", _param, ")");

            //QueryCache[typeof(T)] = query;
            SqlText = query;
            return query;
        }

        protected string updateQuery<T>(T entity) {

            string query = string.Empty;
            bool isFirstTimeColumn = true, isFirstTimeWhere = true;
            Type entityType = typeof(T);
            PropertyInfo[] properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var faceProd = GetInterfaceProperties(entityType);

            StringBuilder sbColumns = new StringBuilder();
            StringBuilder sbWhereParams = new StringBuilder();

            string _tname = GetTableName(entity);
            bool isUpperCase = CheckEntityUpperCase(entity);
            bool isLowerCase = CheckEntityLowerCase(entity);

            foreach (PropertyInfo info in properties) {

                bool isVirtual = typeof(T).GetProperty(info.Name).GetGetMethod().IsVirtual;

                if (isVirtual & !faceProd.Any(x => x.Name == info.Name)) { continue; }

                var isNoUpdate = Attribute.GetCustomAttribute(info, typeof(NoUpdateAttribute)) as NoUpdateAttribute;
                if (isNoUpdate != null && isNoUpdate.NoUpdate) { continue; }

                var isRequired = Attribute.GetCustomAttribute(info, typeof(RequiredAttribute)) as RequiredAttribute;
                var isPrimary = Attribute.GetCustomAttribute(info, typeof(PrimaryKeyAttribute)) as PrimaryKeyAttribute;
                var toUpperAtt = Attribute.GetCustomAttribute(info, typeof(UpperCaseAttribute)) as UpperCaseAttribute;
                var toLowerAtt = Attribute.GetCustomAttribute(info, typeof(LowerCaseAttribute)) as LowerCaseAttribute;

                string paramName = string.Concat("@", info.Name);
                object value;

                value = entity.GetType().GetProperty(info.Name).GetValue(entity, null);

                if ((toUpperAtt != null || isUpperCase) && value is string u && !IsObjectNullOrEmpty(value)) {
                    value = u.ToUpper();
                }

                if ((toLowerAtt != null || isLowerCase) && value is string l && !IsObjectNullOrEmpty(value)) {
                    value = l.ToLower();
                }

                if (isRequired != null && IsObjectNullOrEmpty(value)) {
                    IsValidModel = false;
                    InternalError += string.Concat("Property ", info.Name, " is required" + Environment.NewLine);
                }

                if (isPrimary is null) {
                    string upColumnName = string.Concat(isFirstTimeColumn ? string.Empty : ",", info.Name, " = ", paramName);
                    sbColumns.AppendLine(upColumnName);
                    AddParameter(paramName, value);
                    isFirstTimeColumn = false;
                } else {
                    string upParamName = string.Concat(isFirstTimeWhere ? string.Empty : " AND ", info.Name, " = ", paramName);
                    sbWhereParams.AppendLine(upParamName);
                    AddParameter(paramName, value);
                    isFirstTimeWhere = false;
                }
            }

            if (string.IsNullOrWhiteSpace(sbWhereParams.ToString())) {
                query = "";
                throw new ArgumentNullException("Update primary key property not found");
            }
 
            query = string.Concat("UPDATE ","[" + _tname, "] SET ", sbColumns, " WHERE ", sbWhereParams.ToString());
            
            if (!IsValidModel) { InternalError += "Please check all properties marked as required" + Environment.NewLine; }

            SqlText = query;
            return query;
        }

        private bool IsObjectNullOrEmpty(object value) {

            if (value == null) {
                return true;
            }

            if (value.GetType() == typeof(string)) {
                if (value is string && string.IsNullOrEmpty((string)(value))) {
                    return true;
                }
            }
            return false;
        }

        public bool IsPropertyACollection(PropertyInfo property) {
            return property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition() == typeof(IList<>);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfoMessageRequest(object sender, SqlInfoMessageEventArgs e) {

            _infoMessage = null;
            foreach (var error in e.Errors) {
                _infoMessage.Add(string.Format("Source {0} $ Message{1} $ error{2}", e.Source, e.Message, error.ToString()));
            }
        }

        public static void Create(Action<EntityDataAccess> Execution) {

            EntityDataAccess db = new EntityDataAccess();
            try {
                Execution(db);
            }
            finally {
            }
        }

        public void Dispose() {
            //Cleanup();
            GC.SuppressFinalize(this);
        }

        public class MultiQuery<T, T2> where T : new()
                                       where T2 : new() {

            public List<T> Pro1 { get; set; }
            public List<T2> Pro2 { get; set; }
        }

        private string CleanEndLine(string text) {
            text = text.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
            return text;
        }

        private OSPlatform GetRunningOS() {


            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return OSPlatform.Windows;
            }else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                return OSPlatform.Linux;
            }
            else {
                return OSPlatform.Windows;
            }
        }
    }

}