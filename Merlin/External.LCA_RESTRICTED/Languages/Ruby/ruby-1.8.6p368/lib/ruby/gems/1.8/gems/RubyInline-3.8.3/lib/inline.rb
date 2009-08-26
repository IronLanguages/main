#!/usr/local/bin/ruby -w

##
# Ruby Inline is a framework for writing ruby extensions in foreign
# languages.
#
# == SYNOPSIS
#
#   require 'inline'
#   class MyClass
#     inline do |builder|
#       builder.include "<math.h>"
#       builder.c %q{
#         long factorial(int max) {
#           int i=max, result=1;
#           while (i >= 2) { result *= i--; }
#           return result;
#         }
#       }
#     end
#   end
#
# == DESCRIPTION
#
# Inline allows you to write foreign code within your ruby code. It
# automatically determines if the code in question has changed and
# builds it only when necessary. The extensions are then automatically
# loaded into the class/module that defines it.
#
# You can even write extra builders that will allow you to write
# inlined code in any language. Use Inline::C as a template and look
# at Module#inline for the required API.
#
# == PACKAGING
#
# To package your binaries into a gem, use hoe's INLINE and
# FORCE_PLATFORM env vars.
#
# Example:
#
#   rake package INLINE=1
#
# or:
#
#   rake package INLINE=1 FORCE_PLATFORM=mswin32
#
# See hoe for more details.
#

require "rbconfig"
require "digest/md5"
require 'fileutils'
require 'rubygems'

require 'zentest_mapping'

$TESTING = false unless defined? $TESTING

class CompilationError < RuntimeError; end

##
# The Inline module is the top-level module used. It is responsible
# for instantiating the builder for the right language used,
# compilation/linking when needed, and loading the inlined code into
# the current namespace.

module Inline
  VERSION = '3.8.3'

  WINDOZE  = /mswin|mingw/ =~ RUBY_PLATFORM
  RUBINIUS = defined? RUBY_ENGINE
  DEV_NULL = (WINDOZE ? 'nul'      : '/dev/null')
  GEM      = (WINDOZE ? 'gem.bat'  : 'gem')
  RAKE     = if WINDOZE then
               'rake.bat'
             elsif RUBINIUS then
               File.join(Gem.bindir, 'rake')
             else
               "#{Gem.ruby} -S rake"
             end

  warn "RubyInline v #{VERSION}" if $DEBUG

  def self.register cls
    registered_inline_classes << cls
    registered_inline_classes.uniq!
  end

  def self.registered_inline_classes
    @@registered_inline_classes ||= []
  end

  # rootdir can be forced using INLINEDIR variable
  # if not defined, it should store in user HOME folder
  #
  # Under Windows user data can be stored in several locations:
  #
  #  HOME
  #  HOMEDRIVE + HOMEPATH
  #  APPDATA
  #  USERPROFILE
  #
  # Perform a check in that other to see if the environment is defined
  # and if so, use it. only try this on Windows.

  def self.rootdir
    env = ENV['INLINEDIR'] || ENV['HOME']

    if env.nil? and WINDOZE then
      # try HOMEDRIVE + HOMEPATH combination
      if ENV['HOMEDRIVE'] && ENV['HOMEPATH'] then
        env = ENV['HOMEDRIVE'] + ENV['HOMEPATH']
      end

      # no HOMEDRIVE? use APPDATA
      env = ENV['APPDATA'] if env.nil? and ENV['APPDATA']

      # bummer, still no env? then fall to USERPROFILE
      env = ENV['USERPROFILE'] if env.nil? and ENV['USERPROFILE']
    end

    if env.nil? then
      abort "Define INLINEDIR or HOME in your environment and try again"
    end

    unless defined? @@rootdir and env == @@rootdir and test ?d, @@rootdir then
      rootdir = env
      Dir.mkdir rootdir, 0700 unless test ?d, rootdir
      Dir.assert_secure rootdir
      @@rootdir = rootdir
    end

    @@rootdir
  end

  def self.directory
    directory = File.join(rootdir, ".ruby_inline")
    unless defined? @@directory and directory == @@directory
      @@directory = File.join(self.rootdir, ".ruby_inline")
    end
    Dir.assert_secure directory
    @@directory
  end

  ##
  # Inline::C is the default builder used and the only one provided by
  # Inline. It can be used as a template to write builders for other
  # languages. It understands type-conversions for the basic types and
  # can be extended as needed using #add_type_converter, #alias_type_converter
  # and #remove_type_converter.

  class C

    include ZenTestMapping

    MAGIC_ARITY_THRESHOLD = 15
    MAGIC_ARITY = -1

    ##
    # Default C to ruby and ruby to C type map

    TYPE_MAP = {
      'char'               => [ 'NUM2CHR',        'CHR2FIX'      ],

      'char *'             => [ 'StringValuePtr', 'rb_str_new2'  ],

      'double'             => [ 'NUM2DBL',        'rb_float_new' ],

      'int'                => [ "FI\X2INT",       'INT2FIX'      ],
      'unsigned int'       => [ 'NUM2UINT',       'UINT2NUM'     ],
      'unsigned'           => [ 'NUM2UINT',       'UINT2NUM'     ],

      'long'               => [ 'NUM2LONG',       'LONG2NUM'     ],
      'unsigned long'      => [ 'NUM2ULONG',      'ULONG2NUM'    ],

      'long long'          => [ 'NUM2LL',         'LL2NUM'       ],
      'unsigned long long' => [ 'NUM2ULL',        'ULL2NUM'      ],

      'off_t'              => [ 'NUM2OFFT',       'OFFT2NUM'     ],

      'VALUE'              => [ '',               ''             ],
      # Can't do these converters because they conflict with the above:
      # ID2SYM(x), SYM2ID(x), F\IX2UINT(x)
    }

    def strip_comments(src)
      # strip c-comments
      src = src.gsub(%r%\s*/\*.*?\*/%m, '')
      # strip cpp-comments
      src = src.gsub(%r%^\s*//.*?\n%, '')
      src = src.gsub(%r%[ \t]*//[^\n]*%, '')
      src
    end

    def parse_signature(src, raw=false)

      sig = self.strip_comments(src)
      # strip preprocessor directives
      sig.gsub!(/^\s*\#.*(\\\n.*)*/, '')
      # strip {}s
      sig.gsub!(/\{[^\}]*\}/, '{ }')
      # clean and collapse whitespace
      sig.gsub!(/\s+/, ' ')

      unless defined? @types then
        @types = 'void|' + @type_map.keys.map{|x| Regexp.escape(x)}.join('|')
      end

      if /(#{@types})\s*(\w+)\s*\(([^)]*)\)/ =~ sig then
        return_type, function_name, arg_string = $1, $2, $3
        args = []
        arg_string.split(',').each do |arg|

          # helps normalize into 'char * varname' form
          arg = arg.gsub(/\s*\*\s*/, ' * ').strip

          if /(((#{@types})\s*\*?)+)\s+(\w+)\s*$/ =~ arg then
            args.push([$4, $1])
          elsif arg != "void" then
            warn "WAR\NING: '#{arg}' not understood"
          end
        end

        arity = args.size
        arity = MAGIC_ARITY if raw

        return {
          'return' => return_type,
          'name'   => function_name,
          'args'   => args,
          'arity'  => arity
        }
      end

      raise SyntaxError, "Can't parse signature: #{sig}"
    end # def parse_signature

    def generate(src, options={})
      options = {:expand_types=>options} unless Hash === options

      expand_types = options[:expand_types]
      singleton = options[:singleton]
      result = self.strip_comments(src)

      signature = parse_signature(src, !expand_types)
      function_name = signature['name']
      method_name = options[:method_name]
      method_name ||= test_to_normal function_name
      return_type = signature['return']
      arity = signature['arity']

      raise ArgumentError, "too many arguments" if arity > MAGIC_ARITY_THRESHOLD

      if expand_types then
        prefix = "static VALUE #{function_name}("
        if arity == MAGIC_ARITY then
          prefix += "int argc, VALUE *argv, VALUE self"
        else
          prefix += "VALUE self"
          prefix += signature['args'].map { |arg, type| ", VALUE _#{arg}"}.join
        end
        prefix += ") {\n"
        prefix += signature['args'].map { |arg, type|
          "  #{type} #{arg} = #{ruby2c(type)}(_#{arg});\n"
        }.join

        # replace the function signature (hopefully) with new sig (prefix)
        result.sub!(/[^;\/\"\>]+#{function_name}\s*\([^\{]+\{/, "\n" + prefix)
        result.sub!(/\A\n/, '') # strip off the \n in front in case we added it
        unless return_type == "void" then
          raise SyntaxError, "Couldn't find return statement for #{function_name}" unless
            result =~ /return/
          result.gsub!(/return\s+([^\;\}]+)/) do
            "return #{c2ruby(return_type)}(#{$1})"
          end
        else
          result.sub!(/\s*\}\s*\Z/, "\nreturn Qnil;\n}")
        end
      else
        prefix = "static #{return_type} #{function_name}("
        result.sub!(/[^;\/\"\>]+#{function_name}\s*\(/, prefix)
        result.sub!(/\A\n/, '') # strip off the \n in front in case we added it
      end

      delta = if result =~ /\A(static.*?\{)/m then
                $1.split(/\n/).size
              else
                warn "WAR\NING: Can't find signature in #{result.inspect}\n" unless $TESTING
                0
              end

      file, line = caller[1].split(/:/)
      result = "# line #{line.to_i + delta} \"#{file}\"\n" + result unless $DEBUG and not $TESTING

      @src << result
      @sig[function_name] = [arity,singleton,method_name]

      return result if $TESTING
    end # def generate

    ##
    # Builds a complete C extension suitable for writing to a file and
    # compiling.

    def generate_ext
      ext = []

      if @include_ruby_first
        @inc.unshift "#include \"ruby.h\""
      else
        @inc.push "#include \"ruby.h\""
      end

      ext << @inc
      ext << nil
      ext << @src.join("\n\n")
      ext << nil
      ext << nil
      ext << "#ifdef __cplusplus"
      ext << "extern \"C\" {"
      ext << "#endif"
      ext << "  __declspec(dllexport)" if WINDOZE
      ext << "  void Init_#{module_name}() {"
      ext << "    VALUE c = rb_cObject;"

      # TODO: use rb_class2path
      # ext << "    VALUE c = rb_path2class(#{@mod.name.inspect});"
      ext << @mod.name.split("::").map { |n|
        "    c = rb_const_get(c, rb_intern(\"#{n}\"));"
      }.join("\n")

      ext << nil

      @sig.keys.sort.each do |name|
        method = ''
        arity, singleton, method_name = @sig[name]
        if singleton then
          if method_name == 'allocate' then
            raise "#{@mod}::allocate must have an arity of zero" if arity > 0
            ext << "    rb_define_alloc_func(c, (VALUE(*)(VALUE))#{name});"
            next
          end
          method << "    rb_define_singleton_method(c, \"#{method_name}\", "
        else
          method << "    rb_define_method(c, \"#{method_name}\", "
        end
        method << "(VALUE(*)(ANYARGS))#{name}, #{arity});"
        ext << method
      end

      ext << @init_extra.join("\n") unless @init_extra.empty?

      ext << nil
      ext << "  }"
      ext << "#ifdef __cplusplus"
      ext << "}"
      ext << "#endif"
      ext << nil

      ext.join "\n"
    end

    def module_name
      unless defined? @module_name then
        module_name = @mod.name.gsub('::','__')
        md5 = Digest::MD5.new
        @sig.keys.sort_by { |x| x.to_s }.each { |m| md5 << m.to_s }
        @module_name = "Inline_#{module_name}_#{md5.to_s[0,4]}"
      end
      @module_name
    end

    def so_name
      unless defined? @so_name then
        @so_name = "#{Inline.directory}/#{module_name}.#{Config::CONFIG["DLEXT"]}"
      end
      @so_name
    end

    attr_reader :rb_file, :mod
    attr_writer :mod
    attr_accessor :src, :sig, :flags, :libs, :init_extra

    ##
    # Sets the name of the C struct for generating accessors.  Used with
    # #accessor, #reader, #writer.

    attr_accessor :struct_name

    def initialize(mod)
      raise ArgumentError, "Class/Module arg is required" unless Module === mod
      # new (but not on some 1.8s) -> inline -> real_caller|eval
      stack = caller
      meth = stack.shift until meth =~ /in .(inline|test_|setup)/ or stack.empty?
      raise "Couldn't discover caller" if stack.empty?
      real_caller = stack.first
      real_caller = stack[3] if real_caller =~ /\(eval\)/
      real_caller =~ /(.*):(\d+)/
      real_caller = $1
      @rb_file = File.expand_path real_caller

      @mod = mod
      @src = []
      @inc = []
      @sig = {}
      @flags = []
      @libs = []
      @init_extra = []
      @include_ruby_first = true
      @inherited_methods = {}
      @struct_name = nil

      @type_map = TYPE_MAP.dup
    end

    ##
    # Adds a #reader and #writer for a C struct member wrapped via
    # Data_Wrap_Struct.  +method+ is the ruby name to give the accessor,
    # +type+ is the C type.  Unless the C member name is overridden with
    # +member+, the method name is used as the struct member.
    #
    #   builder.struct_name = 'MyStruct'
    #   builder.accessor :title,        'char *'
    #   builder.accessor :stream_index, 'int',   :index
    #
    # The latter accesses MyStruct->index via the stream_index method.

    def accessor(method, type, member = method)
      reader method, type, member
      writer method, type, member
    end

    ##
    # Adds a reader for a C struct member wrapped via Data_Wrap_Struct.
    # +method+ is the ruby name to give the reader, +type+ is the C type.
    # Unless the C member name is overridden with +member+, the method
    # name is used as the struct member.  See #accessor for an example.

    def reader(method, type, member = method)
      raise "struct name not set for reader #{method} #{type}" unless
        @struct_name

      c <<-C
VALUE #{method}() {
  #{@struct_name} *pointer;

  Data_Get_Struct(self, #{@struct_name}, pointer);

  return #{c2ruby type}(pointer->#{member});
}
      C
    end

    ##
    # Adds a writer for a C struct member wrapped via Data_Get_Struct.
    # +method+ is the ruby name to give the writer, +type+ is the C type.
    # Unless the C member name is overridden with +member+, the method
    # name is used as the struct member.  See #accessor for an example.

    def writer(method, type, member = method)
      raise "struct name not set for writer #{method} #{type}" unless
        @struct_name

      c <<-C
VALUE #{method}_equals(VALUE value) {
  #{@struct_name} *pointer;

  Data_Get_Struct(self, #{@struct_name}, pointer);

  pointer->#{member} = #{ruby2c type}(value);

  return value;
}
      C
    end

    ##
    # Converts ruby type +type+ to a C type

    def ruby2c(type)
      raise ArgumentError, "Unknown type #{type.inspect}" unless @type_map.has_key? type
      @type_map[type].first
    end

    ##
    # Converts C type +type+ to a ruby type

    def c2ruby(type)
      raise ArgumentError, "Unknown type #{type.inspect}" unless @type_map.has_key? type
      @type_map[type].last
    end

    ##
    # Attempts to load pre-generated code returning true if it succeeds.

    def load_cache
      begin
        file = File.join("inline", File.basename(so_name))
        if require file then
          dir = Inline.directory
          warn "WAR\NING: #{dir} exists but is not being used" if test ?d, dir and $VERBOSE
          return true
        end
      rescue LoadError
      end
      return false
    end

    ##
    # Loads the generated code back into ruby

    def load
      require "#{so_name}" or raise LoadError, "require on #{so_name} failed"
    end

    ##
    # Builds the source file, if needed, and attempts to compile it.

    def build
      so_name = self.so_name
      so_exists = File.file? so_name
      unless so_exists and File.mtime(rb_file) < File.mtime(so_name) then

        unless File.directory? Inline.directory then
          warn "NOTE: creating #{Inline.directory} for RubyInline" if $DEBUG
          Dir.mkdir Inline.directory, 0700
        end

        src_name = "#{Inline.directory}/#{module_name}.c"
        old_src_name = "#{src_name}.old"
        should_compare = File.write_with_backup(src_name) do |io|
          io.puts generate_ext
        end

        # recompile only if the files are different
        recompile = true
        if so_exists and should_compare and
            FileUtils.compare_file(old_src_name, src_name) then
          recompile = false

          # Updates the timestamps on all the generated/compiled files.
          # Prevents us from entering this conditional unless the source
          # file changes again.
          t = Time.now
          File.utime(t, t, src_name, old_src_name, so_name)
        end

        if recompile then

          hdrdir = %w(srcdir archdir rubyhdrdir).map { |name|
            Config::CONFIG[name]
          }.find { |dir|
            dir and File.exist? File.join(dir, "/ruby.h")
          } or abort "ERROR: Can't find header dir for ruby. Exiting..."

          flags = @flags.join(' ')
          libs  = @libs.join(' ')

          config_hdrdir = if RUBY_VERSION > '1.9' then
                            "-I #{File.join hdrdir, RbConfig::CONFIG['arch']}"
                          else
                            nil
                          end

          cmd = [ Config::CONFIG['LDSHARED'],
                  flags,
                  Config::CONFIG['CCDLFLAGS'],
                  Config::CONFIG['CFLAGS'],
                  '-I', hdrdir,
                  config_hdrdir,
                  '-I', Config::CONFIG['includedir'],
                  "-L#{Config::CONFIG['libdir']}",
                  '-o', so_name.inspect,
                  File.expand_path(src_name).inspect,
                  libs,
                  crap_for_windoze ].join(' ')

          # TODO: remove after osx 10.5.2
          cmd += ' -flat_namespace -undefined suppress' if
            RUBY_PLATFORM =~ /darwin9\.[01]/
          cmd += " 2> #{DEV_NULL}" if $TESTING and not $DEBUG

          warn "Building #{so_name} with '#{cmd}'" if $DEBUG
          result = `#{cmd}`
          warn "Output:\n#{result}" if $DEBUG
          if $? != 0 then
            bad_src_name = src_name + ".bad"
            File.rename src_name, bad_src_name
            raise CompilationError, "error executing #{cmd.inspect}: #{$?}\nRenamed #{src_name} to #{bad_src_name}"
          end

          # NOTE: manifest embedding is only required when using VC8 ruby
          # build or compiler.
          # Errors from this point should be ignored if Config::CONFIG['arch']
          # (RUBY_PLATFORM) matches 'i386-mswin32_80'
          if WINDOZE and RUBY_PLATFORM =~ /_80$/ then
            Dir.chdir Inline.directory do
              cmd = "mt /manifest lib.so.manifest /outputresource:so.dll;#2"
              warn "Embedding manifest with '#{cmd}'" if $DEBUG
              result = `#{cmd}`
              warn "Output:\n#{result}" if $DEBUG
              if $? != 0 then
                raise CompilationError, "error executing #{cmd}: #{$?}"
              end
            end
          end

          warn "Built successfully" if $DEBUG
        end

      else
        warn "#{so_name} is up to date" if $DEBUG
      end # unless (file is out of date)
    end # def build

    ##
    # Returns extra compilation flags for windoze platforms. Ugh.

    def crap_for_windoze
      # gawd windoze land sucks
      case RUBY_PLATFORM
      when /mswin32/ then
        " -link /LIBPATH:\"#{Config::CONFIG['libdir']}\" /DEFAULTLIB:\"#{Config::CONFIG['LIBRUBY']}\" /INCREMENTAL:no /EXPORT:Init_#{module_name}"
      when /mingw32/ then
        " -Wl,--enable-auto-import -L#{Config::CONFIG['libdir']} -lmsvcrt-ruby18"
      when /i386-cygwin/ then
        ' -L/usr/local/lib -lruby.dll'
      else
        ''
      end
    end

    ##
    # Adds compiler options to the compiler command line.  No
    # preprocessing is done, so you must have all your dashes and
    # everything.

    def add_compile_flags(*flags)
      @flags.push(*flags)
    end

    ##
    # Adds linker flags to the link command line.  No preprocessing is
    # done, so you must have all your dashes and everything.

    def add_link_flags(*flags)
      @libs.push(*flags)
    end

    ##
    # Create a static variable and initialize it to a value.

    def add_static name, init, type = "VALUE"
      prefix      "static #{type} #{name};"
      add_to_init "#{name} = #{init};"
    end

    ##
    # Adds custom content to the end of the init function.

    def add_to_init(*src)
      @init_extra.push(*src)
    end

    ##
    # Registers C type-casts +r2c+ and +c2r+ for +type+.

    def add_type_converter(type, r2c, c2r)
      warn "WAR\NING: overridding #{type} on #{caller[0]}" if @type_map.has_key? type
      @type_map[type] = [r2c, c2r]
    end

    ##
    # Registers C type +alias_type+ as an alias of +existing_type+

    def alias_type_converter(existing_type, alias_type)
      warn "WAR\NING: overridding #{type} on #{caller[0]}" if
        @type_map.has_key? alias_type

      @type_map[alias_type] = @type_map[existing_type]
    end

    ##
    # Unregisters C type-casts for +type+.

    def remove_type_converter(type)
      @type_map.delete type
    end

    ##
    # Maps a ruby constant to C (with the same name)

    def map_ruby_const(*names)
      names.each do |name|
        self.prefix "static VALUE #{name};"
        self.add_to_init "    #{name} = rb_const_get(c, rb_intern(#{name.to_s.inspect}));"
      end
    end

    ##
    # Maps a C constant to ruby. +names_and_types+ is a hash that maps the
    # name of the constant to its C type.
    #
    #   builder.map_c_const :C_NAME => :int
    #
    # If you wish to give the constant a different ruby name:
    #
    #   builder.map_c_const :C_NAME => [:int, :RUBY_NAME]

    def map_c_const(names_and_types)
      names_and_types.each do |name, typ|
        typ, ruby_name = Array === typ ? typ : [typ, name]
        self.add_to_init "    rb_define_const(c, #{ruby_name.to_s.inspect}, #{c2ruby(typ.to_s)}(#{name}));"
      end
    end

    ##
    # Adds an include to the top of the file. Don't forget to use
    # quotes or angle brackets.

    def include(header)
      @inc << "#include #{header}"
    end

    ##
    # Specifies that the the ruby.h header should be included *after* custom
    # header(s) instead of before them.

    def include_ruby_last
      @include_ruby_first = false
    end

    ##
    # Adds any amount of text/code to the source

    def prefix(code)
      @src << code
    end

    ##
    # Adds a C function to the source, including performing automatic
    # type conversion to arguments and the return value. The Ruby
    # method name can be overridden by providing method_name. Unknown
    # type conversions can be extended by using +add_type_converter+.

    def c src, options = {}
      options = {
        :expand_types => true,
      }.merge options
      self.generate src, options
    end

    ##
    # Same as +c+, but adds a class function.

    def c_singleton src, options = {}
      options = {
        :expand_types => true,
        :singleton    => true,
      }.merge options
      self.generate src, options
    end

    ##
    # Adds a raw C function to the source. This version does not
    # perform any type conversion and must conform to the ruby/C
    # coding conventions.  The Ruby method name can be overridden
    # by providing method_name.

    def c_raw src, options = {}
      self.generate src, options
    end

    ##
    # Same as +c_raw+, but adds a class function.

    def c_raw_singleton src, options = {}
      options = {
        :singleton => true,
      }.merge options
      self.generate src, options
    end

  end # class Inline::C
end # module Inline

class Module

  ##
  # options is a hash that allows you to pass extra data to your
  # builder.  The only key that is guaranteed to exist is :testing.

  attr_reader :options

  ##
  # Extends the Module class to have an inline method. The default
  # language/builder used is C, but can be specified with the +lang+
  # parameter.

  def inline(lang = :C, options={})
    Inline.register self

    case options
    when TrueClass, FalseClass then
      warn "WAR\NING: 2nd argument to inline is now a hash, changing to {:testing=>#{options}}" unless options
      options = { :testing => options  }
    when Hash
      options[:testing] ||= false
    else
      raise ArgumentError, "BLAH"
    end

    builder_class = begin
                      Inline.const_get(lang)
                    rescue NameError
                      require "inline/#{lang}"
                      Inline.const_get(lang)
                    end

    @options = options
    builder = builder_class.new self

    yield builder

    unless options[:testing] then
      unless builder.load_cache then
        builder.build
        builder.load
      end
    end
  end
end

class File

  ##
  # Equivalent to +File::open+ with an associated block, but moves
  # any existing file with the same name to the side first.

  def self.write_with_backup(path) # returns true if file already existed

    # move previous version to the side if it exists
    renamed = false
    if test ?f, path then
      renamed = true
      File.rename path, path + ".old"
    end

    File.open(path, "w") do |io|
      yield(io)
    end

    return renamed
  end
end # class File

class Dir

  ##
  # +assert_secure+ checks that if a +path+ exists it has minimally
  # writable permissions. If not, it prints an error and exits. It
  # only works on +POSIX+ systems. Patches for other systems are
  # welcome.

  def self.assert_secure(path)
    mode = File.stat(path).mode
    unless ((mode % 01000) & 0022) == 0 then
      if $TESTING then
        raise SecurityError, "Directory #{path} is insecure"
      else
        abort "#{path} is insecure (#{'%o' % mode}). It may not be group or world writable. Exiting."
      end
    end
  rescue Errno::ENOENT
    # If it ain't there, it's certainly secure
  end
end
