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

require "stringio"
require 'erb'

begin
  require 'c:/dev/repl'
  alias debugger repl
rescue LoadError
  # get repl.rb: http://gist.github.com/116393
end

if ARGV.include?('TRACE')
  begin
    require 'c:/dev/trace'
  rescue LoadError
    # get trace.rb
  end
end

require 'wpf'
include Wpf

require "tutorial"

module WpfTutorial

  class App < System::Windows::Application
    class << self
      def current
        @current ||= App.new
      end

      def run(options = {})
        unless Application.Current
          if options[:explicit_shutdown]
            current.shutdown_mode = ShutdownMode.on_explicit_shutdown
          end
          current.run Window.current
        else
          App.Current.main_window = Window.current
          Window.current.show!
        end
      end

      def create_sta_thread &block
        ts = System::Threading::ThreadStart.new &block

        # Workaround for http://ironruby.codeplex.com/WorkItem/View.aspx?WorkItemId=1306
        param_types = System::Array[System::Type].new(1) { |i| System::Threading::ThreadStart.to_clr_type }
        ctor = System::Threading::Thread.to_clr_type.get_constructor param_types    
        t = ctor.Invoke(System::Array[Object].new(1) { ts })
        t.ApartmentState = System::Threading::ApartmentState.STA
        t.Start
      end

      def run_interactive(proc_obj)
        if Application.Current
          app_callback = System::Threading::ThreadStart.new { proc_obj.call rescue puts $! }
          Application.current.dispatcher.invoke(Threading::DispatcherPriority.Normal, app_callback)
        else
          warn "Setting explicit shutdown. Exit the process by calling 'unload'"
          # Run the app on another thread so that the interactive REPL can stay on the main thread
          create_sta_thread { proc_obj.call rescue puts $! }
        end
      end
    end
  end

  class Tutorial
    attr_reader :tasks, :task, :chapter, :current_tutorial

    def initialize(t)
      @current_tutorial ||= ::Tutorial.get_tutorial(t)
      @window = Window.current
    end

    def self.all
      @tutorials ||= ::Tutorial.all
    end

    def reset
      @chapter = nil
      @tasks = nil
      @window.tutorial_body.children.clear
    end

    def start
      reset
      Window.show_tutorial

      @window.set_or_collapse(:exercise, @current_tutorial.introduction) do |obj, value|
        obj.document = FlowDocument.from_simple_markup(value)
      end
      @window.chapters.items_source = @current_tutorial.sections

      @window.next_chapter.click do |t, e|
        select_section_or_chapter @chapter.next_item if @chapter
      end
      @window.chapters.mouse_left_button_up do |t, e|
        select_section_or_chapter t.selected_item
      end
    end

    def select_section_or_chapter(item)
      return unless item
      Wpf::select_tree_view_item @window.chapters, item
      case item
      when ::Tutorial::Section: select_section item
      when ::Tutorial::Chapter: select_chapter item
      else 
        raise "Unknown selection type: #{item}"
      end
    end

    def select_chapter(chapter)
      @chapter = chapter
      @window.set_or_collapse(:exercise, @chapter.introduction) do |obj, value|
        obj.document = FlowDocument.from_simple_markup value
      end
      @window.tutorial_body.children.clear
      @tasks = @chapter.tasks.clone
      select_next_task
    end

    def select_section(section)
      reset
      @window.set_or_collapse(:exercise, section.introduction) do |obj, value|
        obj.document = FlowDocument.from_simple_markup value
      end
    end

    def select_next_task
      unless @tasks.empty?
        @window.next_step

        @task = @tasks.shift
        if @task.description
          fd = FlowDocument.from_simple_markup @task.description
          if @task.code
            p = Paragraph.new Run.new(@task.code)
            p.font_family = FontFamily.new "Consolas"
            p.font_weight = FontWeights.Bold
            fd.Blocks.Add p
          end
        end
        @window.set_or_collapse(:step_title, @task.title) do |obj, value|
          obj.text = value
        end

        @window.step_description.document = fd
        @window.tutorial_scroll.scroll_to_bottom
      else
        if @chapter.next_item
          @window.complete.show!
          @window.tutorial_scroll.scroll_to_bottom
          if @window.current_step?
            @window.repl_input.collapse!
            @window.repl_input_arrow.collapse!
          end
          @window.set_or_collapse(:complete_title, @chapter.summary.title) do |obj, value|
            obj.text = value
          end if @chapter.summary
          @window.set_or_collapse(:complete_body, @chapter.summary.body) do |obj, value|
            obj.document = FlowDocument.from_simple_markup value
          end if @chapter.summary
        else
          if @current_tutorial.summary && @current_tutorial.summary.body
            @window.exercise.document = FlowDocument.from_simple_markup(@current_tutorial.summary.body)
          else
            @window.exercise.document = FlowDocument.from_simple_markup("Tutorial complete!")
          end
          @window.exercise.show!
          @window.tutorial_body.children.clear
          @window.tutorial_scroll.scroll_to_top
        end
      end
    end

    def process_result(result)
      if @task and @task.success?(::Tutorial::InteractionResult.new(*result))
        select_next_task
      end
    end
  end

  class Window
    module Accessors
      def current_step
        @current_step ||= Step.new(Window.tutorial.task, current_step_id, Window.current)
      end

      def current_step_id
        @current_step_id || 0
      end

      def current_step?
        respond_to?(:"step_#{current_step_id}")
      end

      def current_step_object
        current_step.object
      end

      def next_step
        complete.collapse!
        if current_step?
          repl_input.collapse! 
          repl_input_arrow.collapse! 
        end

        @current_step = nil
        @current_step_id ||= -1
        @current_step_id  +=  1
        
        current_step.show
      end

      def reset_step
        @current_step_id = 0
        @current_step = nil
      end

      def cache_step(name, object)
        instance_variable_set(:"@#{name}", object)
        self.class.class_eval { attr_reader :"#{name}" }
      end

      delegate_methods [:step_title, :step_description], :to => :current_step_object, :append => :current_step_id
   
      delegate_methods [:repl_input, :repl_input_arrow, :repl_history], 
        :to => :current_step_object, :prepend => 'step', :append => :current_step_id
    end

    class << self
      attr_reader :step_xaml, :tut_xaml
      attr_reader :repl, :tutorial

      def current
        @window ||= create
      end

      def create
        win = XamlReader.load xaml do |win|
          win.main.children.clear

          Tutorial.all.each_with_index do |(path,tutorial), index|
            tutorial_id, tutorial_title_id, tutorial_desc_id = "tutorial_#{index}", "tutorial_title_#{index}", "tutorial_desc_#{index}"
            tutorial_id_OnMouseLeave_BeginStoryboard = "tutorial_#{index}_OnMouseLeave_BeginStoryboard"
            
            XamlReader.erb_load(@tut_xaml, binding) do |obj|
              obj.send(tutorial_title_id).text = tutorial.name
              obj.send(tutorial_desc_id).document = FlowDocument.from_simple_markup(tutorial.introduction || '')
              win.main.children.add obj
              
              obj.mouse_left_button_up do |t, e|
                @tutorial = Tutorial.new(path)
                @tutorial.start
              end
              
              obj.show!
            end
          end
          show_main(win)
          @repl = Repl.new
        end
        class << win
          include Accessors
        end
        win
      end

      def show_main(win = current)
        win.header_name.text = 'Pick a tutorial'
        win.main_scroll.show!
        win.tutorial_scroll.collapse!
        win.tutorial_nav.collapse!
        win.complete.collapse!
        if @tutorial
          win.header_navigation.show!
          win.header_navigation.action.text = "Resume tutorial" 
          win.header_navigation.action.mouse_left_button_up {|t,e| show_tutorial}
        else
          win.header_navigation.collapse!
        end
      end

      def show_tutorial
        current.main_scroll.collapse!
        current.tutorial_scroll.show!
        current.tutorial_nav.show!
        current.complete.collapse!
        current.header_name.text = @tutorial.current_tutorial.name
        current.header_navigation.show!
        current.header_navigation.action.text = "Back"
        current.header_navigation.action.mouse_left_button_up {|t,e| show_main}
      end

      def xaml
        @step_xaml, @tut_xaml = ['Step', 'Tutorial'].map do |i|
          sx = sanitize_xaml(File.read("design/Tutorial/#{i}Control.xaml"))
          sx.sub!(/<UserControl.*?>(.*?)<\/UserControl>/, '\1') if i == 'Step'
          sx.gsub!(/("|\{StaticResource )(\S*?)_id(\S*?)("|\})/, '\1<%= \2_id %>\3\4')
          sx.strip
        end

        sanitize_xaml(
          File.read('design/Tutorial/MainWindow.xaml').
               gsub(
                 /<local:TutorialPage.*?\/>/, 
                 File.read('design/Tutorial/TutorialPage.xaml').
                      gsub(/<local:StepControl.*?\/>/, '')).
               gsub(/<local:TutorialControl.*?\/>/, ''))
      end

      private
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
    end
  end

  class Step
    @@vars = :step_id,
      :step_title_id, :step_description_id,
      :step_wrapper_id, :step_repl_id,
      :step_repl_history_id, :step_repl_input_id, :step_repl_input_arrow_id

    attr_reader *@@vars

    attr_reader :object

    def initialize(task, element_id, window)
      @task = task
      @element_id = "step_#{element_id}"
      @window = window
      raise "Element #{@element_id} already exists" if @window.respond_to? @element_id
      init_vars
    end

    def show
      @object = XamlReader.erb_load(Window.step_xaml, binding) do |obj|
        @window.tutorial_body.children.add obj
        @window.cache_step(@element_id, obj)
        obj.show!
      end

      Window.repl.prev_newline = nil
      @window.repl_history.text = ''

      @window.repl_input.show!
      @window.repl_input.focus
      # TODO - Should use TextChanged here
      @window.repl_input.key_up do |target, event_args|
        if event_args.key == Key.enter
          Window.tutorial.process_result Window.repl.on_repl_input
        end
      end
      @window.repl_input_arrow.show!
    end

    private
      def init_vars
        @@vars.each do |v|
          instance_variable_set(
            :"@#{v}", 
            "#{v.to_s.split('_id').first}_#{@window.current_step_id}"
          )
        end
      end
  end

  class Repl
    attr_accessor :prev_newline

    def initialize
      @scope = Object.new
      @bind = @scope.__send__ :binding
      @prev_newline = nil
    end

    def history
      Window.current.repl_history
    end

    def input
      Window.current.repl_input
    end

    def print(s, new_line = true)
      history.text += s
      history.text += "\n" if new_line
      history.scroll_to_line(history.line_count - 1)
    end

    def on_repl_input
      print '' if @prev_newline
      history.show!

      input = self.input.text
      print ">>> #{input}"
      self.input.text = ''

      begin
        output = StringIO.new
        prev_stdout, $stdout = $stdout, output
        result = eval(input.to_s, @bind) # TODO - to_s should not be needed here
        
        print output.string, false unless output.string.empty?
        print "=> #{result.inspect}"
      rescue Exception, SyntaxError, LoadError => e
        print output.string unless output.string.empty?
        print e.to_s
      ensure
        $stdout = prev_stdout
        if history.text.size > 1
          # TODO should be able to do str[-1] on a clrstring
          @prev_newline = history.text.to_s[-1] == 10 # '\n'
          history.text = history.text.to_s[0..-2].to_clr_string
        end
      end
      [@bind, output.string, result, e]
    end
  end

end

if $0 == __FILE__
  WpfTutorial::App.run

elsif $0 == nil or $0 == "iirb"
  include System::Windows
  include System::Threading
  include System::Windows::Threading

  def reload
    load __FILE__
  end

  def unload
    Application.current.dispatcher.invoke(DispatcherPriority.normal, 
      ThreadStart.new{ Application.current.shutdown })
    exit
  end

  WpfTutorial::App.run_interactive lambda{ 
    WpfTutorial::App.run(:explicit_shutdown => true)
  }
end
