# == Object ===========

class Object
  $Object0 = self
  CObject0 = 'CObject0'
end

class << $Object0
  $Object1 = self
  CObject1 = 'CObject1'
end

class << $Object1
  $Object2 = self
  CObject2 = 'CObject2'
end

class << $Object2
  $Object3 = self
  CObject3 = 'CObject3'
end

class << $Object3
  $Object4 = self
  CObject4 = 'CObject4'
end

$ObjectD = $Object4.superclass

# == Module ===========

class Module
  $Module0 = self
  CModule0 = 'CModule0'
end

class << $Module0
  $Module1 = self
  CModule1 = 'CModule1'
end

class << $Module1
  $Module2 = self
  CModule2 = 'CModule2'
end

class << $Module2
  $Module3 = self
  CModule3 = 'CModule3'
end

class << $Module3
  $Module4 = self
  CModule4 = 'CModule4'
end

$ModuleD = $Module4.superclass

# == Class ============

class Class
  $Class0 = self
  CClass0 = 'CClass0'
end

class << $Class0
  $Class1 = self
  CClass1 = 'CClass1'
end

class << $Class1
  $Class2 = self
  CClass2 = 'CClass2'
end

class << $Class2
  $Class3 = self
  CClass3 = 'CClass3'
end

class << $Class3
  $Class4 = self
  CClass4 = 'CClass4'
end

$ClassD = $Class4.superclass

# == D ================

class D
  $D0 = self
  CD0 = 'CD0'
end

class << $D0
  $D1 = self
  CD1 = 'CD1'
end

class << $D1
  $D2 = self
  CD2 = 'CD2'
end

class << $D2
  $D3 = self
  CD3 = 'CD3'
end

class << $D3
  $D4 = self
  CD4 = 'CD4'
end

$DD = $D4.superclass

# == C ================

class C < D
  $C0 = self
  CC0 = 'CC0'
end

class << $C0
  $C1 = self
  CC1 = 'CC1'
end

class << $C1
  $C2 = self
  CC2 = 'CC2'
end

class << $C2
  $C3 = self
  CC3 = 'CC3'
end

class << $C3
  $C4 = self
  CC4 = 'CC4'
end

$CD = $C4.superclass

# == X ================

$X0 = C.new

class << $X0
  $X1 = self
  CX1 = 'CX1'
end

class << $X1
  $X2 = self
  CX2 = 'CX2'
end

class << $X2
  $X3 = self
  CX3 = 'CX3'
end

class << $X3
  $X4 = self
  CX4 = 'CX4'
end

$XD = $X4.superclass

# == Y ================

$Y0 = C.new

class << $Y0
  $Y1 = self
  CY1 = 'CY1'
end

$YD = $Y1.superclass

# =====================

classes = [
$C0,$C1,$C2,$C3,$C4,$CD, 
$D0,$D1,$D2,$D3,$D4,$DD, 
$X0,$X1,$X2,$X3,$X4,$XD,
$Y0,$Y1,$YD,
$Object0,$Object1,$Object2,$Object3,$Object4,$ObjectD,
$Module0,$Module1,$Module2,$Module3,$Module4,$ModuleD, 
$Class0,$Class1,$Class2,$Class3,$Class4,$ClassD, 
]

# TODO: remove
#dummies = [$CD, $DD, $XD, $YD, $ObjectD, $ModuleD, $ClassD]

names = [
:C0,:C1,:C2,:C3,:C4,:CD, 
:D0,:D1,:D2,:D3,:D4,:DD,
:X0,:X1,:X2,:X3,:X4,:XD,
:Y0,:Y1,:YD,
:Object0,:Object1,:Object2,:Object3,:Object4,:ObjectD, 
:Module0,:Module1,:Module2,:Module3,:Module4,:ModuleD, 
:Class0,:Class1,:Class2,:Class3,:Class4,:ClassD, 
]

consts = [
:CC0,:CC1,:CC2,:CC3,:CC4, 
:CD0,:CD1,:CD2,:CD3,:CD4, 
:CX1,:CX2,:CX3,:CX4,
:CY1,
:CObject0,:CObject1,:CObject2,:CObject3,:CObject4,:CObjectD, 
:CModule0,:CModule1,:CModule2,:CModule3,:CModule4,:CModuleD, 
:CClass0,:CClass1,:CClass2,:CClass3,:CClass4,:CClassD, 
]

for i in 0..classes.size-1 do
    cls = classes[i]
    d = []
    consts.each { |cst| 
        begin 
            cls.const_get(cst) 
        rescue 
            #puts "!#{names[i]}::#{cst}"
        else 
            d << cst
        end 
    }
    puts "#{names[i]}: #{d.inspect}"
end

