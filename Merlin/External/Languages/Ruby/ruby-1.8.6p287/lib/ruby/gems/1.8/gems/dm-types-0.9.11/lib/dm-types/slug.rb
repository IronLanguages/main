require 'iconv'

module DataMapper
  module Types
    class Slug < DataMapper::Type
      primitive String
      size 65535

      def self.load(value, property)
        value
      end

      def self.dump(value, property)
        if value.nil?
          nil
        elsif value.is_a?(String)
          #Iconv.new('UTF-8//TRANSLIT//IGNORE', 'UTF-8').iconv(value.gsub(/[^\w\s\-\—]/,'').gsub(/[^\w]|[\_]/,' ').split.join('-').downcase).to_s
          escape(value)
        else
          raise ArgumentError.new("+value+ must be nil or a String")
        end
      end

      # Hugs and kisses to Rick Olsons permalink_fu for the escape method
      def self.escape(string)
        result = Iconv.iconv('ascii//translit//IGNORE', 'utf-8', string).to_s
        result.gsub!(/[^\x00-\x7F]+/, '')  # Remove anything non-ASCII entirely (e.g. diacritics).
        result.gsub!(/[^\w_ \-]+/i,   '')  # Remove unwanted chars.
        result.gsub!(/[ \-]+/i,      '-')  # No more than one of the separator in a row.
        result.gsub!(/^\-|\-$/i,      '')  # Remove leading/trailing separator.
        result.downcase!
        result
      end

    end # class Slug
  end # module Types
end # module DataMapper
