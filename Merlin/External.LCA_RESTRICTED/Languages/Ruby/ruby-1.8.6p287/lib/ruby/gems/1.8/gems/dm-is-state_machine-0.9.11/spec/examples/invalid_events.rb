# An invalid example.
class InvalidEvents
  include DataMapper::Resource

  property :id, Serial

  is :state_machine do
    state :day
    state :night
  end

  # The next lines are intentionally incorrect.
  #
  # 'event' only makes sense in a block under 'is :state_machine'
  event :sunrise
  event :sunset

end

InvalidEvents.auto_migrate!
