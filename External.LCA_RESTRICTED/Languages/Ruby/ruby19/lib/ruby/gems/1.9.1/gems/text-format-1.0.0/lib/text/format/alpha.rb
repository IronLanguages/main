  # Provides a numbering object that will produce letters. Accepts four
  # options for numbering that will control how the letters are presented
  # when given as #[](index). This numbering object will only provide 26
  # values ("a" .. "z") unless :wrap is +true+.
  #
  # <tt>:transform</tt>:: The symbol representing the method to be called on
  #                       the letter. This must be a method that does not
  #                       require any arguments.
  # <tt>:postfix</tt>::   The value that will be appended to the letter
  #                       presented by #[]. Defaults to +nil+.
  # <tt>:prefix</tt>::    The value that will be prepended to the letter
  #                       presented by #[]. Defaults to +nil+.
  # <tt>:wrap</tt>::      If +true+, then indexes will be wrapped from "z"
  #                       to "a".
  #
  #   a1 = Text::Format::Alpha.new(:postfix => ".")
  #   puts a1[0]  # => "a."
  #   puts a1[1]  # => "b.
  #   puts a1[27] # => ""
  #
  #   a2 = Text::Format::Alpha.new(:prefix => "A.")
  #   puts a2[0]  # => "A.a"
  #   puts a2[1]  # => "A.b"
  #   puts a2[27] # => ""
  #
  #   a3 = Text::Format::Alpha.new(:transform => :upcase)
  #   puts a3[0]  # => "A"
  #   puts a3[1]  # => "B"
  #   puts a3[27] # => ""
  #
  #   a4 = Text::Format::Alpha.new(:wrap => true)
  #   puts a4[0]  # => "a"
  #   puts a4[27] # => "b"
class Text::Format::Alpha
  def [](index)
    if @wrap
      ltr = (?a + (index % 26)).chr
    elsif index.between?(0, 25)
      ltr = (?a + index).chr
    else
      ltr = nil
    end

    if ltr
      if @transform
        "#{@prefix}#{ltr.send(transform)}#{@postfix}"
      else
        "#{@prefix}#{ltr}#{@postfix}"
      end
    else
      ""
    end
  end

  def initialize(options = {}) #:yields self:
    @transform  = options[:transform] || nil
    @wrap       = options[:wrap]      || false
    @postfix    = options[:postfix]   || nil
    @prefix     = options[:prefix]    || nil
  end
end
