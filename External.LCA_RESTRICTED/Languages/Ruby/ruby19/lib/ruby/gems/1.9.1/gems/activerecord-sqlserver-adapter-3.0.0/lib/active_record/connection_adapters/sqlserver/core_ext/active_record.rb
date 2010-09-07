require 'active_record/version'

module ActiveRecord
  module ConnectionAdapters
    module Sqlserver
      module CoreExt
        module ActiveRecord

          def self.included(klass)
            klass.extend ClassMethods
            class << klass
              alias_method_chain :reset_column_information, :sqlserver_cache_support
            end
          end

          module ClassMethods

            def execute_procedure(proc_name, *variables)
              if connection.respond_to?(:execute_procedure)
                connection.execute_procedure(proc_name,*variables)
              else
                []
              end
            end

            def coerce_sqlserver_date(*attributes)
              write_inheritable_attribute :coerced_sqlserver_date_columns, Set.new(attributes.map(&:to_s))
            end

            def coerce_sqlserver_time(*attributes)
              write_inheritable_attribute :coerced_sqlserver_time_columns, Set.new(attributes.map(&:to_s))
            end

            def coerced_sqlserver_date_columns
              read_inheritable_attribute(:coerced_sqlserver_date_columns) || []
            end

            def coerced_sqlserver_time_columns
              read_inheritable_attribute(:coerced_sqlserver_time_columns) || []
            end

            def reset_column_information_with_sqlserver_cache_support
              connection.send(:initialize_sqlserver_caches) if connection.respond_to?(:sqlserver?)
              reset_column_information_without_sqlserver_cache_support
            end

          end

        end
      end
    end
  end
end


ActiveRecord::Base.send :include, ActiveRecord::ConnectionAdapters::Sqlserver::CoreExt::ActiveRecord

