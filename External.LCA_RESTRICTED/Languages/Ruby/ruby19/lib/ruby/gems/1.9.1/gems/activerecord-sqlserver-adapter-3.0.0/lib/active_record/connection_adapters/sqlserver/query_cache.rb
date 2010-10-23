module ActiveRecord
  module ConnectionAdapters
    module Sqlserver
      module QueryCache
        
        def select_one(*args)
          if @query_cache_enabled
            cache_sql(args.first) { super }
          else
            super
          end
        end
        
      end
    end
  end
end
