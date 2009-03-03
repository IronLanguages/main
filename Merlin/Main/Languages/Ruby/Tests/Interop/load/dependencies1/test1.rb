# A.dll statically references B.dll in A.Main method.
# We should auto-load B.dll from B directory.

$:.clear
$: << "A"
$: << "B"

require 'a'
p A.Main