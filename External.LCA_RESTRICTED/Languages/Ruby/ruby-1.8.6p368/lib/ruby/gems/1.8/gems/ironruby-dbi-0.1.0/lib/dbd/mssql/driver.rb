module DBI
  module DBD
    module MSSQL

      class Driver < DBI::BaseDriver
                
        PROVIDER_KEY = :mssql

        def initialize
          super(USED_DBD_VERSION, PROVIDER_KEY)
        end

        def create_database(connection, attr)
          Database.new(connection, attr)          
        end

        private

        def data_sources
          conn = factory.create_connection
          conn.open
          ret = conn.get_schema("Databases").rows.collect { |db| db.to_s unless %w(master tempdb model msdb).include? db.to_s  }
          conn.close
          ret
        end

      end

    end
  end
end
