module N_Object;     def f; puts 'N_Object';      super; end; def f_N_Object;      end;end
module N_Module;     def f; puts 'N_Module';      super; end; def f_N_Module;      end;end
module N_Class;      def f; puts 'N_Class';       super; end; def f_N_Class;       end;end
module N_C;          def f; puts 'N_C';           super; end; def f_N_C;           end;end
module N_M;          def f; puts 'N_M';           super; end; def f_N_M;           end;end
module N_NoSM;       def f; puts 'N_NoSM';        super; end; def f_N_NoSM;        end;end
module N_MModule;    def f; puts 'N_MModule';     super; end; def f_N_MModule;     end;end

class Object; def f; puts 'Object';        end; def f_Object; end;include N_Object;end
class Module; def f; puts 'Module';  super; end; def f_Module; end;include N_Module;end
class Class;  def f; puts 'Class';   super; end; def f_Class;  end;include N_Class; end
class C;      def f; puts 'C';       super; end; def f_C;      end;include N_C;     end
module M;     def f; puts 'M';       super; end; def f_M;      end;include N_M;     end
module NoSM;  def f; puts 'NoSM';    super; end; def f_NoSM;   end;include N_NoSM;  end
class MModule < Module;def f; puts 'MModule'; super; end; def f_MModule; end;include N_MModule;end

MM = MModule.new                                                                             
$obj = C.new

# object without a singleton:
$no_singleton = C.new

module N_S1_Object;  def f; puts 'N_S1_Object';   super; end; def f_N_S1_Object;   end;end
module N_S1_Module;  def f; puts 'N_S1_Module';   super; end; def f_N_S1_Module;   end;end
module N_S1_Class;   def f; puts 'N_S1_Class';    super; end; def f_N_S1_Class;    end;end
module N_S1_C;       def f; puts 'N_S1_C';        super; end; def f_N_S1_C;        end;end
module N_S1_M;       def f; puts 'N_S1_M';        super; end; def f_N_S1_M;        end;end
module N_S1_obj;     def f; puts 'N_S1_obj';      super; end; def f_N_S1_obj;      end;end

$S1_Object = class << Object;     def f; puts 'S(Object)';  super; end; def f_S1_Object; end; include N_S1_Object;self; end
$S1_Module = class << Module;     def f; puts 'S(Module)';  super; end; def f_S1_Module; end; include N_S1_Module;self; end
$S1_Class  = class << Class;      def f; puts 'S(Class)';   super; end; def f_S1_Class ; end; include N_S1_Class; self; end
$S1_C      = class << C;          def f; puts 'S(C)';       super; end; def f_S1_C     ; end; include N_S1_C;     self; end
$S1_M      = class << M;          def f; puts 'S(M)';       super; end; def f_S1_M     ; end; include N_S1_M;     self; end
$S1_obj    = class << $obj;       def f; puts 'S(obj)';     super; end; def f_S1_obj   ; end; include N_S1_obj;   self; end
                                                                                                      
$S2_Object = class << $S1_Object; def f; puts 'S2(Object)'; super; end; def f_S2_Object; end; self; end
$S2_Module = class << $S1_Module; def f; puts 'S2(Module)'; super; end; def f_S2_Module; end; self; end
$S2_Class  = class << $S1_Class;  def f; puts 'S2(Class)';  super; end; def f_S2_Class ; end; self; end
$S2_C      = class << $S1_C;      def f; puts 'S2(C)';      super; end; def f_S2_C     ; end; self; end
$S2_M      = class << $S1_M;      def f; puts 'S2(M)';      super; end; def f_S2_M     ; end; self; end
$S2_obj    = class << $S1_obj;    def f; puts 'S2(obj)';    super; end; def f_S2_obj   ; end; self; end
                                                                                             
$S3_Object = class << $S2_Object; def f; puts 'S3(Object)'; super; end; def f_S3_Object; end; self; end
$S3_Module = class << $S2_Module; def f; puts 'S3(Module)'; super; end; def f_S3_Module; end; self; end
$S3_Class  = class << $S2_Class;  def f; puts 'S3(Class)';  super; end; def f_S3_Class ; end; self; end
$S3_C      = class << $S2_C;      def f; puts 'S3(C)';      super; end; def f_S3_C     ; end; self; end
$S3_M      = class << $S2_M;      def f; puts 'S3(M)';      super; end; def f_S3_M     ; end; self; end
$S3_obj    = class << $S2_obj;    def f; puts 'S3(obj)';    super; end; def f_S3_obj   ; end; self; end
                                                                                    
# the following singletons hide Ruby bug in for dummy singletons: method table is shared with the previous singleton
class << $S3_Object; end
class << $S3_Module; end
class << $S3_Class;  end
class << $S3_C;      end
class << $S3_M;      end
class << $S3_obj;    end
                                                                                    
$classes = {
	'Object' => Object,
	'S1_Object' => $S1_Object,
	'S2_Object' => $S2_Object, 
	'S3_Object' => $S3_Object,
	'Module' => Module,
	'S1_Module' => $S1_Module, 
	'S2_Module' => $S2_Module, 
	'S3_Module' => $S3_Module, 
	'Class' => Class,
	'S1_Class' => $S1_Class, 
	'S2_Class' => $S2_Class, 
	'S3_Class' => $S3_Class, 
	'S1_obj' => $S1_obj, 
	'S2_obj' => $S2_obj,    
	'S3_obj' => $S2_obj,
	'M' => M,
	'S1_M' => $S1_M,  
	'S2_M' => $S2_M,
	'S3_M' => $S3_M,
	'C' => C,
	'S1_C' => $S1_C,
	'S2_C' => $S2_C,
	'S3_C' => $S3_C,
	'NoSM' => NoSM,
	'MModule' => MModule,
	'MM' => MM
}

$ordered_names = [
	'Object',
	'S1_Object',
	'S2_Object',
	'S3_Object',
	'Module',
	'S1_Module',
	'S2_Module',
	'S3_Module',
	'Class',
	'S1_Class',
	'S2_Class',
	'S3_Class',
	'S1_obj',
	'S2_obj',
	'S3_obj',
	'M',
	'S1_M',
	'S2_M',
	'S3_M',
	'C',
	'S1_C',
	'S2_C',
	'S3_C',
	'NoSM',
	'MModule',
	'MM'
]

# define a distinct method on each module:
#$classes.each { |name,cls|
#  cls.module_eval {
#    define_method("f_" + name) {}
#  }
#}

