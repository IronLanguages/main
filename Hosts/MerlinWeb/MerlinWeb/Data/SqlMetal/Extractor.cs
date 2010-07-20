using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Globalization;
//using System.Data.DLinq.SqlClient;

namespace SqlMetal {

    internal enum LanguageType {
        CSharp,
        VisualBasic
    }

    internal class ExtractOptions {
        internal ExtractTypes Types;
        internal bool Pluralize;
        /// <summary>
        /// Timeout value to use for SqlCommands.  30 seconds
        /// is the default used by SqlCommand.
        /// </summary>
        internal int CommandTimeout = 30;
        internal LanguageType Language = LanguageType.CSharp;
    }

    internal enum ExtractTypes {
        Tables = 0x01,
        Views = 0x02,
        Functions = 0x04,
        System = 0x08,
        StoredProcs = 0x10,
        Constraints = 0x20,
        Indexes = 0x40,
        Relationships = 0x80
    }

    /// <summary>
    /// This class encapsulates management of the database connection and commands.
    /// For example, by centralizing command creation here, we can ensure that all 
    /// commands use the same user specified command timeout value.
    /// </summary>
    internal class ConnectionManager {
        private SqlConnection connection;
        int timeout;

        public ConnectionManager(string connectionString, int timeout) {
            this.timeout = timeout;
            connection = new SqlConnection(connectionString);
        }

        public SqlCommand CreateCommand() {
            return CreateCommand("");
        }

        public SqlCommand CreateCommand(string sql) {
            SqlCommand command = new SqlCommand(sql, connection);
            command.CommandTimeout = timeout;
            return command;
        }

        public void Open() {
            if (this.connection.State == ConnectionState.Closed) {
                this.connection.Open();
            }
        }

        public void Close() {
            if (this.connection.State != ConnectionState.Closed) {
                this.connection.Close();
            }
        }

        /// <summary>
        /// Whether the server is pre-yukon or not.
        /// </summary>
        public bool IsPreYukon {
            get {
                string version = this.connection.ServerVersion;
                if (version.Contains("08.00") || version.Contains("07.00") || version.Contains("06.00")) {
                    return true;
                }
                return false;
            }
        }
    }

    internal class Extractor {       
        ExtractOptions options;
        Database db;
        Dictionary<string, DbSchema> schemas;
        ConnectionManager connectionManager;

        internal Extractor(string constr, ExtractOptions options) {
            this.options = options;
            this.connectionManager = new ConnectionManager(constr, options.CommandTimeout);           
        }

        private void Open() {
            connectionManager.Open();           
        }

        private void Close() {
            connectionManager.Close();
        }

        internal Database ExtractDatabase(string database) {
            this.Open();
            this.db = new Database();
            string legalName = this.GetLegalLanguageName(database);
            if (string.Compare(legalName, database) == 0) {
                this.db.Name = legalName;
            }
            else {
                this.db.Name = database;
                this.db.Class = legalName;
            }
            this.schemas = new Dictionary<string, DbSchema>();
            this.GetTablesAndViews();
            if( (options.Types & ExtractTypes.Tables) != 0 ) {               
                this.GetPrimaryKeys(db);
                this.GetUniqueKeys(db);
            }
            if ((this.options.Types & ExtractTypes.Indexes) != 0)
                this.GetIndexes(db);
            if ((this.options.Types & ExtractTypes.Relationships) != 0)
                this.GetRelationships(db);

            GetSprocsAndFunctions();

            this.Close();
            return db;
        }

        private DbSchema GetSchema(string name) {
            if (this.schemas.ContainsKey(name)) {
                return this.schemas[name];
            }
            else {
                DbSchema s = new DbSchema();
                s.Name = name;  // save the actual database name
                string legalName = this.GetLegalLanguageName(name);
                if (string.Compare(legalName, name) == 0) {
                    s.Property = legalName;
                }
                else {
                    s.Property = name;
                    s.Class = legalName;
                }
                this.schemas.Add(name, s);
                this.db.Schemas.Add(s);
                return s;
            }
        }

        private void GetTablesAndViews() {
            string tablesQuery = "SELECT TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES " +
                                 "WHERE ISNULL(OBJECTPROPERTY(OBJECT_ID(TABLE_NAME), 'IsMSShipped'), 0) = 0";
            SqlCommand cmd = connectionManager.CreateCommand(tablesQuery);
            SqlDataReader dr = cmd.ExecuteReader();
            try {
                while (dr.Read()) {
                    DbSchema schema = this.GetSchema((string)dr["TABLE_SCHEMA"]);
                    string name = (string)dr["TABLE_NAME"];
                    string type = (string)dr["TABLE_TYPE"];
                    switch (type.Trim()) {
                        case "BASE TABLE":
                            if ((this.options.Types & ExtractTypes.Tables) != 0) {
                                DbTable t = new DbTable();
                                t.Name = name;
                                string pname = this.GetLegalLanguageName(name);
                                string cname = pname;
                                if (this.options.Pluralize) {
                                    pname = this.GetPluralName(pname);
                                    cname = this.GetSingularName(cname);
                                }

                                if (!IsUniqueToDatabase(name, pname)){
                                    pname = ReturnUniqueDatabaseName(pname, name);
                               }

                                t.Class = this.GetIfDifferent(cname, name);
                                t.Property = this.GetIfDifferent(pname, name);
                                schema.Tables.Add(t);
                            }
                            break;
                        case "VIEW":
                            if ((this.options.Types & ExtractTypes.Views) != 0) {
                                DbView v = new DbView();
                                v.Name = name;
                                string pname = this.GetLegalLanguageName(name);
                                string cname = pname;
                                if (this.options.Pluralize) {
                                    pname = this.GetPluralName(pname);
                                    cname = this.GetSingularName(cname);
                                }

                                if (!IsUniqueToDatabase(name, pname)){
                                    pname = ReturnUniqueDatabaseName(pname, name);
                               }

                                v.Class = this.GetIfDifferent(cname, name);
                                v.Property = this.GetIfDifferent(pname, name);
                                schema.Views.Add(v);
                            }
                            break;
                    }
                }
            }
            finally {
                dr.Close();
            }

            // get all table details now
            foreach (DbSchema schema in this.db.Schemas) {
                foreach (DbTable table in schema.Tables) {
                    this.GetTableColumns(schema, table);
                }
                foreach (DbView view in schema.Views) {
                    try {
                        this.GetTableColumns(schema, view);
                    }
                    catch (SqlException e) {
                        // if for any reason we are unable to completely process schema info for a view
                        // log an error, then move to the next view
                        string msg = string.Format("Unable to process view. Exception: {0}", e.Message);
                        view.SchemaErrors = true;
                        Console.WriteLine("Error generating View '{0}'. {1}", view.Name, msg);
                    }
                }
            }
        }

        /// <summary>
        /// Populate the database object with all stored procedure and
        /// function metadata.
        /// </summary>
        private void GetSprocsAndFunctions() {
            GetSprocAndFunctionDefinitions();
            GetSprocAndFunctionResultShapes();
        }

        /// <summary>
        /// Populate the StoreProcedures and Function collections of the database schemas
        /// with routine objects and their parameters.
        /// </summary>
        private void GetSprocAndFunctionDefinitions() {

            bool extractSprocs = (this.options.Types & ExtractTypes.StoredProcs) != 0;
            bool extractFunctions = (this.options.Types & ExtractTypes.Functions) != 0;

            // join INFORMATION_SCHEMA.ROUTINES to INFORMATION_SCHEMA.PARAMETERS to get all
            // routines and parameters.  Ordering is important here since we are manually
            // grouping by procedure name, and processing parameters in order.
            string sql =
                "SELECT r.SPECIFIC_SCHEMA, r.ROUTINE_TYPE, r.SPECIFIC_NAME, r.DATA_TYPE AS ROUTINE_DATA_TYPE, p.ORDINAL_POSITION, p.PARAMETER_MODE, p.PARAMETER_NAME, " +
                "p.DATA_TYPE, p.CHARACTER_MAXIMUM_LENGTH, p.NUMERIC_PRECISION, p.NUMERIC_SCALE, p.IS_RESULT " +
                "FROM INFORMATION_SCHEMA.ROUTINES AS " +
                "r FULL OUTER JOIN INFORMATION_SCHEMA.PARAMETERS AS p on r.SPECIFIC_NAME = p.SPECIFIC_NAME AND r.SPECIFIC_SCHEMA = p.SPECIFIC_SCHEMA " +
                "WHERE (r.ROUTINE_TYPE = 'PROCEDURE' OR r.ROUTINE_TYPE = 'FUNCTION') AND " +
                "ISNULL(OBJECTPROPERTY(OBJECT_ID(r.SPECIFIC_NAME), 'IsMSShipped'), 0) = 0 " +
                "ORDER BY r.SPECIFIC_SCHEMA, r.SPECIFIC_NAME, p.ORDINAL_POSITION";
            SqlCommand command = connectionManager.CreateCommand(sql);
            SqlDataReader reader = command.ExecuteReader();

            // iterate through, collecting all parameters for each unique routine name
            DbRoutine currRoutine = null;
            while (reader.Read()) {

                // each time we come across a new routine name, create a routine
                // and set as current
                string routineName = (string)reader["SPECIFIC_NAME"];
                if (currRoutine == null || currRoutine.Name != routineName) {

                    // determine the schema for this routine
                    string schemaName = (string)reader["SPECIFIC_SCHEMA"];
                    DbSchema dbSchema = GetSchema(schemaName);

                    string routineType = (string)reader["ROUTINE_TYPE"];
                    if (routineType == "FUNCTION" && extractFunctions) {
                        DbFunction function = new DbFunction();
                        dbSchema.Functions.Add(function);

                        string routineDataType = (string)this.ValueOrDefault(reader["ROUTINE_DATA_TYPE"], "");
                        if (routineDataType == "TABLE") {
                            function.IsTableValued = true;
                        }

                        currRoutine = function;
                    }
                    else if (routineType == "PROCEDURE" && extractSprocs) {
                        currRoutine = new DbStoredProcedure();
                        dbSchema.StoreProcedures.Add(currRoutine as DbStoredProcedure);
                    }
                    else {
                        continue;
                    }

                    currRoutine.Name = routineName;
                    string possibleName = GetLegalLanguageName(routineName);
                    if (!IsUniqueToDatabase(routineName, possibleName)){
                        possibleName = ReturnUniqueDatabaseName(possibleName, routineName);
                    }
                    currRoutine.MethodName = possibleName;

                }

                #region Read parameter info into current routine
                if (reader["ORDINAL_POSITION"] != DBNull.Value) {
                    DbParameter param = new DbParameter();

                    // Function returns are listed as parameters with ordinal 0 and
                    // is_result true
                    bool isReturnValue = false;
                    string isResult = (string)ValueOrDefault(reader["IS_RESULT"], "NO");
                    if (isResult == "YES" && currRoutine is DbFunction) {
                        param.Name = "RETURN_VALUE";
                        isReturnValue = true;
                    }
                    else {
                        param.Name = (string)reader["PARAMETER_NAME"];
                        param.Name = param.Name.Replace("@", "");  // remove any parameter symbols
                        StringBuilder sb = new StringBuilder(GetLegalLanguageName(param.Name));
                        sb[0] = Char.ToLower(sb[0], CultureInfo.InvariantCulture);  // camel-case
                        param.ParameterName = sb.ToString();
                    }

                    // parse the data type                    
                    string dataType = (string)reader["DATA_TYPE"];
                    SqlProviderType providerType = SqlProviderType.Parse(dataType);                 
                    SqlDbType sqlDbType = providerType.SqlDbType;
                    Type paramType = this.GetClrType(sqlDbType);
                    param.Type = this.GetScopedTypeName(paramType);

                    int size = (int)this.ValueOrDefault(reader["CHARACTER_MAXIMUM_LENGTH"], 0);
                    short precision = Convert.ToSByte(this.ValueOrDefault(reader["NUMERIC_PRECISION"], (short)-1));
                    short scale = Convert.ToSByte(this.ValueOrDefault(reader["NUMERIC_SCALE"], (short)-1));
                    param.DbType = GetSqlTypeDeclaration(sqlDbType, size, precision, scale, false, false);

                    string parameterMode = (string)reader["PARAMETER_MODE"];
                    ParameterDirection paramDirection;
                    switch (parameterMode) {
                        case "IN":
                            paramDirection = ParameterDirection.Input;
                            break;
                        case "INOUT":
                            paramDirection = ParameterDirection.InputOutput;                            
                            break;
                        case "OUT":
                            paramDirection = ParameterDirection.Output;                           
                            break;
                        default:
                            paramDirection = ParameterDirection.Input;
                            break;
                    }
                    param.ParameterDirection = paramDirection;

                    if (!isReturnValue) {
                        currRoutine.Parameters.Add(param);
                    }
                    else {
                        // return values of functions aren't listed as parameters
                        DbFunction function = currRoutine as DbFunction;
                        function.Type = param.Type;
                        function.DbType = param.DbType;                   
                    }

                }  // end if
                #endregion

            }  // end while

            reader.Close();
        }
 

        private DbTable FindMatchingTable(string originalName, string legalLanguageName) {
            foreach (DbSchema schema in db.Schemas) {
                if (schema == null) {
                    return null;
                }
                foreach (DbTable table in schema.Tables) {
                    if (string.Compare(table.Name, originalName, StringComparison.OrdinalIgnoreCase) !=0) {
                        //Check to make sure that we don't show a match for the same view.
                        if (string.Compare(table.Property, legalLanguageName, StringComparison.OrdinalIgnoreCase) == 0){
                            return table;
                        }
                    }
                }
            }
            return null;
        }

        private DbView FindMatchingView(string originalName, string legalLanguageName) {
            foreach (DbSchema schema in db.Schemas) {
                if (schema == null) {
                    return null;
                }
                foreach (DbView view in schema.Views) {
                    if (string.Compare(view.Name, originalName, StringComparison.OrdinalIgnoreCase) !=0) {
                        //Check to make sure that we don't show a match for the same view.
                        if (string.Compare(view.Property, legalLanguageName, StringComparison.OrdinalIgnoreCase) == 0){
                            return view;
                        }
                    }
                }
            }
            return null;
        }

        private DbStoredProcedure FindMatchingStoredProcedure(string originalName, string legalLanguageName) {
            foreach (DbSchema schema in db.Schemas) {
                if (schema == null) {
                    return null;
                }
                foreach (DbStoredProcedure storedProc in schema.StoreProcedures) {
                    if (string.Compare(storedProc.Name, originalName, StringComparison.OrdinalIgnoreCase) !=0) {
                        //Check to make sure that we don't show a match for the same view.
                        if (string.Compare(storedProc.MethodName, legalLanguageName, StringComparison.OrdinalIgnoreCase) == 0){
                            return storedProc;
                        }
                    }
                }
            }
            return null;
        }

        private DbFunction FindMatchingFunction(string originalName, string legalLanguageName) {
            foreach (DbSchema schema in db.Schemas) {
                if (schema == null) {
                    return null;
                }
                foreach (DbFunction function in schema.Functions) {
                    if (string.Compare(function.Name, originalName, StringComparison.OrdinalIgnoreCase) !=0) {
                        //Check to make sure that we don't show a match for the same view.
                        if (string.Compare(function.MethodName, legalLanguageName, StringComparison.OrdinalIgnoreCase) == 0){
                            return function;
                        }
                    }
                }
            }
            return null;
        }


        private bool IsUniqueToDatabase(string originalName, string legalLanguageName){
            if (FindMatchingTable(originalName, legalLanguageName)!= null){
                return false;
            }
            if (FindMatchingView(originalName, legalLanguageName)!= null){
                return false;
            }
            if (FindMatchingStoredProcedure(originalName, legalLanguageName)!= null){
                return false;
            }
            if (FindMatchingFunction(originalName, legalLanguageName)!= null){
                return false;
            }
            return true;
        }

        private string ReturnUniqueDatabaseName(string candidateLegalLanguageName, string originalName){
            // try some numeric variant of the best candidate
            string bestCandidate = candidateLegalLanguageName;
            for (int i = 2; i < 1000; i++) {
                candidateLegalLanguageName = bestCandidate + i;
                if (this.IsUniqueToDatabase(originalName, candidateLegalLanguageName)) {
                    return candidateLegalLanguageName;
                }
            }
            // if we've arrived here then we havent come up with unique name
            throw new Exception("Unable to make unique table name.");
        }

        /// <summary>
        /// For each sproc or function, determine all possible result shapes that are
        /// possible (for functions it is only ever 1).
        /// </summary>
        private void GetSprocAndFunctionResultShapes() {
            SqlCommand command = connectionManager.CreateCommand();
            
            foreach (DbSchema schema in db.Schemas) {
                // get sproc result shapes
                command.CommandType = CommandType.StoredProcedure;
                foreach (DbStoredProcedure sproc in schema.StoreProcedures) {
                    command.CommandText = string.Format("[{0}].[{1}]", schema.Name, sproc.Name);
                    GetRoutineResultShapes(sproc, command);
                }

                // get function result types for table valued functions
                command.CommandType = CommandType.Text;
                foreach (DbFunction function in schema.Functions) {
                    // skip scalar functions
                    if (!function.IsTableValued)
                        continue;

                    // construct parameter list                       
                    string paramList = "";
                    foreach (DbParameter currParam in function.Parameters) {
                        paramList += string.Format("@{0}, ", currParam.Name);
                    }
                    if (paramList.Length > 0)
                        paramList = paramList.TrimEnd(',', ' ');

                    command.CommandText = string.Format("SELECT * FROM [{0}].[{1}]({2})", schema.Name, function.Name, paramList);
                    GetRoutineResultShapes(function, command);
                }
            }
        }

        /// <summary>
        /// Execute the specified command and retrieve its schema info.
        /// </summary>
        /// <param name="routine">Either a sproc or a function.</param>
        /// <param name="command">The command used to execute the sproc or function.</param>
        private void GetRoutineResultShapes(DbRoutine routine, SqlCommand command) {
            // add null value parameters to the command
            command.Parameters.Clear();
            foreach (DbParameter param in routine.Parameters) {
                SqlParameter sqlParam = new SqlParameter(param.Name, null);                
                command.Parameters.Add(sqlParam);
            }

            SqlDataReader reader = null;
            try {
                // do a schema only execution of the command
                reader = command.ExecuteReader(CommandBehavior.SchemaOnly);
                DataTable schemaTable = reader.GetSchemaTable();

                // For each possible result shape, create a table object and
                // add to the sproc result shapes collection
                List<DbRowset> resultShapes = new List<DbRowset>();
                while (schemaTable != null) {
                    DbRowset resultShape = new DbRowset();
                    resultShape.Class = string.Format(CultureInfo.InvariantCulture, "{0}Result", routine.MethodName);

                    // for each result column
                    int countUnamedColumns = 0;
                    for (int i = 0, count = schemaTable.Rows.Count; i < count; i++) {
                        DbColumn dbCol = new DbColumn();
                        DataRow row = schemaTable.Rows[i];

                        // Get column name, defaulting the property name if the column
                        // is not named.  Note that if the column is not named in the database,
                        // we leave it's name empty (we provide a default name for the property
                        // only).
                        dbCol.Name = Convert.ToString(row["ColumnName"], CultureInfo.InvariantCulture);
                        if (string.IsNullOrEmpty(dbCol.Name) && ++countUnamedColumns > 1) {
                            routine.SchemaErrors = true;
                            LogProcedureSchemaError(routine, "Two or more unamed columns in result set.");
                            return;
                        }
                        string propName = dbCol.Name;
                        if (string.IsNullOrEmpty(propName)) {
                            propName = string.Format("Column{0}", i + 1);
                        }
                        dbCol.Property = GetLegalLanguageName(propName);

                        // set the clr property type of the column
                        string dataTypeName = Convert.ToString(row["DataTypeName"]);
                        SqlProviderType providerType = SqlProviderType.Parse(dataTypeName);
                        SqlDbType sqlDbType = providerType.SqlDbType;
                        Type paramType = this.GetClrType(sqlDbType);
                        dbCol.Type = this.GetScopedTypeName(paramType);

                        int size = (int)this.ValueOrDefault(row["ColumnSize"], 0);

                        if (size != 0 && paramType == typeof(string)) {
                            dbCol.StringLength = size.ToString();
                        }

                        short precision = (short)this.ValueOrDefault(row["NumericPrecision"], (short)-1);
                        short scale = (short)this.ValueOrDefault(row["NumericScale"], (short)-1);

                        if (Generator.IsNumeric(paramType)) {
                            if (precision != -1) {
                                dbCol.Precision = precision.ToString();
                            }

                            if (scale != -1) {
                                dbCol.Scale = scale.ToString();
                            }
                        }

                        // set the database type of the column
                        dbCol.DbType = this.GetSqlTypeDeclaration(sqlDbType, size, precision, scale, false, false);

                        resultShape.Columns.Add(dbCol);
                    }

                    AddUniqueResultShape(resultShapes, resultShape);

                    // increment a count of total result sets returned,
                    // which may be different than the number of unique result
                    // sets returned.
                    routine.ResultSetsReturned += 1;

                    // if multiple result shapes are possible, move to next
                    reader.NextResult();
                    schemaTable = reader.GetSchemaTable();
                }

                if (resultShapes.Count > 1) {
                    int suffix = 1;
                    foreach (DbRowset shape in resultShapes) {
                        shape.Class += suffix++.ToString(CultureInfo.InvariantCulture);
                    }
                }

                routine.ResultShapes.AddRange(resultShapes);
            }
            catch (Exception e) {
                // if for any reason we are unable to completely process schema info for a routine
                // log an error, then move to the next sproc
                string msg = string.Format("Unable to get full schema info. Exception: {0}", e.Message);
                routine.SchemaErrors = true;
                LogProcedureSchemaError(routine, msg);
            }
            finally {
                if (reader != null) {
                    reader.Close();
                }
            }
        }

        private void LogProcedureSchemaError(DbRoutine routine, string errorMsg) {
            string type = routine is DbStoredProcedure ? "stored procedure" : "function";
            Console.WriteLine("Error generating {0} '{1}'. {2}", type, routine.Name, errorMsg);
        }

        /// <summary>
        /// Only add a result shape to the collection if it is not a duplicate of another.
        /// If the column count is the same, with the same order and column names/types,
        /// they are considered the same.
        /// 
        /// Duplicates can occur for example if a sproc returns the same rowset shape, but conditionally
        /// uses a different where clause for each case.
        /// </summary>       
        private void AddUniqueResultShape(List<DbRowset> rowsets, DbRowset rowset) {
            foreach (DbRowset currRowset in rowsets) {
                if (currRowset.Columns.Count != rowset.Columns.Count)
                    continue;

                for (int i = 0; i < currRowset.Columns.Count; i++) {
                    if (rowset.Columns[i].Name == currRowset.Columns[i].Name &&
                        rowset.Columns[i].DbType == currRowset.Columns[i].DbType) {
                        return;  // duplicate, so return without adding
                    }
                }
            }

            rowsets.Add(rowset);
        }

        private void GetTableColumns(DbSchema schema, DbTable table) {
            string fullName = "[" + schema.Name + "].[" + table.Name + "]";
            SqlCommand cmd = connectionManager.CreateCommand("select * from " + fullName + " where 1=0");
            using (cmd) {
                SqlDataReader dr = cmd.ExecuteReader();
                DataTable meta = dr.GetSchemaTable();
                dr.Close();
                this.GetTableColumns(meta, table);
            }
        }

        private void GetTableColumns(DataTable meta, DbTable table) {
            DataColumn AllowDBNullColumn = meta.Columns["AllowDBNull"];
            //DataColumn BaseCatalogNameColumn = meta.Columns["BaseCatalogName"];
            //DataColumn BaseColumnNameColumn = meta.Columns["BaseColumnName"];
            //DataColumn BaseSchemaNameColumn = meta.Columns["BaseSchemaName"];
            //DataColumn BaseServerNameColumn = meta.Columns["BaseServerName"];
            //DataColumn BaseTableNameColumn = meta.Columns["BaseTableName"];
            DataColumn ColumnNameColumn = meta.Columns["ColumnName"];
            //DataColumn ColumnOrdinalColumn = meta.Columns["ColumnOrdinal"];
            DataColumn ColumnSizeColumn = meta.Columns["ColumnSize"];
            DataColumn DataTypeColumn = meta.Columns["DataType"];
            //DataColumn IsAliasedColumn = meta.Columns["IsAliased"];
            DataColumn IsAutoIncrementColumn = meta.Columns["IsAutoIncrement"];
            //DataColumn IsExpressionColumn = meta.Columns["IsExpression"];
            //DataColumn IsHiddenColumn = meta.Columns["IsHidden"];
            DataColumn IsIdentityColumn = meta.Columns["IsIdentity"];
            DataColumn IsKeyColumn = meta.Columns["IsKey"];
            DataColumn IsLongColumn = meta.Columns["IsLong"];
            DataColumn IsReadOnlyColumn = meta.Columns["IsReadOnly"];
            DataColumn IsRowVersionColumn = meta.Columns["IsRowVersion"];
            DataColumn IsUniqueColumn = meta.Columns["IsUnique"];
            DataColumn NumericPrecisionColumn = meta.Columns["NumericPrecision"];
            DataColumn NumericScaleColumn = meta.Columns["NumericScale"];
            DataColumn ProviderTypeColumn = meta.Columns["ProviderType"];

            foreach (DataRow row in meta.Rows) {
                DbColumn col = new DbColumn();
                string name = (string)this.ValueOrDefault(row[ColumnNameColumn], null);
                string cname = this.GetUniqueColumnName(table, this.GetLegalLanguageName(name));
                if (string.Compare(name, cname) == 0) {
                    col.Name = cname;
                }
                else {
                    col.Name = name;
                    col.Property = cname;
                }


                if ((bool)this.ValueOrDefault(row[IsReadOnlyColumn], false)) {
                    col.IsReadOnly = "true";
                }

                bool isAutoGen = (bool)this.ValueOrDefault(row[IsAutoIncrementColumn], false);
                if (isAutoGen) {
                    col.IsAutoGen = "true";
                    col.IsReadOnly = "false";
                }
                                
                if ((bool)this.ValueOrDefault(row[IsIdentityColumn], false))
                    col.IsIdentity = "true";

                if ((bool)this.ValueOrDefault(row[IsLongColumn], false)) {
                    col.UpdateCheck = "Never";
                }

                bool nullable = (bool)this.ValueOrDefault(row[AllowDBNullColumn], false) && col.IsIdentity != "true";
                SqlDbType sqlType = (SqlDbType)(int)this.ValueOrDefault(row[ProviderTypeColumn], SqlDbType.Char);
                int size = (int)this.ValueOrDefault(row[ColumnSizeColumn], 0);

                Type clrType = this.GetClrType(sqlType);
                col.Type = this.GetScopedTypeName(clrType);

                if (nullable && clrType.IsValueType)
                    col.Nullable = "true";
                else if (!nullable && !clrType.IsValueType)
                    col.Nullable = "false";

                if ((bool)this.ValueOrDefault(row[IsRowVersionColumn], false) ||
                    sqlType == SqlDbType.Timestamp) {
                    col.IsVersion = "true";
                    col.IsReadOnly = "false";
                }

                if (size != 0 && clrType == typeof(string)) {
                    col.StringLength = size.ToString();
                }

                short precision = (short)this.ValueOrDefault(row[NumericPrecisionColumn], (short)-1);
                short scale = (short)this.ValueOrDefault(row[NumericScaleColumn], (short)-1);

                if (Generator.IsNumeric(clrType)) {
                    if (precision != -1) {
                        col.Precision = precision.ToString();
                    }

                    if (scale != -1) {
                        col.Scale = scale.ToString();
                    }
                }

                col.DbType = this.GetSqlTypeDeclaration(sqlType, size, precision, scale, !nullable, isAutoGen);

                table.Columns.Add(col);
            }
        }

        private string GetSqlTypeDeclaration(SqlDbType sqlType, int size, short precision, short scale, bool nonNull, bool isAutoGen) {
            StringBuilder sb = new StringBuilder();
            if (sqlType == SqlDbType.Timestamp)
                sb.Append("rowversion");
            else
                sb.Append(sqlType.ToString());
            if (this.HasSize(sqlType)) {
                sb.AppendFormat("({0})", size);
            }
            else if (this.HasPrecision(sqlType)) {
                sb.Append("(");
                sb.Append(precision);
                if (this.HasScale(sqlType)) {
                    sb.Append(",");
                    sb.Append(scale);
                }
                sb.Append(")");
            }
            if (nonNull)
                sb.Append(" NOT NULL");
            if (isAutoGen) {
                if (sqlType == SqlDbType.UniqueIdentifier)
                    sb.Append(" ROWGUIDCOL");
                else
                    sb.Append(" IDENTITY");
            }
            return sb.ToString();
        }

        private void GetPrimaryKeys(Database db) {            
            string query = @"
              select tc.CONSTRAINT_SCHEMA,
                tc.CONSTRAINT_NAME,
                pkcol.ORDINAL_POSITION,
                (select count(*) 
                    from INFORMATION_SCHEMA.KEY_COLUMN_USAGE kc
                    where tc.CONSTRAINT_SCHEMA = kc.CONSTRAINT_SCHEMA and
                          tc.CONSTRAINT_NAME = kc.CONSTRAINT_NAME and
                          ISNULL(OBJECTPROPERTY(OBJECT_ID(tc.TABLE_NAME), 'IsMSShipped'), 0) = 0)
                          as COUNT,
                pkcol.TABLE_SCHEMA as pkSchema,
                pkcol.TABLE_NAME as pkTable,
                pkcol.COLUMN_NAME as pkColumn
	          from INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc,
		           INFORMATION_SCHEMA.KEY_COLUMN_USAGE as pkcol
              where tc.CONSTRAINT_SCHEMA = pkcol.CONSTRAINT_SCHEMA and
                    tc.CONSTRAINT_NAME = pkcol.CONSTRAINT_NAME and
                    tc.CONSTRAINT_TYPE = 'PRIMARY KEY' and
                    ISNULL(OBJECTPROPERTY(OBJECT_ID(tc.TABLE_NAME), 'IsMSShipped'), 0) = 0
              order by 
                 tc.CONSTRAINT_SCHEMA,
                 tc.CONSTRAINT_NAME,
                 pkcol.ORDINAL_POSITION ";

            SqlCommand cmd = connectionManager.CreateCommand(query);
            SqlDataReader dr = cmd.ExecuteReader();

            try {
                while (dr.Read()) {
                    string consSchema = (string)dr["CONSTRAINT_SCHEMA"];
                    string consName = (string)dr["CONSTRAINT_NAME"];
                    int ordinal = (int)dr["ORDINAL_POSITION"];
                    int count = (int)dr["COUNT"];
                    string schemaName = (string)dr["pkSchema"];
                    string tableName = (string)dr["pkTable"];
                    string columnName = (string)dr["pkColumn"];

                    DbSchema schema = this.FindSchema(db, schemaName);
                    DbTable table = this.FindDbTable(schema, tableName);
                    DbColumn col = this.FindDbColumn(table.Columns, columnName);
                    col.IsIdentity = "true";

                    DbPrimaryKey pk = new DbPrimaryKey();
                    pk.Name = consName;
                    DbKeyColumn keyCol = new DbKeyColumn();
                    keyCol.Name = columnName;
                    pk.Columns.Add(keyCol);

                    for (; count > 1 && dr.Read(); count--) {
                        columnName = (string)dr["pkColumn"];
                        col = this.FindDbColumn(table.Columns, columnName);
                        col.IsIdentity = "true";
                        keyCol = new DbKeyColumn();
                        keyCol.Name = columnName;
                        pk.Columns.Add(keyCol);
                    }

                    table.PrimaryKey = pk;
                }
            }
            finally {
                dr.Close();
            }
        }

        private void GetUniqueKeys(Database db) {            
            string query = @"
              select tc.CONSTRAINT_SCHEMA,
                tc.CONSTRAINT_NAME,
                pkcol.ORDINAL_POSITION,
                (select count(*) 
                    from INFORMATION_SCHEMA.KEY_COLUMN_USAGE kc
                    where tc.CONSTRAINT_SCHEMA = kc.CONSTRAINT_SCHEMA and
                          tc.CONSTRAINT_NAME = kc.CONSTRAINT_NAME and
                          ISNULL(OBJECTPROPERTY(OBJECT_ID(tc.TABLE_NAME), 'IsMSShipped'), 0) = 0)
                          as COUNT,
                pkcol.TABLE_SCHEMA as pkSchema,
                pkcol.TABLE_NAME as pkTable,
                pkcol.COLUMN_NAME as pkColumn
	          from INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc,
		           INFORMATION_SCHEMA.KEY_COLUMN_USAGE as pkcol
              where tc.CONSTRAINT_SCHEMA = pkcol.CONSTRAINT_SCHEMA and
                    tc.CONSTRAINT_NAME = pkcol.CONSTRAINT_NAME and
                    tc.CONSTRAINT_TYPE = 'UNIQUE KEY' and
                    ISNULL(OBJECTPROPERTY(OBJECT_ID(tc.TABLE_NAME), 'IsMSShipped'), 0) = 0
              order by 
                 tc.CONSTRAINT_SCHEMA,
                 tc.CONSTRAINT_NAME,
                 pkcol.ORDINAL_POSITION ";

            SqlCommand cmd = connectionManager.CreateCommand(query);           
            SqlDataReader dr = cmd.ExecuteReader();

            try {
                while (dr.Read()) {
                    string consSchema = (string)dr["CONSTRAINT_SCHEMA"];
                    string consName = (string)dr["CONSTRAINT_NAME"];
                    int ordinal = (int)dr["ORDINAL_POSITION"];
                    int count = (int)dr["COUNT"];
                    string schemaName = (string)dr["pkSchema"];
                    string tableName = (string)dr["pkTable"];
                    string columnName = (string)dr["pkColumn"];

                    DbSchema schema = this.FindSchema(db, schemaName);
                    DbTable table = this.FindDbTable(schema, tableName);
                    DbColumn col = this.FindDbColumn(table.Columns, columnName);

                    DbUnique un = new DbUnique();
                    un.Name = consName;
                    DbKeyColumn keyCol = new DbKeyColumn();
                    keyCol.Name = columnName;
                    un.Columns.Add(keyCol);

                    for (; count > 1 && dr.Read(); count--) {
                        columnName = (string)dr["pkColumn"];
                        col = this.FindDbColumn(table.Columns, columnName);
                        keyCol = new DbKeyColumn();
                        keyCol.Name = columnName;
                        un.Columns.Add(keyCol);
                    }

                    table.Uniques.Add(un);
                }
            }
            finally {
                dr.Close();
            }
        }
       
        private void GetIndexes(Database db) {
            string query;

            if (connectionManager.IsPreYukon) {
                query = @"
                    select 
	                    user_name(t.uid) as [schema],
	                    object_name(i.id) as [table], 
                        i.id as [object_id],
                        i.name as [index],
                        case when i.indid = 1 then 'CLUSTERED' else 'NONCLUSTERED' end as [Style],
                        (select count(*) from sysindexkeys where id = i.id and indid = i.indid and
                         ISNULL(OBJECTPROPERTY(id, 'IsMSShipped'), 0) = 0) as [Count],
                        convert(tinyint,ik.keyno) as [Ordinal],
                        c.Name as [Column],
                        convert(bit, case when i.status & 0x2 <> 0 then 1 else 0 end) AS is_unique,
                        convert(bit, case when i.status & 0x800 <> 0 then 1 else 0 end) AS is_primary_key,
                        convert(bit, case when i.status & 0x1000 <> 0 then 1 else 0 end) AS is_unique_constraint
                    from 
                        sysindexes as i
                        join sysindexkeys ik on ik.id = i.id and ik.indid = i.indid
                        join syscolumns c on c.id = i.id and c.colid = ik.colid
                        join sysobjects t on t.id = i.id and (t.xtype='U')
                    where 
                        i.indid>=1 and i.indid<255 AND
                        ISNULL(OBJECTPROPERTY(t.id, 'IsMSShipped'), 0) = 0
                    order by
                        t.uid, t.id, i.indid, ik.keyno
                ";
            }
            else {
                query = @"
                    select s.name as [schema], t.name as [table], t.object_id, 
	                    x.name as [index], x.type_desc as [style], 
                        (select count(*) 
                            from sys.index_columns ic2
                            where ic2.object_id = ic.object_id and
                                  ic2.index_id = ic.index_id and
                                  ISNULL(OBJECTPROPERTY(ic2.object_id, 'IsMSShipped'), 0) = 0)
                            as [count],
	                    ic.key_ordinal as [ordinal], c.name as [column], 
                        x.is_unique, x.is_primary_key, x.is_unique_constraint
                    from sys.indexes as x,
                        sys.index_columns as ic,
                        sys.columns as c,
                        sys.tables as t,
                        sys.schemas as s
                    where x.object_id = ic.object_id and
                            x.index_id = ic.index_id and
                            x.object_id = c.object_id and
                            ic.column_id = c.column_id and
                            c.object_id = t.object_id and
                            t.schema_id = s.schema_id and
                            ISNULL(OBJECTPROPERTY(t.object_id, 'IsMSShipped'), 0) = 0
                    order by s.schema_id, t.object_id, x.index_id, ic.key_ordinal
                    ";
            }

            SqlCommand cmd = connectionManager.CreateCommand(query);
            SqlDataReader dr = cmd.ExecuteReader();

            try {
                while (dr.Read()) {
                    string schemaName = (string)dr["schema"];
                    string tableName = (string)dr["table"];
                    string indexName = (string)dr["index"];
                    string style = (string)dr["style"];
                    int count = (int)dr["count"];
                    int ordinal = (byte)dr["ordinal"];
                    string columnName = (string)dr["column"];
                    bool isUnique = (bool)dr["is_unique"];
                    bool isPrimary = (bool)dr["is_primary_key"];
                    bool isUnqiueConstraint = (bool)dr["is_unique_constraint"];

                    DbSchema schema = this.FindSchema(db, schemaName);
                    DbTable table = this.FindDbTable(schema, tableName);

                    if (table == null) {
                        continue;
                    }

                    DbColumn col = this.FindDbColumn(table.Columns, columnName);

                    DbIndex index = new DbIndex();
                    index.Name = indexName;
                    index.Style = style;
                    index.IsUnique = (isUnique) ? "true" : null;

                    DbKeyColumn keyCol = new DbKeyColumn();
                    if (col != null) {
                        keyCol.Name = columnName;
                        index.Columns.Add(keyCol);
                    }

                    for (; count > 1 && dr.Read(); count--) {
                        if (col != null) {
                            columnName = (string)dr["column"];
                            col = this.FindDbColumn(table.Columns, columnName);
                            keyCol = new DbKeyColumn();
                            keyCol.Name = columnName;
                            index.Columns.Add(keyCol);
                        }
                    }

                    table.Indexes.Add(index);
                }
            }
            finally {
                dr.Close();
            }
        }

        private void GetRelationships(Database db) {
            string query = @"
              select 
                rc.CONSTRAINT_SCHEMA,
                rc.CONSTRAINT_NAME,
                pkcol.ORDINAL_POSITION,
                (select count(*) from INFORMATION_SCHEMA.KEY_COLUMN_USAGE kc
                    where rc.CONSTRAINT_SCHEMA = kc.CONSTRAINT_SCHEMA and
                          rc.CONSTRAINT_NAME = kc.CONSTRAINT_NAME and
                          ISNULL(OBJECTPROPERTY(OBJECT_ID(kc.CONSTRAINT_NAME), 'IsMSShipped'), 0) = 0)
                          as COUNT,
	              fkcol.TABLE_SCHEMA as fkSchema,
                fkcol.TABLE_NAME as fkTable,
                fkcol.COLUMN_NAME as fkColumn,
                pkcol.TABLE_SCHEMA as pkSchema,
                pkcol.TABLE_NAME as pkTable,
                pkcol.COLUMN_NAME as pkColumn,
                rc.UPDATE_RULE,
                rc.DELETE_RULE
	          from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc,
                   INFORMATION_SCHEMA.KEY_COLUMN_USAGE as pkcol,
                   INFORMATION_SCHEMA.KEY_COLUMN_USAGE as fkcol
              where rc.CONSTRAINT_SCHEMA = fkcol.CONSTRAINT_SCHEMA and
                 rc.CONSTRAINT_NAME = fkcol.CONSTRAINT_NAME and
                 rc.UNIQUE_CONSTRAINT_SCHEMA = pkcol.CONSTRAINT_SCHEMA and
                 rc.UNIQUE_CONSTRAINT_NAME = pkcol.CONSTRAINT_NAME and
                 pkcol.ORDINAL_POSITION = fkcol.ORDINAL_POSITION and
                 ISNULL(OBJECTPROPERTY(OBJECT_ID(rc.CONSTRAINT_NAME), 'IsMSShipped'), 0) = 0
              order by 
                 rc.CONSTRAINT_SCHEMA,
                 rc.CONSTRAINT_NAME,
                 pkcol.ORDINAL_POSITION ";

            SqlCommand cmd = connectionManager.CreateCommand(query);           
            SqlDataReader dr = cmd.ExecuteReader();

            try {
                while (dr.Read()) {
                    string consSchema = (string)dr["CONSTRAINT_SCHEMA"];
                    string consName = (string)dr["CONSTRAINT_NAME"];
                    int ordinal = (int)dr["ORDINAL_POSITION"];
                    int count = (int)dr["COUNT"];
                    string fkSchema = (string)dr["fkSchema"];
                    string fkTable = (string)dr["fkTable"];
                    string fkColumn = (string)dr["fkColumn"];
                    string pkSchema = (string)dr["pkSchema"];
                    string pkTable = (string)dr["pkTable"];
                    string pkColumn = (string)dr["pkColumn"];
                    string updateRule = (string)dr["UPDATE_RULE"];
                    string deleteRule = (string)dr["DELETE_RULE"];

                    DbSchema srcSchema = this.FindSchema(db, fkSchema);
                    DbTable srcTable = this.FindDbTable(srcSchema, fkTable);
                    DbColumn srcCol = this.FindDbColumn(srcTable.Columns, fkColumn);
                    DbSchema tarSchema = this.FindSchema(db, pkSchema);
                    DbTable tarTable = this.FindDbTable(tarSchema, pkTable);
                    DbColumn tarCol = this.FindDbColumn(tarTable.Columns, pkColumn);

                    DbAssociation srcRel = new DbAssociation();
                    srcRel.Name = consName;
                    srcRel.Kind = RelationshipKind.ManyToOneParent;
                    srcRel.Target = tarTable.Name;
                    if (updateRule != "NO ACTION")
                        srcRel.UpdateRule = updateRule;
                    if (deleteRule != "NO ACTION")
                        srcRel.DeleteRule = deleteRule;
                    srcTable.Associations.Add(srcRel);

                    DbKeyColumn srcKeyCol = new DbKeyColumn();
                    srcKeyCol.Name = srcCol.Name;
                    srcRel.Columns.Add(srcKeyCol);

                    DbAssociation tarRel = new DbAssociation();
                    tarRel.Name = consName;
                    tarRel.Kind = RelationshipKind.ManyToOneChild;
                    tarRel.Target = srcTable.Name;
                    if (updateRule != "NO ACTION")
                        tarRel.UpdateRule = updateRule;
                    if (deleteRule != "NO ACTION")
                        tarRel.DeleteRule = deleteRule;
                    tarTable.Associations.Add(tarRel);

                    DbKeyColumn tarKeyCol = new DbKeyColumn();
                    tarKeyCol.Name = tarCol.Name;
                    tarRel.Columns.Add(tarKeyCol);

                    // read additional columns for this relationship
                    for (; count > 1 && dr.Read(); count--) {
                        fkColumn = (string)dr["fkColumn"];
                        pkColumn = (string)dr["pkColumn"];
                        srcCol = this.FindDbColumn(srcTable.Columns, fkColumn);
                        tarCol = this.FindDbColumn(tarTable.Columns, pkColumn);
                        srcKeyCol = new DbKeyColumn();
                        srcKeyCol.Name = srcCol.Name;
                        srcRel.Columns.Add(srcKeyCol);
                        tarKeyCol = new DbKeyColumn();
                        tarKeyCol.Name = tarCol.Name;
                        tarRel.Columns.Add(tarKeyCol);
                    }

                    if (this.IsOneToOne(srcTable, srcRel.Columns)) {
                        tarRel.Kind = RelationshipKind.OneToOneChild;
                        srcRel.Kind = RelationshipKind.OneToOneParent;
                    }
                }
            }
            finally {
                dr.Close();
            }

            // now go figure out appropriate names for association properties
            foreach (DbSchema schema in db.Schemas) {
                foreach (DbTable table in schema.Tables) {
                    foreach (DbAssociation assoc in table.Associations) {
                        this.InferAssociationPropertyName(db, table, assoc);
                    }
                }
            }
        }

        private void InferAssociationPropertyName(Database db, DbTable table, DbAssociation assoc) {
            string candidate = null;
            DbTable target = this.FindDbTable(db, assoc.Target);
            // try using the target table's name to infer the property name
            if (assoc.Kind == RelationshipKind.ManyToOneChild) {
                candidate = this.GetLegalLanguageName(target.Property != null ? target.Property : target.Name);
                if (this.options.Pluralize)
                    candidate = this.GetPluralName(candidate);
            }
            else {
                candidate = this.GetLegalLanguageName(target.Class != null ? target.Class : target.Name);
                if (this.options.Pluralize)
                    candidate = this.GetSingularName(candidate);
            }
            string bestCandidate = candidate;
            if (this.IsOnlyAssociationForTarget(table, assoc) && this.IsUniqueName(table, candidate) &&
                (table != target || assoc.Kind == RelationshipKind.ManyToOneChild)) {
                assoc.Property = candidate;
                return;
            }
            // if the association is based on a single column key, then try inferring name from the column
            if (assoc.Columns.Count == 1) {
                candidate = this.GetLegalLanguageName(assoc.Columns[0].Name);
                bool endsInId = candidate.EndsWith("id", StringComparison.CurrentCultureIgnoreCase);
                if (candidate.Length > 4 && endsInId) {
                    candidate = candidate.Substring(0, candidate.Length - 2);
                    if (this.options.Pluralize) {
                        if (assoc.Kind == RelationshipKind.ManyToOneChild)
                            candidate = this.GetPluralName(candidate);
                        else
                            candidate = this.GetSingularName(candidate);
                    }
                    if (this.IsUniqueName(table, candidate)) {
                        assoc.Property = candidate;
                        return;
                    }
                }
                else if (!endsInId) {
                    string ext;
                    if (assoc.Kind == RelationshipKind.ManyToOneChild) {
                        ext = this.GetLegalLanguageName(target.Property != null ? target.Property : target.Name);
                        if (this.options.Pluralize)
                            ext = this.GetPluralName(ext);
                    }
                    else {
                        ext = this.GetLegalLanguageName(target.Class != null ? target.Class : target.Name);
                        if (this.options.Pluralize)
                            ext = this.GetSingularName(ext);
                    }
                    candidate += ext;
                    if (this.IsUniqueName(table, candidate)) {
                        assoc.Property = candidate;
                        return;
                    }
                }
            }
            // if the association has a name, try using it
            if (assoc.Name != null) {
                candidate = assoc.Name;
                if (string.Compare(candidate, 0, "fk_", 0, 3, StringComparison.OrdinalIgnoreCase) == 0)
                    candidate = candidate.Substring(3);
                else if (string.Compare(candidate, 0, "fk", 0, 2, StringComparison.OrdinalIgnoreCase) == 0)
                    candidate = candidate.Substring(2);
                candidate = this.GetLegalLanguageName(candidate);
                if (this.options.Pluralize) {
                    if (assoc.Kind == RelationshipKind.ManyToOneChild)
                        candidate = this.GetPluralName(candidate);
                    else
                        candidate = this.GetSingularName(candidate);
                }
                if (this.IsUniqueName(table, candidate)) {
                    assoc.Property = candidate;
                    return;
                }
            }
            // try some numeric variant of the best candidate
            for (int i = 0; i < 1000; i++) {
                candidate = bestCandidate + i;
                if (this.IsUniqueName(table, candidate)) {
                    assoc.Property = candidate;
                    return;
                }
            }
            throw new Exception("Cannot infer property name for association");
        }

        private bool IsOnlyAssociationForTarget(DbTable table, DbAssociation assoc) {
            foreach (DbAssociation ass in table.Associations) {
                if (ass != assoc && ass.Target == assoc.Target)
                    return false;
            }
            return true;
        }

        private string GetUniqueColumnName(DbTable table, string suggestedName) {
            string typeName = table.Class != null ? table.Class : table.Name;
            if (String.Compare(typeName, suggestedName, StringComparison.Ordinal) == 0) {
                if (this.IsUniqueName(table, "Content")) {
                    return "Content";
                }
                else if (this.IsUniqueName(table, "Body")) {
                    return "Body";
                }
                return suggestedName + "2";
            }
            else {
                return suggestedName;
            }
        }

        private bool IsUniqueName(DbTable table, string name) {
            if (name == null || name.Length == 0)
                return false;
            string typeName = table.Class != null ? table.Class : table.Name;
            if (String.Compare(name, typeName, StringComparison.OrdinalIgnoreCase) == 0)
                return false;
            if (table.Columns != null) {
                foreach (DbColumn col in table.Columns) {
                    string aname = col.Property != null ? col.Property : GetLegalLanguageName(col.Name);
                    if (String.Compare(aname, name, true) == 0) {
                        return false;
                    }
                }
            }
            if (table.Associations != null) {
                foreach (DbAssociation assoc in table.Associations) {
                    string aname = assoc.Property != null ? assoc.Property : assoc.Name;
                    if (String.Compare(aname, name, true) == 0)
                        return false;
                }
            }
            return true;
        }

        private bool IsOneToOne(DbTable table, List<DbKeyColumn> keyColumns) {
            int n = keyColumns.Count;
            // check if matches entire primary key
            if (table.PrimaryKey != null) {
                if (this.ColumnsMatch(table.PrimaryKey.Columns, keyColumns))
                    return true;
            }
            // try unique constraints
            if (table.Uniques != null) {
                foreach (DbUnique un in table.Uniques) {
                    if (this.ColumnsMatch(un.Columns, keyColumns))
                        return true;
                }
            }
            // try unique indexes
            if (table.Indexes != null) {
                foreach (DbIndex index in table.Indexes) {
                    if (index.IsUnique == "true" && this.ColumnsMatch(index.Columns, keyColumns))
                        return true;
                }
            }
            return false;
        }

        private bool ColumnsMatch(List<DbKeyColumn> a, List<DbKeyColumn> b) {
            int n = a.Count;
            if (n != b.Count) return false;
            for (int i = 0; i < n; i++) {
                if (a[i].Name != b[i].Name)
                    return false;
            }
            return true;
        }

        private DbSchema FindSchema(Database db, string name) {
            if (db == null) return null;
            foreach (DbSchema s in db.Schemas) {
                if (string.Compare(s.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return s;
            }
            return null;
        }

        private DbTable FindDbTable(DbSchema schema, string name) {
            if (schema == null) return null;
            foreach (DbTable table in schema.Tables) {
                if (string.Compare(table.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return table;
            }
            return null;
        }
       

        private DbTable FindDbTable(Database db, string name) {
            if (db == null) return null;
            foreach (DbSchema schema in db.Schemas) {
                foreach (DbTable table in schema.Tables) {
                    if (string.Compare(table.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                        return table;
                }
            }
            return null;
        }

        private DbColumn FindDbColumn(IEnumerable<DbColumn> cols, string name) {
            if (cols == null) return null;
            foreach (DbColumn col in cols) {
                if (string.Compare(col.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                    return col;
            }
            return null;
        }

        private bool HasSize(SqlDbType type) {
            switch (type) {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                case SqlDbType.Binary:
                case SqlDbType.VarBinary:
                    return true;
                default:
                    return false;
            }
        }

        private bool HasPrecision(SqlDbType type) {
            switch (type) {
                case SqlDbType.Decimal:
                    return true;
                default:
                    return false;
            }
        }

        private bool HasScale(SqlDbType type) {
            switch (type) {
                case SqlDbType.Decimal:
                    return true;
                default:
                    return false;
            }
        }

        private string GetScopedTypeName(Type type) {
            if (type.Namespace != null && type.Namespace.Length > 0) {
                return type.Namespace + "." + type.Name;
            }
            return type.Name;
        }

        private string StringValueOrDefault(object value, string defvalue) {
            if (value == DBNull.Value || value == null)
                return defvalue;
            return value.ToString();
        }

        private object ValueOrDefault(object value, object defvalue) {
            if (value == DBNull.Value || value == null) return defvalue;
            return value;
        }

        private string GetIfDifferent(string alias, string name) {
            if (name == alias) return null;
            return alias;
        }

        private string GetLegalLanguageName(string name) {
            StringBuilder sb = new StringBuilder();
            bool skipped = true;
            if (name[0] >= '0' && name[0] <= '9') {
                sb.Append("number");
            }
            for (int i = 0; i < name.Length; i++) {
                char c = name[i];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')) {
                    if (skipped) {
                        c = Char.ToUpper(c);
                    }
                    skipped = false;
                    sb.Append(c);
                }
                else {
                    skipped = true;
                }
            }

            name = sb.ToString();
            name = LegalizeKeywords(name);

            return name;
        }

        /// <summary>
        /// Temporary code until new code dom providers for the languages handle
        /// the new keywords properly.  At that time this method should be removed.
        /// </summary>
        private string LegalizeKeywords(string name) {
            //List<string> vbKeywords = new List<string>{"from", "where", "order", "group", "by", "ascending", "descending", "distinct"};
            List<string> vbKeywords = new List<string>();
            vbKeywords.Add("from");
            vbKeywords.Add("where");
            vbKeywords.Add("order");
            vbKeywords.Add("group");
            vbKeywords.Add("by");
            vbKeywords.Add("ascending");
            vbKeywords.Add("descending");
            vbKeywords.Add("distinct");

            if( options.Language == LanguageType.VisualBasic ) {
                // these keywords are not case sensitive
                if( vbKeywords.Contains(name.ToLower()) ) {
                    name += "_";
                }
            }
            else if( options.Language == LanguageType.CSharp ) {
                // case sensitive compare for 'var' keyword
                if( string.Compare(name, "var", false) == 0 ) {
                    name = "@" + name;
                }
            }

            return name;
        }

        private bool IsVowel(char c) {
            switch (Char.ToLower(c)) {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u':
                case 'y':
                    return true;
                default:
                    return false;
            }
        }

        private string GetSingularName(string name) {
            string lower = name.ToLower();
            if (lower.EndsWith("ies") && lower.Length > 3) {
                if (!this.IsVowel(lower[lower.Length - 4])) {
                    name = name.Substring(0, name.Length - 3) + "y";
                }   
            }
            else if (lower.EndsWith("ees")) {
                name = name.Substring(0, name.Length - 1);
            }
            else if (lower.EndsWith("es")) {
                string n = name.Substring(0, name.Length - 2);
                string nl = n.ToLower();
                if (nl.EndsWith("ch") || nl.EndsWith("x"))
                    name = n;
            }
            else if (lower.EndsWith("s") && !this.IsVowel(lower[lower.Length - 1]) & !lower.EndsWith("ss")) {
                name = name.Substring(0, name.Length - 1);
            }

            name = LegalizeKeywords(name);

            return name;
        }

        private string GetPluralName(string name) {
            string lower = name.ToLower();
            if (lower.EndsWith("x") || lower.EndsWith("ch") || lower.EndsWith("ss")) {
                name = name + "es";
            }
            else if (lower.EndsWith("y") && lower.Length > 1 && !this.IsVowel(lower[lower.Length - 2])) {
                name = name.Substring(0, name.Length - 1) + "ies";
            }
            else if (!lower.EndsWith("s")) {
                name = name + "s";
            }

            name = LegalizeKeywords(name);

            return name;
        }

        private string GetBrackettedName(string name) {
            if (name[0] == '[') return name;
            return "[" + name + "]";
        }

        private Type GetClrType(SqlDbType sqlType, bool isNullable) {
            Type baseType = this.GetClrType(sqlType);
            if (isNullable) {
                Type nt = typeof(Nullable<>);
                return nt.MakeGenericType(baseType);
            }
            return baseType;
        }

        private Type GetClrType(SqlDbType sqlType) {
            SqlProviderType providerType = SqlProviderType.Create(sqlType);
            return providerType.GetClosestRuntimeType();                 
        }
    }
}