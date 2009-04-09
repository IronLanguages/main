module DataMapper
  module Is
    module StateMachine
      module Data

        class State

          attr_reader :name, :machine, :options

          def initialize(name, machine, options = {})
            @name    = name
            @options = options
            @machine = machine
          end

        end

      end # Data
    end # StateMachine
  end # Is
end # DataMapper
