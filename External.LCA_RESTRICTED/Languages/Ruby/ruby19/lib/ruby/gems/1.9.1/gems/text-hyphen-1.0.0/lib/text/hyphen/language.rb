# Language scaffolding support for Text::Hyphen. Language hyphenation
# patterns are defined as instances of this class -- and only this class.
# This is a deliberate "breaking" of Ruby's concept of duck-typing and is
# intended to provide an indication that the patterns have been converted
# from TeX encodings to other encodings (e.g., latin1 or UTF-8) that are
# more suitable to general text manipulations.
class Text::Hyphen::Language
  WORD_START_RE         = %r{^\.}
  WORD_END_RE           = %r{\.$}
  DIGIT_RE              = %r{\d}
  NONDIGIT_RE           = %r{\D}
  DASH_RE               = %r{-}
  EXCEPTION_DASH0_RE    = %r{[^-](?=[^-])}
  EXCEPTION_DASH1_RE    = %r{[^-]-}
  EXCEPTION_NONUM_RE    = %r{[^01]}
  ZERO_INSERT_RE        = %r{(\D)(?=\D)}
  ZERO_START_RE         = %r{^(?=\D)}

  def encoding(enc)
  @encoding = enc
  end

  def both
    @patterns[:both]
  end

  def start
    @patterns[:start]
  end

  def stop
    @patterns[:stop]
  end

  def hyphen
    @patterns[:hyphen]
  end

  def patterns(pats = nil)
    return @patterns if pats.nil?

    @patterns = {
      :both   => {}, 
      :start  => {},
      :stop   => {},
      :hyphen => {}
    }

    plist = pats.split($/).map { |ln| ln.gsub(%r{%.*$}, '') }
    plist.each do |line|
      line.split.each do |word|
        next if word.empty?

        start = stop = false

        start = true if word.sub!(WORD_START_RE, '')
        stop  = true if word.sub!(WORD_END_RE, '')

          # Insert zeroes and start with some digit
        word.gsub!(ZERO_INSERT_RE) { "#{$1}0" }
        word.gsub!(ZERO_START_RE, "0")

          # This assumes that the pattern lists are already in lowercase
          # form only.
        tag   = word.gsub(DIGIT_RE, '')
        value = word.gsub(NONDIGIT_RE, '')

        if start and stop
          set = :both
        elsif start
          set = :start
        elsif stop
          set = :stop
        else
          set = :hyphen
        end

        @patterns[set][tag] = value
      end
    end

    true
  end

  attr_accessor :exceptions
  def exceptions(exc = nil) #:nodoc:
    return @exceptions if exc.nil?
    @exceptions = {}

    exc.split.each do |word|
      tag   = word.gsub(DASH_RE,'')
      value = "0" + word.gsub(EXCEPTION_DASH0_RE, '0').gsub(EXCEPTION_DASH1_RE, '1')
      value.gsub!(EXCEPTION_NONUM_RE, '0')
      @exceptions[tag] = value
    end

    true
  end

  attr_accessor :left
  attr_accessor :right

  def initialize
    self.encoding   "latin1"
    self.patterns   ""
    self.exceptions ""
    self.left       = 2
    self.right      = 2

    yield self if block_given?
  end
end
