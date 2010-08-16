  # Provides a numbering object that will produce numbers. Accepts three
  # parameters for numbering that will control how the numbers are presented
  # when given as #[](index).
  #
  # <tt>:offset</tt>::  The number to add to the index in order to produce
  #                     the proper index. This is because #tag_text indexes
  #                     from 0, not 1. This defaults to 1.
  # <tt>:postfix</tt>:: The value that will be appended to the number
  #                     presented by #[]. Defaults to +nil+.
  # <tt>:prefix</tt>::  The value that will be prepended to the number
  #                     presented by #[]. Defaults to +nil+.
  #
  #   n1 = Text::Format::Number.new(:postfix => ".")
  #   puts n1[0]  # => "1."
  #   puts n1[1]  # => "2.
  #
  #   n2 = Text::Format::Number.new(:prefix => "2.")
  #   puts n2[0]  # => "2.1"
  #   puts n2[1]  # => "2.2"
  #
  #   n3 = Text::Format::Number.new(:offset => 3)
  #   puts n3[0]  # => "3"
  #   puts n3[1]  # => "4"
class Text::Format::Number
  def [](index)
    "#{@prefix}#{index + @offset}#{@postfix}"
  end

  def initialize(options = {}) #:yields self:
    @offset   = options[:offset].to_i || 1
    @postfix  = options[:postfix]     || nil
    @prefix   = options[:prefix]      || nil
  end
end
