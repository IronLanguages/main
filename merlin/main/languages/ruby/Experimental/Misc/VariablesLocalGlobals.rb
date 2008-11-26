=begin

 The following gobals have local scope:
   $1 .. $9      NTH_REF         not-aliasable
   $&            BACK_REF
   $+            BACK_REF
   $`            BACK_REF
   $'            BACK_REF
   $~            GVAR
   $_            GVAR

 True globals:
   $*
   $?
   $<
   $>
   $!
   $@
   $;
   $=
   $.
   $/
   $~
   $+
   $_
   $"
   $&
   $.
   $,
   $\
   $$
   $'
   $`
   $$
   $0
   $/
   
=end

require 'English'

puts "CHILD_STATUS = '#{$CHILD_STATUS}'"
puts "DEFAULT_INPUT = '#{$DEFAULT_INPUT}'"


puts "-------------------------------"

def foo
  "cat" =~ /(c)(a)(t)/
  puts "foo:1"
  puts global_variables.join(' ')
  puts "$1 = #{$1}"
  puts
  
  bar()
  puts "foo:2"
  puts global_variables.join(' ')
  puts "$1 = #{$1}"
  puts
  
  baz()
  puts "foo:3"
  puts global_variables.join(' ')
  puts "$1 = #{$1}"
  puts
end

def bar
  "bo" =~ /(b)(o)/
  puts "bar:1"
  puts global_variables.join(' ')
  puts "$1 = #{$1}"
  puts
end

def baz
  puts "baz:1"
  puts global_variables.join(' ')
  puts "$1 = #{$1}"
  puts
end

foo

# $! variable is real global

def exc
  $! = NameError.new "bogus"
  begin
    begin
      $" = 'foo'
    rescue NameError
      puts "Message 1: #{$!}"
      display_message
      $! = 'bar'
    end
  rescue TypeError
    puts "Message 3: #{$!}"
  end
  
  puts "Message 4: #{$!}"
end

def display_message
  puts "Message 2: #{$!}"
end

exc

puts "Message 5: #{$!}"

# testing $_ - it's local scoped global yet can be aliased
# => RubyOps.GetGlobal needs to get the current environment

def test_gets
  gets
  puts "Gets returned 1: '#{$_}'"
  puts "Gets returned 1: '#{$FOO}'"
  dump_gets
end

def dump_gets
  puts "Gets returned 2: '#{$_}'"
end

$X = 'X'
alias $_ $X              # alias hides the real global
alias $FOO $_
alias $BAR $`

test_gets
