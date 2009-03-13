module DataMapper
  module Is
    module StateMachine
      module Data

        class Event

          attr_reader :name, :machine, :transitions

          def initialize(name, machine)
            @name        = name
            @machine     = machine
            @transitions = []
          end

          def add_transition(from, to)
            @transitions << { :from => from, :to => to }
          end

        end

      end # Data
    end # StateMachine
  end # Is
end # DataMapper
