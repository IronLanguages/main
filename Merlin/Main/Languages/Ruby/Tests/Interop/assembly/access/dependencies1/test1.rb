# a.generated.dll statically references b.generated.dll in A.Main method.
# We should auto-load b.generated.dll from B directory.

$:.clear
$: << (File.dirname(__FILE__) + "/../../../A")
$: << (File.dirname(__FILE__) + "/../../../B")

require 'a.generated'
p A.Main
