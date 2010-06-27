"foo" =~ /(f)(o)(o)/
m = $~

"x" =~ /(x)/
x = $~

p m.captures
p m.dup.captures

x.freeze
x.send :initialize_copy, m
p x.captures

class MatchData
  def initialize_copy *a
    puts 'init_copy'
  end
end

n = m.dup
p n.captures

# ruby seg fault:
#p n.pre_match

