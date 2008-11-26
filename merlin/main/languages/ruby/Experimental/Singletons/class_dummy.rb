class C
end

class << C
  ::DUMMY = self.superclass
end

class DUMMY
  def foo
  end
  
  def self.goo
  end
end

puts 'ancestors:'
p DUMMY.ancestors
puts 'instance:'
p DUMMY.instance_methods(false)
puts 'private:'
p DUMMY.private_methods(false)
puts 'protected:'
p DUMMY.protected_methods(false)
puts 'singleton:'
p DUMMY.singleton_methods(false)

p DUMMY.allocate rescue p $!
p DUMMY.new rescue p $!
p DUMMY.superclass
p DUMMY.superclass.object_id
p DUMMY.name
p DUMMY.object_id
