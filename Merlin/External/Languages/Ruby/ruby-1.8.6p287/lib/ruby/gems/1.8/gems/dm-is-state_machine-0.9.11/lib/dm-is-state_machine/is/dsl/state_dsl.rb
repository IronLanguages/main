module DataMapper
  module Is
    module StateMachine
      # State DSL (Domain Specific Language)
      module StateDsl

        # Define a state of the system.
        #
        # Example:
        #
        #   class TrafficLight
        #     include DataMapper::Resource
        #     property :id, Serial
        #     is :state_machine do
        #       state :green,  :enter => Proc.new { |o| o.log("G") }
        #       state :yellow, :enter => Proc.new { |o| o.log("Y") }
        #       state :red,    :enter => Proc.new { |o| o.log("R") }
        #
        #       # event definitions go here...
        #     end
        #
        #     def log(string)
        #       Merb::Logger.info(string)
        #     end
        #   end
        def state(name, options = {})
          unless state_machine_context?(:is)
            raise InvalidContext, "Valid only in 'is :state_machine' block"
          end

          # ===== Setup context =====
          machine = @is_state_machine[:machine]
          state = Data::State.new(name, machine, options)
          machine.states << state
        end

      end # StateDsl
    end # StateMachine
  end # Is
end # DataMapper
