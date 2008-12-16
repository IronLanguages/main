class R < Range
  def initialize
    super(0,0)
  end
  
  def begin; p 'x'; 1; end
  def end; p 'x';2; end
  def exclude_end?; p 'x';false; end
end

a = [1,2,3,4]
p a[R.new]

x = "1234"
x.slice!(R.new)
p x

p "1234".slice(R.new)

x = "1234"
x[R.new] = 'x'
p x

s = Struct.new(:a,:b,:c,:d)[1,2,3,4]
p s.values_at(R.new)


