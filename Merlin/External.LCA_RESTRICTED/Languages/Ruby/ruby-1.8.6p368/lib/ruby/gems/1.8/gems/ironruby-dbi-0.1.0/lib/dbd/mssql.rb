require 'dbi'

module DBI
  module DBD



    module MSSQL

      VERSION          = "0.1.0"
      USED_DBD_VERSION = "0.4.0"
      DESCRIPTION      = "ADO.NET DBI DBD"

      require File.dirname(__FILE__) + "/mssql/types"

      # Hash to translate MS SQL Server type names to DBI SQL type constants
      #
      # Only used in #mssql_type_info.
      #
      MSSQL_TO_XOPEN = {
              "TINYINT"          => [DBI::SQL_TINYINT, 1, nil],
              "SMALLINT"         => [DBI::SQL_SMALLINT, 2, nil],
              "INT"              => [DBI::SQL_INTEGER, 4, nil],
              "INTEGER"          => [DBI::SQL_INTEGER, 4, nil],
              "BIGINT"           => [DBI::SQL_BIGINT, 8, nil],
              "REAL"             => [DBI::SQL_REAL, 24, nil],
              "FLOAT"            => [DBI::SQL_FLOAT, 12, nil],
              "DECIMAL"          => [DBI::SQL_DECIMAL, 18, nil],
              "NUMERIC"          => [DBI::SQL_DECIMAL, 18, nil],
              "MONEY"            => [DBI::SQL_DECIMAL, 8, 4],
              "SMALLMONEY"       => [DBI::SQL_DECIMAL, 4, 4],
              "DATE"             => [DBI::SQL_DATE, 10, nil],
              "TIME"             => [DBI::SQL_TIME, 8, nil],
              "DATETIME2"        => [DBI::SQL_TIMESTAMP, 19, nil],
              "DATETIME"         => [DBI::SQL_TIMESTAMP, 19, nil],
              "CHAR"             => [DBI::SQL_CHAR, 1, nil],
              "VARCHAR"          => [DBI::SQL_VARCHAR, 255, nil],
              "NCHAR"            => [DBI::SQL_CHAR, 1, nil],
              "NVARCHAR"         => [DBI::SQL_VARCHAR, 255, nil],
              "TEXT"             => [DBI::SQL_VARCHAR, 65535, nil],
              "NTEXT"            => [DBI::SQL_VARCHAR, 131070, nil],
              "BINARY"           => [DBI::SQL_VARBINARY, 65535, nil],
              "VARBINARY"        => [DBI::SQL_VARBINARY, 16277215, nil],
              "IMAGE"            => [DBI::SQL_LONGVARBINARY, 2147483657, nil],
              "BIT"              => [DBI::SQL_BIT, 1, nil],
              "UNIQUEIDENTIFIER" => [DBI::SQL_VARCHAR, 20, nil],
              "XML"              => [DBI::SQL_VARCHAR, 65535, nil],
              "TIMESTAMP"        => [DBI::SQL_VARCHAR, 18, nil],
              nil                => [DBI::SQL_OTHER, nil, nil]
      }

      MSSQL_TYPEMAP = {
              "TINYINT"          => DBI::Type::Integer,
              "SMALLINT"         => DBI::Type::Integer,
              "INT"              => DBI::Type::Integer,
              "INTEGER"          => DBI::Type::Integer,
              "BIGINT"           => DBI::Type::Integer,
              "REAL"             => DBI::Type::Float,
              "FLOAT"            => DBI::Type::Float,
              "DECIMAL"          => DBI::DBD::MSSQL::Type::Decimal,
              "NUMERIC"          => DBI::DBD::MSSQL::Type::Decimal,
              "MONEY"            => DBI::DBD::MSSQL::Type::Decimal,
              "SMALLMONEY"       => DBI::DBD::MSSQL::Type::Decimal,
              "DATE"             => DBI::DBD::MSSQL::Type::Date,
              "TIME"             => DBI::DBD::MSSQL::Type::Timestamp,
              "DATETIME2"        => DBI::DBD::MSSQL::Type::Timestamp,
              "DATETIME"         => DBI::DBD::MSSQL::Type::Timestamp,
              "CHAR"             => DBI::Type::Varchar,
              "VARCHAR"          => DBI::Type::Varchar,
              "NCHAR"            => DBI::Type::Varchar,
              "NVARCHAR"         => DBI::Type::Varchar,
              "TEXT"             => DBI::Type::Varchar,
              "NTEXT"            => DBI::Type::Varchar,
              "BINARY"           => DBI::Type::Varchar,
              "VARBINARY"        => DBI::Type::Varchar,
              "IMAGE"            => DBI::Type::Varchar,
              "BIT"              => DBI::Type::Boolean,
              "UNIQUEIDENTIFIER" => DBI::Type::Varchar,
              "XML"              => DBI::Type::Varchar,
              "TIMESTAMP"        => DBI::Type::Varchar,
              nil                => DBI::DBD::MSSQL::Type::Null,
              System::DBNull     => DBI::DBD::MSSQL::Type::Null
      }



      def self.driver_name
        "mssql"
      end

      DBI::TypeUtil.register_conversion(driver_name) do |obj|


        newobj =
                case obj
                  when ::DBI::Timestamp, ::Numeric, ::DBI::Binary
                    obj.to_s
                  when ::DateTime
                    "#{obj.strftime("%Y-%m-%d %H:%M:%S")}"
                  when ::Time
                    "#{obj.strftime("%H:%M:%S")}"
                  when ::Date
                    "#{obj.strftime("%Y-%m-%d")}"
                  when ::NilClass
                    System::DBNull.value
                  when ::String, System::String
                    obj
                  when ::TrueClass
                    "1"
                  when ::FalseClass
                    "0"
                  when ::BigDecimal
                    obj.to_s("F")
                  when ::Numeric
                    obj.to_s
                  else
                    obj
                end

        if newobj.object_id == obj.object_id and not (obj.is_a?(::String) || obj.is_a?(System::String))
          [newobj, true]
        else
          [newobj, false]
        end
      end


      CLR_TYPES = {
              :TINYINT => "byte",
              :SMALLINT => "short",
              :BIGINT => "long",
              :INT => "int",
              :FLOAT => "double",
              :REAL => "float",
              :SMALLMONEY => "decimal",
              :MONEY => "decimal",
              :NUMERIC => "decimal",
              :DECIMAL => "decimal",
              :BIT => "bool",
              :UNIQUEIDENTIFIER => "Guid",
              :VARCHAR => "string",
              :NVARCHAR => "string",
              :TEXT => "string",
              :NTEXT => "string",
              :CHAR => "char",
              :NCHAR => "char",
              :VARBINARY => "byte[]",
              :IMAGE => "byte[]",
              :DATETIME => "DateTime"
      }

      SQL_TYPE_NAMES = {
              :BIT => "BIT",
              :TINYINT => "TINYINT",
              :SMALLINT => "SMALLINT",
              :INTEGER => "INTEGER",
              :INT => "INTEGER",
              :BIGINT => "BIGINT",
              :FLOAT => "FLOAT",
              :REAL => "REAL",
              :DOUBLE => "DOUBLE",
              :NUMERIC => "NUMERIC",
              :DECIMAL => "DECIMAL",
              :CHAR => "CHAR",
              :NCHAR => "CHAR",
              :VARCHAR => "VARCHAR",
              :NVARCHAR => "VARCHAR",
              :LONGVARCHAR => "LONG VARCHAR",
              :TEXT => "LONG VARCHAR",
              :NTEXT => "LONG VARCHAR",
              :DATE => "DATE",
              :DATETIME => "DATETIME",
              :TIME => "TIME",
              :TIMESTAMP => "TIMESTAMP",
              :BINARY => "BINARY",
              :VARBINARY => "VARBINARY",
              :LONGVARBINARY => "LONG VARBINARY",
              :IMAGE => "BLOB",
              :BLOB => "BLOB",
              :CLOB => "CLOB",
              :OTHER => "",
              :BOOLEAN => "BOOLEAN",
              :UNIQUEIDENTIFIER => "VARCHAR"
      }
    end
  end
end

require File.dirname(__FILE__) + "/mssql/driver"
require File.dirname(__FILE__) + "/mssql/database"
require File.dirname(__FILE__) + "/mssql/statement"
