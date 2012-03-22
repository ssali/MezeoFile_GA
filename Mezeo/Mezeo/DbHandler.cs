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
        public const string TABLE_NAME = "FileStructInfo";

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
        public const string TYPE = "type";

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

        public void DeleteDb()
        {
            string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + AboutBox.AssemblyTitle + "\\";

            if (System.IO.File.Exists(dbPath + DATABASE_NAME))
            {
                System.IO.File.Delete(dbPath + DATABASE_NAME);
            }
        }

        private void CreateTables()
        {
            string query = "CREATE TABLE " + TABLE_NAME + " (" + 
                            KEY + " TEXT PRIMARY KEY, " + 
                            MODIFIED_DATE + " DATE, " +
                            CREATED_DATE + " DATE, " +
                            FILE_SIZE + " LONG, " +
                            CONTENT_URL + " TEXT, " +
                            PARENT_URL + " TEXT, " +
                            E_TAG + " TEXT, " +
                            FILE_NAME + " TEXT, " +
                            MIMIE_TYPE + " TEXT, " +
                            PUBLIC + " BOOL, " +
                            SHARED + " BOOL, " +
                            STATUS + " TEXT, " +
                            TYPE + " TEXT, " +
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
                //query=query.Replace("\\","/");
                //query = query.Replace("/", "//");
                //query = query.Replace(":", "");
                sqlCommand = new SQLiteCommand(query, sqlConnection);
                sqlCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool ExecuteNonQuery(FileFolderInfo fileFolderInfo)
        {
            try
            {
                string query = @"INSERT INTO " + TABLE_NAME + " VALUES(" +
                                "@Key ," +
                                "@ModifiedDate , " +
                                "@CreatedDate  ," +
                                "@FileSize  ," +
                                "@ContentUrl , " +
                                "@ParentUrl , " +
                                "@ETag , " +
                                "@FileName , " +
                                "@MimeType , " +
                                "@IsPublic , " +
                                "@IsShared , " +
                                "@Status , " +
                                "@Type , " +
                                "@ParentDir )";

                sqlCommand = new SQLiteCommand(query, sqlConnection);
                           
                sqlCommand.Parameters.Add(new SQLiteParameter("@Key",System.Data.DbType.String));
                sqlCommand.Parameters.Add(new SQLiteParameter("@ModifiedDate",System.Data.DbType.DateTime));
                sqlCommand.Parameters.Add(new SQLiteParameter("@CreatedDate", System.Data.DbType.DateTime));
                sqlCommand.Parameters.Add(new SQLiteParameter("@FileSize", System.Data.DbType.Double));
                sqlCommand.Parameters.Add(new SQLiteParameter("@ContentUrl", System.Data.DbType.String));
                sqlCommand.Parameters.Add(new SQLiteParameter("@ParentUrl", System.Data.DbType.String));
                sqlCommand.Parameters.Add(new SQLiteParameter("@FileName", System.Data.DbType.String));
                sqlCommand.Parameters.Add(new SQLiteParameter("@MimeType", System.Data.DbType.String));
                sqlCommand.Parameters.Add(new SQLiteParameter("@IsPublic", System.Data.DbType.Boolean));
                sqlCommand.Parameters.Add(new SQLiteParameter("@IsShared", System.Data.DbType.Boolean));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Status",System.Data.DbType.String));
                sqlCommand.Parameters.Add(new SQLiteParameter("@Type",System.Data.DbType.String));
                sqlCommand.Parameters.Add(new SQLiteParameter("@ParentDir",System.Data.DbType.String));
                sqlCommand.Parameters.Add(new SQLiteParameter("@ETag",System.Data.DbType.String));

                sqlCommand.Parameters["@Key"].Value = fileFolderInfo.Key;
                sqlCommand.Parameters["@ModifiedDate"].Value = fileFolderInfo.ModifiedDate;
                sqlCommand.Parameters["@CreatedDate"].Value = fileFolderInfo.CreatedDate;
                sqlCommand.Parameters["@FileSize"].Value = fileFolderInfo.FileSize;
                sqlCommand.Parameters["@ContentUrl"].Value = fileFolderInfo.ContentUrl;
                sqlCommand.Parameters["@ParentUrl"].Value = fileFolderInfo.ParentUrl;
                sqlCommand.Parameters["@FileName"].Value = fileFolderInfo.FileName;
                sqlCommand.Parameters["@MimeType"].Value = fileFolderInfo.MimeType;
                sqlCommand.Parameters["@IsPublic"].Value = fileFolderInfo.IsPublic;
                sqlCommand.Parameters["@IsShared"].Value = fileFolderInfo.IsShared;
                sqlCommand.Parameters["@Status"].Value = fileFolderInfo.Status;
                sqlCommand.Parameters["@Type"].Value = fileFolderInfo.Type;
                sqlCommand.Parameters["@ParentDir"].Value = fileFolderInfo.ParentDir;
                sqlCommand.Parameters["@ETag"].Value = fileFolderInfo.ETag;




                sqlCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public int Update(string tableName, string newValues, string whereCondition)
        {
            string query = "update " + tableName + " set " + newValues + " where " + whereCondition;
            int result = -1;

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            
            //sqlCommand.Parameters.Add("@NewValues", newValues);
            //sqlCommand.Parameters.Add("@WhereCondition", whereCondition);

            result = sqlCommand.ExecuteNonQuery();
            
            return result;
        }

        public int Delete(string tableName, string whereCondition)
        {
            string query = "delete from " + tableName + " where " + whereCondition;
            int result = -1;

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            //sqlCommand.Parameters.Add("@WhereCondition", whereCondition);

            result = sqlCommand.ExecuteNonQuery();

            return result;
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

        public string GetString(string tableName, string fieldName, string WhereCondition)
        {
            string query = "select " + fieldName + " from " + tableName + " where " + WhereCondition;
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

        public bool GetBoolean(string tableName, string fieldName, string WhereCondition)
        {
            string query = "select " + fieldName + " from " + tableName + " where " + WhereCondition;
            bool result = false;

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            sqlDataReader.Read();

            result = sqlDataReader.GetBoolean(0);

            sqlDataReader.Close();

            return result;
        }

        public DateTime GetDateTime(string tableName, string fieldName, string WhereCondition)
        {
            string query = "select " + fieldName + " from " + tableName + " where " + WhereCondition;
            DateTime result;

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            sqlDataReader.Read();

            result = sqlDataReader.GetDateTime(0);

            sqlDataReader.Close();

            return result;
        }


        public List<string> GetStringList(string tableName, string fieldName, string WhereCondition)
        {
            string query = "select " + fieldName + " from " + tableName + " where " + WhereCondition; 
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

        public void Write(FileFolderInfo fileFolderInfo)
        {
            ExecuteNonQuery(fileFolderInfo);
        }

        public int UpdateModifiedDate(DateTime newDate, string key)
        {
            string query = "update " + TABLE_NAME + " set " + MODIFIED_DATE + "= @modifiedDate where " + KEY + "='" + key + "'";
            int result = -1;

            sqlCommand = new SQLiteCommand(query, sqlConnection);

            sqlCommand.Parameters.Add("@modifiedDate",System.Data.DbType.DateTime);
            sqlCommand.Parameters["@modifiedDate"].Value = newDate;

            result = sqlCommand.ExecuteNonQuery();

            return result;
        }

        public List<DbKeyModDate> GetStructureList()
        {
            List<DbKeyModDate> keyList = new List<DbKeyModDate>();

            string query = "select " + KEY + "," + MODIFIED_DATE + " from " + TABLE_NAME;
            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            
            while(sqlDataReader.Read())
            {
                DbKeyModDate keyMod = new DbKeyModDate();
                keyMod.Key = sqlDataReader.GetString(0);
                keyMod.ModifiedDate = sqlDataReader.GetDateTime(1);
                keyList.Add(keyMod);
            }

            sqlDataReader.Close();
            return keyList;
        }

        public List<string> GetKeyList()
        {
            List<string> keys = new List<string>();

            string query = "select " + KEY +  " from " + TABLE_NAME;
            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();

            while (sqlDataReader.Read())
            {
                string key = sqlDataReader.GetString(0);
                keys.Add(key);
            }

            sqlDataReader.Close();
            return keys;
        }
    }
}
