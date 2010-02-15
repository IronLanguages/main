begin
  require 'reginald'
rescue LoadError
  $: << File.expand_path(File.join(File.dirname(__FILE__), 'vendor/reginald'))
  require 'reginald'
end

require 'uri'

module Rack::Mount
  # Private utility methods used throughout Rack::Mount.
  #--
  # This module is a trash can. Try to move these functions into
  # more appropriate contexts.
  #++
  module Utils
    # Normalizes URI path.
    #
    # Strips off trailing slash and ensures there is a leading slash.
    #
    #   normalize_path("/foo")  # => "/foo"
    #   normalize_path("/foo/") # => "/foo"
    #   normalize_path("foo")   # => "/foo"
    #   normalize_path("")      # => "/"
    def normalize_path(path)
      path = "/#{path}"
      path.squeeze!('/')
      path.sub!(%r{/+\Z}, '')
      path = '/' if path == ''
      path
    end
    module_function :normalize_path

    # Removes trailing nils from array.
    #
    #   pop_trailing_nils!([1, 2, 3])           # => [1, 2, 3]
    #   pop_trailing_nils!([1, 2, 3, nil, nil]) # => [1, 2, 3]
    #   pop_trailing_nils!([nil])               # => []
    def pop_trailing_nils!(ary)
      while ary.length > 0 && ary.last.nil?
        ary.pop
      end
      ary
    end
    module_function :pop_trailing_nils!

    RESERVED_PCHAR = ':@&=+$,;%'
    SAFE_PCHAR = "#{URI::REGEXP::PATTERN::UNRESERVED}#{RESERVED_PCHAR}"
    if RUBY_VERSION >= '1.9'
      UNSAFE_PCHAR = Regexp.new("[^#{SAFE_PCHAR}]", false).freeze
    else
      UNSAFE_PCHAR = Regexp.new("[^#{SAFE_PCHAR}]", false, 'N').freeze
    end

    def escape_uri(uri)
      URI.escape(uri.to_s, UNSAFE_PCHAR)
    end
    module_function :escape_uri

    if ''.respond_to?(:force_encoding)
      def unescape_uri(uri)
        URI.unescape(uri).force_encoding('utf-8')
      end
    else
      def unescape_uri(uri)
        URI.unescape(uri)
      end
    end
    module_function :unescape_uri

    # Taken from Rack 1.1.x to build nested query strings
    def build_nested_query(value, prefix = nil) #:nodoc:
      case value
      when Array
        value.map { |v|
          build_nested_query(v, "#{prefix}[]")
        }.join("&")
      when Hash
        value.map { |k, v|
          build_nested_query(v, prefix ? "#{prefix}[#{Rack::Utils.escape(k)}]" : Rack::Utils.escape(k))
        }.join("&")
      when String
        raise ArgumentError, "value must be a Hash" if prefix.nil?
        "#{prefix}=#{Rack::Utils.escape(value)}"
      else
        prefix
      end
    end
    module_function :build_nested_query

    def normalize_extended_expression(regexp)
      return regexp unless regexp.options & Regexp::EXTENDED != 0
      source = regexp.source
      source.gsub!(/#.+$/, '')
      source.gsub!(/\s+/, '')
      source.gsub!(/\\\//, '/')
      Regexp.compile(source)
    end
    module_function :normalize_extended_expression

    def parse_regexp(regexp)
      unless regexp.is_a?(RegexpWithNamedGroups)
        regexp = RegexpWithNamedGroups.new(regexp)
      end

      expression = Reginald.parse(regexp)

      unless Reginald.regexp_supports_named_captures?
        tag_captures = Proc.new do |group|
          group.each do |child|
            if child.is_a?(Reginald::Group)
              child.name = regexp.names[child.index] if child.index
              tag_captures.call(child)
            elsif child.is_a?(Reginald::Expression)
              tag_captures.call(child)
            end
          end
        end
        tag_captures.call(expression)
      end

      expression
    rescue Racc::ParseError, Reginald::Parser::ScanError
      []
    end
    module_function :parse_regexp
  end
end
