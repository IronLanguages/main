@class = Class.new(DBDConfig.testbase(DBDConfig.current_dbtype)) do
    def test_ping
        assert @dbh.ping
        # XXX if it isn't obvious, this should be tested better. Not sure what
        # good behavior is yet.
    end

    def test_columns
        assert_nothing_raised do
            cols = @dbh.columns("precision_test")

            assert(cols)
            assert_kind_of(Array, cols)
            assert_equal(2, cols.length)

            # the first column should always be "text_field" and have the following
            # properties:
            assert_equal("text_field", cols[0]["name"])
            assert(!cols[0]["nullable"])

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
            assert(cols[1]["nullable"])
            assert_equal(1, cols[1]["scale"])
            assert_equal(2, cols[1]["precision"])
            assert_equal(
                DBI::Type::Decimal, 
                DBI::TypeUtil.type_name_to_module(cols[1]["type_name"])
            )

            # finally, we ensure that every column in the array is a ColumnInfo
            # object
            cols.each { |col| assert_kind_of(DBI::ColumnInfo, col) }
        end
    end

    def test_prepare
        @sth = @dbh.prepare('select * from names')

        assert @sth
        assert_kind_of DBI::StatementHandle, @sth

        @sth.finish
    end

    def test_do
        assert_equal 1, @dbh.do("insert into names (name, age) values (?, ?)", "Billy", 21)
        @sth = @dbh.prepare("select * from names where name = ?")
        @sth.execute("Billy")
        assert_equal ["Billy", 21], @sth.fetch
        @sth.finish
    end

    def test_tables
        tables = @dbh.tables.sort

        # since this is a general test, let's prune the system tables
        # FIXME not so sure if this should be a general test anymore.
        if dbtype == "odbc"
            tables -= [
                "administrable_role_authorizations",
                "applicable_roles",
                "attributes",
                "check_constraint_routine_usage",
                "check_constraints",
                "column_domain_usage",
                "column_privileges",
                "column_udt_usage",
                "columns",
                "constraint_column_usage",
                "constraint_table_usage",
                "data_type_privileges",
                "domain_constraints",
                "domain_udt_usage",
                "domains",
                "element_types",
                "enabled_roles",
                "information_schema_catalog_name",
                "key_column_usage",
                "parameters",
                "referential_constraints",
                "role_column_grants",
                "role_routine_grants",
                "role_table_grants",
                "role_usage_grants",
                "routine_privileges",
                "routines",
                "schemata",
                "sequences",
                "sql_features",
                "sql_implementation_info",
                "sql_languages",
                "sql_packages",
                "sql_parts",
                "sql_sizing",
                "sql_sizing_profiles",
                "table_constraints",
                "table_privileges",
                "tables",
                "triggered_update_columns",
                "triggers",
                "usage_privileges",
                "view_column_usage",
                "view_routine_usage",
                "view_table_usage",
                "views"
            ]
        end
        
        case dbtype 
        when "postgresql"
            tables.reject! { |x| x =~ /^pg_/ }
            assert_equal %w(array_test bit_test blob_test boolean_test bytea_test db_specific_types_test field_types_test names precision_test time_test timestamp_test view_names), tables
        else
            assert_equal %w(bit_test blob_test boolean_test db_specific_types_test field_types_test names precision_test time_test timestamp_test view_names), tables
        end
    end

    def test_attrs
        # test defaults
        assert @dbh["AutoCommit"] # should be true

        # test setting
        assert !(@dbh["AutoCommit"] = false)
        assert !@dbh["AutoCommit"]

        # test committing an outstanding transaction
        
        @sth = @dbh.prepare("insert into names (name, age) values (?, ?)")
        @sth.execute("Billy", 22)
        @sth.finish

        assert @dbh["AutoCommit"] = true # should commit at this point
        
        @sth = @dbh.prepare("select * from names where name = ?")
        @sth.execute("Billy")
        assert_equal [ "Billy", 22 ], @sth.fetch
        @sth.finish
    end
end
