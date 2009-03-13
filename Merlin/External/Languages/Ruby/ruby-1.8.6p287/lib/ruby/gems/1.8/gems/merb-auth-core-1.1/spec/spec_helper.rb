$TESTING=true
$:.push File.join(File.dirname(__FILE__), '..', 'lib')

require 'rubygems'
require 'merb-core'
require 'merb-core/test'
require 'merb-core/dispatch/session'
require 'spec' # Satisfies Autotest and anyone else not using the Rake tasks
require 'merb-auth-core'

Merb.start  :environment    => "test", 
            :adapter        => "runner", 
            :session_store  => "cookie", 
            :session_secret_key => "d3a6e6f99a25004da82b71af8b9ed0ab71d3ea21"

module StrategyHelper
  def clear_strategies!
    Merb::Authentication.strategies.each do |s|
      begin
        Object.class_eval{ remove_const(s.name) if defined?(s)}
      rescue
      end
    end
    Merb::Authentication.strategies.clear
    Merb::Authentication.default_strategy_order.clear
  end
end

Spec::Runner.configure do |config|
  config.include(Merb::Test::ViewHelper)
  config.include(Merb::Test::RouteHelper)
  config.include(Merb::Test::ControllerHelper)
  config.include(StrategyHelper)
end

class Exceptions < Merb::Controller
  def unauthenticated
    session.abandon!
    "Login please"
  end
end

class User
  attr_accessor :name, :age, :id
  
  def initialize(opts = {})
    @name = opts.fetch(:name, "NAME")
    @age  = opts.fetch(:age,  42)
    @id   = opts.fetch(:id,   24)
  end
end

class Users < Application
  before :ensure_authenticated
  
  def index
    "You Made It!"
  end
end

class Dingbats < Application
  skip_before :ensure_authenticated
  def index
    "You Made It!"
  end
end

class Merb::Authentication
  def fetch_user(id = 24)
    if id.nil?
      nil
    else
      u = User.new(:id => id)
    end
  end
  
  def store_user(user)
    user.nil? ? nil : 24
  end
end

Merb::Authentication.user_class = User

class Viking
  def self.captures
    @captures ||= []
  end
  
  def self.capture(klass)
    @captures ||= []
    case klass
    when Class
      @captures << klass.name
    else
      @captures << klass
    end
  end
end
