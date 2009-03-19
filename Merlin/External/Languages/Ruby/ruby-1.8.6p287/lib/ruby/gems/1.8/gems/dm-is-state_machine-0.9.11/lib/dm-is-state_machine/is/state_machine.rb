module DataMapper
  module Is
    module StateMachine

      class InvalidContext  < RuntimeError; end
      class InvalidState    < RuntimeError; end
      class InvalidEvent    < RuntimeError; end
      class EventConfusion  < RuntimeError; end
      class DuplicateStates < RuntimeError; end
      class NoInitialState  < RuntimeError; end

      ##
      # Makes a column ('state' by default) act as a state machine. It will
      # define the property if it does not exist.
      #
      # @example [Usage]
      #   is :state_machine
      #   is :state_machine, :initial => :internal
      #   is :state_machine, :column => :availability
      #   is :state_machine, :column => :availability, :initial => :external
      #
      # @param options<Hash> a hash of options
      #
      # @option :column<Symbol> the name of the custom column
      #
      def is_state_machine(options = {}, &block)
        extend DataMapper::Is::StateMachine::EventDsl
        extend DataMapper::Is::StateMachine::StateDsl
        include DataMapper::Is::StateMachine::InstanceMethods

        # ===== Setup context =====
        options = { :column => :state, :initial => nil }.merge(options)
        column  = options[:column]
        initial = options[:initial].to_s
        unless properties.detect { |p| p.name == column }
          property column, String, :default => initial
        end
        machine = Data::Machine.new(column, initial)
        @is_state_machine = { :machine => machine }

        # ===== Define callbacks =====
        # TODO: define callbacks
        # before :save do
        #   if self.new_record?
        #     # ...
        #   else
        #     # ...
        #   end
        # end

        before :destroy do
          # Do we need to do anything here?
        end

        # ===== Setup context =====
        push_state_machine_context(:is)

        yield if block_given?

        # ===== Teardown context =====
        pop_state_machine_context
      end

      protected

      def push_state_machine_context(label)
        @is_state_machine ||= {}
        @is_state_machine[:context] ||= []
        @is_state_machine[:context] << label

        # Compacted, but barely readable for humans
        # ((@is_state_machine ||= {})[:context] ||= []) << label
      end

      def pop_state_machine_context
        @is_state_machine[:context].pop
      end

      def state_machine_context?(label)
        (i = @is_state_machine) && (c = i[:context]) &&
        c.respond_to?(:include?) && c.include?(label)
      end

      module InstanceMethods

        def initialize(*args)
          super
          # ===== Run :enter hook if present =====
          return unless is_sm = self.class.instance_variable_get(:@is_state_machine)
          return unless machine = is_sm[:machine]
          return unless initial = machine.initial
          return unless initial_state = machine.find_state(initial)
          run_hook_if_present initial_state.options[:enter]
        end

        # hook may be either a Proc or symbol
        def run_hook_if_present(hook)
          return unless hook
          if hook.respond_to?(:call)
            hook.call(self)
          else
            self.send(hook)
          end
        end

        def transition!(event_name)
          machine = self.class.instance_variable_get(:@is_state_machine)[:machine]
          column = machine.column
          machine.current_state_name = attribute_get(:"#{column}")
          machine.fire_event(event_name, self)
          attribute_set(:"#{column}", machine.current_state_name)
        end

      end # InstanceMethods

    end # StateMachine
  end # Is
end # DataMapper

# Notes
# -----
#
# Since this gets mixed into a class, I try to keep the namespace pollution
# down to a minimum.  This is why I only use the @is_state_machine instance
# variable.
