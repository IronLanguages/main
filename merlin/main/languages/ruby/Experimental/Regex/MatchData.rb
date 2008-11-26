MatchData.new rescue p $!

class M < MatchData; end

class MatchData
  p ancestors          # [MatchData, Object, Kernel]
  p instance_methods(false)
  p private_instance_methods(false)  
  p protected_instance_methods(false)  
  p singleton_methods(false)  
end


