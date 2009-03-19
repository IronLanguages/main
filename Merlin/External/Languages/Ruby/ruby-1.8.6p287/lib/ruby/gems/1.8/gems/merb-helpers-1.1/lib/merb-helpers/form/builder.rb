load File.dirname(__FILE__) / ".." / "tag_helpers.rb"

module Merb::Helpers::Form::Builder

  class Base
    include Merb::Helpers::Tag

    def initialize(obj, name, origin)
      @obj, @origin = obj, origin
      @name = name || @obj.class.name.snake_case.split("/").last
    end

    def form(attrs = {}, &blk)
      captured = @origin.capture(&blk)
      fake_method_tag = process_form_attrs(attrs)
      tag(:form, fake_method_tag + captured, attrs)
    end

    def fieldset(attrs, &blk)
      legend = (l_attr = attrs.delete(:legend)) ? tag(:legend, l_attr) : ""
      tag(:fieldset, legend + @origin.capture(&blk), attrs)
      # @origin.concat(contents, blk.binding)
    end

    %w(text password hidden file).each do |kind|
      self.class_eval <<-RUBY, __FILE__, __LINE__ + 1
        def bound_#{kind}_field(method, attrs = {})
          name = control_name(method)
          update_bound_controls(method, attrs, "#{kind}")
          unbound_#{kind}_field({
            :name => name, 
            :value => control_value(method)
          }.merge(attrs))
        end

        def unbound_#{kind}_field(attrs)
          update_unbound_controls(attrs, "#{kind}")
          self_closing_tag(:input, {:type => "#{kind}"}.merge(attrs))
        end
      RUBY
    end

    def bound_check_box(method, attrs = {})
      name = control_name(method)
      update_bound_controls(method, attrs, "checkbox")
      unbound_check_box({:name => name}.merge(attrs))
    end

    def unbound_check_box(attrs)
      update_unbound_controls(attrs, "checkbox")
      if attrs.delete(:boolean)
        on, off = attrs.delete(:on), attrs.delete(:off)
        unbound_hidden_field(:name => attrs[:name], :value => off) <<
          self_closing_tag(:input, {:type => "checkbox", :value => on}.merge(attrs))
      else
        self_closing_tag(:input, {:type => "checkbox"}.merge(attrs))
      end
    end

    def bound_radio_button(method, attrs = {})
      name = control_name(method)
      update_bound_controls(method, attrs, "radio")
      unbound_radio_button({:name => name, :value => control_value(method)}.merge(attrs))
    end

    def unbound_radio_button(attrs)
      update_unbound_controls(attrs, "radio")
      self_closing_tag(:input, {:type => "radio"}.merge(attrs))
    end

    def bound_radio_group(method, arr)
      val = control_value(method)
      arr.map do |attrs|
        attrs = {:value => attrs} unless attrs.is_a?(Hash)
        attrs[:checked] = true if (val == attrs[:value])
        radio_group_item(method, attrs)
      end.join
    end

    def unbound_radio_group(arr, attrs = {})
      arr.map do |ind_attrs|
        ind_attrs = {:value => ind_attrs} unless ind_attrs.is_a?(Hash)
        joined = attrs.merge(ind_attrs)
        joined.merge!(:label => joined[:label] || joined[:value])
        unbound_radio_button(joined)
      end.join
    end

    def bound_select(method, attrs = {})
      name = control_name(method)
      update_bound_controls(method, attrs, "select")
      unbound_select({:name => name}.merge(attrs))
    end

    def unbound_select(attrs = {})
      update_unbound_controls(attrs, "select")
      attrs[:name] << "[]" if attrs[:multiple] && !(attrs[:name] =~ /\[\]$/)
      tag(:select, options_for(attrs), attrs)
    end

    def bound_text_area(method, attrs = {})
      name = "#{@name}[#{method}]"
      update_bound_controls(method, attrs, "text_area")
      unbound_text_area(control_value(method), {:name => name}.merge(attrs))
    end

    def unbound_text_area(contents, attrs)
      update_unbound_controls(attrs, "text_area")
      tag(:textarea, contents, attrs)
    end

    def button(contents, attrs)
      update_unbound_controls(attrs, "button")
      tag(:button, contents, attrs)
    end

    def submit(value, attrs)
      attrs[:type]  ||= "submit"
      attrs[:name]  ||= "submit"
      attrs[:value] ||= value
      update_unbound_controls(attrs, "submit")
      self_closing_tag(:input, attrs)
    end

    private

    def process_form_attrs(attrs)
      method = attrs[:method]

      # Unless the method is :get, fake out the method using :post
      attrs[:method] = :post unless attrs[:method] == :get
      # Use a fake PUT if the object is not new, otherwise use the method
      # passed in. Defaults to :post if no method is set.
      method ||= (@obj.respond_to?(:new_record?) && !@obj.new_record?) || (@obj.respond_to?(:new?) && !@obj.new?) ? :put : :post

      attrs[:enctype] = "multipart/form-data" if attrs.delete(:multipart) || @multipart

      method == :post || method == :get ? "" : fake_out_method(attrs, method)
    end

    # This can be overridden to use another method to fake out methods
    def fake_out_method(attrs, method)
      self_closing_tag(:input, :type => "hidden", :name => "_method", :value => method)
    end

    def update_bound_controls(method, attrs, type)
      case type
      when "checkbox"
        update_bound_check_box(method, attrs)
      when "select"
        update_bound_select(method, attrs)
      end
    end

    def update_bound_check_box(method, attrs)
      raise ArgumentError, ":value can't be used with a bound_check_box" if attrs.has_key?(:value)

      attrs[:boolean] = attrs.fetch(:boolean, true)

      val = control_value(method)
      attrs[:checked] = attrs.key?(:on) ? val == attrs[:on] : considered_true?(val)
    end

    def update_bound_select(method, attrs)
      attrs[:value_method] ||= method
      attrs[:text_method] ||= attrs[:value_method] || :to_s
      attrs[:selected] ||= control_value(attrs[:value_method])
    end

    def update_unbound_controls(attrs, type)
      case type
      when "checkbox"
        update_unbound_check_box(attrs)
      when "radio"
        update_unbound_radio_button(attrs)
      when "file"
        @multipart = true
      end

      attrs[:disabled] ? attrs[:disabled] = "disabled" : attrs.delete(:disabled)
    end

    def update_unbound_check_box(attrs)
      boolean = attrs[:boolean] || (attrs[:on] && attrs[:off]) ? true : false

      case
      when attrs.key?(:on) ^ attrs.key?(:off)
        raise ArgumentError, ":on and :off must be specified together"
      when (attrs[:boolean] == false) && (attrs.key?(:on))
        raise ArgumentError, ":boolean => false cannot be used with :on and :off"
      when boolean && attrs.key?(:value)
        raise ArgumentError, ":value can't be used with a boolean checkbox"
      end

      if attrs[:boolean] = boolean
        attrs[:on] ||= "1"; attrs[:off] ||= "0"
      end

      attrs[:checked] = "checked" if attrs.delete(:checked)
    end

    def update_unbound_radio_button(attrs)
      attrs[:checked] = "checked" if attrs.delete(:checked)
    end
    
    # Accepts a collection (hash, array, enumerable, your type) and returns a string of option tags. 
    # Given a collection where the elements respond to first and last (such as a two-element array), 
    # the "lasts" serve as option values and the "firsts" as option text. Hashes are turned into
    # this form automatically, so the keys become "firsts" and values become lasts. If selected is
    # specified, the matching "last" or element will get the selected option-tag. Selected may also
    # be an array of values to be selected when using a multiple select.
    #
    # ==== Parameters
    # attrs<Hash>:: HTML attributes and options
    #
    # ==== Options
    # +selected+:: The value of a selected object, which may be either a string or an array.
    # +prompt+:: Adds an addtional option tag with the provided string with no value.
    # +include_blank+:: Adds an additional blank option tag with no value.
    #
    # ==== Returns
    # String:: HTML
    #
    # ==== Examples
    #   <%= options_for [["apple", "Apple Pie"], ["orange", "Orange Juice"]], :selected => "orange"
    #   => <option value="apple">Apple Pie</option><option value="orange" selected="selected">Orange Juice</option>
    #
    #   <%= options_for [["apple", "Apple Pie"], ["orange", "Orange Juice"]], :selected => ["orange", "apple"], :prompt => "Select One"
    #   => <option value="">Select One</option><option value="apple" selected="selected">Apple Pie</option><option value="orange" selected="selected">Orange Juice</option>
    def options_for(attrs)
      blank, prompt = attrs.delete(:include_blank), attrs.delete(:prompt)
      b = blank || prompt ? tag(:option, prompt || "", :value => "") : ""

      # yank out the options attrs
      collection, selected, text_method, value_method = 
        attrs.extract!(:collection, :selected, :text_method, :value_method)

      # if the collection is a Hash, optgroups are a-coming
      if collection.is_a?(Hash)
        ([b] + collection.map do |g,col|
          tag(:optgroup, options(col, text_method, value_method, selected), :label => g)
        end).join
      else
        options(collection || [], text_method, value_method, selected, b)
      end
    end

    def options(col, text_meth, value_meth, sel, b = nil)
      ([b] + col.map do |item|
        text_meth = text_meth && item.respond_to?(text_meth) ? text_meth : :last
        value_meth = value_meth && item.respond_to?(value_meth) ? value_meth : :first

        text  = item.is_a?(String) ? item : item.send(text_meth)
        value = item.is_a?(String) ? item : item.send(value_meth)
        
        unless Merb.disabled?(:merb_helper_escaping)
          text  = Merb::Parse.escape_xml(text)
          value = Merb::Parse.escape_xml(value)
        end

        option_attrs = {:value => value}
        if sel.is_a?(Array)
          option_attrs.merge!(:selected => "selected") if value.in? sel
        else
          option_attrs.merge!(:selected => "selected") if value == sel
        end
        tag(:option, text, option_attrs)
      end).join
    end

    def radio_group_item(method, attrs)
      attrs.merge!(:checked => "checked") if attrs[:checked]
      bound_radio_button(method, attrs)
    end

    def considered_true?(value)
      value && value != "false" && value != "0" && value != 0
    end

    def control_name(method)
      @obj ? "#{@name}[#{method}]" : method
    end
    
    def control_value(method)
      value = @obj ? @obj.send(method) : @origin.params[method]
      if Merb.disabled?(:merb_helper_escaping)
        value.to_s
      else
        Merb::Parse.escape_xml(value.to_s)
      end
    end

    def add_css_class(attrs, new_class)
      attrs[:class] = attrs[:class] ? "#{attrs[:class]} #{new_class}" : new_class
    end
  end

  class Form < Base
    def label(contents, attrs = {})
      if contents
        if contents.is_a?(Hash)
          label_attrs = contents
          contents = label_attrs.delete(:title)
        else
          label_attrs = attrs
        end
        tag(:label, contents, label_attrs)
      else
        ""
      end
    end
    
    %w(text password file).each do |kind|
      self.class_eval <<-RUBY, __FILE__, __LINE__ + 1
        def unbound_#{kind}_field(attrs = {})
          unbound_label(attrs) + super
        end
      RUBY
    end
    
    def unbound_label(attrs = {})
      if attrs[:id]
        label_attrs = {:for => attrs[:id]}
      elsif attrs[:name]
        label_attrs = {:for => attrs[:name]}
      else
        label_attrs = {}
      end

      label_option = attrs.delete(:label)
      if label_option.is_a? Hash
        label(label_attrs.merge(label_option))
      else
        label(label_option, label_attrs)
      end
    end

    def unbound_check_box(attrs = {})
      label_text = unbound_label(attrs)
      super + label_text
    end

    def unbound_hidden_field(attrs = {})
      attrs.delete(:label)
      super
    end

    def unbound_radio_button(attrs = {})
      label_text = unbound_label(attrs)
      super + label_text
    end

    def unbound_select(attrs = {})
      unbound_label(attrs) + super
    end

    def unbound_text_area(contents, attrs = {})
      unbound_label(attrs) + super
    end

    def button(contents, attrs = {})
      unbound_label(attrs) + super
    end

    def submit(value, attrs = {})
      unbound_label(attrs) + super
    end

    private

    def update_bound_controls(method, attrs, type)
      attrs.merge!(:id => "#{@name}_#{method}") unless attrs[:id]
      super
    end

    def update_unbound_controls(attrs, type)
      attrs.merge!(:id => attrs[:name]) if attrs[:name] && !attrs[:id]

      case type
      when "text", "radio", "password", "hidden", "checkbox", "file"
        add_css_class(attrs, type)
      end
      super
    end

    def radio_group_item(method, attrs)
      unless attrs[:id]
        attrs.merge!(:id => "#{@name}_#{method}_#{attrs[:value]}")
      end

      attrs.merge!(:label => attrs[:label] || attrs[:value])
      super
    end
  end

  module Errorifier
    def error_messages_for(obj, error_class, build_li, header, before)
      obj ||= @obj
      return "" unless obj.respond_to?(:errors)

      sequel = !obj.errors.respond_to?(:each)
      errors = sequel ? obj.errors.full_messages : obj.errors

      return "" if errors.empty?

      header_message = header % [errors.size, errors.size == 1 ? "" : "s"]
      markup = %Q{<div class='#{error_class}'>#{header_message}<ul>}
      errors.each {|err| markup << (build_li % (sequel ? err : err.join(" ")))}
      markup << %Q{</ul></div>}
    end

    private

    def update_bound_controls(method, attrs, type)
      if @obj && !@obj.errors.on(method.to_sym).blank?
        add_css_class(attrs, "error")
      end
      super
    end
  end

  class FormWithErrors < Form
    include Errorifier
  end

  module Resourceful
    private

    def process_form_attrs(attrs)
      attrs[:action] ||= @origin.url(@name, @obj) if @origin
      super
    end
  end

  class ResourcefulForm < Form
    include Resourceful
  end

  class ResourcefulFormWithErrors < FormWithErrors
    include Errorifier
    include Resourceful
  end

end
