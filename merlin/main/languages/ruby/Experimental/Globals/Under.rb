$foo = 123

alias $under $_
alias $_ $foo

gets
p $_, $foo, $under  #123, 123, foo -> gets goes directly to the $_ variable

puts '-' * 20

def foo
  p $_, $foo, $under  #123, 123, nil -> $_ is on scope
end

foo

puts '-' * 20

$_ = 'bob'
$_ = 1

$/ = 'bob'
($/ = 1) rescue p $!

alias $goo $/
($goo = 1) rescue p $!

class S
  def to_s
    "foo"
  end
  def to_str
    "bar"
  end
end

($/ = S.new) rescue p $!
$/ = nil

puts '-' * 20

$x = "a"
$/ = "b"

alias $old_slash $/
alias $/ $x

p $/
p gets                # "12a34b" -> doesn't go thru alias

puts '-' * 20

p $/.object_id == $/.object_id

alias $/ $old_slash       # restore $/

puts '-' * 20

$/ = "\n"
p gets

p $.   #3
p gets #3
p $.   #4
p gets #4
p $.   #5

$. = 1
p gets #5 !!!
p $.   #2 !!!

p gets #5 !!!
p $.   #2 !!!

puts '-' * 20

($. = 'foo') rescue p $!

alias $_dot $.
($_dot = 'foo') rescue p $!

class I
  def to_i
    1
  end
end

($. = I.new) rescue p $!

puts '-' * 20

alias $old_dot $.
alias $. $y

$old_dot = 20
$y = 30

gets
p $., $y, $old_dot  # 30, 30, 21 -> gets ignores alias     

alias $. $old_dot          #restore $.



