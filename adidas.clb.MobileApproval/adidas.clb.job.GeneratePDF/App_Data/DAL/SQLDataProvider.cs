using adidas.clb.job.GeneratePDF.Models;
using adidas.clb.job.GeneratePDF.Exceptions;
using adidas.clb.job.GeneratePDF.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
namespace adidas.clb.job.GeneratePDF.App_Data.DAL
{
    class SQLDataProvider
    {
        #region "Private Members"
        //Variable for sql connection object
        private SqlConnection sqlConnection;
        //Variable for sql transaction object
        private SqlTransaction sqlTransaction;
        //Variable for sql command object
        private SqlCommand sqlCommand;
        //Variable with the reference to the log writer for the application
        //private LogWriter writer = EnterpriseLibraryContainer.Current.GetInstance<LogWriter>();
        #endregion

        #region "Properties"
        /// <summary>
        /// Property for retreiving SQL Connection
        /// </summary>
        public SqlConnection Connection
        {
            get { return sqlConnection; }
        }

        /// <summary>
        /// Property for retreiving SQL Transaction
        /// </summary>
        public SqlTransaction Transaction
        {
            get { return sqlTransaction; }
        }

        /// <summary>
        /// Property for retreiving SQL Command
        /// </summary>
        public SqlCommand Command
        {
            get { return sqlCommand; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Opens a new database connection
        /// </summary>
        public void OpenConnection(string strConnectionString)
        {
            //Create a new sql Connection using the connection string
            this.sqlConnection = new SqlConnection(strConnectionString);
            //Check if the sql connection is open
            if (this.sqlConnection.State != ConnectionState.Open)
            {
                //Open the sql connection
                this.sqlConnection.Open();
            }
        }

        /// <summary>
        /// Close an existing Transaction
        /// </summary>
        public void DisposeConnection()
        {
            //Close Connection
            this.CloseConnection();
            //Set object instances to null
            this.sqlCommand = null;
            this.sqlConnection = null;
        }

        /// <summary>
        /// Returns a dataset object for the executed query
        /// </summary>
        /// <param name="commandType">Determines the command type</param>
        /// <param name="strCommandText">Input query string</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteDataSet(CommandType commandType, string strCommandText, SqlParameter[] sqlParameters)
        {

            //Validate input parameters
            if (string.IsNullOrEmpty(strCommandText) || (null == sqlParameters))
            {
                string message = String.Empty;

                if (string.IsNullOrEmpty(strCommandText))
                    message = string.Format(CoreConstants.INVALIDPARAMETER_MSG, "strCommandText");
                else if (null == sqlParameters)
                    message = string.Format(CoreConstants.INVALIDPARAMETER_MSG, "sqlParameters");
                DataAccessException ex = new DataAccessException(message);
                throw ex;
            }            
            //Prepare sql command object
            this.PrepareCommand(commandType, strCommandText, sqlParameters);
            //Instantiate new sql data adapter object
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(Command);
            //instantiate data set
            DataSet dataSet = new DataSet();
            //Fill data adapter with data set
            sqlDataAdapter.Fill(dataSet);

            CommitTransaction();

            return dataSet;
        }

        /// <summary>
        /// Used for insert, update and delete transactions. Returns the number of rows affected
        /// </summary>
        /// <param name="commandType">Determines the command type</param>
        /// <param name="strCommandText">Input query string</param>
        /// <returns>Number of rows affected</returns>
        public int ExecuteNonQuery(CommandType commandType, String strCommandText, SqlParameter[] sqlParameters)
        {

            //Validate input parameters
            if (string.IsNullOrEmpty(strCommandText) || (null == sqlParameters))
            {
                string message = String.Empty;

                if (string.IsNullOrEmpty(strCommandText))
                    message = string.Format(CoreConstants.INVALIDPARAMETER_MSG, "strCommandText");
                else if (null == sqlParameters)
                    message = string.Format(CoreConstants.INVALIDPARAMETER_MSG, "sqlParameters");
                DataAccessException ex = new DataAccessException(message);
                throw ex;
            }

            //prepare sql command object
            this.PrepareCommand(commandType, strCommandText, sqlParameters);
            //Call execute non query method on the sql command
            int returnValue = Command.ExecuteNonQuery();
            //Commit transaction
            this.CommitTransaction();

            return returnValue;
        }

        /// <summary>
        /// Returns the count of the executed transaction
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="strCommandText"></param>
        /// <param name="sqlParameters"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public object ExecuteScalar(CommandType commandType, String strCommandText, SqlParameter[] sqlParameters)
        {

            //Validate input parameters
            if (string.IsNullOrEmpty(strCommandText) || (null == sqlParameters))
            {
                string message = String.Empty;

                if (string.IsNullOrEmpty(strCommandText))
                    message = string.Format(CoreConstants.INVALIDPARAMETER_MSG, "strCommandText");
                else if (null == sqlParameters)
                    message = string.Format(CoreConstants.INVALIDPARAMETER_MSG, "sqlParameters");
                DataAccessException ex = new DataAccessException(message);
                throw ex;
            }

            //prepare sql command object
            this.PrepareCommand(commandType, strCommandText, sqlParameters);
            //Call execute non query method on the sql command
            object returnValue = Command.ExecuteScalar();
            //Commit transaction
            this.CommitTransaction();
            return returnValue;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Closes an existing database connection
        /// </summary>
        private void CloseConnection()
        {
            //Check if the sql connection object instance is null
            if (this.sqlConnection != null)
            {
                //Check if the sql connection is closed
                if (this.sqlConnection.State != ConnectionState.Closed)
                {
                    this.sqlConnection.Close();
                }
            }
        }

        /// <summary>
        /// Open a new Transaction
        /// </summary>
        private void OpenTransaction()
        {
            //Check if the sql transcation object instance is null
            if (this.sqlTransaction == null)
            {
                //Begin sql transaction
                this.sqlTransaction = Connection.BeginTransaction();
            }
        }

        /// <summary>
        /// Commit the transaction
        /// </summary>
        private void CommitTransaction()
        {
            //Check if the sql transaction object instance is null
            if (this.sqlTransaction != null)
            {
                //Commit the sql transaction
                this.sqlTransaction.Commit();
                this.sqlTransaction = null;
            }
        }

        /// <summary>
        /// This method prepares the command object
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="strCommandText"></param>
        /// <param name="sqlParameters"></param>
        private void PrepareCommand(CommandType commandType, string strCommandText, SqlParameter[] sqlParameters)
        {
            //Check if the sql connection object is null
            if (this.sqlConnection != null)
            {
                //Create a new sql command object using the stored procedure name and sql connection
                this.sqlCommand = new SqlCommand(strCommandText, Connection);

                //This would decide the sql command type for e.g stored procedure or text
                this.sqlCommand.CommandType = commandType;

                //Check if sql parameter count is greater than zero
                if (sqlParameters != null && sqlParameters.Length > 0)
                {
                    foreach (SqlParameter sqlParameter in sqlParameters)
                    {
                        sqlCommand.Parameters.Add(sqlParameter);
                    }
                }

                //Open a new transaction
                this.OpenTransaction();

                //Check if the sql transaction object instance is null
                if (this.sqlTransaction != null)
                {
                    //Assign the sql transaction to the command object
                    this.sqlCommand.Transaction = Transaction;
                }
            }
        }
        #endregion 
    }
}
