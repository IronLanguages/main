# A.dll statically references B.dll in A.Main method.
# We should auto-load B.dll from B directory.

class C
  def to_str
    raise Exception.new("!!!") if $raise
    "B"
  end
end

$:.clear
$: << "A"
$: << C.new

require 'a'
$raise = true

begin
  p A.Main
rescue
  p $!.class.name == "System::IO::FileLoadException"
end