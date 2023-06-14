namespace SQLConnect
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Data;
    using System.Data.SqlClient;

    /// <summary>
    /// Used to connect to the Test Database to retrive data
    /// </summary>
    class DBConnect{
        private string server;
        private string database;
        private string connectionString;
        private SqlConnection connection;
        public static DBConnect instance {set; get;}

        /// <summary>
        /// Constructor
        /// </summary>
        private DBConnect(){
            server = "localhost,1433";
            database = "Test";
            connectionString = $"data source=tcp:{server}; initial catalog={database};trusted_connection=True;";
            connection = new SqlConnection(connectionString);
        }

        /// <summary>
        /// Get the instance of the DBConnect <br/>
        /// Singleton Pattern
        /// </summary>
        /// <returns>DBConnect instance</returns>
        public static DBConnect Instance{
            get{
                instance = instance ?? new DBConnect();
                return instance;
            }
        }

        /// <summary>
        /// Open connection
        /// </summary>
        /// <returns>bool if connect worked</returns>
        private bool OpenConnection(){
            try{
                connection.Open();
                return true;
            }
            catch(SqlException ex){
                //When handling errors, you can switch your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch(ex.Number){
                    case 0:
                        Logger.Instance.log("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Logger.Instance.log("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        /// <summary>
        /// Close connection
        /// </summary>
        /// <returns>bool if disconnect worked</returns>
        private bool CloseConnection(){
            try{
                connection.Close();
                return true;
            }
            catch(SqlException ex){
                Logger.Instance.log(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Insert statement
        /// </summary>
        /// <param name="query"> The SQL query</param>    
        public void Insert(string query){
            //open connection
            if(this.OpenConnection()){
                //create command and assign the query and connection from the constructor
                SqlCommand cmd = new SqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Update statement
        /// </summary>
        /// <param name="query">The SQL query</param>
        public void Update(string query){
            //Open connection
            if(this.OpenConnection()){
                //create mysql command
                
                SqlCommand cmd = new SqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                //Execute query
                int affectedRows = cmd.ExecuteNonQuery();
                //Logger.Instance.log($"Updated {affectedRows} rows");

                //close connection
                this.CloseConnection();
                return;
            }
        }

        /// <summary>
        /// Delete statement
        /// </summary>
        /// <param name="query">The SQL query</param>
        public void Delete(string query){
            if(this.OpenConnection() == true){
                SqlCommand cmd = new SqlCommand(query, connection);
                int affectedRows = cmd.ExecuteNonQuery();
                Logger.Instance.log($"Deleted {affectedRows} rows");
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Select statement
        /// </summary>
        /// <param name="query">The SQL query</param>
        /// <returns>DataTable with the fetched Items</returns>
        public async Task<DataTable> Select(string query){
            //Create a list to store the result
            DataTable dt = new DataTable();
            
            //Open connection
            if(this.OpenConnection()){
                
                return await Task.Run(() => {
                    //Create Command
                    SqlCommand cmd = new SqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    SqlDataReader dataReader = cmd.ExecuteReader();
                    
                    //Read the data and store them in the list
                    dt.Load(dataReader);

                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    this.CloseConnection();
                    
                    //return list
                    return dt;
                });
            }
            else{
                return dt;
            }
        }

        /// <summary>
        /// ReadInt
        /// </summary>
        /// <param name="ColName">Name of the Column</param>
        /// <param name="row">DataRow</param>
        /// <returns>int or null</returns>
        private int ReadInt(string ColName, DataRow row){
            if(row[ColName] == DBNull.Value){
                return 0;
            }
            else{
                return (int)row[ColName];
            }
        }

        /// <summary>
        /// ReadString
        /// </summary>
        /// <param name="ColName">Name of the Column</param>
        /// <param name="row">DataRow</param>
        /// <returns>string or null</returns>
        private string ReadString(string ColName, DataRow row){
            if(row[ColName] == DBNull.Value){
                return null;
            }
            else{
                return row[ColName].ToString();
            }
        }

        /// <summary>
        /// ReadDouble
        /// </summary>
        /// <param name="ColName">Name of the Column</param>
        /// <param name="row">DataRow</param>
        /// <returns>double or null</returns>
        private double? ReadDouble(string ColName, DataRow row){
            if(row[ColName] == DBNull.Value){
                return null;
            }
            else{
                return (double?)row[ColName];
            }
        }

        /// <summary>
        /// ReadBool
        /// </summary>
        /// <param name="ColName">Name of the Column</param>
        /// <param name="row">DataRow</param>
        /// <returns>bool or null</returns>
        private bool? ReadBool(string ColName, DataRow row){
            if(row[ColName] == DBNull.Value){
                return null;
            }
            else{
                return (bool?)row[ColName];
            }
        }

        /// <summary>
        /// ReadDateTime
        /// </summary>
        /// <param name="ColName">Name of the Column</param>
        /// <param name="row">DataRow</param>
        /// <returns>DateTime or null</returns>
        private DateTime? ReadDateTime(string ColName, DataRow row){
            if(row[ColName] == DBNull.Value){
                return null;
            }
            else{
                return (DateTime?)row[ColName];
            }
        }
    }
}