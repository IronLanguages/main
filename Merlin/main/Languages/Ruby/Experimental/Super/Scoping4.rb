class A
  def foo
    puts 'A::foo'
  end
end

class B < A
  def foo
    puts 'B::foo'
  end
end

class C < B
  def foo
    A.module_eval {
      super
    }
    A.new.instance_eval {
      super
    }
  end
end 


C.new.foo


