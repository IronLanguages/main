p $*
p $*.object_id == $*.object_id

puts '-' * 20

alias $old_star $*
alias $* $z

$old_start0 = $old_star[0]
$old_star[0] = "Under.txt"
$z = ["Under2.txt"]

p gets                      # "foo" -> alias ignored

alias $* $old_star          #restore $*
$old_star[0] = $old_start0  #restore $*[0]

p $*
p gets
p gets
p gets
p gets
p gets
p gets
p gets
p gets
p gets
p gets
p gets
p $*
p gets
p $*
p gets
p $*
p gets
p $*