using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SqlMetal {

    public abstract class DbElement {
        [XmlAttribute]
        public string Name;
        [XmlAttribute]
        public string Hidden;
        [XmlAttribute]
        public string Access;
    }

    public abstract class DbObject : DbElement {
    }

    public class Database : DbElement {
        [XmlAttribute]
        public string Class;
        [XmlElement(ElementName = "Schema")]
        public List<DbSchema> Schemas = new List<DbSchema>();
    }

    public class DbSchema : DbElement {
        [XmlAttribute]
        public string Property;
        [XmlAttribute]
        public string Class;
        [XmlElement(ElementName = "Table")]
        public List<DbTable> Tables = new List<DbTable>();
        [XmlElement(ElementName = "View")]
        public List<DbView> Views = new List<DbView>();
        [XmlElement(ElementName = "StoredProcedure")]
        public List<DbStoredProcedure> StoreProcedures = new List<DbStoredProcedure>();
        [XmlElement(ElementName = "Function")]
        public List<DbFunction> Functions = new List<DbFunction>();
    }

    /// <summary>
    /// Can represent base information for either a Stored Procedure
    /// or a User Defined Function.
    /// </summary>
    public class DbRoutine : DbObject {
        /// <summary>
        /// This is a count of all possible result sets returned from
        /// the routine.  Note that this might differ from ResultShapes.Count,
        /// since this count includes duplicate shapes.
        /// </summary>
        internal int ResultSetsReturned;
        [XmlAttribute]
        public string MethodName;
        [XmlElement(ElementName = "Parameter")]
        public List<DbParameter> Parameters = new List<DbParameter>();
        /// <summary>
        /// List of all unique result shapes returned from the routine.
        /// </summary>
        [XmlElement(ElementName = "ResultShape")]
        public List<DbRowset> ResultShapes = new List<DbRowset>();
        [XmlAttribute]
        public bool SchemaErrors;
    }

    public class DbStoredProcedure : DbRoutine {
    }

    public class DbFunction : DbRoutine {
        public bool IsTableValued;
        [XmlAttribute]
        public string Type;
        [XmlAttribute]
        public string DbType;
    }

    public class DbRowset : DbObject {
        [XmlAttribute]
        public string Class;
        [XmlElement(ElementName = "Column")]
        public List<DbColumn> Columns = new List<DbColumn>();
    }

    public class DbParameter : DbElement {
        [XmlAttribute]
        public string ParameterName;
        [XmlAttribute]
        public string Type;
        [XmlAttribute]
        public string DbType;
        [XmlAttribute]
        public System.Data.ParameterDirection ParameterDirection;        
    }

    public class DbTable : DbObject {
        [XmlAttribute]
        public string Property;
        [XmlAttribute]
        public string Class;
        [XmlElement(ElementName = "Column")]
        public List<DbColumn> Columns = new List<DbColumn>();
        [XmlElement(ElementName = "Association")]
        public List<DbAssociation> Associations = new List<DbAssociation>();
        [XmlElement(ElementName = "PrimaryKey")]
        public DbPrimaryKey PrimaryKey;
        [XmlElement(ElementName = "Unique")]
        public List<DbUnique> Uniques;
        [XmlElement(ElementName = "Index")]
        public List<DbIndex> Indexes = new List<DbIndex>();

        #region Inheritance support
        [XmlElement(ElementName = "Type")]
        public DbType Type;
        #endregion
    }

    #region Inheritance support
    public class DbType : DbObject {
        [XmlElement(ElementName = "Column")]
        public List<DbColumn> Columns = new List<DbColumn>();

        [XmlElement(ElementName = "Association")]
        public List<DbAssociation> Associations = new List<DbAssociation>();

        [XmlElement(ElementName = "Type")]
        public List<DbType> DerivedTypes = new List<DbType>();

        [XmlAttribute]
        public string InheritanceCode;

        [XmlAttribute]
        public string IsInheritanceDefault = "false";
    }
    #endregion

    public class DbView : DbTable {
        [XmlAttribute]
        public bool SchemaErrors;
   }

    public class DbColumn : DbElement {
        [XmlAttribute]
        public string Property;
        [XmlAttribute]
        public string Type;
        [XmlAttribute]
        public string DbType;
        [XmlAttribute]
        public string Nullable;
        [XmlAttribute]
        public string IsIdentity;
        [XmlAttribute]
        public string IsAutoGen;
        [XmlAttribute]
        public string IsVersion;
        [XmlAttribute]
        public string IsReadOnly;
        [XmlAttribute]
        public string UpdateCheck;

        #region ASP.NET support
        [XmlAttribute]
        public string StringLength;
        [XmlAttribute]
        public string Precision;
        [XmlAttribute]
        public string Scale;
        [XmlAttribute]
        public string DisplayName;
        #endregion

        #region Inheritance support
        [XmlAttribute]
        public string IsDiscriminator;
        #endregion
    }


    public enum RelationshipKind {
        OneToOneChild,
        OneToOneParent,
        ManyToOneChild,
        ManyToOneParent
    }

    public class DbAssociation : DbObject {
        [XmlAttribute]
        public string Property;
        [XmlAttribute]
        public RelationshipKind Kind;
        [XmlAttribute]
        public string Target;
        [XmlAttribute]
        public string UpdateRule;
        [XmlAttribute]
        public string DeleteRule;
        [XmlElement(ElementName = "Column")]
        public List<DbKeyColumn> Columns = new List<DbKeyColumn>();
    }

    public class DbKeyColumn {
        [XmlAttribute]
        public string Name;
    }

    public class DbPrimaryKey : DbObject {
        [XmlElement(ElementName = "Column")]
        public List<DbKeyColumn> Columns = new List<DbKeyColumn>();
    }

    public class DbUnique : DbObject {
        [XmlElement(ElementName = "Column")]
        public List<DbKeyColumn> Columns = new List<DbKeyColumn>();
    }

    public class DbIndex : DbObject {
        [XmlAttribute]
        public string Style;
        [XmlAttribute]
        public string IsUnique;
        [XmlElement(ElementName = "Column")]
        public List<DbKeyColumn> Columns = new List<DbKeyColumn>();
    }
}
