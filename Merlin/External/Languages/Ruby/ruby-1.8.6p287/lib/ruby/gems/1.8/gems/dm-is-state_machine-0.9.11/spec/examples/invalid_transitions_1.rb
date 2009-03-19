# An invalid example.
class InvalidTransitions1
  include DataMapper::Resource

  property :id, Serial

  is :state_machine do
    state :happy
    state :sad

    event :toggle

    # The next lines are intentionally incorrect.
    #
    # 'transition' is only valid when nested beneath 'event'
    transition :to => :happy, :from => :sad
    transition :to => :sad,   :from => :happy
  end

end

InvalidTransitions1.auto_migrate!
