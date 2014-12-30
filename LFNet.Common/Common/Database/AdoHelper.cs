using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace LFNet.Common.Database
{
    /// <summary>
    /// The AdoHelper class is intended to encapsulate high performance, scalable best practices for
    /// common data access uses.
    /// </summary>
    /// <typeparam name="TFactory">The type of the factory.</typeparam>
    public class AdoHelper<TFactory> where TFactory : DbProviderFactory, new()
    {
        /// <summary>
        /// Delegate for creating a RowUpdatedEvent handler
        /// </summary>
        /// <param name="sender">The object that published the event</param>
        /// <param name="e">The RowUpdatedEventArgs for the event</param>
        public delegate void RowUpdatedHandler(object sender, RowUpdatedEventArgs e);
        /// <summary>
        /// Delegate for creating a RowUpdatingEvent handler
        /// </summary>
        /// <param name="sender">The object that published the event</param>
        /// <param name="e">The RowUpdatingEventArgs for the event</param>
        public delegate void RowUpdatingHandler(object sender, RowUpdatingEventArgs e);
        /// <summary>
        /// This enum is used to indicate whether the connection was provided by the caller, or created by AdoHelper, so that
        /// we can set the appropriate CommandBehavior when calling ExecuteReader()
        /// </summary>
        protected enum AdoConnectionOwnership
        {
            /// <summary>Connection is owned and managed by ADOHelper</summary>
            Internal,
            /// <summary>Connection is owned and managed by the caller</summary>
            External
        }
        /// <summary>
        /// ADOHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the
        /// ability to discover parameters for stored procedures at run-time.
        /// </summary>
        private sealed class ADOHelperParameterCache
        {
            private static readonly Hashtable paramCache = Hashtable.Synchronized(new Hashtable());
            /// <summary>
            /// Deep copy of cached IDataParameter array
            /// </summary>
            /// <param name="originalParameters"></param>
            /// <returns></returns>
            internal static IDataParameter[] CloneParameters(IDataParameter[] originalParameters)
            {
                IDataParameter[] array = new IDataParameter[originalParameters.Length];
                int i = 0;
                int num = originalParameters.Length;
                while (i < num)
                {
                    array[i] = (IDataParameter)((ICloneable)originalParameters[i]).Clone();
                    i++;
                }
                return array;
            }
            /// <summary>
            /// Add parameter array to the cache
            /// </summary>
            /// <param name="connectionString">A valid connection string for an IDbConnection</param>
            /// <param name="commandText">The stored procedure name or SQL command</param>
            /// <param name="commandParameters">An array of IDataParameters to be cached</param>
            /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
            /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
            internal static void CacheParameterSet(string connectionString, string commandText, params IDataParameter[] commandParameters)
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentNullException("connectionString");
                }
                if (string.IsNullOrEmpty(commandText))
                {
                    throw new ArgumentNullException("commandText");
                }
                string key = connectionString + ":" + commandText;
                AdoHelper<TFactory>.ADOHelperParameterCache.paramCache[key] = commandParameters;
            }
            /// <summary>
            /// Retrieve a parameter array from the cache
            /// </summary>
            /// <param name="connectionString">A valid connection string for an IDbConnection</param>
            /// <param name="commandText">The stored procedure name or SQL command</param>
            /// <returns>An array of IDataParameters</returns>
            /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
            /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
            internal static IDataParameter[] GetCachedParameterSet(string connectionString, string commandText)
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentNullException("connectionString");
                }
                if (string.IsNullOrEmpty(commandText))
                {
                    throw new ArgumentNullException("commandText");
                }
                string key = connectionString + ":" + commandText;
                IDataParameter[] array = AdoHelper<TFactory>.ADOHelperParameterCache.paramCache[key] as IDataParameter[];
                if (array == null)
                {
                    return null;
                }
                return AdoHelper<TFactory>.ADOHelperParameterCache.CloneParameters(array);
            }
        }
        private readonly TFactory factory = Activator.CreateInstance<TFactory>();
        /// <summary>
        /// Internal handler used for bubbling up the event to the user
        /// </summary>
        protected AdoHelper<TFactory>.RowUpdatedHandler m_rowUpdated;
        /// <summary>
        /// Internal handler used for bubbling up the event to the user
        /// </summary>
        protected AdoHelper<TFactory>.RowUpdatingHandler m_rowUpdating;
        /// <summary>
        /// Get an IDataParameter for use in a SQL command
        /// </summary>
        /// <param name="name">The name of the parameter to create</param>
        /// <param name="value">The value of the specified parameter</param>
        /// <returns>An IDataParameter object</returns>
        public virtual IDataParameter GetParameter(string name, object value)
        {
            IDataParameter parameter = this.GetParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            return parameter;
        }
        /// <summary>
        /// Get an IDataParameter for use in a SQL command
        /// </summary>
        /// <param name="name">The name of the parameter to create</param>
        /// <param name="dbType">The System.Data.DbType of the parameter</param>
        /// <param name="size">The size of the parameter</param>
        /// <param name="direction">The System.Data.ParameterDirection of the parameter</param>
        /// <returns>An IDataParameter object</returns>
        public virtual IDataParameter GetParameter(string name, DbType dbType, int size, ParameterDirection direction)
        {
            IDataParameter parameter = this.GetParameter();
            parameter.DbType = dbType;
            parameter.Direction = direction;
            parameter.ParameterName = name;
            if (size > 0 && parameter is IDbDataParameter)
            {
                IDbDataParameter dbDataParameter = (IDbDataParameter)parameter;
                dbDataParameter.Size = size;
            }
            return parameter;
        }
        /// <summary>
        /// Get an IDataParameter for use in a SQL command
        /// </summary>
        /// <param name="name">The name of the parameter to create</param>
        /// <param name="dbType">The System.Data.DbType of the parameter</param>
        /// <param name="size">The size of the parameter</param>
        /// <param name="sourceColumn">The source column of the parameter</param>
        /// <param name="sourceVersion">The System.Data.DataRowVersion of the parameter</param>
        /// <returns>An IDataParameter object</returns>
        public virtual IDataParameter GetParameter(string name, DbType dbType, int size, string sourceColumn, DataRowVersion sourceVersion)
        {
            IDataParameter parameter = this.GetParameter();
            parameter.DbType = dbType;
            parameter.ParameterName = name;
            parameter.SourceColumn = sourceColumn;
            parameter.SourceVersion = sourceVersion;
            if (size > 0 && parameter is IDbDataParameter)
            {
                IDbDataParameter dbDataParameter = (IDbDataParameter)parameter;
                dbDataParameter.Size = size;
            }
            return parameter;
        }
        /// <summary>
        /// This method is used to attach array of IDataParameters to an IDbCommand.
        ///
        /// This method will assign a value of DbNull to any parameter with a direction of
        /// InputOutput and a value of null.  
        ///
        /// This behavior will prevent default values from being used, but
        /// this will be the less common case than an intended pure output parameter (derived as InputOutput)
        /// where the user provided no input value.
        /// </summary>
        /// <param name="command">The command to which the parameters will be added</param>
        /// <param name="commandParameters">An array of IDataParameterParameters to be added to command</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if command is null.</exception>
        protected virtual void AttachParameters(IDbCommand command, IDataParameter[] commandParameters)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (commandParameters != null)
            {
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    IDataParameter dataParameter = commandParameters[i];
                    if (dataParameter != null)
                    {
                        if ((dataParameter.Direction == ParameterDirection.InputOutput || dataParameter.Direction == ParameterDirection.Input) && dataParameter.Value == null)
                        {
                            dataParameter.Value = DBNull.Value;
                        }
                        if (dataParameter.DbType == DbType.Binary)
                        {
                            command.Parameters.Add(this.GetBlobParameter(command.Connection, dataParameter));
                        }
                        else
                        {
                            command.Parameters.Add(dataParameter);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// This method assigns dataRow column values to an IDataParameterCollection
        /// </summary>
        /// <param name="commandParameters">The IDataParameterCollection to be assigned values</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the parameter names are invalid.</exception>
        protected internal void AssignParameterValues(IDataParameterCollection commandParameters, DataRow dataRow)
        {
            if (commandParameters == null || dataRow == null)
            {
                return;
            }
            DataColumnCollection columns = dataRow.Table.Columns;
            int num = 0;
            foreach (IDataParameter dataParameter in commandParameters)
            {
                if (dataParameter.ParameterName == null || dataParameter.ParameterName.Length <= 1)
                {
                    throw new InvalidOperationException(string.Format("Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.", num, dataParameter.ParameterName));
                }
                if (columns.Contains(dataParameter.ParameterName))
                {
                    dataParameter.Value = dataRow[dataParameter.ParameterName];
                }
                else
                {
                    if (columns.Contains(dataParameter.ParameterName.Substring(1)))
                    {
                        dataParameter.Value = dataRow[dataParameter.ParameterName.Substring(1)];
                    }
                }
                num++;
            }
        }
        /// <summary>
        /// This method assigns dataRow column values to an array of IDataParameters
        /// </summary>
        /// <param name="commandParameters">Array of IDataParameters to be assigned values</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values</param>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the parameter names are invalid.</exception>
        protected void AssignParameterValues(IDataParameter[] commandParameters, DataRow dataRow)
        {
            if (commandParameters == null || dataRow == null)
            {
                return;
            }
            DataColumnCollection columns = dataRow.Table.Columns;
            int num = 0;
            for (int i = 0; i < commandParameters.Length; i++)
            {
                IDataParameter dataParameter = commandParameters[i];
                if (dataParameter.ParameterName == null || dataParameter.ParameterName.Length <= 1)
                {
                    throw new InvalidOperationException(string.Format("Please provide a valid parameter name on the parameter #{0}, the ParameterName property has the following value: '{1}'.", num, dataParameter.ParameterName));
                }
                if (columns.Contains(dataParameter.ParameterName))
                {
                    dataParameter.Value = dataRow[dataParameter.ParameterName];
                }
                else
                {
                    if (columns.Contains(dataParameter.ParameterName.Substring(1)))
                    {
                        dataParameter.Value = dataRow[dataParameter.ParameterName.Substring(1)];
                    }
                }
                num++;
            }
        }
        /// <summary>
        /// This method assigns an array of values to an array of IDataParameters
        /// </summary>
        /// <param name="commandParameters">Array of IDataParameters to be assigned values</param>
        /// <param name="parameterValues">Array of objects holding the values to be assigned</param>
        /// <exception cref="T:System.ArgumentException">Thrown if an incorrect number of parameters are passed.</exception>
        protected void AssignParameterValues(IDataParameter[] commandParameters, object[] parameterValues)
        {
            if (commandParameters == null || parameterValues == null)
            {
                return;
            }
            if (commandParameters.Length != parameterValues.Length)
            {
                throw new ArgumentException("Parameter count does not match Parameter Value count.");
            }
            int i = 0;
            int num = commandParameters.Length;
            int num2 = 0;
            while (i < num)
            {
                if (commandParameters[i].Direction != ParameterDirection.ReturnValue)
                {
                    if (parameterValues[num2] is IDataParameter)
                    {
                        IDataParameter dataParameter = (IDataParameter)parameterValues[num2];
                        if (dataParameter.Direction == ParameterDirection.ReturnValue)
                        {
                            dataParameter = (IDataParameter)parameterValues[++num2];
                        }
                        if (dataParameter.Value == null)
                        {
                            commandParameters[i].Value = DBNull.Value;
                        }
                        else
                        {
                            commandParameters[i].Value = dataParameter.Value;
                        }
                    }
                    else
                    {
                        if (parameterValues[num2] == null)
                        {
                            commandParameters[i].Value = DBNull.Value;
                        }
                        else
                        {
                            commandParameters[i].Value = parameterValues[num2];
                        }
                    }
                    num2++;
                }
                i++;
            }
        }
        /// <summary>
        /// This method cleans up the parameter syntax for the provider
        /// </summary>
        /// <param name="command">The IDbCommand containing the parameters to clean up.</param>
        public virtual void CleanParameterSyntax(IDbCommand command)
        {
        }
        /// <summary>
        /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
        /// to the provided command
        /// </summary>
        /// <param name="command">The IDbCommand to be prepared</param>
        /// <param name="connection">A valid IDbConnection, on which to execute this command</param>
        /// <param name="transaction">A valid IDbTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="mustCloseConnection"><c>true</c> if the connection was opened by the method, otherwose is false.</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if command is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null.</exception>
        protected virtual void PrepareCommand(IDbCommand command, IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, IDataParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }
            command.Connection = connection;
            command.CommandText = commandText;
            if (transaction != null)
            {
                if (transaction.Connection == null)
                {
                    throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
                }
                command.Transaction = transaction;
            }
            command.CommandType = commandType;
            if (commandParameters != null)
            {
                this.AttachParameters(command, commandParameters);
            }
        }
        /// <summary>
        /// This method clears (if necessary) the connection, transaction, command type and parameters 
        /// from the provided command
        /// </summary>
        /// <remarks>
        /// Not implemented here because the behavior of this method differs on each data provider. 
        /// </remarks>
        /// <param name="command">The IDbCommand to be cleared</param>
        protected virtual void ClearCommand(IDbCommand command)
        {
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <param name="command">The IDbCommand object to use</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual DataSet ExecuteDataset(IDbCommand command)
        {
            bool flag = false;
            this.CleanParameterSyntax(command);
            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                flag = true;
            }
            IDbDataAdapter dbDataAdapter = null;
            DataSet result;
            try
            {
                dbDataAdapter = this.GetDataAdapter();
                dbDataAdapter.SelectCommand = command;
                DataSet dataSet = new DataSet();
                try
                {
                    dbDataAdapter.Fill(dataSet);
                }
                catch (Exception)
                {
                    throw;
                }
                result = dataSet;
            }
            finally
            {
                if (flag)
                {
                    command.Connection.Close();
                }
                if (dbDataAdapter != null)
                {
                    IDisposable disposable = dbDataAdapter as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders");
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        public virtual DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return this.ExecuteDataset(connectionString, commandType, commandText, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new IDbParameter("@prodid", 24));
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDbParamters used to execute the command</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            DataSet result;
            using (IDbConnection connection = this.GetConnection(connectionString))
            {
                connection.Open();
                result = this.ExecuteDataset(connection, commandType, commandText, commandParameters);
            }
            return result;
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(connString, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual DataSet ExecuteDataset(string connectionString, string spName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return this.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                return this.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, dataParameters);
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connectionString, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            return this.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders");
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText)
        {
            return this.ExecuteDataset(connection, commandType, commandText, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual DataSet ExecuteDataset(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            IDbCommand command = connection.CreateCommand();
            bool flag;
            this.PrepareCommand(command, connection, null, commandType, commandText, commandParameters, out flag);
            this.CleanParameterSyntax(command);
            DataSet result = this.ExecuteDataset(command);
            if (flag)
            {
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(conn, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual DataSet ExecuteDataset(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return this.ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                return this.ExecuteDataset(connection, CommandType.StoredProcedure, spName, dataParameters);
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connection, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            return this.ExecuteDataset(connection, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <example><code>
        ///  DataSet ds = helper.ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders");
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return this.ExecuteDataset(transaction, commandType, commandText, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(trans, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual DataSet ExecuteDataset(IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            }
            IDbCommand command = transaction.Connection.CreateCommand();
            bool flag;
            this.PrepareCommand(command, transaction.Connection, transaction, commandType, commandText, commandParameters, out flag);
            this.CleanParameterSyntax(command);
            return this.ExecuteDataset(command);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified 
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// DataSet ds = helper.ExecuteDataset(tran, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual DataSet ExecuteDataset(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return this.ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                return this.ExecuteDataset(transaction, CommandType.StoredProcedure, spName, dataParameters);
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(transaction.Connection, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            return this.ExecuteDataset(transaction, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns no resultset) against the database
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual int ExecuteNonQuery(IDbCommand command)
        {
            bool flag = false;
            this.CleanParameterSyntax(command);
            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                flag = true;
            }
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            int result = command.ExecuteNonQuery();
            if (flag)
            {
                command.Connection.Close();
            }
            return result;
        }
        /// <summary>
        /// Execute an IDbCommand (that returns no resultset and takes no parameters) against the database specified in 
        /// the connection string
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return this.ExecuteNonQuery(connectionString, commandType, commandText, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            int result;
            using (IDbConnection connection = this.GetConnection(connectionString))
            {
                connection.Open();
                result = this.ExecuteNonQuery(connection, commandType, commandText, commandParameters);
            }
            return result;
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        /// </remarks>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored prcedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual int ExecuteNonQuery(string connectionString, string spName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return this.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                return this.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, dataParameters);
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connectionString, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            return this.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns no resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText)
        {
            return this.ExecuteNonQuery(connection, commandType, commandText, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns no resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDbParamters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual int ExecuteNonQuery(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            IDbCommand command = connection.CreateCommand();
            bool flag;
            this.PrepareCommand(command, connection, null, commandType, commandText, commandParameters, out flag);
            this.CleanParameterSyntax(command);
            int result = this.ExecuteNonQuery(command);
            if (flag)
            {
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        ///
        /// </remarks>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual int ExecuteNonQuery(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return this.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                return this.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, dataParameters);
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connection, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            return this.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns no resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return this.ExecuteNonQuery(transaction, commandType, commandText, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns no resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual int ExecuteNonQuery(IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            }
            IDbCommand command = transaction.Connection.CreateCommand();
            bool flag;
            this.PrepareCommand(command, transaction.Connection, transaction, commandType, commandText, commandParameters, out flag);
            this.CleanParameterSyntax(command);
            return this.ExecuteNonQuery(command);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) against the specified 
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual int ExecuteNonQuery(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return this.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                return this.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, dataParameters);
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(transaction.Connection, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            return this.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <param name="command">The IDbCommand object to use</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual IDataReader ExecuteReader(IDbCommand command)
        {
            return this.ExecuteReader(command, AdoHelper<TFactory>.AdoConnectionOwnership.External);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <param name="command">The IDbCommand object to use</param>
        /// <param name="connectionOwnership">Enum indicating whether the connection was created internally or externally.</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if command is null.</exception>
        protected virtual IDataReader ExecuteReader(IDbCommand command, AdoHelper<TFactory>.AdoConnectionOwnership connectionOwnership)
        {
            this.CleanParameterSyntax(command);
            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                connectionOwnership = AdoHelper<TFactory>.AdoConnectionOwnership.Internal;
            }
            IDataReader dataReader;
            if (connectionOwnership == AdoHelper<TFactory>.AdoConnectionOwnership.External)
            {
                dataReader = command.ExecuteReader();
            }
            else
            {
                try
                {
                    dataReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            this.ClearCommand(command);
            if (dataReader != null)
            {
                dataReader = new SafeDataReader(dataReader);
            }
            return dataReader;
        }
        /// <summary>
        /// Create and prepare an IDbCommand, and call ExecuteReader with the appropriate CommandBehavior.
        /// </summary>
        /// <remarks>
        /// If we created and opened the connection, we want the connection to be closed when the DataReader is closed.
        ///
        /// If the caller provided the connection, we want to leave it to them to manage.
        /// </remarks>
        /// <param name="connection">A valid IDbConnection, on which to execute this command</param>
        /// <param name="transaction">A valid IDbTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="connectionOwnership">Indicates whether the connection parameter was provided by the caller, or created by AdoHelper</param>
        /// <returns>IDataReader containing the results of the command</returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        private IDataReader ExecuteReader(IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, IDataParameter[] commandParameters, AdoHelper<TFactory>.AdoConnectionOwnership connectionOwnership)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            bool flag = false;
            IDbCommand command = connection.CreateCommand();
            IDataReader result;
            try
            {
                this.PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, out flag);
                this.CleanParameterSyntax(command);
                if (flag)
                {
                    connectionOwnership = AdoHelper<TFactory>.AdoConnectionOwnership.Internal;
                }
                IDataReader dataReader = this.ExecuteReader(command, connectionOwnership);
                this.ClearCommand(command);
                result = dataReader;
            }
            catch
            {
                if (flag)
                {
                    connection.Close();
                }
                throw;
            }
            return result;
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            return this.ExecuteReader(connectionString, commandType, commandText, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual IDataReader ExecuteReader(string connectionString, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            IDbConnection dbConnection = null;
            IDataReader result;
            try
            {
                dbConnection = this.GetConnection(connectionString);
                dbConnection.Open();
                result = this.ExecuteReader(dbConnection, null, commandType, commandText, commandParameters, AdoHelper<TFactory>.AdoConnectionOwnership.Internal);
            }
            catch
            {
                if (dbConnection != null)
                {
                    dbConnection.Close();
                }
                throw;
            }
            return result;
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// IDataReader dr = helper.ExecuteReader(connString, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual IDataReader ExecuteReader(string connectionString, string spName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return this.ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                return this.ExecuteReader(connectionString, CommandType.StoredProcedure, spName, dataParameters);
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connectionString, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            return this.ExecuteReader(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// IDataReader dr = helper.ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders");
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>an IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText)
        {
            return this.ExecuteReader(connection, commandType, commandText, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// IDataReader dr = helper.ExecuteReader(conn, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>an IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual IDataReader ExecuteReader(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            return this.ExecuteReader(connection, null, commandType, commandText, commandParameters, AdoHelper<TFactory>.AdoConnectionOwnership.External);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// IDataReader dr = helper.ExecuteReader(conn, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual IDataReader ExecuteReader(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return this.ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                return this.ExecuteReader(connection, CommandType.StoredProcedure, spName, dataParameters);
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connection, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            return this.ExecuteReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <example><code>
        ///  IDataReader dr = helper.ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders");
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return this.ExecuteReader(transaction, commandType, commandText, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <example>
        /// <code>
        /// IDataReader dr = helper.ExecuteReader(trans, CommandType.StoredProcedure, "GetOrders", new IDataParameter("@prodid", 24));
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        public virtual IDataReader ExecuteReader(IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            }
            return this.ExecuteReader(transaction.Connection, transaction, commandType, commandText, commandParameters, AdoHelper<TFactory>.AdoConnectionOwnership.External);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// IDataReader dr = helper.ExecuteReader(tran, "GetOrders", 24, 36);
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>an IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual IDataReader ExecuteReader(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return this.ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                return this.ExecuteReader(transaction, CommandType.StoredProcedure, spName, dataParameters);
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(transaction.Connection, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            return this.ExecuteReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual object ExecuteScalar(IDbCommand command)
        {
            bool flag = false;
            this.CleanParameterSyntax(command);
            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                flag = true;
            }
            object result = command.ExecuteScalar();
            if (flag)
            {
                command.Connection.Close();
            }
            return result;
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <example>
        /// <code>
        /// int orderCount = (int)helper.ExecuteScalar(connString, CommandType.StoredProcedure, "GetOrderCount");
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual object ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            return this.ExecuteScalar(connectionString, commandType, commandText, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual object ExecuteScalar(string connectionString, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            IDbConnection dbConnection = null;
            object result;
            try
            {
                dbConnection = this.GetConnection(connectionString);
                dbConnection.Open();
                result = this.ExecuteScalar(dbConnection, commandType, commandText, commandParameters);
            }
            finally
            {
                IDisposable disposable = dbConnection;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            return result;
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// int orderCount = (int)helper.ExecuteScalar(connString, "GetOrderCount", 24, 36);
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual object ExecuteScalar(string connectionString, string spName, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return this.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                return this.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, dataParameters);
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connectionString, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            return this.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// int orderCount = (int)helper.ExecuteScalar(conn, CommandType.StoredProcedure, "GetOrderCount");
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText)
        {
            return this.ExecuteScalar(connection, commandType, commandText, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual object ExecuteScalar(IDbConnection connection, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            IDbCommand command = connection.CreateCommand();
            bool flag;
            this.PrepareCommand(command, connection, null, commandType, commandText, commandParameters, out flag);
            this.CleanParameterSyntax(command);
            object result = this.ExecuteScalar(command);
            if (flag)
            {
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// int orderCount = (int)helper.ExecuteScalar(conn, "GetOrderCount", 24, 36);
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual object ExecuteScalar(IDbConnection connection, string spName, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return this.ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                return this.ExecuteScalar(connection, CommandType.StoredProcedure, spName, dataParameters);
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connection, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            return this.ExecuteScalar(connection, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <example>
        /// <code>
        /// int orderCount = (int)helper.ExecuteScalar(tran, CommandType.StoredProcedure, "GetOrderCount");
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText)
        {
            return this.ExecuteScalar(transaction, commandType, commandText, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a 1x1 resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDbParamters used to execute the command</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual object ExecuteScalar(IDbTransaction transaction, CommandType commandType, string commandText, params IDataParameter[] commandParameters)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            }
            IDbCommand command = transaction.Connection.CreateCommand();
            bool flag;
            this.PrepareCommand(command, transaction.Connection, transaction, commandType, commandText, commandParameters, out flag);
            this.CleanParameterSyntax(command);
            return this.ExecuteScalar(command);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the specified
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// int orderCount = (int)helper.ExecuteScalar(tran, "GetOrderCount", 24, 36);
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the transaction is rolled back or commmitted</exception>
        public virtual object ExecuteScalar(IDbTransaction transaction, string spName, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                return this.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                return this.ExecuteScalar(transaction, CommandType.StoredProcedure, spName, dataParameters);
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(transaction.Connection, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            return this.ExecuteScalar(transaction, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <param name="dataSet">A DataSet wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual void FillDataset(IDbCommand command, DataSet dataSet, string[] tableNames)
        {
            bool flag = false;
            this.CleanParameterSyntax(command);
            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
                flag = true;
            }
            IDbDataAdapter dbDataAdapter = null;
            try
            {
                dbDataAdapter = this.GetDataAdapter();
                dbDataAdapter.SelectCommand = command;
                if (tableNames != null && tableNames.Length > 0)
                {
                    for (int i = 0; i < tableNames.Length; i++)
                    {
                        if (string.IsNullOrEmpty(tableNames[i]))
                        {
                            throw new ArgumentException("The tableNames parameter must contain a list of tables, a value was provided as null or empty string.", "tableNames");
                        }
                        dbDataAdapter.TableMappings.Add("Table" + ((i == 0) ? "" : i.ToString()), tableNames[i]);
                    }
                }
                dbDataAdapter.Fill(dataSet);
                if (flag)
                {
                    command.Connection.Close();
                }
            }
            finally
            {
                IDisposable disposable = dbDataAdapter as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the database specified in 
        /// the connection string. 
        /// </summary>
        /// <example>
        /// <code>
        /// helper.FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="dataSet">A DataSet wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        public virtual void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            IDbConnection dbConnection = null;
            try
            {
                dbConnection = this.GetConnection(connectionString);
                dbConnection.Open();
                this.FillDataset(dbConnection, commandType, commandText, dataSet, tableNames);
            }
            finally
            {
                IDisposable disposable = dbConnection;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <param name="dataSet">A DataSet wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual void FillDataset(string connectionString, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params IDataParameter[] commandParameters)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            IDbConnection dbConnection = null;
            try
            {
                dbConnection = this.GetConnection(connectionString);
                dbConnection.Open();
                this.FillDataset(dbConnection, commandType, commandText, dataSet, tableNames, commandParameters);
            }
            finally
            {
                IDisposable disposable = dbConnection;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// helper.FillDataset(connString, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"}, 24);
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>    
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual void FillDataset(string connectionString, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            IDbConnection dbConnection = null;
            try
            {
                dbConnection = this.GetConnection(connectionString);
                dbConnection.Open();
                this.FillDataset(dbConnection, spName, dataSet, tableNames, parameterValues);
            }
            finally
            {
                IDisposable disposable = dbConnection;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbConnection. 
        /// </summary>
        /// <example>
        /// <code>
        /// helper.FillDataset(conn, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>    
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual void FillDataset(IDbConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            this.FillDataset(connection, commandType, commandText, dataSet, tableNames, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameters.
        /// </summary>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="dataSet">A DataSet wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual void FillDataset(IDbConnection connection, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params IDataParameter[] commandParameters)
        {
            this.FillDataset(connection, null, commandType, commandText, dataSet, tableNames, commandParameters);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// helper.FillDataset(conn, "GetOrders", ds, new string[] {"orders"}, 24, 36);
        /// </code></example>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual void FillDataset(IDbConnection connection, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                this.FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames);
                return;
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                this.FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, dataParameters);
                return;
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connection, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            this.FillDataset(connection, CommandType.StoredProcedure, spName, dataSet, tableNames, spParameterSet);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset and takes no parameters) against the provided IDbTransaction. 
        /// </summary>
        /// <example>
        /// <code>
        /// helper.FillDataset(tran, CommandType.StoredProcedure, "GetOrders", ds, new string[] {"orders"});
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>    
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual void FillDataset(IDbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames)
        {
            this.FillDataset(transaction, commandType, commandText, dataSet, tableNames, null);
        }
        /// <summary>
        /// Execute an IDbCommand (that returns a resultset) against the specified IDbTransaction
        /// using the provided parameters.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="dataSet">A DataSet wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual void FillDataset(IDbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params IDataParameter[] commandParameters)
        {
            this.FillDataset(transaction.Connection, transaction, commandType, commandText, dataSet, tableNames, commandParameters);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified 
        /// IDbTransaction using the provided parameter values.  This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <remarks>
        /// This method provides no access to output parameters or the stored procedure's return value parameter.
        /// </remarks>
        /// <example>
        /// <code>
        /// helper.FillDataset(tran, "GetOrders", ds, new string[] {"orders"}, 24, 36);
        /// </code></example>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataSet">A dataset wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual void FillDataset(IDbTransaction transaction, string spName, DataSet dataSet, string[] tableNames, params object[] parameterValues)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (parameterValues == null || parameterValues.Length <= 0)
            {
                this.FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames);
                return;
            }
            IDataParameter[] dataParameters = this.GetDataParameters(parameterValues.Length);
            if (AdoHelper<TFactory>.AreParameterValuesIDataParameters(parameterValues, dataParameters))
            {
                this.FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, dataParameters);
                return;
            }
            bool includeReturnValueParameter = AdoHelper<TFactory>.CheckForReturnValueParameter(parameterValues);
            IDataParameter[] spParameterSet = this.GetSpParameterSet(transaction.Connection, spName, includeReturnValueParameter);
            this.AssignParameterValues(spParameterSet, parameterValues);
            this.FillDataset(transaction, CommandType.StoredProcedure, spName, dataSet, tableNames, spParameterSet);
        }
        /// <summary>
        /// Private helper method that execute an IDbCommand (that returns a resultset) against the specified IDbTransaction and IDbConnection
        /// using the provided parameters.
        /// </summary>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="transaction">A valid IDbTransaction</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="dataSet">A DataSet wich will contain the resultset generated by the command</param>
        /// <param name="tableNames">This array will be used to create table mappings allowing the DataTables to be referenced
        /// by a user defined name (probably the actual table name)
        /// </param>
        /// <param name="commandParameters">An array of IDataParameters used to execute the command</param>
        private void FillDataset(IDbConnection connection, IDbTransaction transaction, CommandType commandType, string commandText, DataSet dataSet, string[] tableNames, params IDataParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (dataSet == null)
            {
                throw new ArgumentNullException("dataSet");
            }
            IDbCommand command = connection.CreateCommand();
            bool flag;
            this.PrepareCommand(command, connection, transaction, commandType, commandText, commandParameters, out flag);
            this.CleanParameterSyntax(command);
            this.FillDataset(command, dataSet, tableNames);
            if (flag)
            {
                connection.Close();
            }
        }
        /// <summary>
        /// This method consumes the RowUpdatingEvent and passes it on to the consumer specifed in the call to UpdateDataset
        /// </summary>
        /// <param name="obj">The object that generated the event</param>
        /// <param name="e">The System.Data.Common.RowUpdatingEventArgs</param>
        protected void RowUpdating(object obj, RowUpdatingEventArgs e)
        {
            if (this.m_rowUpdating != null)
            {
                this.m_rowUpdating(obj, e);
            }
        }
        /// <summary>
        /// This method consumes the RowUpdatedEvent and passes it on to the consumer specifed in the call to UpdateDataset
        /// </summary>
        /// <param name="obj">The object that generated the event</param>
        /// <param name="e">The System.Data.Common.RowUpdatingEventArgs</param>
        protected void RowUpdated(object obj, RowUpdatedEventArgs e)
        {
            if (this.m_rowUpdated != null)
            {
                this.m_rowUpdated(obj, e);
            }
        }
        /// <summary>
        /// Set up a command for updating a DataSet.
        /// </summary>
        /// <param name="command">command object to prepare</param>
        /// <param name="mustCloseConnection">output parameter specifying whether the connection used should be closed by the DAAB</param>
        /// <returns>An IDbCommand object</returns>
        protected virtual IDbCommand SetCommand(IDbCommand command, out bool mustCloseConnection)
        {
            mustCloseConnection = false;
            if (command != null)
            {
                IDataParameter[] array = new IDataParameter[command.Parameters.Count];
                command.Parameters.CopyTo(array, 0);
                command.Parameters.Clear();
                this.PrepareCommand(command, command.Connection, null, command.CommandType, command.CommandText, array, out mustCloseConnection);
                this.CleanParameterSyntax(command);
            }
            return command;
        }
        /// <summary>
        /// Executes the respective command for each inserted, updated, or deleted row in the DataSet.
        /// </summary>
        /// <example>
        /// <code>
        /// helper.UpdateDataset(conn, insertCommand, deleteCommand, updateCommand, dataSet, "Order");
        /// </code></example>
        /// <param name="insertCommand">A valid SQL statement or stored procedure to insert new records into the data source</param>
        /// <param name="deleteCommand">A valid SQL statement or stored procedure to delete records from the data source</param>
        /// <param name="updateCommand">A valid SQL statement or stored procedure used to update records in the data source</param>
        /// <param name="dataSet">The DataSet used to update the data source</param>
        /// <param name="tableName">The DataTable used to update the data source.</param>
        public virtual void UpdateDataset(IDbCommand insertCommand, IDbCommand deleteCommand, IDbCommand updateCommand, DataSet dataSet, string tableName)
        {
            this.UpdateDataset(insertCommand, deleteCommand, updateCommand, dataSet, tableName, null, null);
        }
        /// <summary> 
        /// Executes the IDbCommand for each inserted, updated, or deleted row in the DataSet also implementing RowUpdating and RowUpdated Event Handlers 
        /// </summary> 
        /// <example> 
        /// <code>
        /// RowUpdatingEventHandler rowUpdatingHandler = new RowUpdatingEventHandler( OnRowUpdating ); 
        /// RowUpdatedEventHandler rowUpdatedHandler = new RowUpdatedEventHandler( OnRowUpdated ); 
        /// helper.UpdateDataSet(sqlInsertCommand, sqlDeleteCommand, sqlUpdateCommand, dataSet, "Order", rowUpdatingHandler, rowUpdatedHandler); 
        /// </code></example> 
        /// <param name="insertCommand">A valid SQL statement or stored procedure to insert new records into the data source</param> 
        /// <param name="deleteCommand">A valid SQL statement or stored procedure to delete records from the data source</param> 
        /// <param name="updateCommand">A valid SQL statement or stored procedure used to update records in the data source</param> 
        /// <param name="dataSet">The DataSet used to update the data source</param> 
        /// <param name="tableName">The DataTable used to update the data source.</param> 
        /// <param name="rowUpdatingHandler">RowUpdatingEventHandler</param> 
        /// <param name="rowUpdatedHandler">RowUpdatedEventHandler</param> 
        public void UpdateDataset(IDbCommand insertCommand, IDbCommand deleteCommand, IDbCommand updateCommand, DataSet dataSet, string tableName, AdoHelper<TFactory>.RowUpdatingHandler rowUpdatingHandler, AdoHelper<TFactory>.RowUpdatedHandler rowUpdatedHandler)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }
            IDbDataAdapter dbDataAdapter = null;
            try
            {
                dbDataAdapter = this.GetDataAdapter();
                bool flag;
                dbDataAdapter.UpdateCommand = this.SetCommand(updateCommand, out flag);
                bool flag2;
                dbDataAdapter.InsertCommand = this.SetCommand(insertCommand, out flag2);
                bool flag3;
                dbDataAdapter.DeleteCommand = this.SetCommand(deleteCommand, out flag3);
                this.AddUpdateEventHandlers(dbDataAdapter, rowUpdatingHandler, rowUpdatedHandler);
                if (dbDataAdapter is DbDataAdapter)
                {
                    try
                    {
                        ((DbDataAdapter)dbDataAdapter).Update(dataSet, tableName);
                        goto IL_8E;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                dbDataAdapter.TableMappings.Add(tableName, "Table");
                dbDataAdapter.Update(dataSet);
            IL_8E:
                dataSet.Tables[tableName].AcceptChanges();
                if (flag)
                {
                    updateCommand.Connection.Close();
                }
                if (flag2)
                {
                    insertCommand.Connection.Close();
                }
                if (flag3)
                {
                    deleteCommand.Connection.Close();
                }
            }
            finally
            {
                IDisposable disposable = dbDataAdapter as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }
        /// <summary>
        /// Simplify the creation of an IDbCommand object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <example>
        /// <code>
        /// IDbCommand command = helper.CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </code></example>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters</param>
        /// <returns>A valid IDbCommand object</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.InvalidOperationException">Thrown if any of the IDataParameters.ParameterNames are null, or if the parameter count does not match the number of values supplied</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentException">Thrown if the parameter count does not match the number of values supplied</exception>
        public virtual IDbCommand CreateCommand(string connectionString, string spName, params string[] sourceColumns)
        {
            return this.CreateCommand(this.GetConnection(connectionString), spName, sourceColumns);
        }
        /// <summary>
        /// Simplify the creation of an IDbCommand object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <example>
        /// <code>
        /// IDbCommand command = helper.CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </code></example>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="sourceColumns">An array of string to be assigned as the source columns of the stored procedure parameters</param>
        /// <returns>A valid IDbCommand object</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual IDbCommand CreateCommand(IDbConnection connection, string spName, params string[] sourceColumns)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            IDbCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandText = spName;
            dbCommand.CommandType = CommandType.StoredProcedure;
            if (sourceColumns != null && sourceColumns.Length > 0)
            {
                IDataParameter[] spParameterSet = this.GetSpParameterSet(connection, spName);
                for (int i = 0; i < sourceColumns.Length; i++)
                {
                    if (spParameterSet[i].SourceColumn == string.Empty)
                    {
                        spParameterSet[i].SourceColumn = sourceColumns[i];
                    }
                }
                this.AttachParameters(dbCommand, spParameterSet);
            }
            return dbCommand;
        }
        /// <summary>
        /// Simplify the creation of an IDbCommand object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandText">A valid SQL statement</param>
        /// <param name="commandType">A System.Data.CommandType</param>
        /// <param name="commandParameters">The parameters for the SQL statement</param>
        /// <returns>A valid IDbCommand object</returns>
        public virtual IDbCommand CreateCommand(string connectionString, string commandText, CommandType commandType, params IDataParameter[] commandParameters)
        {
            return this.CreateCommand(this.GetConnection(connectionString), commandText, commandType, commandParameters);
        }
        /// <summary>
        /// Simplify the creation of an IDbCommand object by allowing
        /// a stored procedure and optional parameters to be provided
        /// </summary>
        /// <example><code>
        /// IDbCommand command = helper.CreateCommand(conn, "AddCustomer", "CustomerID", "CustomerName");
        /// </code></example>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="commandText">A valid SQL statement</param>
        /// <param name="commandType">A System.Data.CommandType</param>
        /// <param name="commandParameters">The parameters for the SQL statement</param>
        /// <returns>A valid IDbCommand object</returns>
        public virtual IDbCommand CreateCommand(IDbConnection connection, string commandText, CommandType commandType, params IDataParameter[] commandParameters)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }
            IDbCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandText = commandText;
            dbCommand.CommandType = commandType;
            if (commandParameters != null && commandParameters.Length > 0)
            {
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    commandParameters[i].SourceColumn = commandParameters[i].ParameterName.TrimStart(new char[]
					{
						'@'
					});
                }
                this.AttachParameters(dbCommand, commandParameters);
            }
            return dbCommand;
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) 
        /// against the database specified in the connection string using the 
        /// dataRow column values as the stored procedure's parameters values.
        /// This method will assign the parameter values based on row values.
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual int ExecuteNonQueryTypedParams(IDbCommand command, DataRow dataRow)
        {
            this.CleanParameterSyntax(command);
            int result;
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                this.AssignParameterValues(command.Parameters, dataRow);
                result = this.ExecuteNonQuery(command);
            }
            else
            {
                result = this.ExecuteNonQuery(command);
            }
            return result;
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual int ExecuteNonQueryTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                IDataParameter[] spParameterSet = this.GetSpParameterSet(connectionString, spName);
                this.AssignParameterValues(spParameterSet, dataRow);
                return this.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return this.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, spName);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) against the specified IDbConnection 
        /// using the dataRow column values as the stored procedure's parameters values.  
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual int ExecuteNonQueryTypedParams(IDbConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                IDataParameter[] spParameterSet = this.GetSpParameterSet(connection, spName);
                this.AssignParameterValues(spParameterSet, dataRow);
                return this.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return this.ExecuteNonQuery(connection, CommandType.StoredProcedure, spName);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns no resultset) against the specified
        /// IDbTransaction using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An int representing the number of rows affected by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual int ExecuteNonQueryTypedParams(IDbTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (dataRow != null && dataRow.ItemArray.Length > 0)
            {
                IDataParameter[] spParameterSet = this.GetSpParameterSet(transaction.Connection, spName);
                this.AssignParameterValues(spParameterSet, dataRow);
                return this.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName, spParameterSet);
            }
            return this.ExecuteNonQuery(transaction, CommandType.StoredProcedure, spName);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will assign the paraemter values based on row values.
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual DataSet ExecuteDatasetTypedParams(IDbCommand command, DataRow dataRow)
        {
            this.CleanParameterSyntax(command);
            if (dataRow == null || dataRow.ItemArray.Length <= 0)
            {
                return this.ExecuteDataset(command);
            }
            this.AssignParameterValues(command.Parameters, dataRow);
            return this.ExecuteDataset(command);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual DataSet ExecuteDatasetTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (dataRow == null || dataRow.ItemArray.Length <= 0)
            {
                return this.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connectionString, spName);
            this.AssignParameterValues(spParameterSet, dataRow);
            return this.ExecuteDataset(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the dataRow column values as the store procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual DataSet ExecuteDatasetTypedParams(IDbConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (dataRow == null || dataRow.ItemArray.Length <= 0)
            {
                return this.ExecuteDataset(connection, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connection, spName);
            this.AssignParameterValues(spParameterSet, dataRow);
            return this.ExecuteDataset(connection, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbTransaction 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on row values.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A DataSet containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual DataSet ExecuteDatasetTypedParams(IDbTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (dataRow == null || dataRow.ItemArray.Length <= 0)
            {
                return this.ExecuteDataset(transaction, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] spParameterSet = this.GetSpParameterSet(transaction.Connection, spName);
            this.AssignParameterValues(spParameterSet, dataRow);
            return this.ExecuteDataset(transaction, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will assign the parameter values based on parameter order.
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual IDataReader ExecuteReaderTypedParams(IDbCommand command, DataRow dataRow)
        {
            this.CleanParameterSyntax(command);
            if (dataRow == null || dataRow.ItemArray.Length <= 0)
            {
                return this.ExecuteReader(command);
            }
            this.AssignParameterValues(command.Parameters, dataRow);
            return this.ExecuteReader(command);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual IDataReader ExecuteReaderTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (dataRow == null || dataRow.ItemArray.Length <= 0)
            {
                return this.ExecuteReader(connectionString, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connectionString, spName);
            this.AssignParameterValues(spParameterSet, dataRow);
            return this.ExecuteReader(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbConnection 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual IDataReader ExecuteReaderTypedParams(IDbConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (dataRow == null || dataRow.ItemArray.Length <= 0)
            {
                return this.ExecuteReader(connection, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connection, spName);
            this.AssignParameterValues(spParameterSet, dataRow);
            return this.ExecuteReader(connection, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a resultset) against the specified IDbTransaction 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>A IDataReader containing the resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual IDataReader ExecuteReaderTypedParams(IDbTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (dataRow == null || dataRow.ItemArray.Length <= 0)
            {
                return this.ExecuteReader(transaction, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] spParameterSet = this.GetSpParameterSet(transaction.Connection, spName);
            this.AssignParameterValues(spParameterSet, dataRow);
            return this.ExecuteReader(transaction, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will assign the parameter values based on parameter order.
        /// </summary>
        /// <param name="command">The IDbCommand to execute</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if command is null.</exception>
        public virtual object ExecuteScalarTypedParams(IDbCommand command, DataRow dataRow)
        {
            this.CleanParameterSyntax(command);
            if (dataRow == null || dataRow.ItemArray.Length <= 0)
            {
                return this.ExecuteScalar(command);
            }
            this.AssignParameterValues(command.Parameters, dataRow);
            return this.ExecuteScalar(command);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the database specified in 
        /// the connection string using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual object ExecuteScalarTypedParams(string connectionString, string spName, DataRow dataRow)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (dataRow == null || dataRow.ItemArray.Length <= 0)
            {
                return this.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connectionString, spName);
            this.AssignParameterValues(spParameterSet, dataRow);
            return this.ExecuteScalar(connectionString, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the specified IDbConnection 
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual object ExecuteScalarTypedParams(IDbConnection connection, string spName, DataRow dataRow)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (dataRow == null || dataRow.ItemArray.Length <= 0)
            {
                return this.ExecuteScalar(connection, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] spParameterSet = this.GetSpParameterSet(connection, spName);
            this.AssignParameterValues(spParameterSet, dataRow);
            return this.ExecuteScalar(connection, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Execute a stored procedure via an IDbCommand (that returns a 1x1 resultset) against the specified IDbTransaction
        /// using the dataRow column values as the stored procedure's parameters values.
        /// This method will query the database to discover the parameters for the 
        /// stored procedure (the first time each stored procedure is called), and assign the values based on parameter order.
        /// </summary>
        /// <param name="transaction">A valid IDbTransaction object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="dataRow">The dataRow used to hold the stored procedure's parameter values.</param>
        /// <returns>An object containing the value in the 1x1 resultset generated by the command</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if transaction.Connection is null</exception>
        public virtual object ExecuteScalarTypedParams(IDbTransaction transaction, string spName, DataRow dataRow)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction != null && transaction.Connection == null)
            {
                throw new ArgumentException("The transaction was rolled back or commited, please provide an open transaction.", "transaction");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            if (dataRow == null || dataRow.ItemArray.Length <= 0)
            {
                return this.ExecuteScalar(transaction, CommandType.StoredProcedure, spName);
            }
            IDataParameter[] spParameterSet = this.GetSpParameterSet(transaction.Connection, spName);
            this.AssignParameterValues(spParameterSet, dataRow);
            return this.ExecuteScalar(transaction, CommandType.StoredProcedure, spName, spParameterSet);
        }
        /// <summary>
        /// Checks for the existence of a return value parameter in the parametervalues
        /// </summary>
        /// <param name="parameterValues">An array of objects to be assigned as the input values of the stored procedure</param>
        /// <returns>true if the parameterValues contains a return value parameter, false otherwise</returns>
        private static bool CheckForReturnValueParameter(object[] parameterValues)
        {
            bool result = false;
            for (int i = 0; i < parameterValues.Length; i++)
            {
                object obj = parameterValues[i];
                if (obj is IDataParameter)
                {
                    IDataParameter dataParameter = (IDataParameter)obj;
                    if (dataParameter.Direction == ParameterDirection.ReturnValue)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// Check to see if the parameter values passed to the helper are, in fact, IDataParameters.
        /// </summary>
        /// <param name="parameterValues">Array of parameter values passed to helper</param>
        /// <param name="iDataParameterValues">new array of IDataParameters built from parameter values</param>
        /// <returns>True if the parameter values are IDataParameters</returns>
        private static bool AreParameterValuesIDataParameters(object[] parameterValues, IDataParameter[] iDataParameterValues)
        {
            bool result = true;
            for (int i = 0; i < parameterValues.Length; i++)
            {
                if (!(parameterValues[i] is IDataParameter))
                {
                    result = false;
                    break;
                }
                iDataParameterValues[i] = (IDataParameter)parameterValues[i];
            }
            return result;
        }
        /// <summary>
        /// Retrieves the set of IDataParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of IDataParameterParameters</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual IDataParameter[] GetSpParameterSet(string connectionString, string spName)
        {
            return this.GetSpParameterSet(connectionString, spName, false);
        }
        /// <summary>
        /// Retrieves the set of IDataParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of IDataParameters</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        public virtual IDataParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            IDataParameter[] spParameterSetInternal;
            using (IDbConnection connection = this.GetConnection(connectionString))
            {
                spParameterSetInternal = this.GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
            }
            return spParameterSetInternal;
        }
        /// <summary>
        /// Retrieves the set of IDataParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid IDataConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <returns>An array of IDataParameters</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual IDataParameter[] GetSpParameterSet(IDbConnection connection, string spName)
        {
            return this.GetSpParameterSet(connection, spName, false);
        }
        /// <summary>
        /// Retrieves the set of IDataParameters appropriate for the stored procedure
        /// </summary>
        /// <remarks>
        /// This method will query the database for this information, and then store it in a cache for future requests.
        /// </remarks>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of IDataParameters</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public virtual IDataParameter[] GetSpParameterSet(IDbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (!(connection is ICloneable))
            {
                throw new ArgumentException("cant discover parameters if the connection doesnt implement the ICloneable interface", "connection");
            }
            IDbConnection connection2 = (IDbConnection)((ICloneable)connection).Clone();
            return this.GetSpParameterSetInternal(connection2, spName, includeReturnValueParameter);
        }
        /// <summary>
        /// Retrieves the set of IDataParameters appropriate for the stored procedure
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">A bool value indicating whether the return value parameter should be included in the results</param>
        /// <returns>An array of IDataParameters</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        private IDataParameter[] GetSpParameterSetInternal(IDbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            string commandText = spName + (includeReturnValueParameter ? ":include ReturnValue Parameter" : "");
            IDataParameter[] array = this.GetCachedParameterSet(connection, commandText);
            if (array == null)
            {
                IDataParameter[] array2 = this.DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
                this.CacheParameterSet(connection, commandText, array2);
                array = AdoHelper<TFactory>.ADOHelperParameterCache.CloneParameters(array2);
            }
            return array;
        }
        /// <summary>
        /// Retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An array of IDataParameters</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        public IDataParameter[] GetCachedParameterSet(string connectionString, string commandText)
        {
            IDataParameter[] cachedParameterSetInternal;
            using (IDbConnection connection = this.GetConnection(connectionString))
            {
                cachedParameterSetInternal = AdoHelper<TFactory>.GetCachedParameterSetInternal(connection, commandText);
            }
            return cachedParameterSetInternal;
        }
        /// <summary>
        /// Retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An array of IDataParameters</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if commandText is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        public IDataParameter[] GetCachedParameterSet(IDbConnection connection, string commandText)
        {
            return AdoHelper<TFactory>.GetCachedParameterSetInternal(connection, commandText);
        }
        /// <summary>
        /// Retrieve a parameter array from the cache
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <returns>An array of IDataParameters</returns>
        private static IDataParameter[] GetCachedParameterSetInternal(IDbConnection connection, string commandText)
        {
            bool flag = false;
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                flag = true;
            }
            IDataParameter[] cachedParameterSet = AdoHelper<TFactory>.ADOHelperParameterCache.GetCachedParameterSet(connection.ConnectionString, commandText);
            if (flag)
            {
                connection.Close();
            }
            return cachedParameterSet;
        }
        /// <summary>
        /// Add parameter array to the cache
        /// </summary>
        /// <param name="connectionString">A valid connection string for an IDbConnection</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters to be cached</param>
        public void CacheParameterSet(string connectionString, string commandText, params IDataParameter[] commandParameters)
        {
            using (IDbConnection connection = this.GetConnection(connectionString))
            {
                AdoHelper<TFactory>.CacheParameterSetInternal(connection, commandText, commandParameters);
            }
        }
        /// <summary>
        /// Add parameter array to the cache
        /// </summary>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters to be cached</param>
        public void CacheParameterSet(IDbConnection connection, string commandText, params IDataParameter[] commandParameters)
        {
            if (connection is ICloneable)
            {
                using (IDbConnection dbConnection = (IDbConnection)((ICloneable)connection).Clone())
                {
                    AdoHelper<TFactory>.CacheParameterSetInternal(dbConnection, commandText, commandParameters);
                    return;
                }
            }
            throw new InvalidCastException();
        }
        /// <summary>
        /// Add parameter array to the cache
        /// </summary>
        /// <param name="connection">A valid IDbConnection</param>
        /// <param name="commandText">The stored procedure name or SQL command</param>
        /// <param name="commandParameters">An array of IDataParameters to be cached</param>
        private static void CacheParameterSetInternal(IDbConnection connection, string commandText, params IDataParameter[] commandParameters)
        {
            connection.Open();
            AdoHelper<TFactory>.ADOHelperParameterCache.CacheParameterSet(connection.ConnectionString, commandText, commandParameters);
            connection.Close();
        }
        /// <summary>
        /// Resolve at run time the appropriate set of IDataParameters for a stored procedure
        /// </summary>
        /// <param name="connection">A valid IDbConnection object</param>
        /// <param name="spName">The name of the stored procedure</param>
        /// <param name="includeReturnValueParameter">Whether or not to include their return value parameter</param>
        /// <returns>The parameter array discovered.</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if spName is null</exception>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connection is null</exception>
        private IDataParameter[] DiscoverSpParameterSet(IDbConnection connection, string spName, bool includeReturnValueParameter)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (string.IsNullOrEmpty(spName))
            {
                throw new ArgumentNullException("spName");
            }
            IDbCommand dbCommand = connection.CreateCommand();
            dbCommand.CommandText = spName;
            dbCommand.CommandType = CommandType.StoredProcedure;
            connection.Open();
            this.DeriveParameters(dbCommand);
            connection.Close();
            if (!includeReturnValueParameter && dbCommand.Parameters.Count > 0 && ((IDataParameter)dbCommand.Parameters[0]).Direction == ParameterDirection.ReturnValue)
            {
                dbCommand.Parameters.RemoveAt(0);
            }
            IDataParameter[] array = new IDataParameter[dbCommand.Parameters.Count];
            dbCommand.Parameters.CopyTo(array, 0);
            IDataParameter[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                IDataParameter dataParameter = array2[i];
                dataParameter.Value = DBNull.Value;
            }
            return array;
        }
        /// <summary>
        /// Returns an IDbConnection object for the given connection string
        /// </summary>
        /// <param name="connectionString">The connection string to be used to create the connection</param>
        /// <returns>An IDbConnection object</returns>
        /// <exception cref="T:System.ArgumentNullException">Thrown if connectionString is null</exception>
        public virtual IDbConnection GetConnection(string connectionString)
        {
            TFactory tFactory = this.factory;
            IDbConnection dbConnection = tFactory.CreateConnection();
            dbConnection.ConnectionString = connectionString;
            return dbConnection;
        }
        /// <summary>
        /// Returns an IDbDataAdapter object
        /// </summary>
        /// <returns>The IDbDataAdapter</returns>
        public virtual IDbDataAdapter GetDataAdapter()
        {
            TFactory tFactory = this.factory;
            return tFactory.CreateDataAdapter();
        }
        /// <summary>
        /// Calls the CommandBuilder.DeriveParameters method for the specified provider, doing any setup and cleanup necessary
        /// </summary>
        /// <param name="cmd">The IDbCommand referencing the stored procedure from which the parameter information is to be derived. The derived parameters are added to the Parameters collection of the IDbCommand. </param>
        public virtual void DeriveParameters(IDbCommand cmd)
        {
        }
        /// <summary>
        /// Returns an IDataParameter object
        /// </summary>
        /// <returns>The IDataParameter object</returns>
        public virtual IDataParameter GetParameter()
        {
            TFactory tFactory = this.factory;
            return tFactory.CreateParameter();
        }
        /// <summary>
        /// Provider specific code to set up the updating/ed event handlers used by UpdateDataset
        /// </summary>
        /// <param name="dataAdapter">DataAdapter to attach the event handlers to</param>
        /// <param name="rowUpdatingHandler">The handler to be called when a row is updating</param>
        /// <param name="rowUpdatedHandler">The handler to be called when a row is updated</param>
        protected virtual void AddUpdateEventHandlers(IDbDataAdapter dataAdapter, AdoHelper<TFactory>.RowUpdatingHandler rowUpdatingHandler, AdoHelper<TFactory>.RowUpdatedHandler rowUpdatedHandler)
        {
        }
        /// <summary>
        /// Returns an array of IDataParameters of the specified size
        /// </summary>
        /// <param name="size">size of the array</param>
        /// <returns>The array of IDataParameters</returns>
        protected virtual IDataParameter[] GetDataParameters(int size)
        {
            return null;
        }
        /// <summary>
        /// Handle any provider-specific issues with BLOBs here by "washing" the IDataParameter and returning a new one that is set up appropriately for the provider.
        /// </summary>
        /// <param name="connection">The IDbConnection to use in cleansing the parameter</param>
        /// <param name="p">The parameter before cleansing</param>
        /// <returns>The parameter after it's been cleansed.</returns>
        protected virtual IDataParameter GetBlobParameter(IDbConnection connection, IDataParameter p)
        {
            return p;
        }
    }
}
