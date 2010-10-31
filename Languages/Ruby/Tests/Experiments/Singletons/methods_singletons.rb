# == Object ===========

class Object
  $Object0 = self
  def self.c_Object0; :c_Object0; end
end

class << $Object0
  $Object1 = self
  def self.c_Object1; :c_Object1; end
end

class << $Object1
  $Object2 = self
  def self.c_Object2; :c_Object2; end
end

class << $Object2
  $Object3 = self
  def self.c_Object3; :c_Object3; end
end

class << $Object3
  $Object4 = self
  def self.c_Object4; :c_Object4; end
end

$ObjectD = $Object4.superclass

# == Module ===========

class Module
  $Module0 = self
  def self.c_Module0; :c_Module0; end
end

class << $Module0
  $Module1 = self
  def self.c_Module1; :c_Module1; end
end

class << $Module1
  $Module2 = self
  def self.c_Module2; :c_Module2; end
end

class << $Module2
  $Module3 = self
  def self.c_Module3; :c_Module3; end
end

class << $Module3
  $Module4 = self
  def self.c_Module4; :c_Module4; end
end

$ModuleD = $Module4.superclass

# == Class ============

class Class
  $Class0 = self
  def self.c_Class0; :c_Class0; end
end

class << $Class0
  $Class1 = self
  def self.c_Class1; :c_Class1; end
end

class << $Class1
  $Class2 = self
  def self.c_Class2; :c_Class2; end
end

class << $Class2
  $Class3 = self
  def self.c_Class3; :c_Class3; end
end

class << $Class3
  $Class4 = self
  def self.c_Class4; :c_Class4; end
end

$ClassD = $Class4.superclass

# == D ================

class D
  $D0 = self
  def self.c_D0; :c_D0; end
end

class << $D0
  $D1 = self
  def self.c_D1; :c_D1; end
end

class << $D1
  $D2 = self
  def self.c_D2; :c_D2; end
end

class << $D2
  $D3 = self
  def self.c_D3; :c_D3; end
end

class << $D3
  $D4 = self
  def self.c_D4; :c_D4; end
end

$DD = $D4.superclass

# == C ================

class C < D
  $C0 = self
  def self.c_C0; :c_C0; end
end

class << $C0
  $C1 = self
  def self.c_C1; :c_C1; end
end

class << $C1
  $C2 = self
  def self.c_C2; :c_C2; end
end

class << $C2
  $C3 = self
  def self.c_C3; :c_C3; end
end

class << $C3
  $C4 = self
  def self.c_C4; :c_C4; end
end

$CD = $C4.superclass

# == X ================

$X0 = C.new

class << $X0
  $X1 = self
  def self.c_X1; :c_X1; end
end

class << $X1
  $X2 = self
  def self.c_X2; :c_X2; end
end

class << $X2
  $X3 = self
  def self.c_X3; :c_X3; end
end

class << $X3
  $X4 = self
  def self.c_X4; :c_X4; end
end

$XD = $X4.superclass

# == Y ================

$Y0 = C.new

class << $Y0
  $Y1 = self
  def self.c_Y1; :c_Y1; end
end

$YD = $Y1.superclass

# =====================

classes = [
$C0,$C1,$C2,$C3,$C4, 
$D0,$D1,$D2,$D3,$D4, 
$X0,$X1,$X2,$X3,$X4,
$Y0,$Y1,
$Object0,$Object1,$Object2,$Object3,$Object4,
$Module0,$Module1,$Module2,$Module3,$Module4,
$Class0,$Class1,$Class2,$Class3,$Class4,
]

# IronRuby doesn't support member table sharing on dummies, so the results are different from MRI:
dummies = [$CD, $DD, $XD, $YD, $ClassD, $ModuleD, $ObjectD]

names = [
:C0,:C1,:C2,:C3,:C4,
:D0,:D1,:D2,:D3,:D4,
:X0,:X1,:X2,:X3,:X4,
:Y0,:Y1,
:Object0,:Object1,:Object2,:Object3,:Object4,
:Module0,:Module1,:Module2,:Module3,:Module4,
:Class0,:Class1,:Class2,:Class3,:Class4,
]

methods = [
:c_C0,:c_C1,:c_C2,:c_C3,:c_C4, 
:c_D0,:c_D1,:c_D2,:c_D3,:c_D4, 
:c_X1,:c_X2,:c_X3,:c_X4,
:c_Y1,
:c_Object0,:c_Object1,:c_Object2,:c_Object3,:c_Object4,
:c_Module0,:c_Module1,:c_Module2,:c_Module3,:c_Module4,
:c_Class0,:c_Class1,:c_Class2,:c_Class3,:c_Class4,
]

for i in 0..classes.size-1 do
    cls = classes[i]
    d = []
    methods.each { |m| 
        begin 
            cls.public_class_method(m) 
        rescue 
            #puts "!#{names[i]}::#{m}"
        else 
            d << m
        end 
    }
    puts "#{names[i]}: #{d.inspect}"
end

