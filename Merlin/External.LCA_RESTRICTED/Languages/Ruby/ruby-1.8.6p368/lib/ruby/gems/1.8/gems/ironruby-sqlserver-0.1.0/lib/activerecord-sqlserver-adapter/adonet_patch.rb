# Monkey patch SQLServerAdapter. This should be moved into SQLServerAdapter itself in activerecord-sqlserver-adapter\lib\active_record\connection_adapters\sqlserver_adapter.rb
module ActiveRecord
  class Base
    def self.sqlserver_connection(config) #:nodoc:
      config.symbolize_keys!
      mode        = config[:mode] ? config[:mode].to_s.upcase : 'ADO'
      username    = config[:username] ? config[:username].to_s : 'sa'
      password    = config[:password] ? config[:password].to_s : ''
      database    = config[:database]
      host        = config[:host] ? config[:host].to_s : 'localhost'
      integrated_security = config[:integrated_security]

      if mode == "ODBC"
        raise ArgumentError, "Missing DSN. Argument ':dsn' must be set in order for this adapter to work." unless config.has_key?(:dsn)
        dsn       = config[:dsn]
        driver_url = "DBI:ODBC:#{dsn}"
        connection_options = [driver_url, username, password]
      elsif mode == "ADO"
        raise ArgumentError, "Missing Database. Argument ':database' must be set in order for this adapter to work." unless config.has_key?(:database)
        if integrated_security
          connection_options = ["DBI:ADO:Provider=SQLOLEDB;Data Source=#{host};Initial Catalog=#{database};Integrated Security=SSPI"]
        else
          driver_url = "DBI:ADO:Provider=SQLOLEDB;Data Source=#{host};Initial Catalog=#{database};User ID=#{username};Password=#{password};"
          connection_options = [driver_url, username, password]
        end
      elsif mode == "ADONET"
        raise ArgumentError, "Missing Database. Argument ':database' must be set in order for this adapter to work." unless config.has_key?(:database)
        if integrated_security
          connection_options = ["DBI:MSSQL:server=#{host};initial catalog=#{database};integrated security=true"]
        else
          connection_options = ["DBI:MSSQL:server=#{host};initial catalog=#{database};user id=#{username};password=#{password}"]
        end
      else
        raise "Unknown mode #{mode}"
      end
      ConnectionAdapters::SQLServerAdapter.new(logger, connection_options)
    end
  end
end
