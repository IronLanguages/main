$:.push File.join(File.dirname(__FILE__), '..', 'lib')

require 'rubygems'
require 'merb-gen'
require 'merb-slices'

require 'generators/base'
require 'generators/full'
require 'generators/thin'
require 'generators/very_thin'

module Merb
  module Test
    
    class SampleAppController < Merb::Controller
      
      include Merb::Slices::Support
      
      def index
      end
      
    end
    
    module SliceHelper
      
      def current_slice_root=(path)
        @current_slice_root = File.expand_path(path)
      end
      
      # The absolute path to the current slice
      def current_slice_root
        @current_slice_root || File.expand_path(File.dirname(__FILE__))
      end
      
    end
  end
end

Spec::Runner.configure do |config|
  config.include(Merb::Test::ViewHelper)
  config.include(Merb::Test::RouteHelper)
  config.include(Merb::Test::ControllerHelper)
  config.include(Merb::Test::SliceHelper)
end