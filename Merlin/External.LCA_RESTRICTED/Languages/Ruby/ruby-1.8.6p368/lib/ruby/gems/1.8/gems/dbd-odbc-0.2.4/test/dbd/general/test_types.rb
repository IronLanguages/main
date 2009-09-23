@class = Class.new(DBDConfig.testbase(DBDConfig.current_dbtype)) do
    def skip_bit
        # FIXME this test fails because DBI's type system blows goats.
       @sth = nil

        assert_nothing_raised do
            @sth = @dbh.prepare("insert into bit_test (mybit) values (?)")
            @sth.bind_param(1, 0, DBI::SQL_TINYINT)
            @sth.execute
#             if dbtype == "postgresql"
#                 @sth.execute("0")
#             else
#                 @sth.execute(0)
#             end
            @sth.finish
        end

        assert_nothing_raised do
            @sth = @dbh.prepare("select * from bit_test")
            @sth.execute
            row = @sth.fetch
            @sth.finish

            assert_equal [0], row
        end
    end

    # FIXME
    # Ideally, this test should be split across the DBI tests and DBD, but for
    # now testing against the DBDs really doesn't cost us anything other than
    # debugging time if something breaks.
    def test_bind_coltype
        # ensure type conv didn't get turned off somewhere.
        assert(DBI.convert_types)
        assert(@dbh.convert_types)

        assert_nothing_raised do
            @sth = @dbh.prepare("select name, age from names order by age")
            assert(@sth.convert_types) # again
            @sth.execute
            @sth.bind_coltype(2, DBI::Type::Varchar)
            assert_equal(
                [
                    ["Joe", "19"], 
                    ["Bob", "21"],
                    ["Jim", "30"], 
                ], @sth.fetch_all
            )
            @sth.finish
        end

        # just to be sure..
        assert_nothing_raised do
            @sth = @dbh.prepare("select name, age from names order by age")
            @sth.execute
            @sth.bind_coltype(2, DBI::Type::Float)
            @sth.fetch_all.collect { |x| assert_kind_of(Float, x[1]) }
            @sth.finish
        end

        # now, let's check some failure cases
        @sth = @dbh.prepare("select name, age from names order by age")

        # can't bind_coltype before execute
        assert_raise(DBI::InterfaceError) { @sth.bind_coltype(1, DBI::Type::Float) }
        # can't index < 1
        assert_raise(DBI::InterfaceError) { @sth.bind_coltype(0, DBI::Type::Float) }
    end

    def test_noconv
        # XXX this test will fail the whole test suite miserably if it fails at any point.
        assert(DBI.convert_types)

        DBI.convert_types = false
        @sth.finish rescue nil
        @dbh.disconnect
        set_base_dbh

        assert(!@dbh.convert_types)

        assert_nothing_raised do
            @sth = @dbh.prepare("select * from names order by age")
            assert(!@sth.convert_types)
            @sth.execute
            assert_equal(
                [
                    ["Joe", "19"], 
                    ["Bob", "21"],
                    ["Jim", "30"], 
                ], @sth.fetch_all
            )
            @sth.finish
        end

        DBI.convert_types = true
        @sth.finish rescue nil
        @dbh.disconnect
        set_base_dbh

        assert(DBI.convert_types)
        assert(@dbh.convert_types)

        assert_nothing_raised do
            @sth = @dbh.prepare("select * from names order by age")
            assert(@sth.convert_types)
            @sth.execute
            assert_equal(
                [
                    ["Joe", 19], 
                    ["Bob", 21],
                    ["Jim", 30], 
                ], @sth.fetch_all
            )
            @sth.finish
        end

        @dbh.convert_types = false

        assert_nothing_raised do
            @sth = @dbh.prepare("select * from names order by age")
            assert(!@sth.convert_types)
            @sth.execute
            assert_equal(
                [
                    ["Joe", "19"], 
                    ["Bob", "21"],
                    ["Jim", "30"], 
                ], @sth.fetch_all
            )
            @sth.finish
        end

        @dbh.convert_types = true

        assert_nothing_raised do
            @sth = @dbh.prepare("select * from names order by age")
            assert(@sth.convert_types)
            @sth.convert_types = false
            @sth.execute
            assert_equal(
                [
                    ["Joe", "19"], 
                    ["Bob", "21"],
                    ["Jim", "30"], 
                ], @sth.fetch_all
            )
            @sth.finish
        end
    rescue Exception => e
        DBI.convert_types = true
        @sth.finish
        @dbh.disconnect
        set_base_dbh
        raise e
    end

    def test_null
        assert_nothing_raised do
            @sth = @dbh.prepare('insert into names (name, age) values (?, ?)')
            @sth.execute("'NULL'", 201)
            @sth.execute(nil, 202)
            @sth.execute("NULL", 203)
            @sth.finish
        end

        assert_nothing_raised do
            @sth = @dbh.prepare('select * from names where age > 200 order by age')
            @sth.execute
            assert_equal(["'NULL'", 201], @sth.fetch)
            assert_equal([nil, 202], @sth.fetch)
            assert_equal(["NULL", 203], @sth.fetch)
            @sth.finish
        end
    end

    def test_time
        @sth = nil
        t = nil
        assert_nothing_raised do
            @sth = @dbh.prepare("insert into time_test (mytime) values (?)")
            t = Time.now
            @sth.execute(t)
            @sth.finish
        end

        assert_nothing_raised do
            @sth = @dbh.prepare("select * from time_test")
            @sth.execute
            row = @sth.fetch
            assert_kind_of DateTime, row[0]
            assert_equal t.hour, row[0].hour
            assert_equal t.min, row[0].min
            assert_equal t.sec, row[0].sec
            @sth.finish
        end
    end

    def test_timestamp
        @sth = nil
         # We omit fractional second testing here -- timestamp precision
         # is a very slippery, dependent on driver and driver version.
        t = DBI::Timestamp.new(2008, 3, 8, 10, 39, 1)
        assert_nothing_raised do
            @sth = @dbh.prepare("insert into timestamp_test (mytimestamp) values (?)")
            @sth.execute(t)
            @sth.finish
        end

        assert_nothing_raised do
            @sth = @dbh.prepare("select * from timestamp_test")
            @sth.execute
            row = @sth.fetch
            assert_kind_of DateTime, row[0]
            assert_equal t.year, row[0].year
            assert_equal t.month, row[0].month
            assert_equal t.day, row[0].day
            assert_equal t.hour, row[0].hour
            assert_equal t.min, row[0].min
            assert_equal t.sec, row[0].sec
             # omit fractional tests
            @sth.finish
        end
    end

    def test_boolean_return
        @sth = nil

        unless dbtype == "odbc" # ODBC has no boolean type
            assert_nothing_raised do
                @sth = @dbh.prepare("insert into boolean_test (num, mybool) values (?, ?)")
                @sth.execute(1, true)
                @sth.execute(2, false)
                @sth.finish
            end

            assert_nothing_raised do
                @sth = @dbh.prepare("select * from boolean_test order by num")
                @sth.execute

                pairs = @sth.fetch_all

                assert_equal(
                    [
                                 [1, true],
                                 [2, false],
                    ], pairs
                )

                @sth.finish
            end
        end
    end
end
