class Class
  alias x_new new
end

def MatchData.new *a
  x_new *a
end

m = MatchData.new

p m.string

p m.begin(0) rescue p $!
p m.end(0) rescue p $!
p m.length
p m.captures

# this crashes MRI:
#p m.pre_match
#p m.post_match
