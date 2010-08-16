  # Provides a numbering object that will produce numbers. Accepts three
  # parameters for numbering that will control how the numbers are presented
  # when given as #[](index).
  #
  # <tt>:offset</tt>::  The number to add to the index in order to produce
  #                     the proper index. This is because #tag_text indexes
  #                     from 0, not 1. This defaults to 1. Any value less
  #                     than 1 will be set to 1 (because Romans did not know
  #                     about zero or negative numbers).
  # <tt>:lower</tt>::   Renders the Roman numerals in lowercase if +true+.
  #                     Defaults to +false+.
  # <tt>:postfix</tt>:: The value that will be appended to the number
  #                     presented by #[]. Defaults to +nil+.
  # <tt>:prefix</tt>::  The value that will be prepended to the number
  #                     presented by #[]. Defaults to +nil+.
  #
  #   r1 = Text::Format::Roman.new(:postfix => ".")
  #   puts r1[0]  # => "I."
  #   puts r1[8]  # => "IX.
  #
  #   r2 = Text::Format::Roman.new(:prefix => "M.")
  #   puts r2[0]  # => "M.I"
  #   puts r2[8]  # => "M.IX"
  #
  #   r3 = Text::Format::Roman.new(:offset => 3)
  #   puts r3[0]  # => "III"
  #   puts r3[9]  # => "XII"
  #
  #   r4 = Text::Format::Roman.new(:offset => 0)
  #   puts r4[0]  # => "I"
  #   puts r4[8]  # => "IX"
  #
  #   r5 = Text::Format::Roman.new(:lower => true)
  #   puts r5[0]  # => "i"
  #   puts r5[8]  # => "ix"
class Text::Format::Roman
  def [](index)
    roman = ""
    index += @offset

      # Do 1,000s
    roman << "M" * (index / 1000)
    index %= 1000

      # Do 900s
    roman << "CM" * (index / 900)
    index %= 900

      # Do 500s
    roman << "D" * (index / 500)
    index %= 500

      # Do 400s
    roman << "CD" * (index / 400)
    index %= 400

      # Do 100s
    roman << "C" * (index / 100)
    index %= 100

      # Do 90s
    roman << "XC" * (index / 90)
    index %= 90

      # Do 50s
    roman << "L" * (index / 50)
    index %= 50

      # Do 40s
    roman << "XL" * (index / 40)
    index %= 40

      # Do 10s
    roman << "X" * (index / 10)
    index %= 10

      # Do 9s
    roman << "IX" * (index / 9)
    index %= 9

      # Do 5s
    roman << "V" * (index / 5)
    index %= 5

      # Do 4s
    roman << "IV" * (index / 4)
    index %= 4

      # Do 1s
    roman << "I" * index

    roman.downcase! if @lower

    "#{@prefix}#{roman}#{@postfix}"
  end

  def initialize(options = {})
    @offset   = options[:offset].to_i || 1
    @lower    = options[:lower]       || false
    @postfix  = options[:postfix]     || nil
    @prefix   = options[:prefix]      || nil

    @offset   = 1 if @offset < 1
  end
end
