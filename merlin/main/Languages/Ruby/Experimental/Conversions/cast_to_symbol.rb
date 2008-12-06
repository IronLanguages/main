
module M

[1, :X.to_i, nil, 1.2].each do |x|

puts '='*80, "== #{x.inspect} ==", '='*80

instance_variable_get(x) rescue p $!
instance_variable_set(x, nil) rescue p $!
instance_variable_defined?(x) rescue p $!
remove_instance_variable(x) rescue p $!

puts '-' * 20

class_variable_get(x) rescue p $!
class_variable_set(x, nil) rescue p $!
remove_class_variable(x) rescue p $!

puts '-' * 20

const_set(x, nil) rescue p $!
const_get(x) rescue p $!
const_defined?(x) rescue p $!
remove_const(x) rescue p $!

puts '-' * 20

method_defined?(x) rescue p $!
respond_to?(x) rescue p $!
send(x) rescue p $!
method(x) rescue p $!
throw(x) rescue p $!
catch(x) rescue p $!

puts '-' * 20

public(x) rescue p $!
define_method(x, &lambda {}) rescue p $!
attr(x) rescue p $!
attr_accessor(x) rescue p $!
attr_writer(x) rescue p $!
attr_reader(x) rescue p $!
alias_method(x,x) rescue p $!
remove_method(x,x) rescue p $!
undef_method(x,x) rescue p $!
Struct.new(x) rescue p $!

end
end