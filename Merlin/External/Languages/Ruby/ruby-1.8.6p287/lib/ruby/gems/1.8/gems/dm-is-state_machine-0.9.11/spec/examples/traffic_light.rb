# A valid example
class TrafficLight
  include DataMapper::Resource

  property :id, Serial # see note 1

  is :state_machine, :initial => :green, :column => :color do
    state :green,  :enter => Proc.new { |o| o.log << "G" }
    state :yellow, :enter => Proc.new { |o| o.log << "Y" }
    state :red,    :enter => Proc.new { |o| o.log << "R" }

    event :forward do
      transition :from => :green,  :to => :yellow
      transition :from => :yellow, :to => :red
      transition :from => :red,    :to => :green
    end

    event :backward do
      transition :from => :green,  :to => :red
      transition :from => :yellow, :to => :green
      transition :from => :red,    :to => :yellow
    end
  end

  before :transition!, :before_hook
  after  :transition!, :after_hook

  def before_hook
    before_hook_log << attribute_get(:color)
  end

  def after_hook
    after_hook_log << attribute_get(:color)
  end

  def log; @log ||= [] end
  def before_hook_log; @bh_log ||= [] end
  def after_hook_log; @ah_log ||= [] end

  attr_reader :init
  def initialize(*args)
    (@init ||= []) << :init
    super
  end

end

TrafficLight.auto_migrate!

# ===== Note 1 =====
#
# One would expect that these two would be the same:
#   property :id, Serial
#   property :id, Integer, :serial => true
#
# But on 2008-07-05, the 2nd led to problems with an in-memory SQLite
# database.
