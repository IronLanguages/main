# dm-is-state_machine #

DataMapper plugin that adds state machine functionality to your models.

## Why is this plugin useful? ##

Your DataMapper resource might benefit from a state machine if it:

* has different "modes" of operation
* has discrete behaviors
* especially if the behaviors are mutually exclusive

And you want a clean, high-level way of describing these modes / behaviors
and how the resource moves between them.  This plugin allows you to
declaratively describe the states and transitions involved.

## Installation ##

1. Download dm-more.
2. Install dm-is-state_machine using the supplied rake files.

## Setting up with Merb ##

Add this line to your init.rb:

    dependency "dm-is-state_machine"

## Example DataMapper resource (i.e. model) ##

    # /app/models/traffic_light.rb
    class TrafficLight
      include DataMapper::Resource

      property :id, Serial

      is :state_machine, :initial => :green, :column => :color do
        state :green
        state :yellow
        state :red,    :enter => :red_hook
        state :broken

        event :forward do
          transition :from => :green,  :to => :yellow
          transition :from => :yellow, :to => :red
          transition :from => :red,    :to => :green
        end
      end
      
      def red_hook
        # Do something
      end
    end

## What this gives you ##

### Explained in words ###

The above DSL (domain specific language) does these things "behind the scenes":

1. Defines a DataMapper property called 'color'.

2. Makes the current state available by using 'traffic_light.color'.

3. Defines the 'forward!' transition method.  This method triggers the
   appropriate transition based on the current state and comparing it against
   the various :from states.  It will raise an error if you attempt to call
   it with an invalid state (such as :broken, see above).  After the method
   runs successfully, the state machine will be left in the :to state.  
    
### Explained with some code examples ###

    # Somewhere in your controller, perhaps
    light = TrafficLight.new
    
    # Move to the next state
    light.forward!
    
    # Do something based on the current state
    case light.color
    when "green"
      # do something green-related
    when "yellow"
      # do something yellow-related
    when "red"
      # do something red-related
    end
    
## Specific examples ##

We would also like to hear how *you* are using state machines in your code.

## See also ##

Here are some other projects you might want to look at.  Most of them
are probably intended for ActiveRecord.  They take different approaches,
which is pretty interesting.  If you find something you like in these other
projects, let us know.  Maybe we can incorporate some of your favorite parts.
That said, I do not want to create a Frankenstein. :)

* http://github.com/pluginaweek/state_machine/tree/master
* http://github.com/davidlee/stateful/tree/master
* http://github.com/sbfaulkner/has_states/tree/master
