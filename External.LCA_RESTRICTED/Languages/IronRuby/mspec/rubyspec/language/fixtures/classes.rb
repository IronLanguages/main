module LanguageSpecs
  class ClassWith_to_s
    def initialize(to_s_callback = nil)
      @to_s_callback = to_s_callback
    end
    
    def to_s
      if @to_s_callback then
        @to_s_callback.call()
      else
        "class_with_to_s"
      end
    end
  end

  class ClassWithout_to_s
    undef :to_s
  end
  
  ruby_version_is("" ... "1.9") { SCRIPT_LINES_NAME = 'SCRIPT_LINES__' }
  ruby_version_is("1.9") { SCRIPT_LINES_NAME = :SCRIPT_LINES__ }
  
  def self.script_lines_target_file
    File.dirname(__FILE__) + '/empty.rb'
  end
  
  def self.preserving_script_lines()
    old_script_lines = if Object.constants.include? SCRIPT_LINES_NAME
      Object.const_get SCRIPT_LINES_NAME
    else
      :undefined
    end
    
    begin
      yield
    ensure
      if old_script_lines == :undefined
        Object.class_eval { remove_const SCRIPT_LINES_NAME if Object.constants.include? SCRIPT_LINES_NAME }
      else
        Object.const_set SCRIPT_LINES_NAME, old_script_lines
      end
    end
  end
  
  def self.get_script_lines(filename)
    preserving_script_lines do
      Object.const_set :SCRIPT_LINES__, {}
      load filename
      SCRIPT_LINES__
    end
  end
  
  class MyHash < Hash
  end
  
  #############################################################################
  # Regexp support
  #############################################################################

  def self.paired_delimiters
    [%w[( )], %w[{ }], %w[< >], ["[", "]"]]
  end
  
  def self.non_paired_delimiters
    %w[~ ! # $ % ^ & * _ + ` - = " ' , . ? / | \\]
  end
  
  def self.blanks
    " \t"
  end
  
  def self.white_spaces
    ruby_version_is "1.9" do
      # 1.9 treats \v as white space.
      return blanks + "\f\n\r\v"
    end
    return blanks + "\f\n\r"
  end
  
  def self.non_alphanum_non_space
    '~!@#$%^&*()+-\|{}[]:";\'<>?,./'
  end
  
  def self.punctuations
    ",.?" # TODO - Need to fill in the full list
  end
  
  def self.get_regexp_with_substitution o
    /#{o}/o
  end
end