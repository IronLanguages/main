# A valid example
class SlotMachine
  include DataMapper::Resource

  property :id, Serial
  property :power_on, Boolean, :default => false

  is :state_machine, :initial => :off, :column => :mode do
    state :off,
      :enter => :power_down,
      :exit  => :power_up
    state :idle
    state :spinning
    state :report_loss
    state :report_win
    state :pay_out

    event :pull_crank do
      transition :from => :idle,  :to => :spinning
    end

    event :turn_off do
      transition :from => :idle,  :to => :off
    end

    event :turn_on do
      transition :from => :off,   :to => :idle
    end
  end

  def initialize
    @log = []
    super
  end

  def power_up
    self.power_on = true
    @log << [:power_up, Time.now]
  end

  def power_down
    self.power_on = false
    @log << [:power_down, Time.now]
  end

end

SlotMachine.auto_migrate!
