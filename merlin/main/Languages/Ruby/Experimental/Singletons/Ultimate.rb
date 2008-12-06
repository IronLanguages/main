class Object; def f; puts 'Object'; end; end
class Module; def f; puts 'Module'; super; end; end
class Class; def f; puts 'Class'; super; end; end
class C; def f; puts 'C'; super; end; end
module M; def f; puts 'M'; super; end; end
$obj = C.new

$S1_Object = class << Object;     def f; puts 'S(Object)'; super; end; self; end
$S1_Module = class << Module;     def f; puts 'S(Module)'; super; end; self; end
$S1_Class  = class << Class;      def f; puts 'S(Class)'; super; end; self; end
$S1_C      = class << C;          def f; puts 'S(C)'; super; end; self; end
$S1_M      = class << M;          def f; puts 'S(M)'; super; end; self; end
$S1_obj    = class << $obj;       def f; puts 'S(obj)'; super; end; self; end

$S2_Object = class << $S1_Object; def f; puts 'S2(Object)'; super; end; self; end
$S2_Module = class << $S1_Module; def f; puts 'S2(Module)'; super; end; self; end
$S2_Class  = class << $S1_Class;  def f; puts 'S2(Class)'; super; end; self; end
$S2_C      = class << $S1_C;      def f; puts 'S2(C)'; super; end; self; end
$S2_M      = class << $S1_M;      def f; puts 'S2(M)'; super; end; self; end
$S2_obj    = class << $S1_obj;    def f; puts 'S2(obj)'; super; end; self; end

$S3_Object = class << $S2_Object; def f; puts 'S3(Object)'; super; end; self; end
$S3_Module = class << $S2_Module; def f; puts 'S3(Module)'; super; end; self; end
$S3_Class  = class << $S2_Class;  def f; puts 'S3(Class)'; super; end; self; end
$S3_C      = class << $S2_C;      def f; puts 'S3(C)'; super; end; self; end
$S3_M      = class << $S2_M;      def f; puts 'S3(M)'; super; end; self; end
$S3_obj    = class << $S2_obj;    def f; puts 'S3(obj)'; super; end; self; end

def t(name, start)
  puts
  puts "#{name}.f:"
  start.f
end

t 'obj',       $obj
t 'S1(obj)',   $S1_obj
t 'S2(obj)',   $S2_obj

puts '-' * 20

t 'C',         C
t 'S1(C)',     $S1_C
t 'S2(C)',     $S2_C

puts '-' * 20

t 'M',         M
t 'S1(M)',     $S1_M
t 'S2(M)',     $S2_M

puts '-' * 20
 
t 'Class',     Class
t 'S1(Class)', $S1_Class
t 'S2(Class)', $S2_Class

puts '-' * 20

t 'Module',    Module
t 'S1(Module)',$S1_Module
t 'S2(Module)',$S2_Module

puts '-' * 20

t 'Object',    Object
t 'S1(Object)',$S1_Object
t 'S2(Object)',$S2_Object
