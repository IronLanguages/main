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

require "tutorial"
require "stringio"
require 'wpf'

begin
  require 'c:/dev/repl'
rescue LoadError
  # get repl.rb: http://gist.github.com/116393
end

class System::Windows::Window
  def repl_input
    current_step.send "step_repl_input_#{_current_task}"
  end

  def repl_input_arrow
    current_step.send "step_repl_input_arrow_#{_current_task}"
  end

  def repl_history
    current_step.send "step_repl_history_#{_current_task}"
  end

  def reset_task
    @current_task = nil
  end

  def incr_task
    @current_task ||= -1
    @current_task += 1
    load_current_step
  end

  def _current_task
    @current_task || 0
  end

  def current_step
    send "step_#{_current_task}"
  end

  def step_title
    current_step.send "step_title_#{_current_task}"
  end

  def step_description
    current_step.send "step_description_#{_current_task}"
  end

  def load_current_step
    name = "step_#{_current_task}"
    unless respond_to?(name)
      require 'erb'
      vars = :step_id, :step_title_id, :step_description_id, :step_wrapper_id, :step_repl_id, :step_repl_history_id, :step_repl_input_id, :step_repl_input_arrow_id
      Step = Struct.new *vars unless defined?(Step)
      cstep = Step.new *(vars.map{|v| "#{v.to_s.split("_id")[0]}_#{_current_task}"})
      step_xaml = ERB.new(WpfTutorial.__send__(:class_variable_get, :"@@step")).
                      result(cstep.instance_eval{binding})

      step_obj = System::Windows::Markup::XamlReader.load(
        System::Xml::XmlReader.create(
          System::IO::StringReader.new(step_xaml)))
      body.children.add step_obj

      instance_variable_set(:"@#{name}", step_obj)
      self.class.instance_eval { attr_reader :"#{name}" }
    end
  end

  def start_loading
    self.loading.visibility = System::Windows::Visibility.visible
  end

  def stop_loading
    self.loading.visibility = System::Windows::Visibility.collapsed
  end
end
 
class WpfTutorial
  include Wpf

  class << self
    def sanitize_xaml(xaml)
      {
        /x:Class=".*?"/          => '',
        /xmlns:local=".*?"/      => '',
        /mc:Ignorable=".*?"/     => '',
        /xmlns:mc=".*?"/         => '',
        /xmlns:d=".*?"/          => '',
        'Loaded="Window_Loaded"' => '',
        '<TreeView '             => '<TreeView ItemTemplate="{StaticResource SectionTemplate}" ',
        /\357\273\277/           =>  '',
        /d:.*?=".*?"/            =>  ''
      }.inject(xaml) do |_xaml,(k,v)| 
        _xaml.gsub(k, v)
      end.gsub(/(\n|\t)/, ' ').squeeze(' ')
    end

    def load_xaml
      sanitize_xaml(
        if File.exist?('design')
          @@step = sanitize_xaml(File.read('design/Tutorial/StepControl.xaml'))
          @@step.sub!(/<UserControl.*?>(.*?)<\/UserControl>/, '\1').
                 gsub!(/x:Name="(.*?)"/, 'x:Name="<%= \1 %>"').
                 strip!
          File.read('design/Tutorial/MainWindow.xaml').
               gsub(/<local:TutorialPage.*?\/>/, 
                 File.read('design/Tutorial/TutorialPage.xaml').
                      gsub(/<local:StepControl.*?\/>/, '')
               )
        else
          File.read 'wpf.xaml'
        end
      )
    end

    def generate_xaml
      generated_content = File.exist?('wpf.generated.xaml') ? File.read('wpf.generated.xaml') : nil
      unless @@xaml == generated_content
        File.open('wpf.generated.xaml', 'w'){|f| f.write @@xaml}
      end
    end

    private :sanitize_xaml, :load_xaml, :generate_xaml
  end
 
  @@xaml = load_xaml
  generate_xaml

  attr_reader :window

  def initialize tutorial = nil
    if tutorial
        @tutorial = tutorial
    else
        @tutorial = Tutorial.get_tutorial
    end

    scope = Object.new
    @bind = scope.instance_eval { binding }

    @window = XamlReader.load System::Xml::XmlReader.create(System::IO::StringReader.new(@@xaml))

    @window.tutorial_name.text = @tutorial.name
    if @tutorial.introduction
      @window.exercise.document = FlowDocument.from_simple_markup(@tutorial.introduction)
      @window.exercise.visibility = Visibility.visible
    else
      @window.exercise.visibility = Visibility.collapsed
    end
    @window.chapters.ItemsSource = @tutorial.sections
    @window.complete.visibility = Visibility.collapsed
    @window.next_chapter.click do |target, event_args| 
      select_section_or_chapter @chapter.next_item
    end
    @window.chapters.mouse_left_button_up { |target, event_args| select_section_or_chapter target.SelectedItem }
  end

  def select_next_task
    if @tasks.empty?
      if @chapter.next_item
        @window.complete.visibility = Visibility.visible
        @window.tutorial_scroll.scroll_to_bottom
        @window.repl_input.visibility = Visibility.collapsed if @window.respond_to?(:"step_#{@window._current_task}")
        @window.repl_input_arrow.visibility = Visibility.collapsed if @window.respond_to?(:"step_#{@window._current_task}")
        if @chapter.summary && @chapter.summary.title
          @window.complete_title.text = @chapter.summary.title
          @window.complete_title.visibility = Visibility.visible
        else
          @window.complete_title.visibility = Visibility.collapsed
        end
        if @chapter.summary && @chapter.summary.body
          @window.complete_body.document = FlowDocument.from_simple_markup(@chapter.summary.body)
          @window.complete_body.visibility = Visibility.visible
        else
          @window.complete_body.visibility = Visibility.collapsed
        end
      else
        if @tutorial.summary && @tutorial.summary.body
          @window.exercise.document = FlowDocument.from_simple_markup(@tutorial.summary.body)
        else
          @window.exercise.document = FlowDocument.from_simple_markup("Tutorial complete!")
        end
        @window.exercise.visibility = Visibility.visible
        @window.body.children.clear
        @window.tutorial_scroll.scroll_to_top
      end
    else
      @window.complete.visibility = Visibility.collapsed
      @window.repl_input.visibility = Visibility.collapsed if @window.respond_to?(:"step_#{@window._current_task}")
      @window.repl_input_arrow.visibility = Visibility.collapsed if @window.respond_to?(:"step_#{@window._current_task}")
      @window.incr_task
      @window.current_step.visibility = Visibility.visible
      @window.repl_input.visibility = Visibility.visible
      @window.repl_input_arrow.visibility = Visibility.visible
      @prev_newline = nil

      # TODO - Should use TextChanged here
      @window.repl_input.key_up.add method(:on_repl_input) 

      @task = @tasks.shift
      
      if @task.description 
        flowDoc = FlowDocument.from_simple_markup(@task.description)
        #flowDoc.Blocks.Add(Paragraph.new(Run.new("Enter the following code:")))
        if @task.code
          p = Paragraph.new(Run.new(@task.code))
          p.font_family = FontFamily.new "Consolas"
          p.font_weight = FontWeights.Bold
          flowDoc.Blocks.Add(p)
        end
      end
      if @window.step_title
        @window.step_title.text = @task.title
        @window.step_title.visibility = Visibility.visible
      else
        @window.step_title.visibility = Visibility.collapsed
      end
      @window.step_description.document = flowDoc
      @window.repl_history.text = ""
      @window.repl_input.focus
      @window.tutorial_scroll.scroll_to_bottom
    end
  end

  def self.select_item tree_view, item
    return false unless tree_view and item

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

        if select_item(childControl, item)
          return true
        else
          childControl.is_expanded = old_is_expanded
        end
      end
    end

    false
  end

  def select_section_or_chapter item
    return unless item
    WpfTutorial.select_item @window.chapters, item
    case item
    when Tutorial::Section: select_section item
    when Tutorial::Chapter: select_chapter item
    else 
      raise "Unknown selection type: #{item}"
    end
  end
    
  def select_chapter chapter
    @window.start_loading
    @chapter = chapter
    if @chapter.introduction
      @window.exercise.document = FlowDocument.from_simple_markup(@chapter.introduction)
      @window.exercise.visibility = Visibility.visible
    else
      @window.exercise.visibility = Visibility.collapsed
    end
    @window.body.children.clear
    @tasks = @chapter.tasks.clone
    select_next_task
    @window.stop_loading
  end

  def select_section section
    if section.introduction
      @window.exercise.document = FlowDocument.from_simple_markup(section.introduction)
      @window.exercise.visibility = Visibility.visible
    else
      @window.exercise.visibility = Visibility.collapsed
    end
    @window.body.children.clear
  end

  def print_to_repl s, new_line = true
    @window.repl_history.text += s
    @window.repl_history.text += "\n" if new_line
    @window.repl_history.scroll_to_line(@window.repl_history.line_count - 1)
  end

  def on_repl_input target, event_args
    if event_args.Key == Key.Enter
      print_to_repl '' if @prev_newline
      @window.repl_history.visibility = Visibility.visible
      input = @window.repl_input.text
      print_to_repl ">>> " + input
      @window.repl_input.text = ""
      begin
        output = StringIO.new
        old_stdout, $stdout = $stdout, output
        result = nil
        result = eval(input.to_s, @bind) # TODO - to_s should not be needed here
        print_to_repl(output.string, false) if not output.string.empty?
        print_to_repl "=> #{result.inspect}"
      rescue Exception, SyntaxError, LoadError => e
        print_to_repl output.string if not output.string.empty?
        print_to_repl e.to_s
      ensure
        $stdout = old_stdout
        if @window.repl_history.text.size > 1
          # BUG: should be able to do str[-1] on a clrstring
          @prev_newline = @window.repl_history.text.to_s[-1] == 10 # '\n'
          @window.repl_history.text = @window.repl_history.text.to_s[0..-2].to_clr_string
        end
      end
      if @task and @task.success?(Tutorial::InteractionResult.new(@bind, output.string, result, e))
        select_next_task
      end
    end
  end

  def run explicit_shutdown = false
    if Application.current
      Application.current.main_window = @window
      @window.visibility = Visibility.visible
     else
      app = Application.new
      if explicit_shutdown
        app.ShutdownMode = ShutdownMode.on_explicit_shutdown
      end
      app.run @window
    end
  end

  def self.create_sta_thread &block
    ts = System::Threading::ThreadStart.new &block

    # Workaround for http://ironruby.codeplex.com/WorkItem/View.aspx?WorkItemId=1306
    param_types = System::Array[System::Type].new(1) { |i| System::Threading::ThreadStart.to_clr_type }
    ctor = System::Threading::Thread.to_clr_type.get_constructor param_types    
    t = ctor.Invoke(System::Array[Object].new(1) { ts })

    t.ApartmentState = System::Threading::ApartmentState.STA
    t.start
  end

  def self.run_interactive
    if Application.current
      Application.current.invoke { WpfTutorial.new.run(true) rescue puts $! }
    else
      warn "Setting explicit shutdown. Exit the process by calling 'unload'"
      # Run the app on another thread so that the interactive REPL can stay on the main thread
      create_sta_thread { WpfTutorial.new.run(true) rescue puts $! }
    end
  end
end

if $0 == __FILE__
  WpfTutorial.new.run
elsif $0 == nil or $0 == "iirb"
  def reload
    load __FILE__
    # load any other files as needed...
  end

  def unload
    System::Windows::Application.current.invoke { System::Windows::Application.current.shutdown }
    exit
  end

  WpfTutorial.run_interactive
end
