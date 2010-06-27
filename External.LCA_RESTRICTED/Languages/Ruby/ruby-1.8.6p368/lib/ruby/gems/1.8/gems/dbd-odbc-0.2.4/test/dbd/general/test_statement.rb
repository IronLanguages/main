@class = Class.new(DBDConfig.testbase(DBDConfig.current_dbtype)) do

    def test_execute
        assert_nothing_raised do
            @dbh.execute("select * from names order by age") do |sth|
                assert_equal([["Joe", 19], ["Bob", 21], ["Jim", 30]], sth.fetch_all)
            end
        end
    end

    def test_quoting # FIXME breaks sqlite-ruby to a segfault - research
        @sth = nil

        assert_nothing_raised do
            if dbtype == "postgresql"
                @sth = @dbh.prepare('select E\'\\\\\'')
            else
                @sth = @dbh.prepare('select \'\\\\\'')
            end
            @sth.execute
            row = @sth.fetch
            assert_equal ['\\'], row
            @sth.finish
        end
    end

    def test_duplicate_columns
        assert_nothing_raised do
            @sth = @dbh.prepare("select name, name from names where name = ?")
            @sth.execute("Bob")
            assert_equal [["Bob", "Bob"]], @sth.fetch_all
            @sth.finish
        end
    end

    def test_columninfo
        @sth = nil
        
        assert_nothing_raised do
            @sth = @dbh.prepare("select * from precision_test")
            @sth.execute

            cols = @sth.column_info

            assert(cols)
            assert_kind_of(Array, cols)
            assert_equal(2, cols.length)
            
            # the first column should always be "text_field" and have the following
            # properties:
            assert_equal("text_field", cols[0]["name"])
            assert_equal(20, cols[0]["precision"])
            # scale can be either nil or 0 for character types.
            case cols[0]["scale"]
            when nil
                assert_equal(nil, cols[0]["scale"])
            when 0
                assert_equal(0, cols[0]["scale"])
            else
                flunk "scale can be either 0 or nil for character types"
            end
                
            assert_equal(
                DBI::Type::Varchar, 
                DBI::TypeUtil.type_name_to_module(cols[0]["type_name"])
            )

            # the second column should always be "integer_field" and have the following
            # properties:
            assert_equal("integer_field", cols[1]["name"])
            assert_equal(1, cols[1]["scale"])
            assert_equal(2, cols[1]["precision"])
            assert_equal(
                DBI::Type::Decimal, 
                DBI::TypeUtil.type_name_to_module(cols[1]["type_name"])
            )

            cols.each { |col| assert_kind_of(DBI::ColumnInfo, col) }
            @sth.finish
        end
    end

    def test_duplicate_columns
        assert_nothing_raised do
            @sth = @dbh.prepare("select name, name from names where name = ?")
            @sth.execute("Bob")
            assert_equal [["Bob", "Bob"]], @sth.fetch_all
            @sth.finish
        end
    end

    def test_rows
        @sth = nil

        assert_nothing_raised do
            @sth = @dbh.prepare("insert into names (name, age) values (?, ?)")
            @sth.execute("Bill", 22);
        end

        assert 1, @sth.rows

        @sth.finish
        @sth = nil

        assert_nothing_raised do
            @sth = @dbh.prepare("delete from names where name = ?")
            @sth.execute("Bill");
        end

        assert 1, @sth.rows

        @sth.finish

        assert_nothing_raised do
            @sth = @dbh.prepare("select * from names")
            @sth.execute
        end

        assert_equal 0, @sth.rows
        assert @sth.fetchable?
        assert @sth.any?
        assert @sth.rows.zero?
        @sth.finish
    end

    def test_prepare_execute
        assert_nothing_raised do 
            @sth = @dbh.prepare("select * from names")
            @sth.execute
            @sth.finish
        end

        assert_nothing_raised do
            @sth = @dbh.prepare("select * from names where name = ?")
            @sth.execute("Bob")
            @sth.finish
        end

        assert_nothing_raised do
            @sth = @dbh.prepare("insert into names (name, age) values (?, ?)")
            @sth.execute("Bill", 22);
            @sth.finish
        end
    end

    def test_prepare_execute_with_transactions
        @dbh["AutoCommit"] = false 
        config = DBDConfig.get_config['sqlite3']

        # rollback 1 (the right way)
        @sth = nil
        @sth2 = nil

        assert_nothing_raised do
            @sth = @dbh.prepare("insert into names (name, age) values (?, ?)")
            @sth.execute("Billy", 23)
            @sth2 = @dbh.prepare("select * from names where name = ?")
            @sth2.execute("Billy")
        end
        assert_equal ["Billy", 23 ], @sth2.fetch
        @sth2.finish
        @sth.finish
        assert_nothing_raised { @dbh.rollback }

        @sth = @dbh.prepare("select * from names where name = ?")
        @sth.execute("Billy")
        assert_nil @sth.fetch
        @sth.finish
       
        # rollback 2 (without closing statements first)

        @sth = nil
        @sth2 = nil

        assert_nothing_raised do
            @sth = @dbh.prepare("insert into names (name, age) values (?, ?)")
            @sth.execute("Billy", 23)
            @sth2 = @dbh.prepare("select * from names where name = ?")
            @sth2.execute("Billy")
        end

        assert_equal ["Billy", 23], @sth2.fetch

        # FIXME some throw here, some don't. we should probably normalize this
        @dbh.rollback rescue true
        
        @sth2.finish
        @sth.finish
        assert_nothing_raised { @dbh.rollback }
        
        @sth = @dbh.prepare("select * from names where name = ?")
        @sth.execute("Billy")
        assert_nil @sth.fetch
        @sth.finish

        # commit

        @sth = nil
        @sth2 = nil

        assert_nothing_raised do
            @sth = @dbh.prepare("insert into names (name, age) values (?, ?)")
            @sth.execute("Billy", 23)
            @sth2 = @dbh.prepare("select * from names where name = ?")
            @sth2.execute("Billy")
        end
        assert_equal ["Billy", 23 ], @sth2.fetch
        @sth2.finish
        @sth.finish
        assert_nothing_raised { @dbh.commit }

        @sth = @dbh.prepare("select * from names where name = ?")
        @sth.execute("Billy")
        assert_equal ["Billy", 23 ], @sth.fetch
        @sth.finish
    end

    def test_fetch
        @sth = nil
        assert_nothing_raised do 
            @sth = @dbh.prepare("select * from names order by age")
            @sth.execute
        end

        # this tests that we're getting the rows in the right order,
        # and that the types are being converted. 
        assert_equal ["Joe", 19], @sth.fetch
        assert_equal ["Bob", 21], @sth.fetch
        assert_equal ["Jim", 30], @sth.fetch
        assert_nil @sth.fetch

        @sth.finish
    end

    def test_transaction_block
        @dbh["AutoCommit"] = false
        # this transaction should not fail because it called return early
        @dbh.transaction do |dbh|
            dbh.do('INSERT INTO names (name, age) VALUES (?, ?)', "Cooter", 69)
            return 42
        end 
        @sth = @dbh.prepare("select * from names where name = ?")
        @sth.execute("Cooter")
        row = @sth.fetch
        assert row
        assert_equal ["Cooter", 69], row
        @sth.finish
    end
end
