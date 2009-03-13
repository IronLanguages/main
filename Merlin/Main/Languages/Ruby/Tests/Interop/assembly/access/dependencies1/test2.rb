# a.generated.dll statically references b.generated.dll in A.Main method.
# We should auto-load b.generated.dll from B directory.

class C
  def to_str
    raise Exception.new("!!!") if $raise
    File.dirname(__FILE__) + "/../../../B"
  end
end

$:.clear
$: << (File.dirname(__FILE__) + "/../../../A")
$: << C.new

require 'a.generated'
$raise = true

begin
  p A.Main
rescue
  p $!.class.name == "System::IO::FileLoadException"
end
