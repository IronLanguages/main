module DataObjects

  module Quoting
    # Escape a string of SQL with a set of arguments.
    # The first argument is assumed to be the SQL to escape,
    # the remaining arguments (if any) are assumed to be
    # values to escape and interpolate.
    #
    # ==== Examples
    #   escape_sql("SELECT * FROM zoos")
    #   # => "SELECT * FROM zoos"
    #
    #   escape_sql("SELECT * FROM zoos WHERE name = ?", "Dallas")
    #   # => "SELECT * FROM zoos WHERE name = `Dallas`"
    #
    #   escape_sql("SELECT * FROM zoos WHERE name = ? AND acreage > ?", "Dallas", 40)
    #   # => "SELECT * FROM zoos WHERE name = `Dallas` AND acreage > 40"
    #
    # ==== Warning
    # This method is meant mostly for adapters that don't support
    # bind-parameters.
    def escape_sql(args)
      sql = @text.dup

      unless args.empty?
        sql.gsub!(/\?/) do |x|
          quote_value(args.shift)
        end
      end

      sql
    end

    def quote_value(value)
      return 'NULL' if value.nil?

      case value
        when Numeric then quote_numeric(value)
        when String then quote_string(value)
        when Class then quote_class(value)
        when Time then quote_time(value)
        when DateTime then quote_datetime(value)
        when Date then quote_date(value)
        when TrueClass, FalseClass then quote_boolean(value)
        when Array then quote_array(value)
        when Range then quote_range(value)
        when Symbol then quote_symbol(value)
        when Regexp then quote_regexp(value)
        else
          if value.respond_to?(:to_sql)
            value.to_sql
          else
            raise "Don't know how to quote #{value.class} objects (#{value.inspect})"
          end
      end
    end

    def quote_symbol(value)
      quote_string(value.to_s)
    end

    def quote_numeric(value)
      value.to_s
    end

    def quote_string(value)
      "'#{value.gsub("'", "''")}'"
    end

    def quote_class(value)
      quote_string(value.name)
    end

    def quote_time(value)
      "'#{value.strftime('%Y-%m-%d %H:%M:%S')}" + (value.usec > 0 ? ".#{value.usec.to_s.rjust(6, '0')}'" : "'")
    end

    def quote_datetime(value)
      "'#{value.dup}'"
    end

    def quote_date(value)
      "'#{value.strftime("%Y-%m-%d")}'"
    end

    def quote_boolean(value)
      value.to_s.upcase
    end

    def quote_array(value)
      "(#{value.map { |entry| quote_value(entry) }.join(', ')})"
    end

    def quote_range(value)
      "#{quote_value(value.first)} AND #{quote_value(value.last)}"
    end

    def quote_regexp(value)
      quote_string(value.source)
    end
  end

end
