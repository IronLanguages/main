module DataMapper
  module Is
    module StateMachine
      module Data

        # This Machine class represents one state machine.
        #
        # A model (i.e. a DataMapper resource) can have more than one Machine.
        class Machine

          # The property of the DM resource that will hold this Machine's
          # state.
          #
          # TODO: change :column to :property
          attr_accessor :column

          # The initial value of this Machine's state
          attr_accessor :initial

          # The current value of this Machine's state
          #
          # This is the "primary control" of this Machine's state.  All
          # other methods key off the value of @current_state_name.
          attr_accessor :current_state_name

          attr_accessor :events

          attr_accessor :states

          def initialize(column, initial)
            @column, @initial   = column, initial
            @events, @states    = [], []
            @current_state_name = initial
          end

          # Fire (activate) the event with name +event_name+
          #
          # @api public
          def fire_event(event_name, resource)
            unless event = find_event(event_name)
              raise InvalidEvent, "Could not find event (#{event_name.inspect})"
            end
            transition = event.transitions.find do |t|
               t[:from].to_s == @current_state_name.to_s
            end
            unless transition
              raise InvalidEvent, "Event (#{event_name.inspect}) does not" +
              "exist for current state (#{@current_state_name.inspect})"
            end

            # == Run :exit hook (if present) ==
            resource.run_hook_if_present current_state.options[:exit]

            # == Change the current_state ==
            @current_state_name = transition[:to]

            # == Run :enter hook (if present) ==
            resource.run_hook_if_present current_state.options[:enter]
          end

          # Return the current state
          #
          # @api public
          def current_state
            find_state(@current_state_name)
            # TODO: add caching, i.e. with `@current_state ||= ...`
          end

          # Find event whose name is +event_name+
          #
          # @api semipublic
          def find_event(event_name)
            @events.find { |event| event.name.to_s == event_name.to_s }
            # TODO: use a data structure that prevents duplicates
          end

          # Find state whose name is +event_name+
          #
          # @api semipublic
          def find_state(state_name)
            @states.find { |state| state.name.to_s == state_name.to_s }
            # TODO: use a data structure that prevents duplicates
          end

        end

      end # Data
    end # StateMachine
  end # Is
end # DataMapper
