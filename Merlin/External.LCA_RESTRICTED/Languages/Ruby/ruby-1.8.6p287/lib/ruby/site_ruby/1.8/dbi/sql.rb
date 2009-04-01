#
# $Id: sql.rb,v 1.3 2006/03/27 20:25:02 francis Exp $
#
# parts extracted from Jim Weirichs DBD::Pg
#

module DBI
require "dbi/utils"
require "parsedate"
require "time"

module SQL

  ## Is the SQL statement a query?
  def SQL.query?(sql)
    sql =~ /^\s*select\b/i
  end


  ####################################################################
  # Mixin module useful for expanding SQL statements.
  #
  module BasicQuote

    # by Masatoshi SEKI
    class Coerce
      def as_int(str)
        return nil if str.nil?
        if str == "" then nil else str.to_i end 
      end 

      def as_float(str)
        return nil if str.nil?
        str.to_f
      end

      def as_str(str)
        str
      end

      def as_bool(str)
        if str == "t" or str == "1"
          true
        elsif str == "f" or str == "0"
          false
        else
          nil
        end
      end

      def as_time(str)
        return nil if str.nil? or str.empty?
        t = ParseDate.parsedate(str)
        DBI::Time.new(*t[3,3])
      end


      def as_timestamp(str)
        return nil if str.nil? or str.empty?
        ary = ParseDate.parsedate(str)
        begin 
          time = ::Time.gm(*(ary[0,6])) 
        rescue ArgumentError => ae 
          # don't fault stupid values that MySQL nevertheless stores 
          return nil 
        end
        if ary[6] =~ /^((\+|\-)\d+)(:\d+)?$/
          diff = $1.to_i * 3600  # seconds per hour 
          time -= diff
          time.localtime
        end 
        DBI::Timestamp.new(time)
      end


      def as_date(str)
        return nil if str.nil?
        ary = ParseDate.parsedate(str)
        DBI::Date.new(*ary[0,3])
      rescue
        nil
      end


      def coerce(sym, str)
        self.send(sym, str)
      end

    end # class Coerce

    
    ## Quote strings appropriately for SQL statements
    def quote(value)
      case value
      when String
	value = value.gsub(/'/, "''")	# ' (for ruby-mode)
	"'#{value}'"
      when NilClass
	"NULL"
      when TrueClass
        "'t'"
      when FalseClass
        "'f'"
      when Array
	value.collect { |v| quote(v) }.join(", ")
      when DBI::Date, DBI::Time, DBI::Timestamp, ::Date
        "'#{value.to_s}'"
      when ::Time
        "'#{value.rfc2822}'"
      else
	value.to_s
      end
    end
  end # module BasicQuote



  ####################################################################
  # Mixin module useful for binding arguments to an SQL string.
  #
  module BasicBind

    ## Bind the :sql string to an array of :args, quoting with :quoter.
    #
    def bind(quoter, sql, args)
      arg_index = 0
      result = ""
      tokens(sql).each { |part|
	case part
	when '?'
	  result << quoter.quote(args[arg_index])
	  arg_index += 1
	when '??'
	  result << "?"
	else
	  result << part
	end
      }
      if arg_index < args.size
        raise "Too many SQL parameters"
      elsif arg_index > args.size
        raise "Not enough SQL parameters"
      end
      result
    end

    ## Break the sql string into parts.
    #
    # This is NOT a full lexer for SQL.  It just breaks up the SQL
    # string enough so that question marks, double question marks and
    # quoted strings are separated.  This is used when binding
    # arguments to "?" in the SQL string.  
    #
    # C-style (/* */) and Ada-style (--) comments are handled.
    # Note: Nested C-style comments are NOT handled!
    #
    def tokens(sql)
      sql.scan(%r{
        (
            -- .*                               (?# matches "--" style comments to the end of line or string )
        |   -                                   (?# matches single "-" )
        |
            /[*] .*? [*]/                       (?# matches C-style comments )
        |   /                                   (?# matches single slash )    
        |
            ' ( [^'\\]  |  ''  |  \\. )* '      (?# match strings surrounded by apostophes )
        |
            " ( [^"\\]  |  ""  |  \\. )* "      (?# match strings surrounded by " )
        |
            \?\??                               (?# match one or two question marks )
        |
            [^-/'"?]+                           (?# match all characters except ' " ? - and / )
            
        )}x).collect {|t| t.first}
    end

  end # module BasicBind


  class PreparedStatement
    include BasicBind # for method tokens(sql)

    attr_accessor :unbound

    def initialize(quoter, sql)
      @quoter, @sql = quoter, sql
      prepare
    end

    def bind(args)
      if @arg_index < args.size
        raise "Too many SQL parameters"
      elsif @arg_index > args.size
        raise "Not enough SQL parameters"
      end

      @unbound.each do |res_pos, arg_pos|
        @result[res_pos] = @quoter.quote(args[arg_pos])
      end

      @result.join("")
    end

    private

    def prepare
      @result = [] 
      @unbound = {}
      pos = 0
      @arg_index = 0

      tokens(@sql).each { |part|
	case part
	when '?'
          @result[pos] = nil
          @unbound[pos] = @arg_index
          pos += 1
          @arg_index += 1
	when '??'
          if @result[pos-1] != nil
            @result[pos-1] << "?"
          else
            @result[pos] = "?"
            pos += 1
          end
	else
          if @result[pos-1] != nil
            @result[pos-1] << part
          else
            @result[pos] = part
            pos += 1
          end
	end
      }
    end
  end

end # module SQL
end # module DBI
