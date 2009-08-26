# This plugin exposes two new controller methods which allow us to simply and flexibly filter the parameters available within the controller.

# Setup:
# The request sets:
# params => { :post => { :title => "ello", :body => "Want it", :status => "green", :author_id => 3, :rank => 4 } }
#
# Example 1: params_accessable
# MyController < Application
#   params_accessible :post => [:title, :body]
# end

# params.inspect # => { :post => { :title => "ello", :body => "Want it" } }

# So we see that params_accessible removes everything except what is explictly specified.

# Example 2: params_protected
# MyOtherController < Application
#   params_protected :post => [:status, :author_id]
# end

# params.inspect # => { :post => { :title => "ello", :body => "Want it", :rank => 4 } }

# We also see that params_protected removes ONLY those parameters explicitly specified.

if defined?(Merb::Plugins)

  # Merb gives you a Merb::Plugins.config hash...feel free to put your stuff in your piece of it
  #Merb::Plugins.config[:merb_param_protection] = {
    #:chickens => false
  #}

  #Merb::Plugins.add_rakefiles "merb_param_protection/merbtasks"

  module Merb
    module ParamsFilter
      module ControllerMixin
        def self.included(base)
          base.send(:extend, ClassMethods)
          base.send(:include, InstanceMethods)
          base.send(:class_inheritable_accessor, :accessible_params_args)
          base.send(:class_inheritable_accessor, :protected_params_args)
          base.send(:class_inheritable_accessor, :log_params_args)
          # Don't expose these as public methods - otherwise they'll become controller actions
          base.send(:protected, :accessible_params_args, :protected_params_args, :log_params_args)
          base.send(:protected, :accessible_params_args=, :protected_params_args=, :log_params_args=)

          base.send(:before, :initialize_params_filter)
        end

        module ClassMethods
          # Ensures these parameters are sent for the object
          #
          #   params_accessible :post => [:title, :body]
          #
          def params_accessible(args = {})
            assign_filtered_params(:accessible_params_args, args)
          end

          # Protects parameters of an object
          #
          #   params_protected :post => [:status, :author_id]
          #
          def params_protected(args = {})
            assign_filtered_params(:protected_params_args, args)
          end

          # Filters parameters out from the default log string
          #  Params will still be passed to the controller properly, they will
          #  show up as [FILTERED] in the merb logs.
          #
          # log_params_filtered :password, 'token'
          #
          def log_params_filtered(*args)
            self.log_params_args = args.collect { |arg| arg.to_sym }
          end

          private

          def assign_filtered_params(method, args)
            validate_filtered_params(method, args)

            # If the method is nil, set to initial hash, otherwise merge
            self.send(method).nil? ? self.send(method.to_s + '=', args) : self.send(method).merge!(args)
          end

          def validate_filtered_params(method, args)
            # Reversing methods
            params_methods = [:accessible_params_args, :protected_params_args]
            params_methods.delete(method)
            params_method = params_methods.first

            # Make sure the opposite method is not nil
            unless self.send(params_method).nil?
              # Loop through arg's keys
              args.keys.each do |key|
                # If the key exists on the opposite method, raise exception
                if self.send(params_method).include?(key)
                  case method
                  when :accessible_params_args : raise "Cannot make accessible a controller (#{self}) that is already protected"
                  when :protected_params_args : raise "Cannot protect controller (#{self}) that is already accessible"
                  end
                end
              end
            end
          end
        end

        module InstanceMethods
          def initialize_params_filter
            if accessible_params_args.is_a?(Hash)
              accessible_params_args.keys.each do |obj|
                self.request.restrict_params(obj, accessible_params_args[obj])
              end
            end

            if protected_params_args.is_a?(Hash)
              protected_params_args.keys.each do |obj|
                self.request.remove_params_from_object(obj, protected_params_args[obj])
              end
            end
          end
        end

      end

      module RequestMixin
        attr_accessor :trashed_params

        # Removes specified parameters of an object
        #
        #   remove_params_from_object(:post, [:status, :author_id])
        #
        def remove_params_from_object(obj, attrs = [])
          unless params[obj].nil?
            filtered = params
            attrs.each {|a| filtered[obj].delete(a)}
            @params = filtered
          end
        end

        # Restricts parameters of an object
        #
        #   restrict_params(:post, [:title, :body])
        #
        def restrict_params(obj, attrs = [])
          # Make sure the params for the object exists
          unless params[obj].nil?
            attrs = attrs.collect {|a| a.to_s}
            trashed_params_keys = params[obj].keys - attrs

            # Store a hash of the key/value pairs we are going
            # to remove in case we need them later.  Lighthouse Bug # 105
            @trashed_params = {}
            trashed_params_keys.each do |key|
              @trashed_params.merge!({key => params[obj][key]})
            end

            remove_params_from_object(obj, trashed_params_keys)
          end
        end

      end
    end
  end

  Merb::Controller.send(:include, Merb::ParamsFilter::ControllerMixin)
  Merb::Request.send(:include, Merb::ParamsFilter::RequestMixin)

  class Merb::Controller
    def self._filter_params(params)
      return params if self.log_params_args.nil?
      result = { }
      params.each do |k,v|
        result[k] = (self.log_params_args.include?(k.to_sym) ? '[FILTERED]' : v)
      end
      result
    end
  end
end
