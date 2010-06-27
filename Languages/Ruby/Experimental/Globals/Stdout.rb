$old_out = $>

out = []

out << ($>.object_id == $stdout.object_id)

$> = File.open("foo.txt", "w+")

out << ($>.object_id == $stdout.object_id)  # true

$> = $old_out

puts out
out = []

puts '-' * 20

$x = 123
alias $old_gt $>
alias $> $x

puts 'foo'  # ignores alias
p $>        # 123

alias $> $old_gt  #restore

puts '-' * 20

class W
  def write x
    $old_out.write "[#{x}]"
  end
end

($> = 1) rescue p $!
$> = W.new
puts 'foo'



