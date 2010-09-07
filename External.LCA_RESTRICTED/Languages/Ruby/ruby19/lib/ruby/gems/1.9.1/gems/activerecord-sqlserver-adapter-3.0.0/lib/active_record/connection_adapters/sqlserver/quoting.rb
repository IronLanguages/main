module ActiveRecord
  module ConnectionAdapters
    module Sqlserver
      module Quoting
        
        def quote(value, column = nil)
          case value
          when String, ActiveSupport::Multibyte::Chars
            if column && column.type == :binary
              column.class.string_to_binary(value)
            elsif quote_value_as_utf8?(value) || column && column.respond_to?(:is_utf8?) && column.is_utf8?
              quoted_utf8_value(value)
            else
              super
            end
          else
            super
          end
        end

        def quote_string(string)
          string.to_s.gsub(/\'/, "''")
        end

        def quote_column_name(column_name)
          column_name.to_s.split('.').map{ |name| name =~ /^\[.*\]$/ ? name : "[#{name}]" }.join('.')
        end

        def quote_table_name(table_name)
          return table_name if table_name =~ /^\[.*\]$/
          quote_column_name(table_name)
        end

        def quoted_true
          '1'
        end

        def quoted_false
          '0'
        end

        def quoted_date(value)
          if value.acts_like?(:time) && value.respond_to?(:usec)
            "#{super}.#{sprintf("%03d",value.usec/1000)}"
          else
            super
          end
        end

        def quoted_utf8_value(value)
          "N'#{quote_string(value)}'"
        end
        
        def quote_value_as_utf8?(value)
          value.is_utf8? || enable_default_unicode_types
        end
        
      end
    end
  end
end
