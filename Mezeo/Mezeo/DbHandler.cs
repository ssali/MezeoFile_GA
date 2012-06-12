using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MezeoFileSupport;


#if MEZEO32
using Finisar.SQLite;
#elif MEZEO64
using System.Data.SQLite;
#else
using Finisar.SQLite;
#endif

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

        // Event queue information table fields.
        public const string EVENT_QUEUE_INFO_TABLE_NAME = "EventQueueInfo";
        public const string EVENT_QUEUE_INFO_JOB_COUNT = "JobCount";
        public const string EVENT_QUEUE_INFO_NAME = "TableName";

        //Event table fields.
        public const string EVENT_TABLE_NAME = "EventInfo";
        public const string EVENT_INDEX = "EventIndex";
        public const string EVENT_ORIGIN = "Origin";
        public const string EVENT_LOCAL_FILE_NAME = "LFileName";
        public const string EVENT_LOCAL_OLD_FILE_NAME = "LOldFileName";
        public const string EVENT_LOCAL_FULL_PATH = "LFullPath";
        public const string EVENT_LOCAL_OLD_FULL_PATH = "LOldFullPath";
        public const string EVENT_LOCAL_TYPE = "LEventType";
        public const string EVENT_LOCAL_TIMESTAMP = "LEventTimeStamp";
        public const string EVENT_LOCAL_IS_DIRECTORY = "LIsDirectory";
        public const string EVENT_LOCAL_IS_FILE = "LIsFile";
        public const string EVENT_LOCAL_FILE_ATTRIBUTES = "LFileAttributes";
        public const string EVENT_NQ_SIZE = "NQSize";
        public const string EVENT_NQ_DOMAIN_URI = "NQDomainURI";
        public const string EVENT_NQ_EVENT = "NQEvent";
        public const string EVENT_NQ_RESULT = "NQResult";
        public const string EVENT_NQ_TIME = "NQTime";
        public const string EVENT_NQ_USER = "NQUser";
        public const string EVENT_NQ_HASH = "NQHash";
        public const string EVENT_NQ_EXPORTED_PATH = "NQExportedPath";
        public const string EVENT_NQ_ID = "NQID";
        public const string EVENT_NQ_NAME = "NQName";
        public const string EVENT_NQ_OBJ_TYPE = "NQObjType";
        public const string EVENT_NQ_PARENT_ID = "NQParentID";
        public const string EVENT_NQ_PARENT_URI = "NQParentURI";


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

            //sqlConnection = new SQLiteConnection("Data Source=" + dbPath + DATABASE_NAME + ";Version=3;New=" + createNew + ";Compress=True;DateTimeFormat=Ticks;");
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

            if(sqlConnection != null)
                sqlConnection.Close();
            
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
            CreateEventsTable();
        }

        public void CreateEventsTable()
        {
            // See if the table already exists.
            if (null == sqlConnection)
                OpenConnection();
            //if (null != sqlConnection)
            //{
            //    string query = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='" + EVENT_TABLE_NAME + "';";
            //    SQLiteDataReader sqlDataReader = ExecuteQuery(query);
            //    sqlDataReader.Read();
            //    string result = sqlDataReader.GetString(0);
            //    sqlDataReader.Close();
            //    if (0 != result.Length)
            //    {
            //    }
            //}

            // Create the events table if it doesn't already exist.
            string queryEvents = "CREATE TABLE IF NOT EXISTS " + EVENT_TABLE_NAME + " (" +
                            EVENT_INDEX + " INTEGER PRIMARY KEY, " +
                            EVENT_ORIGIN + " TEXT, " +
                            EVENT_LOCAL_FILE_NAME + " TEXT, " +
                            EVENT_LOCAL_OLD_FILE_NAME + " TEXT, " +
                            EVENT_LOCAL_FULL_PATH + " TEXT, " +
                            EVENT_LOCAL_OLD_FULL_PATH + " TEXT, " +
                            EVENT_LOCAL_TYPE + " TEXT, " +
                            EVENT_LOCAL_TIMESTAMP + " INTEGER, " +
                            EVENT_LOCAL_IS_DIRECTORY + " BOOL, " +
                            EVENT_LOCAL_IS_FILE + " BOOL, " +
                            EVENT_LOCAL_FILE_ATTRIBUTES + " INTEGER, " +
                            EVENT_NQ_SIZE + " INTEGER, " +
                            EVENT_NQ_DOMAIN_URI + " TEXT, " +
                            EVENT_NQ_EVENT + " TEXT, " +
                            EVENT_NQ_RESULT + " TEXT, " +
                            EVENT_NQ_TIME + " TEXT, " +
                            EVENT_NQ_USER + " TEXT, " +
                            EVENT_NQ_HASH + " TEXT, " +
                            EVENT_NQ_EXPORTED_PATH + " TEXT, " +
                            EVENT_NQ_ID + " TEXT, " +
                            EVENT_NQ_NAME + " TEXT, " +
                            EVENT_NQ_OBJ_TYPE + " TEXT, " +
                            EVENT_NQ_PARENT_ID + " TEXT, " +
                            EVENT_NQ_PARENT_URI + " TEXT)";

            ExecuteNonQuery(queryEvents);

            // Since the database schema has changed, we need to get a new connection.
            sqlConnection.Close();
            OpenConnection();

            // Create the events table if it doesn't already exist.
            string queryEventInfo = "CREATE TABLE IF NOT EXISTS " + EVENT_QUEUE_INFO_TABLE_NAME + " (" +
                            EVENT_QUEUE_INFO_NAME + " TEXT, " +
                            EVENT_QUEUE_INFO_JOB_COUNT + " INTEGER);";

            ExecuteNonQuery(queryEventInfo);

            // Since the database schema has changed, we need to get a new connection.
            sqlConnection.Close();
            OpenConnection();
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
                LogWrapper.LogMessage("DbHandler - ExecuteNonQuery", "Caught exception: " + ex.Message);
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
                LogWrapper.LogMessage("DbHandler - ExecuteNonQuery", "Caught exception: " + ex.Message);
                return false;
            }
        }

        public void ClearLocalEvents()
        {
            int result = -1;

            // Make sure we have a connection.
            if (null == sqlConnection)
                OpenConnection();

            string query = "DELETE FROM " + EVENT_TABLE_NAME + " WHERE " + EVENT_ORIGIN + "='L';";
            sqlCommand = new SQLiteCommand(query, sqlConnection);
            LogWrapper.LogMessage("DBHandler - ClearLocalEvents", "Running query: " + query);
            result = sqlCommand.ExecuteNonQuery();

            string query2 = "DELETE FROM " + EVENT_QUEUE_INFO_TABLE_NAME + ";";
            sqlCommand = new SQLiteCommand(query2, sqlConnection);
            LogWrapper.LogMessage("DBHandler - ClearLocalEvents", "Running query: " + query2);
            result = sqlCommand.ExecuteNonQuery();

            string query3 = "INSERT INTO " + EVENT_QUEUE_INFO_TABLE_NAME + " (" + EVENT_QUEUE_INFO_NAME + ", " + EVENT_QUEUE_INFO_JOB_COUNT + ") VALUES ('" + EVENT_TABLE_NAME + "', 0);";
            sqlCommand = new SQLiteCommand(query3, sqlConnection);
            LogWrapper.LogMessage("DBHandler - ClearLocalEvents", "Running query: " + query3);
            result = sqlCommand.ExecuteNonQuery();
        }

        public Int64 GetJobCount()
        {
            Int64 jobCount = 0;
            string query = "SELECT " + EVENT_QUEUE_INFO_JOB_COUNT + " FROM " + EVENT_QUEUE_INFO_TABLE_NAME + " WHERE " + EVENT_QUEUE_INFO_NAME + "='" + EVENT_TABLE_NAME + "';";

            sqlCommand = new SQLiteCommand();
            sqlCommand.CommandText = query;
            sqlCommand.Connection = sqlConnection;
            try
            {
                sqlDataReader = sqlCommand.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    jobCount = (Int64)sqlDataReader[EVENT_QUEUE_INFO_JOB_COUNT];
                }
            }
            catch (Exception ex)
            {
                LogWrapper.LogMessage("DbHandler - GetJobCount", "Caught exception: " + ex.Message);
            }

            return jobCount;
        }

        public void IncrementJobCount()
        {
            int result = -1;
            string query = "UPDATE " + EVENT_QUEUE_INFO_TABLE_NAME + " SET " + EVENT_QUEUE_INFO_JOB_COUNT + " = (SELECT " + EVENT_QUEUE_INFO_JOB_COUNT + "+1 FROM " + EVENT_QUEUE_INFO_TABLE_NAME + " WHERE " + EVENT_QUEUE_INFO_NAME + "='" + EVENT_TABLE_NAME + "') WHERE " + EVENT_QUEUE_INFO_NAME + "='" + EVENT_TABLE_NAME + "';";
            sqlCommand = new SQLiteCommand(query, sqlConnection);
            LogWrapper.LogMessage("DBHandler - IncrementJobCount", "Running query: " + query);
            result = sqlCommand.ExecuteNonQuery();
        }

        public string EscapeString(string value)
        {
            return value.Replace("'", "''");
        }

        public int AddEvent(LocalEvents newEvent)
        {
            int result = -1;
            string query = "insert into " + EVENT_TABLE_NAME + " (" +
                            EVENT_ORIGIN + ", " +
                            EVENT_LOCAL_FILE_NAME + ", " +
                            EVENT_LOCAL_OLD_FILE_NAME + ", " +
                            EVENT_LOCAL_FULL_PATH + ", " +
                            EVENT_LOCAL_OLD_FULL_PATH + ", " +
                            EVENT_LOCAL_TYPE + ", " +
                            EVENT_LOCAL_TIMESTAMP + ", " +
                            EVENT_LOCAL_IS_DIRECTORY + ", " +
                            EVENT_LOCAL_IS_FILE + ", " +
                            EVENT_LOCAL_FILE_ATTRIBUTES + ") values ('L', '" +
                            EscapeString(newEvent.FileName) + "','" +
                            EscapeString(newEvent.OldFileName) + "','" +
                            EscapeString(newEvent.FullPath) + "','" +
                            EscapeString(newEvent.OldFullPath) + "','" +
                            newEvent.EventType + "','" +
                            newEvent.EventTimeStamp + "','" +
                            newEvent.IsDirectory + "','" +
                            newEvent.IsFile + "','" +
                            (long)newEvent.Attributes + "');";

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            LogWrapper.LogMessage("DBHandler - AddEvent", "Running query: " + query);

            result = sqlCommand.ExecuteNonQuery();

            // Increment the job count.
            if (0 < result)
                IncrementJobCount();

            // A result of 1 is success (# of rows affected and we should
            // only have 1), not the index of the newly created entry.
            return result;
        }

        public int AddNQEvent(NQDetails nqEvent)
        {
            int result = -1;
            string query = "insert into " + EVENT_TABLE_NAME + " (" +
                            EVENT_ORIGIN + ", " +
                            EVENT_NQ_SIZE + ", " +
                            EVENT_NQ_DOMAIN_URI + ", " +
                            EVENT_NQ_EVENT + ", " +
                            EVENT_NQ_RESULT + ", " +
                            EVENT_NQ_TIME + ", " +
                            EVENT_NQ_USER + ", " +
                            EVENT_NQ_HASH + ", " +
                            EVENT_NQ_EXPORTED_PATH + ", " +
                            EVENT_NQ_ID + ", " +
                            EVENT_NQ_NAME + ", " +
                            EVENT_NQ_OBJ_TYPE + ", " +
                            EVENT_NQ_PARENT_ID + ", " +
                            EVENT_NQ_PARENT_URI + ") values ('N', '" +
                            nqEvent.lSize + "','" +
                            nqEvent.StrDomainUri + "','" +
                            nqEvent.StrEvent + "','" +
                            nqEvent.StrEventResult + "','" +
                            nqEvent.StrEventTime + "','" +
                            nqEvent.StrEventUser + "','" +
                            nqEvent.StrHash + "','" +
                            EscapeString(nqEvent.StrMezeoExportedPath) + "','" +
                            nqEvent.StrObjectID + "','" +
                            EscapeString(nqEvent.StrObjectName) + "','" +
                            nqEvent.StrObjectType + "','" +
                            nqEvent.StrParentID + "','" +
                            nqEvent.StrParentUri + "');";

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            LogWrapper.LogMessage("DBHandler - AddNQEvent", "Running query: " + query);

            result = sqlCommand.ExecuteNonQuery();

            // Increment the job count.
            if (0 < result)
                IncrementJobCount();

            // A result of 1 is success (# of rows affected and we should
            // only have 1), not the index of the newly created entry.
            return result;
        }

        public int DeleteEvent(Int64 eventID)
        {
            int result = -1;
            string query = "DELETE FROM " + EVENT_TABLE_NAME + " WHERE EventIndex=" + eventID + ";";
            sqlCommand = new SQLiteCommand(query, sqlConnection);
            LogWrapper.LogMessage("DBHandler - DeleteEvent", "Running query: " + query);

            result = sqlCommand.ExecuteNonQuery();

            // A result of 1 is success (# of rows affected and we should
            // only have 1), not the index of the newly created entry.
            return result;
        }

        public void PopulateLocalEventFromReader(ref LocalEvents item, ref SQLiteDataReader sqlDataReader)
        {
            item.EventDbId = (Int64)sqlDataReader[EVENT_INDEX];
            item.FileName = (string)sqlDataReader[EVENT_LOCAL_FILE_NAME];
            item.OldFileName = (string)sqlDataReader[EVENT_LOCAL_OLD_FILE_NAME];
            item.FullPath = (string)sqlDataReader[EVENT_LOCAL_FULL_PATH];
            item.OldFullPath = (string)sqlDataReader[EVENT_LOCAL_OLD_FULL_PATH];
            item.IsDirectory = (bool)sqlDataReader[EVENT_LOCAL_IS_DIRECTORY];
            item.IsFile = (bool)sqlDataReader[EVENT_LOCAL_IS_FILE];
            item.EventTimeStamp.AddTicks((Int64)sqlDataReader[EVENT_LOCAL_TIMESTAMP]);
            Int64 test = (Int64)sqlDataReader[EVENT_LOCAL_FILE_ATTRIBUTES];
            item.Attributes = (System.IO.FileAttributes)test;
            switch ((string)sqlDataReader[EVENT_LOCAL_TYPE])
            {
                case "FILE_ACTION_ADDED":
                    item.EventType = LocalEvents.EventsType.FILE_ACTION_ADDED;
                    break;
                case "FILE_ACTION_MODIFIED":
                    item.EventType = LocalEvents.EventsType.FILE_ACTION_MODIFIED;
                    break;
                case "FILE_ACTION_REMOVED":
                    item.EventType = LocalEvents.EventsType.FILE_ACTION_REMOVED;
                    break;
                case "FILE_ACTION_RENAMED":
                    item.EventType = LocalEvents.EventsType.FILE_ACTION_RENAMED;
                    break;
                case "FILE_ACTION_MOVE":
                    item.EventType = LocalEvents.EventsType.FILE_ACTION_MOVE;
                    break;
            }
        }

        public LocalEvents GetLocalEvent()
        {
            int result = -1;
            string query = "SELECT * FROM " + EVENT_TABLE_NAME + " WHERE " + EVENT_ORIGIN + " = 'L' ORDER BY " + EVENT_INDEX + " LIMIT 1;";
            sqlCommand = new SQLiteCommand(query, sqlConnection);
            LogWrapper.LogMessage("DBHandler - GetLocalEvent", "Running query: " + query);
            LocalEvents item = new LocalEvents();

            result = sqlCommand.ExecuteNonQuery();

            sqlCommand = new SQLiteCommand();
            sqlCommand.CommandText = query;
            sqlCommand.Connection = sqlConnection;
            try
            {
                sqlDataReader = sqlCommand.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    PopulateLocalEventFromReader(ref item, ref sqlDataReader);
                    //DeleteEvent(item.EventDbId);  // Remove the item from the database.
                }
            }
            catch (Exception ex)
            {
                LogWrapper.LogMessage("DbHandler - GetLocalEvent", "Caught exception: " + ex.Message);
                item = null;
            }

            if (null != sqlDataReader)
                sqlDataReader.Close();

            return item;
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

        public int Update(string tableName, string fieldName, string newValues, string whereFields,string whereValues)
        {
            int result = -1;
            string query = "";

            sqlCommand = new SQLiteCommand();

            query = "update " + tableName + " set " + fieldName + "=@newValue where " + whereFields + "=@whereValue";

            sqlCommand.Parameters.Add("@newValue" , System.Data.DbType.String);
            sqlCommand.Parameters["@newValue" ].Value = newValues;

            sqlCommand.Parameters.Add("@whereValue", System.Data.DbType.String);
            sqlCommand.Parameters["@whereValue"].Value = whereValues;
            
            sqlCommand.CommandText = query;
            sqlCommand.Connection = sqlConnection;

            result = sqlCommand.ExecuteNonQuery();

            return result;
        }

        public int Update(string tableName, string fieldName, bool newValues, string whereFields, string whereValues)
        {
            int result = -1;
            string query = "";

            sqlCommand = new SQLiteCommand();

            query = "update " + tableName + " set " + fieldName + "=@newValue where " + whereFields + "=@whereValue";

            sqlCommand.Parameters.Add("@newValue", System.Data.DbType.Boolean);
            sqlCommand.Parameters["@newValue"].Value = newValues;

            sqlCommand.Parameters.Add("@whereValue", System.Data.DbType.String);
            sqlCommand.Parameters["@whereValue"].Value = whereValues;

            sqlCommand.CommandText = query;
            sqlCommand.Connection = sqlConnection;

            result = sqlCommand.ExecuteNonQuery();

            return result;
        }

        public int Delete(string tableName, string wherefield, string whereValue)
        {
            string query = "delete from " + tableName + " where " + wherefield + "=@whereValue";
            int result = -1;

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            //sqlCommand.Parameters.Add("@WhereCondition", whereCondition);
            sqlCommand.Parameters.Add("@whereValue", System.Data.DbType.String);
            sqlCommand.Parameters["@whereValue"].Value = whereValue;

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

            //query = HandleSpecialCharacter(query);

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            sqlDataReader.Read();

            result = sqlDataReader.GetString(0);

            sqlDataReader.Close();

            return result;
        }

        public string GetString(string tableName, string fieldName, string[] whereFields,string[] whereValues,System.Data.DbType[] fieldDataType)
        {
            string whereQuery = "";
            string query = "";// "select " + fieldName + " from " + tableName + " where " + WhereCondition;
            string result = "";

            sqlCommand = new SQLiteCommand();

            if (whereFields == null || whereValues == null)
            {
                query = "select " + fieldName + " from " + tableName;
            }
            else
            {
                for (int i = 0; i < whereFields.Length; i++)
                {
                    whereQuery += whereFields[i] + "=@" + whereFields[i] ;
                    
                    if (i < whereFields.Length - 1)
                        whereQuery += " and ";

                    sqlCommand.Parameters.Add("@" + whereFields[i], fieldDataType[i]);
                    sqlCommand.Parameters["@" + whereFields[i]].Value = whereValues[i];

                }

                query = "select " + fieldName + " from " + tableName + " where " + whereQuery;
            }

            //query = HandleSpecialCharacter(query);

            //sqlCommand = new SQLiteCommand(query, sqlConnection);
            sqlCommand.CommandText = query;
            sqlCommand.Connection = sqlConnection;
            try
            {
                sqlDataReader = sqlCommand.ExecuteReader();
                sqlDataReader.Read();
            }
            catch (Exception ex)
            {
                LogWrapper.LogMessage("DbHandler - GetString", "Caught exception: " + ex.Message);
                sqlConnection.Close();
                OpenConnection();
                sqlCommand.Connection = sqlConnection;
                sqlDataReader = sqlCommand.ExecuteReader();
                sqlDataReader.Read();
            }

            #if MEZEO32
                result = sqlDataReader.GetString(0);
            #elif MEZEO64
                if (sqlDataReader.HasRows)
                        result = sqlDataReader.GetString(0);
                    else
                        result = "";
            #else
                result = sqlDataReader.GetString(0);
            #endif

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

        private string HandleSpecialCharacter(string query)
        {
            string handeledQuery = query;

            if (handeledQuery.Contains("'"))
            {
                handeledQuery = handeledQuery.Replace("'", "''");
            }

            return handeledQuery;
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

        public DateTime GetDateTime(string tableName, string fieldName, string WhereField, string whereValue)
        {
            string query = "select " + fieldName + " from " + tableName + " where " + WhereField + "=@whereValue";
            DateTime result;

            sqlCommand = new SQLiteCommand(query, sqlConnection);

            sqlCommand.Parameters.Add("@whereValue", System.Data.DbType.String);
            sqlCommand.Parameters["@whereValue"].Value = whereValue;

            sqlDataReader = sqlCommand.ExecuteReader();
            sqlDataReader.Read();

            result = sqlDataReader.GetDateTime(0);

            sqlDataReader.Close();

            return result;
        }

        public List<string> GetStringList(string tableName, string fieldName, string WhereField, string whereValue)
        {
            string query = "select " + fieldName + " from " + tableName + " where " + WhereField + "=@whereValue"; 
            List<string> result =new List<string>();

            sqlCommand = new SQLiteCommand(query, sqlConnection);
            
            sqlCommand.Parameters.Add("@whereValue", System.Data.DbType.String);
            sqlCommand.Parameters["@whereValue"].Value = whereValue;

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
            string query = "update " + TABLE_NAME + " set " + MODIFIED_DATE + "= @modifiedDate where " + KEY + "=@key";
            int result = -1;

            sqlCommand = new SQLiteCommand(query, sqlConnection);

            sqlCommand.Parameters.Add("@modifiedDate",System.Data.DbType.DateTime);
            sqlCommand.Parameters["@modifiedDate"].Value = newDate;

            sqlCommand.Parameters.Add("@key", System.Data.DbType.String);
            sqlCommand.Parameters["@key"].Value = key;

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
