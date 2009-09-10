#
# See DBI::BaseDriver
#
class DBI::DBD::ODBC::Driver < DBI::BaseDriver
    def initialize
        super("0.4.0")
    end

    def data_sources
        ::ODBC.datasources.collect {|dsn| "dbi:ODBC:" + dsn.name }
    rescue DBI::DBD::ODBC::ODBCErr => err
        raise DBI::DatabaseError.new(err.message)
    end

    def connect(dbname, user, auth, attr)
        driver_attrs = dbname.split(';')

        if driver_attrs.size > 1
            # DNS-less connection
            drv = ::ODBC::Driver.new
            drv.name = 'Driver1'
            driver_attrs.each do |param|
                pv = param.split('=')
                next if pv.size < 2
                drv.attrs[pv[0]] = pv[1]
            end
            db = ::ODBC::Database.new
            handle = db.drvconnect(drv)
        else
            # DNS given
            handle = ::ODBC.connect(dbname, user, auth)
        end

        return DBI::DBD::ODBC::Database.new(handle, attr)
    rescue DBI::DBD::ODBC::ODBCErr => err
        raise DBI::DatabaseError.new(err.message)
    end
end
