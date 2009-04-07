#
# $Id: utils.rb,v 1.5 2006/01/29 06:14:19 djberg96 Exp $
#

module DBI
   class Date
      attr_accessor :year, :month, :day

      # DBI::Date.new(year = 0, month = 0, day = 0)
      # DBI::Date.new(Date)
      # DBI::Date.new(Time)
      #
      # Creates and returns a new DBI::Date object.  It's similar to the
      # standard Date class' constructor except that it also accepts a
      # Date or Time object.
      def initialize(year=0, month=0, day=0)
         case year
            when ::Date
               @year, @month, @day = year.year, year.month, year.day 
               @original_date = year
            when ::Time
               @year, @month, @day = year.year, year.month, year.day 
               @original_time = year
            else
               @year, @month, @day = year, month, day
         end
      end

      # Aliases
      alias :mon :month
      alias :mon= :month=
      alias :mday :day
      alias :mday= :day=

      # Returns a new Time object based on the year, month and day or, if a
      # Time object was passed to the constructor, returns that object.
      def to_time
         @original_time || ::Time.local(@year, @month, @day, 0, 0, 0)
      end

      # Returns a new Date object based on the year, month and day or, if a
      # Date object was passed to the constructor, returns that object.
      def to_date
         @original_date || ::Date.new(@year, @month, @day)
      end

      # Returns a DBI::Date object as a string in YYYY-MM-DD format.
      def to_s
         sprintf("%04d-%02d-%02d", @year, @month, @day)
      end
   end

   class Time
      attr_accessor :hour, :minute, :second

      # DBI::Time.new(hour = 0, minute = 0, second = 0)
      # DBI::Time.new(Time)
      #
      # Creates and returns a new DBI::Time object.  Unlike the Time object
      # in the standard library, accepts an hour, minute and second, or a
      # Time object.
      def initialize(hour=0, minute=0, second=0)
         case hour
            when ::Time
               @hour, @minute, @second = hour.hour, hour.min, hour.sec
               @original_time = hour
            else
               @hour, @minute, @second = hour, minute, second
         end
      end

      alias :min :minute
      alias :min= :minute=
      alias :sec :second
      alias :sec= :second=

      # Returns a new Time object based on the hour, minute and second, using
      # the current year, month and day.  If a Time object was passed to the
      # constructor, returns that object instead.
      def to_time
         if @original_time
            @original_time
         else
            t = ::Time.now
            ::Time.local(t.year, t.month, t.day, @hour, @minute, @second)
         end
      end

      # Returns a DBI::Time object as a string in HH:MM:SS format.
      def to_s
         sprintf("%02d:%02d:%02d", @hour, @minute, @second)
      end
   end

   class Timestamp
      attr_accessor :year, :month, :day
      attr_accessor :hour, :minute, :second
      attr_writer   :fraction
  
      # DBI::Timestamp(year=0,month=0,day=0,hour=0,min=0,sec=0,fraction=nil)
      # DBI::Timestamp(Time)
      # DBI::Timestamp(Date)
      #
      # Creates and returns a new DBI::Timestamp object.  This is similar to
      # a Time object in the standard library, but it also contains fractional
      # seconds, expressed in nanoseconds.  In addition, the constructor
      # accepts either a Date or Time object.
      def initialize(year=0, month=0, day=0, hour=0, min=0, sec=0, fraction=nil)
         case year
            when ::Time
               @year, @month, @day = year.year, year.month, year.day 
               @hour, @minute, @second, @fraction = year.hour, year.min, year.sec, nil 
               @original_time = year
            when ::Date
               @year, @month, @day = year.year, year.month, year.day 
               @hour, @minute, @second, @fraction = 0, 0, 0, nil 
               @original_date = year
            else
               @year, @month, @day = year, month, day
               @hour, @minute, @second, @fraction = hour, min, sec, fraction
         end
      end

      # Returns true if +timestamp+ has a year, month, day, hour, minute,
      # second and fraction equal to the comparing object.
      #
      # Returns false if the comparison fails for any reason.
      def ==(timestamp)
         @year == timestamp.year and @month == timestamp.month and
         @day == timestamp.day and @hour == timestamp.hour and
         @minute == timestamp.minute and @second == timestamp.second and
         (fraction() == timestamp.fraction)
      rescue
         false
      end

      # Returns fractional seconds, or 0 if not set.
      def fraction
         @fraction || 0
      end

      # Aliases
      alias :mon :month
      alias :mon= :month=
      alias :mday :day
      alias :mday= :day=
      alias :min :minute
      alias :min= :minute=
      alias :sec :second
      alias :sec= :second=

      # Returns a DBI::Timestamp object as a string in YYYY-MM-DD HH:MM:SS
      # format.  If a fraction is present, then it is appended in ".FF" format.
      def to_s
         string = sprintf("%04d-%02d-%02d %02d:%02d:%02d",
             @year, @month, @day, @hour, @minute, @second) 

         if @fraction
            fraction = ("%.9f" % (@fraction.to_i / 1e9)).
                        to_s[1..-1].gsub(/0{1,8}$/, "")
            string += fraction
         end

         string
      end

      # Returns a new Time object based on the year, month and day or, if a
      # Time object was passed to the constructor, returns that object.
      def to_time
         @original_time || ::Time.local(@year, @month, @day, @hour, @minute, @second)
      end

      # Returns a new Date object based on the year, month and day or, if a
      # Date object was passed to the constructor, returns that object.
      def to_date
         @original_date || ::Date.new(@year, @month, @day)
      end
   end

module Utils

  module ConvParam
    def conv_param(*params)
      params.collect do |p|
        case p
        when ::Date
          DBI::Date.new(p)
        when ::Time
          DBI::Timestamp.new(p)
        else
          p
        end
      end
    end
  end


  def Utils.measure
    start = ::Time.now
    yield
    ::Time.now - start
  end
  
  ##
  # parse a string of the form "database=xxx;key=val;..."
  # or database:host and return hash of key/value pairs
  #
  # improved by John Gorman <jgorman@webbysoft.com>
  def Utils.parse_params(str)
    params = str.split(";")
    hash = {}
    params.each do |param| 
      key, val = param.split("=") 
      hash[key] = val if key and val
    end 
    if hash.empty?
      database, host = str.split(":")
      hash['database'] = database if database
      hash['host']     = host if host   
    end
    hash 
  end


module XMLFormatter
  def XMLFormatter.row(dbrow, rowtag="row", output=STDOUT)
    #XMLFormatter.extended_row(dbrow, "row", [],  
    output << "<#{rowtag}>\n"
    dbrow.each_with_name do |val, name|
      output << "  <#{name}>" + textconv(val) + "</#{name}>\n" 
    end
    output << "</#{rowtag}>\n"
  end

  # nil in cols_as_tag, means "all columns expect those listed in cols_in_row_tag"
  # add_row_tag_attrs are additional attributes which are inserted into the row-tag
  def XMLFormatter.extended_row(dbrow, rowtag="row", cols_in_row_tag=[], cols_as_tag=nil, add_row_tag_attrs={}, output=STDOUT)
    if cols_as_tag.nil?
      cols_as_tag = dbrow.column_names - cols_in_row_tag
    end

    output << "<#{rowtag}"
    add_row_tag_attrs.each do |key, val|  
      # TODO: use textconv ? " substitution?
      output << %{ #{key}="#{textconv(val)}"}
    end
    cols_in_row_tag.each do |key|
       # TODO: use textconv ? " substitution?
      output << %{ #{key}="#{dbrow[key]}"}
    end
    output << ">\n"

    cols_as_tag.each do |key|
      output << "  <#{key}>" + textconv(dbrow[key]) + "</#{key}>\n" 
    end
    output << "</#{rowtag}>\n"
  end


 
  def XMLFormatter.table(rows, roottag = "rows", rowtag = "row", output=STDOUT)
    output << '<?xml version="1.0" encoding="UTF-8" ?>'
    output << "\n<#{roottag}>\n"
    rows.each do |row|
      row(row, rowtag, output)
    end
    output << "</#{roottag}>\n"
  end

  class << self
    private
    # from xmloracle.rb 
    def textconv(str)
      str = str.to_s.gsub('&', "&#38;")
      str = str.gsub('\'', "&#39;")
      str = str.gsub('"', "&#34;")
      str = str.gsub('<', "&#60;")
      str.gsub('>', "&#62;")
    end
  end # class self

end # module XMLFormatter


module TableFormatter

  # TODO: add a nr-column where the number of the column is shown
  def TableFormatter.ascii(header, rows, 
    header_orient=:left, rows_orient=:left, 
    indent=2, cellspace=1, pagebreak_after=nil,
    output=STDOUT)

    header_orient ||= :left
    rows_orient   ||= :left
    indent        ||= 2
    cellspace     ||= 1

    # pagebreak_after n-rows (without counting header or split-lines)
    # yield block with output as param after each pagebreak (not at the end)

    col_lengths = (0...(header.size)).collect do |colnr|
      [
      (0...rows.size).collect { |rownr|
        value = rows[rownr][colnr]
        (value.nil? ? "NULL" : value).to_s.size
      }.max,
      header[colnr].size
      ].max
    end

    indent = " " * indent

    split_line = indent + "+"
    col_lengths.each {|col| split_line << "-" * (col+cellspace*2) + "+" }

    cellspace = " " * cellspace

    output_row = proc {|row, orient|
      output << indent + "|"
      row.each_with_index {|c,i|
        output << cellspace
        str = (c.nil? ? "NULL" : c).to_s
        output << case orient
        when :left then   str.ljust(col_lengths[i])
        when :right then  str.rjust(col_lengths[i])
        when :center then str.center(col_lengths[i])
        end 
        output << cellspace
        output << "|"
      }
      output << "\n" 
    } 

    rownr = 0
 
    loop do 
      output << split_line + "\n"
      output_row.call(header, header_orient)
      output << split_line + "\n"
      if pagebreak_after.nil?
        rows.each {|ar| output_row.call(ar, rows_orient)}
        output << split_line + "\n"
        break
      end      

      rows[rownr,pagebreak_after].each {|ar| output_row.call(ar, rows_orient)}
      output << split_line + "\n"

      rownr += pagebreak_after

      break if rownr >= rows.size
      
      yield output if block_given?
    end
    
  end



end # module TableFormatter

end # module Utils
end # module DBI


