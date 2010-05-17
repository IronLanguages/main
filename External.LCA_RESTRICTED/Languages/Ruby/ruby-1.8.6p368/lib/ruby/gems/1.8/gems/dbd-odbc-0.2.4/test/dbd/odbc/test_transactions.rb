class TestODBCTransaction < DBDConfig.testbase(:odbc)
    def test_rollback
        dbh = get_dbh
        dbh["AutoCommit"] = false
        @sth = dbh.prepare('insert into names (name, age) values (?, ?)')
        @sth.execute("Foo", 51)
        dbh.rollback
        assert_equal 1, @sth.rows
        @sth.finish


        @sth = dbh.prepare('select name, age from names where name=?')
        @sth.execute("Foo")
        assert !@sth.fetch
        @sth.finish
        dbh.disconnect 
    end

    def test_commit
        dbh = get_dbh
        dbh["AutoCommit"] = false
        @sth = dbh.prepare('insert into names (name, age) values (?, ?)')
        @sth.execute("Foo", 51)
        dbh.commit
        assert_equal 1, @sth.rows
        @sth.finish
        
        @sth = dbh.prepare('select name, age from names where name=?')
        @sth.execute("Foo")
        row = @sth.fetch
        assert row
        assert_equal "Foo", row[0]
        assert_equal 51, row[1]
        @sth.finish
        dbh.disconnect 
    end

    def test_select_transaction
        # per bug #10466
        dbh = get_dbh
        dbh["AutoCommit"] = false
        @sth = dbh.prepare('select * from test_insert(?, ?)');
        @sth.execute("Foo", 51)
        dbh.rollback
        @sth.finish

        @sth = dbh.prepare('select name, age from names where name=?')
        @sth.execute("Foo")
        assert !@sth.fetch
        @sth.finish
        dbh.disconnect 
    end

    def get_dbh
        config = DBDConfig.get_config['odbc']
        DBI.connect("dbi:ODBC:#{config['dbname']}", config['username'], config['password'])
    end
end
