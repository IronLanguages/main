def get_singletons(cls, n)
  result = [cls]
  n.times do  
    cls = class << cls; self; end
    result << cls
  end
  result
end

class MetaModule < Module
end
MM = MetaModule.new

[Object, Module, Class, MetaModule].each do |c|
  s = class << c; self; end
  
  c.send(:define_method, :f) { c.name }
  s.send(:define_method, :f) { 'S(' + c.name + ')' }
end

[
  MM,
  module M; self; end,
  Module,
  MetaModule
].each { |c| get_singletons(c, 10) }

[
  MM,
  module M; self; end,
  Module,
  MetaModule
].each do |c|
  get_singletons(c, 3).each do |s|
    printf '%-50s %s', s, s.f  
    puts
  end
  puts
end