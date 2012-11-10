using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;


namespace WindowsFormsApplication2
{
    public sealed class StorageResource
    {
        static StorageResource instance = null;
        static readonly object padlock = new object();
        private SqlConnection connection;
        private bool needStoredProc;
        private string connectionString;
        private DataSet globDataSet;


       
        public List<string> getListOfTables()
        {
            List<string> tables = new List<string>();
            DataTable dt = connection.GetSchema("Tables");
            foreach (DataRow row in dt.Rows)
            {
                string tablename = (string)row[2];
                tables.Add(tablename);
            }
            return tables;
        }
      
        public int getNumberOfRowsForTable(string name) {
            string queryString = "select * from " + name;
            SqlCommand command = new SqlCommand(queryString, connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataSet ds = new DataSet();

            adapter.Fill(ds, name);
       
            return ds.Tables[name].Rows.Count;  
        }

        public string getColumntNameByIndexFromTable(int index, string tableName)
        {
            string queryString = "select column_name from information_schema.columns where table_name = '"+tableName+"' order by ordinal_position";
            string column = null;
            SqlCommand command = new SqlCommand(queryString, connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataSet ds = new DataSet();

            adapter.Fill(ds, tableName);
            Console.WriteLine("added to dataset " + tableName);
             column = ds.Tables[tableName].Rows[index][0].ToString();
           
            return column;
        }

        public List<string> getAllColumnNamesFromTable(string tableName)
        {
            var columns = new List<string>();
            string queryString = "select column_name from information_schema.columns where table_name = '" + tableName + "' order by ordinal_position";
            SqlCommand command = new SqlCommand(queryString, connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataSet ds = new DataSet();
        
                adapter.Fill(ds, tableName);
                Console.WriteLine("added to dataset "+tableName);
          
            foreach (DataRow produs in ds.Tables[tableName].Rows)
            {
                columns.Add(produs[0].ToString());
                Console.WriteLine("Achtung !!!!! " + produs[0]);
            }
            
            return columns;
        }
        public string getPKColumnFromTable(string table)
        {
            string queryString = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE TABLE_NAME = '" + table + "'";
            string column = null;
           SqlCommand cmd = new SqlCommand(queryString, connection);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            
         //   if(!globDataSet.Tables.Contains(table))
                adapter.Fill(ds, table);
            foreach (DataRow produs in ds.Tables[table].Rows)
            {
                column = produs[0].ToString();
                Console.WriteLine("Achtung !!!!! " + produs[0].ToString());
            }

            /* 
            DataSet ds = new DataSet();
            var command = new SqlCommand(queryString, connection);
            var reader = command.ExecuteReader();
           
            while (reader.Read())
            {
                column = (string)reader[0];
            }
            reader.Close();
           
            * 
            */
            return column;

        }

        public int getNumberOfColumns(string tableName) {
            int rez = 0;
            if (!NeedStoredProc)
            {
                string queryString = "SELECT count(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'" + tableName +
                                     "')";
                var command = new SqlCommand(queryString, connection);
                int count = (Int32)command.ExecuteScalar();
                rez = count;
                Console.WriteLine("\nsimple query");
            }
            else
            {

                var command = new SqlCommand();
                command.Connection = connection;

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "getColumns";

                command.Parameters.Add(new SqlParameter("@tableName", SqlDbType.Text));
                command.Parameters.Add(new SqlParameter("@rezult", SqlDbType.Int));

                command.Parameters["@tableName"].Value = tableName;

                command.Parameters["@rezult"].Direction = ParameterDirection.Output;

                if (command.Connection.State != ConnectionState.Open)
                    command.Connection.Open();

                command.ExecuteNonQuery();

                rez = (int)command.Parameters["@rezult"].Value;
                Console.WriteLine("\nstored PRocedure");
            }
            return rez;
            
        }
        public List<int>getAllPKFromTable(string tableName)
        {            
            List<int>pKeys = new List<int>();
            string pk = getPKColumnFromTable(tableName);
            var queryString = "select "+ pk+" from "+tableName;
                var command = new SqlCommand(queryString, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataSet ds = new DataSet();

                adapter.Fill(ds, tableName);
                Console.WriteLine("added to dataset " + tableName);

                foreach (DataRow produs in ds.Tables[tableName].Rows)
                {
                    try
                    {
                        pKeys.Add(int.Parse(produs[0].ToString()));
                      Console.WriteLine("Achtung !!!!! " + produs[0]);
                    }
                    catch (Exception)
                    {
                        
                    }
                   
                }
            
            return pKeys;
        }

        public Dictionary<string,string> getRowValues(int index,string tableName)
        {
            var values = new Dictionary<string,string>();
            var columnNames = getAllColumnNamesFromTable(tableName);
            string pk = this.getPKColumnFromTable(tableName);
            int count = getNumberOfColumns(tableName);
            string value = "";
            string queryString = "SELECT * FROM " + tableName + " where " + pk + " = " + index;
            SqlCommand command = new SqlCommand(queryString, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataSet ds = new DataSet();

            adapter.Fill(ds, tableName);
           
            for (int i = 0; i < ds.Tables[tableName].Rows.Count; i++)
                for (int j = 0; j < ds.Tables[tableName].Columns.Count;j++ )
                {
                    value = ds.Tables[tableName].Rows[i][j].ToString();
                    values.Add(columnNames[j], value);

                }

            return values;

        }
        public void updateOrInsertRow(Dictionary<string,string >values, string tableName)
        {
            try
            {


                var dataTypeArray = getColumnTypesForTable(tableName);
                var columns = getAllColumnNamesFromTable(tableName);
                string pk = this.getPKColumnFromTable(tableName);
                string pkComingValue = values[pk];
                string rezult;
                string queryString = null;
                SqlCommand command;
                SqlDataReader reader;
                if (pkComingValue.Length > 0)
                {
                    queryString = "SELECT * FROM " + tableName + " where " + pk + "=" + pkComingValue;
                    command = new SqlCommand(queryString, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds, tableName);
                    rezult = ds.Tables[tableName].Rows[0][pk].ToString();
                    
                }
                else
                {
                    rezult = "";
                }

                Console.WriteLine("selected primary key is " + rezult);
                var dict = new Dictionary<string, string>(values);
                dict.Remove(pk);
                string queryBody = null;
                int i = 0;
                int count = dict.Keys.Count;
                if (rezult.Length > 0)
                {
                    /*
                     * we found an primary key => we will make an UPDATE
                     */

                    foreach (var key in dict.Keys)
                    {
                        i++;
                        if (i < count)
                        {
                            switch (dataTypeArray[key])
                            {
                                case 1:
                                    queryBody += key + "=" + dict[key] + ", ";
                                    break;
                                case 2:
                                    queryBody += key + "='" + dict[key] + "', ";
                                    break;
                            }

                        }
                        else
                        {
                            switch (dataTypeArray[key])
                            {
                                case 1:
                                    queryBody += key + "=" + dict[key];
                                    break;
                                case 2:
                                    queryBody += key + "='" + dict[key] + "'";
                                    break;
                            }

                        }

                    }

                    queryString = "UPDATE " + tableName + " set " + queryBody + " WHERE " + pk + "=" + pkComingValue;
                    Console.WriteLine("on update query is " + queryString);


                }
                else
                {
                    /*
                     * nothing was found, so, INSERT
                     * 
                     */
                    string cols = null;
                    foreach (var key in dict.Keys)
                    {
                        i++;
                        if (i < count)
                        {
                            cols += key + ", ";
                            switch (dataTypeArray[key])
                            {
                                case 1:
                                    queryBody += dict[key] + ", ";
                                    break;
                                case 2:
                                    queryBody += "'" + dict[key] + "', ";
                                    break;
                            }

                        }
                        else
                        {
                            cols += key;
                            switch (dataTypeArray[key])
                            {
                                case 1:
                                    queryBody += dict[key];
                                    break;
                                case 2:
                                    queryBody += "'" + dict[key] + "'";
                                    break;

                            }

                        }

                        queryString = "INSERT into " + tableName + "(" + cols + ")" + "Values(" + queryBody + ")";
                        Console.WriteLine("on insert query is " + queryString);
                    }
                }

                command = new SqlCommand(queryString, connection);
                command.ExecuteNonQuery();
            }
            
            catch (Exception)
            {
                
                
            }



        }

        public void deleteRecordFromTableWithIndex(string tableName,int index)
        {
            try
            {
                string pk = this.getPKColumnFromTable(tableName);
                var array = getAllPKFromTable(tableName);
                int value = array[index];
                var sqlquery = "Delete from " + tableName + " where " + pk + " = " + value;
                var command = new SqlCommand(sqlquery, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception)
            {


            }

        }
        public Dictionary<string, int> getColumnTypesForTable(string tableName)
        {
            var rezultList = new Dictionary<string, int>();
            string pk = this.getPKColumnFromTable(tableName);
            var columns = getAllColumnNamesFromTable(tableName);
            string queryString = "select TOP 1 * from " + tableName;
            var command = new SqlCommand(queryString, connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataSet ds = new DataSet();
            string column = "";
            adapter.Fill(ds, tableName);
            for (int i = 0; i < ds.Tables[tableName].Rows.Count; i++)
                for (int j = 0; j < ds.Tables[tableName].Columns.Count; j++)
                {
                    Type type = ds.Tables[tableName].Columns[j].DataType;
                    column = columns[j];
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.DateTime:
                            rezultList.Add(column, 3);
                            break;
                        case TypeCode.String:
                            rezultList.Add(column, 2);
                            break;
                        case TypeCode.Int32:
                            rezultList.Add(column, 1);
                            break;
                        default:
                            rezultList.Add(column, 1);
                            break;
                    }
                }


            return rezultList;
        }
    
        StorageResource()
        {
            try
            {
                connectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=c:\gara_auto.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True";
                //globDataSet = new DataSet();
                connection = new SqlConnection(connectionString);
                connection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("exeprion in constructor\t"+e.ToString());
              
            }
            
        }

        public static StorageResource Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new StorageResource();
                    }
                    return instance;
                }
            }
        }

        public bool NeedStoredProc
        {
            get { return needStoredProc; }
            set { needStoredProc = value; }
        }
    }
}
