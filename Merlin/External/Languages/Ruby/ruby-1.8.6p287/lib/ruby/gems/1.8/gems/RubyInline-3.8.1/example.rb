#!/usr/local/bin/ruby -w

begin
  require 'rubygems'
rescue LoadError
  $: << 'lib'
end
require 'inline'

class MyTest

  def factorial(n)
    f = 1
    n.downto(2) { |x| f *= x }
    f
  end

  inline do |builder|
    builder.c "
    long factorial_c(int max) {
      int i=max, result=1;
      while (i >= 2) { result *= i--; }
      return result;
    }"

    builder.c_raw "
    static
    VALUE
    factorial_c_raw(int argc, VALUE *argv, VALUE self) {
      int i=FIX2INT(argv[0]), result=1;
      while (i >= 2) { result *= i--; }
      return INT2NUM(result);
    }"
  end
end

# breakeven for build run vs native doing 5 factorial:
#   on a PIII/750 running FreeBSD:        about 5000
#   on a PPC/G4/800 running Mac OSX 10.2: always faster

require 'benchmark'
puts "RubyInline #{Inline::VERSION}" if $DEBUG

MyTest.send(:alias_method, :factorial_alias, :factorial_c_raw)

t = MyTest.new()
max = (ARGV.shift || 1_000_000).to_i
n   = (ARGV.shift || 5).to_i
m   = t.factorial(n)

def validate(n, m)
  if n != m then raise "#{n} != #{m}"; end
end

puts "# of iterations = #{max}, n = #{n}"
Benchmark::bm(20) do |x|
  x.report("null_time") do
    for i in 0..max do
      # do nothing
    end
  end

  x.report("c") do
    for i in 0..max do
      validate(t.factorial_c(n), m)
    end
  end

  x.report("c-raw") do
    for i in 0..max do
      validate(t.factorial_c_raw(n), m)
    end
  end

  x.report("c-alias") do
    for i in 0..max do
      validate(t.factorial_alias(n), m)
    end
  end

  x.report("pure ruby") do
    for i in 0..max do
      validate(t.factorial(n), m)
    end
  end
end
