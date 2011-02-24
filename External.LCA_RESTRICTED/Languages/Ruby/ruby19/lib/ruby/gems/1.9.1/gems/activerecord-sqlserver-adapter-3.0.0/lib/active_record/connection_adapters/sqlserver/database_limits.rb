module ActiveRecord
  module ConnectionAdapters
    module Sqlserver
      module DatabaseLimits
        
        def table_alias_length
          128
        end

        def column_name_length
          128
        end

        def table_name_length
          128
        end

        def index_name_length
          128
        end

        def columns_per_table
          1024
        end

        def indexes_per_table
          999
        end

        def columns_per_multicolumn_index
          16
        end

        def in_clause_length
          65536
        end

        def sql_query_length
          65536 * 4096
        end

        def joins_per_query
          256
        end
        
      end
    end
  end
end
