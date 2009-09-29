#
# See DBI::BaseStatement.
#
class DBI::DBD::ODBC::Statement < DBI::BaseStatement
    def initialize(handle, statement)
        @statement = statement
        @handle = handle
        @params = []
        @arr = []
    end

    #
    # See DBI::BaseStatement#bind_param. This method will also raise
    # DBI::InterfaceError if +param+ is not a Fixnum, to prevent incorrect
    # binding.
    #
    def bind_param(param, value, attribs)
        raise DBI::InterfaceError, "only ? parameters supported" unless param.is_a? Fixnum
        @params[param-1] = value
    end

    def execute
        @handle.execute(*@params)
    rescue DBI::DBD::ODBC::ODBCErr => err
        raise DBI::DatabaseError.new(err.message)
    end

    def finish
        @handle.drop
    rescue DBI::DBD::ODBC::ODBCErr => err
        raise DBI::DatabaseError.new(err.message)
    end

    def cancel
        @handle.cancel
    rescue DBI::DBD::ODBC::ODBCErr => err
        raise DBI::DatabaseError.new(err.message)
    end

    def fetch
        convert_row(@handle.fetch)
    rescue DBI::DBD::ODBC::ODBCErr => err
        raise DBI::DatabaseError.new(err.message)
    end

    #
    # See DBI::BaseStatement#fetch_scroll.
    #
    # ODBC has a native version of this method and the constnats in the ODBC
    # driver themselves are supported. If you'd prefer to use DBI constants
    # (recommended), you can use these which map to the ODBC functionality:
    #
    # * DBI::SQL_FETCH_FIRST
    # * DBI::SQL_FETCH_LAST
    # * DBI::SQL_FETCH_NEXT
    # * DBI::SQL_FETCH_PRIOR
    # * DBI::SQL_FETCH_ABSOLUTE
    # * DBI::SQL_FETCH_RELATIVE
    #
    def fetch_scroll(direction, offset)
        direction = case direction
                    when DBI::SQL_FETCH_FIRST    then ::ODBC::SQL_FETCH_FIRST
                    when DBI::SQL_FETCH_LAST     then ::ODBC::SQL_FETCH_LAST
                    when DBI::SQL_FETCH_NEXT     then ::ODBC::SQL_FETCH_NEXT
                    when DBI::SQL_FETCH_PRIOR    then ::ODBC::SQL_FETCH_PRIOR
                    when DBI::SQL_FETCH_ABSOLUTE then ::ODBC::SQL_FETCH_ABSOLUTE
                    when DBI::SQL_FETCH_RELATIVE then ::ODBC::SQL_FETCH_RELATIVE
                    else
                        direction
                    end

        convert_row(@handle.fetch_scroll(direction, offset))
    rescue DBI::DBD::ODBC::ODBCErr => err
        raise DBI::DatabaseError.new(err.message)
    end

    #
    # See DBI::BaseStatement#column_info. These additional attributes are also
    # supported:
    #
    # * table: the table this column came from, if available.
    # * nullable: boolean, true if NULL is accepted as a value in this column.
    # * searchable: FIXME DOCUMENT
    # * length: FIXME DOCUMENT
    # * unsigned: For numeric columns, whether or not the result value is signed.
    #
    def column_info
        info = []
        @handle.columns(true).each do |col|
            info << {
                'name'       => col.name, 
                'table'      => col.table,
                'nullable'   => col.nullable,
                'searchable' => col.searchable,
                'precision'  => col.precision,
                'scale'      => col.scale,
                'sql_type'   => col.type,
                'type_name'  => DBI::SQL_TYPE_NAMES[col.type],
                'length'     => col.length,
                'unsigned'   => col.unsigned
            }
        end
        info
    rescue DBI::DBD::ODBC::ODBCErr => err
        raise DBI::DatabaseError.new(err.message)
    end

    #
    # See DBI::BaseStatement#rows.
    #
    # For queries which DBI::SQL.query? returns true, will explicitly return 0.
    # Otherwise, it will return the row processed count.
    #
    def rows
        return 0 if DBI::SQL.query?(@statement)
        return @handle.nrows
    rescue DBI::DBD::ODBC::ODBCErr => err
        raise DBI::DatabaseError.new(err.message)
    end

    private # -----------------------------------

    # convert the ODBC datatypes to DBI datatypes
    def convert_row(row)
        return nil if row.nil?
        row.collect do |col|
            col = col.to_s unless col.nil?
            col
        end
    end 
end
