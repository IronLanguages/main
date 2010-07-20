using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using SqlMetal;

namespace MerlinWeb.UI {
    internal class MerlinColumn {
        private string _name;
        private TypeCode _typeCode;
        private bool _primaryKey;
        private string _foreignTableName;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public TypeCode TypeCode {
            get { return _typeCode; }
            set { _typeCode = value; }
        }

        public bool PrimaryKey {
            get { return _primaryKey; }
            set { _primaryKey = value; }
        }

        public string ForeignTableName {
            get { return _foreignTableName; }
            set { _foreignTableName = value; }
        }

        public bool IsForeignKey {
            get { return _foreignTableName != null; }
        }
    }

    internal class MerlinTable {
        private string _name;
        private List<MerlinColumn> _columns = new List<MerlinColumn>();
        internal string[] _primaryKeys;
        private List<DbAssociation> _foreignKeys = new List<DbAssociation>();
        private string _selectCommand;
        private string _selectOneCommand;
        private string _selectSomeCommand;
        private string _updateCommand;
        private string _deleteCommand;
        private string _insertCommand;

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public List<MerlinColumn> Columns {
            get { return _columns; }
            set { _columns = value; }
        }

        public string[] PrimaryKeys {
            get { return _primaryKeys; }
        }

        public List<DbAssociation> ForeignKeys {
            get { return _foreignKeys; }
            set { _foreignKeys = value; }
        }

        public bool HasPrimaryKey {
            get { return PrimaryKeys.Length > 0; }
        }

        public string SelectCommand {
            get {
                if (_selectCommand == null) {
                    _selectCommand = "SELECT * FROM [" + Name + "]";
                }

                return _selectCommand;
            }
        }

        public string SelectOneCommand {
            get {
                if (_selectOneCommand == null) {
                    StringBuilder builder = new StringBuilder();

                    // It starts the same as the select all command
                    builder.Append(SelectCommand);

                    AppendPKWhereClause(builder);

                    _selectOneCommand = builder.ToString();
                }

                return _selectOneCommand;
            }
        }

        public string GetSelectSomeCommand(string columnName) {
            if (_selectSomeCommand == null) {
                _selectSomeCommand = SelectCommand + "WHERE [{0}] = @{1}";
            }

            return String.Format(_selectSomeCommand, columnName, columnName);
        }

        public string UpdateCommand {
            get {
                // Can't perform update if there is no primary key
                if (!HasPrimaryKey)
                    return null;

                if (_updateCommand == null) {
                    // "UPDATE [TableName] SET [Name] = @Name, [Age] = @Age WHERE [PK1] = @PK1 AND [PK2] = @PK2"
                    StringBuilder builder = new StringBuilder();
                    builder.Append("UPDATE [");
                    builder.Append(Name);
                    builder.Append("] SET ");

                    bool first = true;
                    foreach (MerlinColumn column in Columns) {

                        // Ignore columns that are primary keys since their vales don't change
                        if (column.PrimaryKey) continue;

                        if (!first) builder.Append(", ");

                        builder.Append("[");
                        builder.Append(column.Name);
                        builder.Append("] = @");
                        builder.Append(column.Name);
                        first = false;
                    }

                    AppendPKWhereClause(builder);

                    _updateCommand = builder.ToString();
                }

                return _updateCommand;
            }
        }

        public string DeleteCommand {
            get {
                // Can't perform delete if there is no primary key
                if (!HasPrimaryKey)
                    return null;

                if (_deleteCommand == null) {
                    // DELETE FROM [TableName] WHERE [PK1] = @PK1 AND [PK2] = @PK2
                    StringBuilder builder = new StringBuilder();
                    builder.Append("DELETE FROM [");
                    builder.Append(Name);
                    builder.Append("]");

                    AppendPKWhereClause(builder);

                    _deleteCommand = builder.ToString();
                }

                return _deleteCommand;
            }
        }

        public string InsertCommand {
            get {
                if (_insertCommand == null) {
                    // e.g. "INSERT INTO [TableName] ([PK1], [PK2], [Name]) VALUES (@PK1, @PK2, @Name)"
                    StringBuilder builder = new StringBuilder();
                    builder.Append("INSERT INTO [");
                    builder.Append(Name);
                    builder.Append("] (");

                    bool first = true;
                    foreach (MerlinColumn column in Columns) {
                        if (!first) builder.Append(", ");

                        builder.Append("[");
                        builder.Append(column.Name);
                        builder.Append("]");
                        first = false;
                    }

                    builder.Append(") VALUES (");

                    first = true;
                    foreach (MerlinColumn column in Columns) {
                        if (!first) builder.Append(", ");

                        builder.Append("@");
                        builder.Append(column.Name);
                        first = false;
                    }

                    builder.Append(")");

                    _insertCommand = builder.ToString();
                }

                return _insertCommand;
            }
        }

        private void AppendPKWhereClause(StringBuilder builder) {

            builder.Append(" WHERE ");

            // Append a WHERE clause.  e.g. WHERE [PK1] = @PK1 AND [PK2] = @PK2
            bool first = true;
            foreach (MerlinColumn column in Columns) {

                // Ignore columns that are not primary keys
                if (!column.PrimaryKey) continue;

                if (!first) builder.Append(" AND ");

                builder.Append("[");
                builder.Append(column.Name);
                builder.Append("] = @");
                builder.Append(column.Name);
                first = false;
            }
        }
    }

    internal class MerlinDatabase {
        private static MerlinDatabase _theDatabase;

        private string _connString;
        private Dictionary<string, MerlinTable> _tables = new Dictionary<string, MerlinTable>();

        public static MerlinDatabase TheDatabase {
            get {
                if (_theDatabase == null) {
                    _theDatabase = new MerlinDatabase(ConfigurationManager.ConnectionStrings[2].ConnectionString);
                }

                return _theDatabase;
            }
        }

        public MerlinDatabase(string connString) {
            _connString = connString;
            GetSchema();
        }

        public string ConnectionString {
            get { return _connString; }
            set { _connString = value; }
        }

        public Dictionary<string, MerlinTable> Tables {
            get { return _tables; }
            set { _tables = value; }
        }

        private void GetSchema() {
            ExtractOptions extractOptions = new ExtractOptions();
            extractOptions.Types = ExtractTypes.Tables | ExtractTypes.Relationships;
            Extractor extractor = new Extractor(ConnectionString, extractOptions);

            // REVIEW: is the database name of any relevance?
            Database database = extractor.ExtractDatabase("dummy");

            // REVIEW: when would we get multiple schemas?
            DbSchema schema = database.Schemas[0];

            foreach (DbTable dbTable in schema.Tables) {
                MerlinTable table = new MerlinTable();
                table.Name = dbTable.Name;
                _tables[dbTable.Name] = table;

                foreach (DbColumn dbColumn in dbTable.Columns) {
                    MerlinColumn column = new MerlinColumn();
                    column.Name = dbColumn.Name;
                    column.TypeCode = TypeCodeFromTypeName(dbColumn.Type);
                    column.PrimaryKey = (dbColumn.IsIdentity == "true");

                    if (column.TypeCode == TypeCode.DBNull)
                        continue;

                    table.Columns.Add(column);
                }

                if (dbTable.PrimaryKey != null) {
                    // Build a string array of primary key names
                    table._primaryKeys = new string[dbTable.PrimaryKey.Columns.Count];

                    int index = 0;
                    foreach (DbKeyColumn dbKeyColumn in dbTable.PrimaryKey.Columns) {
                        table._primaryKeys[index++] = dbKeyColumn.Name;
                    }
                }
                else {
                    table._primaryKeys = new string[0];
                }

                foreach (DbAssociation association in dbTable.Associations) {
                    if (association.Kind == RelationshipKind.ManyToOneParent) {

                        // Our column(s) is the primary key of another table
                        
                        Debug.Assert(association.Columns.Count == 1);
                        string columnName = association.Columns[0].Name;
                        MerlinColumn column = table.Columns.Find(
                            delegate(MerlinColumn c) { return (c.Name == columnName); });
                        Debug.Assert(column != null);

                        column.ForeignTableName = association.Target;
                    }
                    else if (association.Kind == RelationshipKind.ManyToOneChild) {

                        // Other tables that have a column matching our primary key

                        Debug.Assert(association.Columns.Count == 1);
                        table.ForeignKeys.Add(association);
                    }
                }
            }
        }


        private static Dictionary<string, TypeCode> s_typeNameToTypeCodeMap;

        private static TypeCode TypeCodeFromTypeName(string typeName) {

            if (s_typeNameToTypeCodeMap == null) {
                s_typeNameToTypeCodeMap = new Dictionary<string, TypeCode>();
                s_typeNameToTypeCodeMap["System.Boolean"] = TypeCode.Boolean;
                s_typeNameToTypeCodeMap["System.String"] = TypeCode.String;
                s_typeNameToTypeCodeMap["System.Int16"] = TypeCode.Int16;
                s_typeNameToTypeCodeMap["System.Int32"] = TypeCode.Int32;
                s_typeNameToTypeCodeMap["System.Single"] = TypeCode.Double;
                s_typeNameToTypeCodeMap["System.Double"] = TypeCode.Double;
                s_typeNameToTypeCodeMap["System.Decimal"] = TypeCode.Decimal;
                s_typeNameToTypeCodeMap["System.DateTime"] = TypeCode.DateTime;

                // We don't support byte arrays
                s_typeNameToTypeCodeMap["System.Byte[]"] = TypeCode.DBNull;
            }

            TypeCode typeCode;
            if (s_typeNameToTypeCodeMap.TryGetValue(typeName, out typeCode))
                return typeCode;

            Debug.Assert(false);
            return TypeCode.DBNull;
        }
    }


}
