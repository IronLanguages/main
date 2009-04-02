module Merb::Helpers::Text
  # Allows you to cycle through elements in an array
  #
  # ==== Parameters
  # values<Array>:: Array of objects to cycle through
  # values<Hash>:: Last element of array can be a hash with the key of
  #                :name to specify the name of the cycle
  #
  # ==== Returns
  # String
  #
  # ==== Notes
  # * Default name is :default
  #
  # ==== Example
  # <%= 5.times { cycle("odd! ","even! "} %>
  #
  # Generates:
  #
  # odd! even! odd! even! odd!
  def cycle(*values)
    options = extract_options_from_args!(values) || {}
    key = (options[:name] || :default).to_sym
    (@cycle_positions ||= {})[key] ||= {:position => -1, :values => values}
    unless values == @cycle_positions[key][:values]
      @cycle_positions[key] = {:position => -1, :values => values}
    end
    current = @cycle_positions[key][:position]
    @cycle_positions[key][:position] = current + 1
    values.at( (current + 1) % values.length).to_s
  end

  # Allows you to reset a cycle
  #
  # ==== Parameters
  # name<Symbol|String>:: Name of the cycle
  #
  # ==== Returns
  # True if successful, otherwise nil
  #
  # ==== Notes
  # * Default name is :default
  #
  # ==== Example
  # <%= cycle("odd! ","even! ","what comes after even?") %>
  # <%= cycle("odd! ","even! ","what comes after even?") %>
  # <% reset_cycle %>
  # <%= cycle("odd! ","even! ","what comes after even?") %>
  #
  # Generates:
  #
  # odd! even! odd!
  def reset_cycle(name = :default)
    (@cycle_positions[name.to_sym] = nil) &&
      true if @cycle_positions && @cycle_positions[name.to_sym]
  end
end

module Merb::GlobalHelpers
  include Merb::Helpers::Text
end
