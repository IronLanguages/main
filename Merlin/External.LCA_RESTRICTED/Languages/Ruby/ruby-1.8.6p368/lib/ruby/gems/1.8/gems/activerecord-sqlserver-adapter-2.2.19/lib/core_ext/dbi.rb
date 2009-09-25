
module SQLServerDBI
  
  module Timestamp
    # Deprecated DBI. See documentation for Type::SqlserverTimestamp which 
    # this method tries to mimic as ODBC is still going to convert SQL Server 
    # milliconds to whole number representation of nanoseconds.
    def to_sqlserver_string
      datetime, nanoseconds = to_s.split('.')
      "#{datetime}.#{sprintf("%03d",nanoseconds.to_i/1000000)}"
    end
  end
  
  module Type
    
    # Make sure we get DBI::Type::Timestamp returning a string NOT a time object
    # that represents what is in the DB before type casting while letting core 
    # ActiveRecord do the reset. It is assumed that DBI is using ODBC connections 
    # and that ODBC::Timestamp is taking the native milliseconds that SQL Server 
    # stores and returning them incorrect using ODBC::Timestamp#fraction which is 
    # nanoseconds. Below shows the incorrect ODBC::Timestamp represented by DBI 
    # and the conversion we expect to have in the DB before type casting.
    # 
    #   "1985-04-15 00:00:00 0"           # => "1985-04-15 00:00:00.000"
    #   "2008-11-08 10:24:36 30000000"    # => "2008-11-08 10:24:36.003"
    #   "2008-11-08 10:24:36 123000000"   # => "2008-11-08 10:24:36.123"
    class SqlserverTimestamp
      def self.parse(obj)
        return nil if ::DBI::Type::Null.parse(obj).nil?
        date, time, nanoseconds = obj.split(' ')
        "#{date} #{time}.#{sprintf("%03d",nanoseconds.to_i/1000000)}"
      end
    end
    
    # The adapter and rails will parse our floats, decimals, and money field correctly 
    # from a string. Do not let the DBI::Type classes create Float/BigDecimal objects 
    # for us. Trust rails .type_cast to do what it is built to do.
    class SqlserverForcedString
      def self.parse(obj)
        return nil if ::DBI::Type::Null.parse(obj).nil?
        obj.to_s
      end
    end
    
  end
  
  module TypeUtil
    
    def self.included(klass)
      klass.extend ClassMethods
      class << klass
        alias_method_chain :type_name_to_module, :sqlserver_types
      end
    end
    
    module ClassMethods
      
      # Capture all types classes that we need to handle directly for SQL Server 
      # and allow normal processing for those that we do not.
      def type_name_to_module_with_sqlserver_types(type_name)
        case type_name
        when /^timestamp$/i
          DBI::Type::SqlserverTimestamp
        when /^float|decimal|money$/i
          DBI::Type::SqlserverForcedString
        else
          type_name_to_module_without_sqlserver_types(type_name)
        end
      end
      
    end
    
  end
  
  
end


if defined?(DBI::TypeUtil)
  DBI::Type.send :include, SQLServerDBI::Type
  DBI::TypeUtil.send :include, SQLServerDBI::TypeUtil
elsif defined?(DBI::Timestamp) # DEPRECATED in DBI 0.4.0 and above. Remove when 0.2.2 and lower is no longer supported.
  DBI::Timestamp.send :include, SQLServerDBI::Timestamp
end

