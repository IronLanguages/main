module M
  def foo
    puts 'foo'
    puts self
    def goo
      puts 'goo'
    end
  end
end

class C
  include M
end

x = C.new
x.foo
x.goo

class D
  include M  
end

y = D.new
y.goo

p M.instance_methods(false)
p D.instance_methods(false)
p C.instance_methods(false)

 