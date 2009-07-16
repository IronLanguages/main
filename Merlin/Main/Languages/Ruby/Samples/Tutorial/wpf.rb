# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************

SILVERLIGHT = begin; System::Windows::Browser; true; rescue; false; end

if not SILVERLIGHT
  # Reference the WPF assemblies
  require 'system.xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' 
  require 'PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
  require 'PresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
  require 'windowsbase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
else
  require 'System.Xml, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e'
end

class System::Windows::FrameworkElement
  # Monkey-patch FrameworkElement to allow window.ChildName instead of window.FindName("ChildName")
  # TODO - Make window.child_name work as well
  def method_missing name, *args
    find_name(name.to_s.to_clr_string) || super
  end

  def hide!
    self.visibility = System::Windows::Visibility.hidden
  end

  def collapse!
    self.visibility = System::Windows::Visibility.collapsed
  end

  def show!
    self.visibility = System::Windows::Visibility.visible
  end

  def set_or_collapse(property, value)
    obj = send(property)
    if obj && value
      yield obj, value
      obj.show!
    else
      obj.collapse!
    end
  end
  
  def content=(value)
    send "#{respond_to?(:Content) ? :Content : :Text}=", value
  end
end

class System::Windows::Controls::RichTextBox
  def document=(value)
    self.Document = value.kind_of?(String) ? FlowDocument.from_simple_markup(value || '') : value
  end
end

class System::Windows::Controls::TextBox
  alias document= Text=
end

class System::Windows::Controls::TextBlock
  alias document= Text=
end

if SILVERLIGHT
  class System::Windows::Controls::ScrollViewer
    def scroll_to_top
      scroll_to_vertical_offset(0)
    end

    def scroll_to_bottom
      scroll_to_vertical_offset(actual_height)
    end
  end
end

class System::Windows::Markup::XamlReader
  class << self
    alias raw_load load unless method_defined? :raw_load
  end

  def self.load(xaml)
    obj = if SILVERLIGHT
      self.Load(xaml)
    else
      return raw_load(xaml) unless xaml.respond_to? :to_clr_string
    
      self.Load(
        System::Xml::XmlReader.create(
          System::IO::StringReader.new(xaml.to_clr_string)))
    end
    yield obj if block_given?
    obj
  end

  def self.erb_load(xaml, b, &block)
    require 'erb'
    self.load(ERB.new(xaml).result(b).to_s, &block)
  end
end

class Module
  def delegate_methods(methods, opts = {})
    raise "methods should be an array" unless methods.kind_of?(Array)
    this = self
    opts[:to]      ||= self
    opts[:prepend]   = opts[:prepend] ? "#{opts[:prepend]}_" : ''
    opts[:append]    = if opts[:append]
                         append = opts[:append]
                         lambda{|this| "_#{this::send(append)}" }
                       else
                         lambda{|this| '' }
                       end

    methods.each do |method|
      define_method(method.to_s.to_sym) do
        send(opts[:to]).send "#{opts[:prepend]}#{method}#{opts[:append][self]}"
      end
    end
  end
end

class System::Windows::Threading::DispatcherObject
  def invoke &block
    require "system.core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
    dispatch_callback = System::Action[].new block
    self.dispatcher.invoke(System::Windows::Threading::DispatcherPriority.Normal, dispatch_callback)
  end
end

class System::Windows::Documents::FlowDocument
  def <<(text)
    paragraph = System::Windows::Documents::Paragraph.new
    paragraph.inlines.add(System::Windows::Documents::Run.new(text))
    self.blocks.add paragraph
  end
  
  # Converts text in RDoc simple markup format to a WPF FlowDocument object
  def self.from_simple_markup text

    return text if SILVERLIGHT

    require 'rdoc/markup/simple_markup'
    require 'rdoc/markup/simple_markup/inline'

    # TODO - This is a workaround for http://ironruby.codeplex.com/WorkItem/View.aspx?WorkItemId=1301
    text = "#{$1}dummy\n\n#{text}" if text =~ /\A(\s+)/
    
    if not @markupParser
      @markupParser = SM::SimpleMarkup.new
      
      # external hyperlinks
      @markupParser.add_special(/((link:|https?:|mailto:|ftp:|www\.)\S+\w)/, :HYPERLINK)

      # and links of the form  <text>[<url>]
      @markupParser.add_special(/(((\{.*?\})|\b\S+?)\[\S+?\.\S+?\])/, :TIDYLINK)
      # @markupParser.add_special(/\b(\S+?\[\S+?\.\S+?\])/, :TIDYLINK)
    end
    
    begin
      @markupParser.convert(text, Wpf::ToFlowDocument.new)
    rescue Exception => e
      puts "Error while converting:\n#{text}"
      raise e
    end
  end
end

module Wpf
  include System::Windows
  include System::Windows::Documents
  include System::Windows::Controls
  include System::Windows::Input
  include System::Windows::Markup
  include System::Windows::Media


  def self.load_xaml_file(filename)
    f = System::IO::FileStream.new filename, System::IO::FileMode.open, System::IO::FileAccess.read
    begin
      element = XamlReader.load f
    ensure
      f.close
    end
    element
  end
  
  # Returns an array with all the children, or invokes the block (if given) for each child
  # Note that it also includes content (which could be just strings).
  def self.walk(tree, &b)
    if not block_given?
      result = []
      walk(tree) { |child| result << child }
      return result
    end

    yield tree

    if tree.respond_to? :Children
      tree.Children.each { |child| walk child, &b }
    elsif tree.respond_to? :Child
      walk tree.Child, &b
    elsif tree.respond_to? :Content
      walk tree.Content, &b
    end
  end

  # If you constructed your treeview with XAML, you should
  # use this XAML snippet instead to auto-expand items:
  #
  # <TreeView.ItemContainerStyle>
  #   <Style>
  #     <Setter Property="TreeViewItem.IsExpanded" Value="True"/>
  #     <Style.Triggers>
  #       <DataTrigger Binding="{Binding Type}" Value="menu">
  #         <Setter Property="TreeViewItem.IsSelected" Value="True"/>
  #       </DataTrigger>
  #     </Style.Triggers>
  #   </Style>
  # </TreeView.ItemContainerStyle>
  #
  # If your treeview was constructed with code, use this method
  def self.select_tree_view_item(tree_view, item)
    return false unless self and item

    childNode = tree_view.ItemContainerGenerator.ContainerFromItem item
    if childNode
      childNode.focus
      childNode.IsSelected = true
      # TODO - BringIntoView ?
      return true
    end

    if tree_view.Items.Count > 0
      tree_view.Items.each do |childItem|
        childControl = tree_view.ItemContainerGenerator.ContainerFromItem(childItem)
        return false if not childControl

        # If tree node is not loaded, its sub-nodes will be nil. Force them to be loaded
        old_is_expanded = childControl.is_expanded
        childControl.is_expanded = true
        childControl.update_layout

        if select_tree_view_item childControl, item
          return true
        else
          childControl.is_expanded = old_is_expanded
        end
      end
    end

    false
  end

  def self.create_sta_thread &block
    ts = System::Threading::ThreadStart.new &block

    # Workaround for http://ironruby.codeplex.com/WorkItem/View.aspx?WorkItemId=1306
    param_types = System::Array[System::Type].new(1) { |i| System::Threading::ThreadStart.to_clr_type }
    ctor = System::Threading::Thread.to_clr_type.get_constructor param_types    
    t = ctor.Invoke(System::Array[Object].new(1) { ts })
    t.ApartmentState = System::Threading::ApartmentState.STA
    t.Start
  end

  # Some setup is needed to use WPF from an interactive session console (like iirb). This is because
  # WPF needs to do message pumping on a thread, iirb also requires a thread to read user input,
  # and all commands that interact with UI need to be executed on the message pump thread.
  def self.interact
    raise NotImplementedError, "Wpf.interact is not implemented yet"
    def CallBack(function, priority = DispatcherPriority.Normal)
      Application.Current.Dispatcher.BeginInvoke(priority, System::Action[].new(function))
    end
    
    def CallBack1(function, arg0, priority = DispatcherPriority.Normal)
       Application.Current.Dispatcher.BeginInvoke(priority, System::Action[arg0.class].new(function), arg0)
    end
    
    dispatcher = nil    
    message_pump_started = System::Threading::AutoResetEvent.new false

    create_sta_thread do
      app = Application.new
      app.startup do
        dispatcher = Dispatcher.FromThread System::Threading::Thread.current_thread
        message_pump_started.set
      end
      begin
        app.run
      ensure
        IronRuby::Ruby.SetCommandDispatcher(None) # This is a non-existent method that will need to be implemented
      end
    end
    
    message_pump_started.wait_one
    
    def dispatch_console_command(console_command)
      if console_command
        dispatcher.invoke DispatcherPriority.Normal, console_command
      end
    end
    
    IronRuby::Ruby.SetCommandDispatcher dispatch_console_command # This is a non-existent method that will need to be implemented
  end

  class ToFlowDocument
    include System::Windows
    include System::Windows::Documents

    def start_accepting
      @@bold_mask = SM::Attribute.bitmap_for :BOLD
      @@italics_mask = SM::Attribute.bitmap_for :EM
      @@tt_mask = SM::Attribute.bitmap_for :TT
      @@hyperlink_mask = SM::Attribute.bitmap_for :HYPERLINK
      @@tidylink_mask = SM::Attribute.bitmap_for :TIDYLINK

      @flowDoc = FlowDocument.new
      @attributes = []
    end
    
    def end_accepting
      @flowDoc
    end

    def accept_paragraph(am, fragment)
      paragraph = convert_flow(am.flow(fragment.txt))
      @flowDoc.blocks.add paragraph
    end

    def convert_flow(flow)
      paragraph = Paragraph.new
      active_attribute = nil

      flow.each do |item|
        case item
        when String
          case active_attribute
          when @@bold_mask
            paragraph.inlines.add(Bold.new(Run.new(item)))
            @attributes.clear
          when @@italics_mask
            paragraph.inlines.add(Italic.new(Run.new(item)))
          when @@tt_mask
              run = Run.new(item)
              run.font_family = FontFamily.new "Consolas"
              run.font_weight = FontWeights.Bold
              paragraph.inlines.add(run)
          when nil
            paragraph.inlines.add(Run.new(item))
          else
            raise "unexpected active_attribute: #{active_attribute}"
          end
            
        when SM::AttrChanger
          on_mask = item.turn_on
          active_attribute = on_mask if not on_mask.zero?
          off_mask = item.turn_off
          if not off_mask.zero?
            raise NotImplementedError.new("mismatched attribute #{SM::Attribute.as_string(off_mask)} with active_attribute=#{SM::Attribute.as_string(active_attribute)}") if off_mask != active_attribute
            active_attribute = nil
          end

        when SM::Special
          convert_special(item, paragraph)

        else
          raise "Unknown flow element: #{item.inspect}"
        end
      end
    
      raise "mismatch" if active_attribute
      
      paragraph
    end

    def accept_verbatim(am, fragment)
        paragraph = Paragraph.new
        paragraph.font_family = FontFamily.new "Consolas"
        paragraph.font_weight = FontWeights.Bold
        paragraph.inlines.add(Run.new(fragment.txt))
        @flowDoc.blocks.add paragraph
    end

    def accept_list_start(am, fragment)
      @list = System::Windows::Documents::List.new
    end

    def accept_list_end(am, fragment)
      @flowDoc.blocks.add @list
    end

    def accept_list_item(am, fragment)
      paragraph = convert_flow(am.flow(fragment.txt))
      list_item = ListItem.new paragraph
      @list.list_items.add list_item
    end

    def accept_blank_line(am, fragment)
    end

    def accept_rule(am, fragment)
      raise NotImplementedError, "accept_rule: #{fragment.to_s}"
    end
    
    def convert_special(special, paragraph)
      handled = false
      SM::Attribute.each_name_of(special.type) do |name|
        method_name = "handle_special_#{name}"
        return send(method_name, special, paragraph) if self.respond_to? method_name
      end
      raise "Unhandled special: #{special}"
    end

    def handle_special_HYPERLINK(special, paragraph)
      paragraph.inlines.add(Hyperlink.new(Run.new(special.text)))
    end

    def handle_special_TIDYLINK(special, paragraph)
      text = special.text
      # text =~ /(\S+)\[(.*?)\]/
      unless text =~ /\{(.*?)\}\[(.*?)\]/ or text =~ /(\S+)\[(.*?)\]/ 
        handle_special_HYPERLINK(special, paragraph)
        return
      end

      label = $1
      url   = $2

      hyperlink = Hyperlink.new(Run.new(label))
      hyperlink.NavigateUri = System::Uri.new url
      paragraph.inlines.add(hyperlink)
    end
  end
end

