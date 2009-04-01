#!/usr/local/bin/ruby -w

$TESTING = true

$: << 'lib'

SKIP_PROCS = ENV['FAST'] or RUBY_VERSION >= "1.9" or defined? RUBY_ENGINE

require 'test/unit'
require 'ruby2ruby'
require 'pt_testcase'
require 'fileutils'
require 'tmpdir'

FileUtils.rm_rf File.expand_path("~/.ruby_inline") # for self-translation

class R2RTestCase < ParseTreeTestCase
  def self.previous key
    "ParseTree"
  end

  def self.generate_test klass, node, data, input_name, output_name
    output_name = data.has_key?('Ruby2Ruby') ? 'Ruby2Ruby' : 'Ruby'

    klass.class_eval <<-EOM
      def test_#{node}
        pt = #{data[input_name].inspect}
        rb = #{data[output_name].inspect}

        refute_nil pt, \"ParseTree for #{node} undefined\"
        refute_nil rb, \"Ruby for #{node} undefined\"

        assert_equal rb, @processor.process(pt)
      end
    EOM
  end
end

class TestRuby2Ruby < R2RTestCase
  alias :refute_nil :assert_not_nil unless defined? Mini

  def setup
    super
    @processor = Ruby2Ruby.new
  end

  def teardown
    unless $DEBUG then
      FileUtils.rm_rf @rootdir
      ENV.delete 'INLINEDIR'
    end if defined?(@rootdir) && @rootdir
  end

  def test_dregx_slash
    inn = util_thingy(:dregx)
    out = "/blah\\\"blah#\{(1 + 1)}blah\\\"blah\\/blah/"
    util_compare inn, out, /blah\"blah2blah\"blah\/blah/
  end

  def test_dstr_quote
    inn = util_thingy(:dstr)
    out = "\"blah\\\"blah#\{(1 + 1)}blah\\\"blah/blah\""
    util_compare inn, out, "blah\"blah2blah\"blah/blah"
  end

  def test_dsym_quote
    inn = util_thingy(:dsym)
    out = ":\"blah\\\"blah#\{(1 + 1)}blah\\\"blah/blah\""
    util_compare inn, out, :"blah\"blah2blah\"blah/blah"
  end

  def test_lit_regexp_slash
    util_compare s(:lit, /blah\/blah/), '/blah\/blah/', /blah\/blah/
  end

  def test_call_self_index
    util_compare s(:call, nil, :[], s(:arglist, s(:lit, 42))), "self[42]"
  end

  def test_call_self_index_equals
    util_compare(s(:call, nil, :[]=, s(:arglist, s(:lit, 42), s(:lit, 24))),
                 "self[42] = 24")
  end

  def test_masgn_wtf
    inn = s(:block,
            s(:masgn,
              s(:array, s(:lasgn, :k), s(:lasgn, :v)),
              s(:array,
                s(:splat,
                  s(:call,
                    s(:call, nil, :line, s(:arglist)),
                    :split,
                    s(:arglist, s(:lit, /\=/), s(:lit, 2)))))),
            s(:attrasgn,
              s(:self),
              :[]=,
              s(:arglist, s(:lvar, :k),
                s(:call, s(:lvar, :v), :strip, s(:arglist)))))

    out = "k, v = *line.split(/\\=/, 2)\nself[k] = v.strip\n"

    util_compare inn, out
  end


  def test_masgn_splat_wtf
    inn = s(:masgn,
            s(:array, s(:lasgn, :k), s(:lasgn, :v)),
            s(:array,
              s(:splat,
                s(:call,
                  s(:call, nil, :line, s(:arglist)),
                  :split,
                  s(:arglist, s(:lit, /\=/), s(:lit, 2))))))
    out = 'k, v = *line.split(/\\=/, 2)'
    util_compare inn, out
  end

  def test_splat_call
    inn = s(:call, nil, :x,
            s(:arglist,
              s(:splat,
                s(:call,
                  s(:call, nil, :line, s(:arglist)),
                  :split,
                  s(:arglist, s(:lit, /=/), s(:lit, 2))))))

    out = 'x(*line.split(/=/, 2))'
    util_compare inn, out
  end

  def util_compare sexp, expected_ruby, expected_eval = nil
    assert_equal expected_ruby, @processor.process(sexp)
    assert_equal expected_eval, eval(expected_ruby) if expected_eval
  end

  def util_setup_inline
    @rootdir = File.join(Dir.tmpdir, "test_ruby_to_ruby.#{$$}")
    Dir.mkdir @rootdir, 0700 unless test ?d, @rootdir
    ENV['INLINEDIR'] = @rootdir
  end

  def util_thingy(type)
    s(type,
      'blah"blah',
      s(:call, s(:lit, 1), :+, s(:arglist, s(:lit, 1))),
      s(:str, 'blah"blah/blah'))
  end
end

##
# Converts a +target+ using a +processor+ and converts +target+ name
# in the source adding +gen+ to allow easy renaming.

$broken = false
def morph_and_eval(processor, target, gen, n)
  return if $broken
  begin
    processor = Object.const_get processor if Symbol === processor
    target    = Object.const_get target    if Symbol === target
    old_name  = target.name
    new_name  = target.name.sub(/\d*$/, gen.to_s)
    ruby      = processor.translate(target).sub(old_name, new_name)

    old, $-w = $-w, nil
    eval ruby
    $-w = old

    target.constants.each do |constant|
      eval "#{new_name}::#{constant} = #{old_name}::#{constant}"
    end
  rescue Exception => e
    warn "Self-Translation Generation #{n} failed:"
    warn "#{e.class}: #{e.message}"
    warn e.backtrace.join("\n  ")
    warn ""
    warn ruby
    warn ""
    $broken = true
  else
    begin
      yield if block_given?
    rescue
      # probably already handled
    end
  end
end

####################
#         impl
#         old  new
#
# t  old    0    1
# e
# s
# t  new    2    3

# Self-Translation: 1st Generation - morph Ruby2Ruby using Ruby2Ruby
morph_and_eval :Ruby2Ruby, :Ruby2Ruby, 2, 1 do
  class TestRuby2Ruby1 < TestRuby2Ruby
    def setup
      super
      @processor = Ruby2Ruby2.new
    end
  end
end unless SKIP_PROCS

# Self-Translation: 2nd Generation - morph TestRuby2Ruby using Ruby2Ruby
morph_and_eval :Ruby2Ruby, :TestRuby2Ruby, 2, 2 do
  # Self-Translation: 3rd Generation - test Ruby2Ruby2 with TestRuby2Ruby1
  class TestRuby2Ruby3 < TestRuby2Ruby2
    def setup
      super
      @processor = Ruby2Ruby2.new
    end
  end
end unless SKIP_PROCS

# Self-Translation: 4th (and final) Generation - fully circular
morph_and_eval(:Ruby2Ruby2, :Ruby2Ruby2, 3, 4) do
  class TestRuby2Ruby4 < TestRuby2Ruby3
    def setup
      super
      @processor = Ruby2Ruby3.new
    end
  end
end unless SKIP_PROCS
