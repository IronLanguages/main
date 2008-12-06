class Struct
  def i *a
    initialize *a
  end
end

x = Struct.new(:foo, &lambda { puts 'foo' }).new("foo")

p x
x.i("bar")
p x
