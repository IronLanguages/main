$x = "a"
$\ = "b"

alias $old_slash $\
alias $\ $x

p $\

puts "---"
print '123'                  # 123b -> ignores alias
puts "","---"

p $\.object_id == $\.object_id

alias $\ $old_slash       # restore $\

puts '-' * 20

$\ = nil                           
($\ = 1) rescue puts $!

class S
  def to_s; "bob"; end
  def to_str; "bob"; end
end

($\ = S.new) rescue puts $!


