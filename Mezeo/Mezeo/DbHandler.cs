using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Finisar.SQLite;

namespace Mezeo
{
    class DbHandler
    {
        //private string ConnectionString = "Data Source=DemoT.s3db;Version=3;New=false;Compress=True;";
        private const string DATABASE_NAME = "mezeoDb.s3db";
        private const string TABLE_NAME = "FileStructInfo";

        public const string KEY = "key";
        public const string MODIFIED_DATE = "modified_date";
        public const string CREATED_DATE = "created_date";
        public const string FILE_SIZE = "file_size";
        public const string CONTENT_URL = "content_url";
        public const string PARENT_URL = "parent_url";
        public const string E_TAG = "e_tag";
        public const string FILE_NAME = "file_name";
        public const string MIMIE_TYPE = "mime_type";
        public const string PUBLIC = "public";
        public const string SHARED = "shared";
        public const string STATUS = "status";
        public const string PARENT_DIR = "parent_dir";

        private SQLiteConnection sqlConnection;
        private SQLiteCommand sqlCommand;
        private SQLiteDataReader sqlDataReader;

        public bool OpenConnection()
        {
            string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + AboutBox.AssemblyTitle + "\\";
            
            bool createNew = false;
            
            if (!System.IO.File.Exists(dbPath + DATABASE_NAME))
            {
                System.IO.Directory.CreateDirectory(dbPath);
                createNew = true;
            }

            sqlConnection = new SQLiteConnection("Data Source=" + dbPath + DATABASE_NAME + ";Version=3;New=" + createNew + ";Compress=True;");
            sqlConnection.Open();

            if (createNew)
            {
                CreateTables();
            }

            return createNew;
        }


        private void CreateTables()
        {
            string query = "CREATE TABLE " + TABLE_NAME + " (" + 
                            KEY + " TEXT PRIMARY KEY, " + 
                            MODIFIED_DATE + " DATE, " +
                            CREATED_DATE + " DATE, " +
                            FILE_SIZE + " INTEGER, " +
                            CONTENT_URL + " TEXT, " +
                            PARENT_URL + " TEXT, " +
                            E_TAG + " TEXT, " +
                            FILE_NAME + " TEXT, " +
                            MIMIE_TYPE + " TEXT, " +
                            PUBLIC + " BOOL, " +
                            SHARED + " BOOL, " +
                            STATUS + " TEXT, " +
                            PARENT_DIR + " TEXT)";

            ExecuteNonQuery(query);
        }

        public SQLiteDataReader ExecuteQuery(string query)
        {
            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();

            return sqlDataReader;
        }

        public bool ExecuteNonQuery(string query)
        {
            try
            {
                sqlCommand = new SQLiteCommand(query, sqlConnection);
                sqlCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string GetString(string tableName, string fieldName)
        {
            string query = "select " + fieldName + " from " + tableName;
            string result = "";

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            sqlDataReader.Read();

            result = sqlDataReader.GetString(0);
            
            sqlDataReader.Close();
            
            return result;
        }

        public int GetInt(string tableName, string fieldName)
        {
            string query = "select " + fieldName + " from " + tableName;
            int result = -1;

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            sqlDataReader.Read();

            result = sqlDataReader.GetInt32(0);

            sqlDataReader.Close();

            return result;
        }

        public decimal GetFloat(string tableName, string fieldName)
        {
            string query = "select " + fieldName + " from " + tableName;
            decimal result = -1;

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            sqlDataReader.Read();

            result = sqlDataReader.GetDecimal(0);

            sqlDataReader.Close();

            return result;
        }

        public bool GetBoolean(string tableName, string fieldName)
        {
            string query = "select " + fieldName + " from " + tableName;
            bool result = false;

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            sqlDataReader.Read();

            result = sqlDataReader.GetBoolean(0);

            sqlDataReader.Close();

            return result;
        }

        public DateTime GetDateTime(string tableName, string fieldName)
        {
            string query = "select " + fieldName + " from " + tableName;
            DateTime result;

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            sqlDataReader.Read();

            result = sqlDataReader.GetDateTime(0);

            sqlDataReader.Close();

            return result;
        }


        public List<string> GetStringList(string tableName, string fieldName)
        {
            string query = "select " + fieldName + " from " + tableName;
            List<string> result =new List<string>();

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            while (sqlDataReader.Read())
            {
                result.Add(sqlDataReader.GetString(0));
            }
            sqlDataReader.Close();

            return result;
        }

        public List<int> GetIntList(string tableName, string fieldName)
        {
            string query = "select " + fieldName + " from " + tableName;
            List<int> result = new List<int>();

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            while(sqlDataReader.Read())
            {
                result.Add(sqlDataReader.GetInt32(0));
            }
            sqlDataReader.Close();

            return result;
        }

        public List<decimal> GetFloatList(string tableName, string fieldName)
        {
            string query = "select " + fieldName + " from " + tableName;
            List<decimal> result = new List<decimal>();

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            while(sqlDataReader.Read())
            {
                result.Add(sqlDataReader.GetDecimal(0));
            }
            sqlDataReader.Close();

            return result;
        }

        public List<bool> GetBooleanList(string tableName, string fieldName)
        {
            string query = "select " + fieldName + " from " + tableName;
            List<bool> result = new List<bool>();

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            while (sqlDataReader.Read())
            {
                result.Add(sqlDataReader.GetBoolean(0));
            }
            sqlDataReader.Close();

            return result;
        }

        public List<DateTime> GetDateTimeList(string tableName, string fieldName)
        {
            string query = "select " + fieldName + " from " + tableName;
            List<DateTime> result=new List<DateTime>();

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            while (sqlDataReader.Read())
            {
                result.Add(sqlDataReader.GetDateTime(0));
            }
            sqlDataReader.Close();

            return result;
        }
    }
}
