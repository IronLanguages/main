class C
  def f
    puts 'C.f'
  end
  
  def g
    puts 'C.g'
  end
end

class D < C
  def f
    puts 'D.f'
    super
  end

  alias g f
end

p D.instance_methods(false)
D.new.g