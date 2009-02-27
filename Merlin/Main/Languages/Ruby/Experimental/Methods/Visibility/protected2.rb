class C
end

obj = C.new

class << obj
  protected
  def foo
    puts 'ok'
  end
end

obj.foo rescue p $!

obj.instance_eval {
  self.foo
}