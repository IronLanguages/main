# An invalid example.
class InvalidStates
  include DataMapper::Resource

  property :id, Serial

  is :state_machine do
    event :sunrise
    event :sunset
  end

  # The next lines are intentionally incorrect.
  #
  # 'state' only makes sense in a block under 'is :state_machine'
  state :light
  state :dark

end

InvalidStates.auto_migrate!
