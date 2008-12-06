C = 'ON OBJECT'
$S = self

class << self
  $S = self
end

class P
  p Module.nesting
end

def define_c
  p self.object_id
  p $S.object_id
  p $S1.object_id
  p ::C rescue puts $!
  eval('::C = "C2"') rescue puts $!
end

$tlb = TOPLEVEL_BINDING

load 'Constants_Global_1.rb', true
