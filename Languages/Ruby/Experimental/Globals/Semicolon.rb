puts "Initial value"
p $;

$x = "a"
$; = "b"

alias $old_slash $;
alias $; $x

p $;

puts "---"
p '123a456b789'.split               # [123a456,789] -> ignores alias
puts "","---"

p $;.object_id == $;.object_id

alias $; $old_slash       # restore $;

puts '-' * 20

$; = nil                           
p '123a456b789'.split               # [123a456b789] 

$; = 5
p '123a456b789'.split rescue p $!   # `split': wrong argument type Fixnum (expected Regexp)

class S
  def to_s; "a"; end
  def to_str; "b"; end
end

$; = S.new
p '123a456b789'.split rescue p $!   # ["123a456", "789"]

puts 'Done'



