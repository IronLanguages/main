# aliasing match globals is weird, the aliases could be made but are ignored

$foo = 123

alias $1 $foo
alias $2 $foo
alias $3 $foo

alias $~ $foo  # not-ignored

alias $& $foo
alias $+ $foo
alias $` $foo

"abc" =~ /(a)(b)(c)/

p $1
p $2
p $3

p $~
p $&
p $+
p $`

p global_variables.sort