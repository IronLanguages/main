alias $foo $bar

p defined? $foo
p defined? $bar

$bar = nil

p defined? $foo
p defined? $bar

$bar = 1

p defined? $foo
p defined? $bar

puts $foo

p defined? $foo
p defined? $bar

alias $foo2 $bar2

$foo2 = 2
puts $bar2

puts '-0-'

p defined? $-a
alias $dasha $-a
p defined? $dasha

puts '-1-'

p defined? $-
alias $dash $-
p defined? $dash

puts '-2-'

p defined? $+
alias $plus1 $+
p defined? $plus1

puts '-3-'

"x" =~ /(x)/
p defined? $+
alias $plus2 $+
p defined? $plus2  # doesn't go thru alias, but the difference is only in 1.8


