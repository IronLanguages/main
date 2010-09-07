module ActiveRecord
  module ConnectionAdapters
    module Sqlserver
      module CoreExt
        module ODBC

          module TimeStamp
            
            def to_sqlserver_string
              date, time, nanoseconds = to_s.split(' ')
              "#{date} #{time}.#{sprintf("%03d",nanoseconds.to_i/1000000)}"
            end
            
          end

          module Statement
            
            def finished?
              begin
                connected?
                false
              rescue *Database.parent_modules_error_exceptions
                true
              end
            end
            
          end

          module Database
            
            def self.parent_modules
              @parent_module ||= ['ODBC','ODBC_UTF8','ODBC_NONE'].map{ |odbc_ns| odbc_ns.constantize rescue nil }.compact
            end
            
            def self.parent_modules_error_exceptions
              @parent_modules_error_exceptions ||= parent_modules.map { |odbc_ns| "::#{odbc_ns}::Error".constantize }
            end
            
            def run_block(*args)
              yield sth = run(*args)
              sth.drop
            end
            
          end

        end
      end
    end
  end
end

['ODBC','ODBC_UTF8','ODBC_NONE'].map{ |odbc_ns| odbc_ns.constantize rescue nil }.compact.each do |ns|
  ns::TimeStamp.send :include, ActiveRecord::ConnectionAdapters::Sqlserver::CoreExt::ODBC::TimeStamp
  ns::Statement.send :include, ActiveRecord::ConnectionAdapters::Sqlserver::CoreExt::ODBC::Statement
  ns::Database.send :include, ActiveRecord::ConnectionAdapters::Sqlserver::CoreExt::ODBC::Database
end

