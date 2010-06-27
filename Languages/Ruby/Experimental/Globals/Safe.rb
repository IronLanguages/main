p $SAFE
$SAFE += 1
p $SAFE
($SAFE -= 1) rescue p $!
p $SAFE

class I
  def to_i
    2
  end
end

($SAFE = "foo") rescue p $!
($SAFE = I.new) rescue p $!

