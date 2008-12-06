
x = <<EOT
bar
  EOT
 EOT
EOTTT
EOT;
EOT 
EOT

x = <<EOT         #whitespace and comment here
bar
EOT

# error:
# x = <<EOT   foo
# EOT

x = <<EOT

EOT

# following prints:
#blah
#foo
#bar

x = <<EOT1,<<EOT2,<<EOT3
blah
EOT1
foo
EOT2
bar
EOT3

puts x

# following prints:
#blah
#foo inner foos
#
#bar

puts <<EOT1,      <<EOT2,            <<EOT3
blah
EOT1
foo #{ "inner " + x = <<EOT5
foos
EOT5
}
EOT2
bar
EOT3

puts <<'EOT1'
cup<\t>
EOT1

puts <<'EOT1 blah'
cup<\t>
EOT1 blah

puts <<"EOT1 blah"
cup<\t>
EOT1 blah

puts <<"EOT1-bar", <<'  EOT2', <<-"EOT3".upcase, <<-'EOT4'
really cool cup <\t>
EOT1-bar
one more cup <\t>
  EOT2
space-indent-ended heredoc here "\t"
     EOT3
one more space-indent-ended heredoc here "\t"
       EOT4

puts "---------"

x = <<"EOT".downcase() << "FOO" + "BAR" + <<"GOO"
A
FOO
B
EOT
C
GOO

puts x

puts "---------"

def foo(a,b)
  yield
  "[#{a} #{b}]"
end

# block must be on the same line (next line is part of heredoc)
puts x = foo(<<"FOO", <<"BAR") { puts "BLOCK" }
foofoo
FOO
barbar
BAR

# puts <<-" space"
# doesn't work because lexer eats all whitespace and then doesn't see a space as a part of heredoc-end token
#      space

# no whitespace following <<:
#puts <<  "EOT1 blah"  
#cup<\t>
#EOT1 blah

#error:
#x = <<EOT1,
# 
#         <<EOT2,
#             <<EOT3
#blah
#EOT1

