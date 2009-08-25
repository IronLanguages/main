$:.unshift File.join(File.dirname(__FILE__), '..', 'lib')
require "rubygems"
require "merb-core"
require "spec"
require "merb-exceptions"

class Application < Merb::Controller
end

class RaiseError < Application
  def index
    raise StandardError, 'Something went wrong'
  end
end

class NotFound < Application
end

class Exceptions < Application
  after :notify_of_exceptions, :only => :not_found

  def not_found
    render '404 not found'
  end
end

Merb::Plugins.config[:exceptions] = {
  :email_addresses => ['user1@test.com', 'user2@test.com'],
  :web_hooks => ['http://www.test1.com', 'http://www.test2.com'],
  :environments    => ['test'],
  :mailer_delivery_method => :test_send
}
Merb.start :environment => 'test'

module Merb
  module Test
    module RspecMatchers
      class IncludeLog
        def initialize(expected)
          @expected = expected
        end

        def matches?(target)
          target.rewind
          @text = target.read
          @text =~ (String === @expected ? /#{Regexp.escape @expected}/ : @expected)
        end

        def failure_message
          "expected to find `#{@expected}' in the log but got:\n" <<
          @text.map {|s| "  #{s}" }.join
        end

        def negative_failure_message
          "exected not to find `#{@expected}' in the log but got:\n" <<
          @text.map {|s| "  #{s}" }.join
        end

        def description
          "include #{@text} in the log"
        end
      end

      def include_log(expected)
        IncludeLog.new(expected)
      end
    end
  end
end

Spec::Runner.configure do |config|
  config.include Merb::Test::ControllerHelper
  config.include Merb::Test::RequestHelper
  config.include Merb::Test::RouteHelper
  config.include Merb::Test::RspecMatchers

  def with_level(level)
    Merb::Config[:log_stream] = StringIO.new
    Merb::Config[:log_level] = level
    Merb.reset_logger!
    yield
    Merb::Config[:log_stream]
  end
end

module NotificationSpecHelper
  def mock_details(opts={})
    {
      'exceptions'      => [],
      'params'         => { :controller=>'errors', :action=>'show' },
      'environment'    => { 'key1'=>'value1', 'key2'=>'value2' },
      'url'            => 'http://www.my-app.com/errors/1'
    }.merge(opts)
  end
end