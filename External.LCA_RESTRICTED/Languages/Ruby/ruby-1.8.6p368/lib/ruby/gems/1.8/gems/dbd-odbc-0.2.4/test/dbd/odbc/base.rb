require 'test/unit'
require 'fileutils'

DBDConfig.set_testbase(:odbc, Class.new(Test::Unit::TestCase) do
        
        def dbtype
            "odbc"
        end

        def test_base
            assert_equal(@dbh.driver_name, "odbc")
            assert_kind_of(DBI::DBD::ODBC::Database, @dbh.instance_variable_get(:@handle))
        end
        
        def set_base_dbh
            config = DBDConfig.get_config['odbc']
            @dbh = DBI.connect("dbi:ODBC:#{config['dbname']}", config['username'], config['password'])
        end

        def setup
            set_base_dbh
            DBDConfig.inject_sql(@dbh, dbtype, "dbd/odbc/up.sql")
        end

        def teardown
            @sth.finish if @sth && !@sth.finished?
            DBDConfig.inject_sql(@dbh, dbtype, "dbd/odbc/down.sql")
            @dbh.disconnect
        end
    end
)
