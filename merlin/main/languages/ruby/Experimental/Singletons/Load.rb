load 'Load_1.rb', true
$S1 = $S
$T1 = $T

load 'Load_1.rb', true
$S2 = $S
$T2 = $T

p self.object_id
p $S1.object_id
p $S2.object_id

p TOPLEVEL_BINDING.object_id
p $T1.object_id
p $T2.object_id

