$a = []
class << $stdout
  def write *args
    $a << args
  end
end

alias $stdout $foo
printf("%d %d", 1, 2)

class << STDOUT
  remove_method :write
end

p $a