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
require 'rdoc/markup/simple_markup'
require 'rdoc/markup/simple_markup/inline'

# Reference the WPF assemblies
require 'system.xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' 
require 'PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
require 'PresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
require 'windowsbase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'

class System::Windows::FrameworkElement
    def method_missing name, *args
        child = FindName(name.to_clr_string)
        if child then return child end
        super
    end
end

class ToFlowDocument
	include System::Windows
	include System::Windows::Documents

    @@bold_mask = SM::Attribute.bitmap_for :BOLD
    @@italics_mask = SM::Attribute.bitmap_for :EM

    def start_accepting
      @flowDoc = FlowDocument.new
      @attributes = []
    end
    
    def end_accepting
      @flowDoc
    end

    def accept_paragraph(am, fragment)
      paragraph = Paragraph.new
      convert_flow(am.flow(fragment.txt), paragraph)
      @flowDoc.blocks.add paragraph
    end

    def convert_flow(flow, paragraph)
      flow.each do |item|
        case item
        when String
          if @attributes.empty?
            paragraph.inlines.add(Run.new(item))
          else
            @attributes << item
          end
        when SM::AttrChanger
           on_mask = item.turn_on
           if not on_mask.zero?
             @attributes << on_mask
           end

           off_mask = item.turn_off
           if not off_mask.zero?
             raise NotImplementedError if @attributes.size != 2
             raise NotImplementedError if @attributes[0] != off_mask
             case @attributes[0]
               when @@bold_mask
                 paragraph.inlines.add(Bold.new(Run.new(@attributes[1])))
                 @attributes.clear
               when @@italics_mask
                 paragraph.inlines.add(Italic.new(Run.new(@attributes[1])))
                 @attributes.clear
               else
                 raise "unknown attribute"
             end
           end
        when Special
          raise "Unknown flow element: #{item.inspect}"
          # convert_special(item)
        else
          raise "Unknown flow element: #{item.inspect}"
        end
      end
      
      raise "mismatch" if not @attributes.empty?
    end

    def accept_verbatim(am, fragment)
      @flowDoc.blocks.add(Paragraph.new(Run.new("accept_verbatim")))
    end

    def accept_list_start(am, fragment)
      @flowDoc.blocks.add(Paragraph.new(Run.new("accept_list_start")))
    end

    def accept_list_end(am, fragment)
      @flowDoc.blocks.add(Paragraph.new(Run.new("accept_list_end")))
    end

    def accept_list_item(am, fragment)
      @flowDoc.blocks.add(Paragraph.new(Run.new("accept_list_item")))
    end

    def accept_blank_line(am, fragment)
      #@flowDoc.blocks.add(Paragraph.new(Run.new("accept_blank_line")))
    end

    def accept_heading(am, fragment)
      @flowDoc.blocks.add(Paragraph.new(Run.new("accept_heading")))
    end

    def accept_rule(am, fragment)
      @flowDoc.blocks.add(Paragraph.new(Run.new("accept_rule")))
    end
    
    def self.convert text
		if text =~ /\A(\s+)/
			text = "#{$1}dummy\n\n" + text # TODO - This is a workaround for http://ironruby.codeplex.com/WorkItem/View.aspx?WorkItemId=1301
		end
		markupParser = SM::SimpleMarkup.new
		markupParser.convert(text, ToFlowDocument.new)
    end
end

class WpfApp
	include System::Windows
	include System::Windows::Documents
	include System::Windows::Controls
	include System::Windows::Input
	include System::Windows::Markup
	include System::Windows::Media
	
    @@xaml = <<EOF
<Window x:Class="WpfApplication1.Window1"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="Window1" Height="829" Width="868.507" Loaded="Window_Loaded">
    <StackPanel Name="top_panel" Background="BurlyWood">
        <StackPanel Height="100.02" Name="header" Orientation="Horizontal">
            <Label Name="tutorial_name" Width="403.414" HorizontalAlignment="Left" VerticalAlignment="Center" FontSize="36">Tutorial Name</Label>
            <Image Name="tutorial_icon" Stretch="Fill" Width="116.69" VerticalAlignment="Center" HorizontalAlignment="Right" />
        </StackPanel>
        <StackPanel Name="body" Orientation="Horizontal">
            <StackPanel.Resources>
                <!-- Chapter TEMPLATE -->
                <DataTemplate x:Key="ChapterTemplate">
                    <TextBlock Text="{Binding Path=name}" />
                </DataTemplate>

                <!-- Section TEMPLATE -->
                <HierarchicalDataTemplate 
						  x:Key="SectionTemplate"
						  ItemsSource="{Binding Path=chapters}"
						  ItemTemplate="{StaticResource ChapterTemplate}"
						  >
                    <TextBlock Text="{Binding Path=name}" />
                </HierarchicalDataTemplate>
            </StackPanel.Resources>
            <TreeView Height="580.116" Name="chapters" Width="281.723" HorizontalAlignment="Left" VerticalAlignment="Top" />
            <StackPanel Name="tutorial_and_repl" Width="531.773" VerticalAlignment="Top">
                <RichTextBox Height="270.054" Name="exercise" Background="BurlyWood" Focusable="False" IsReadOnly="True" IsUndoEnabled="False" BorderThickness="0" />
                <Button Height="23.338" Name="start_chapter" Width="75.015" IsEnabled="False" IsTabStop="False" Visibility="Hidden" HorizontalAlignment="Right">Start chapter...</Button>
                <Button Height="23.338" Name="next_chapter" Width="75.015" IsEnabled="False" IsTabStop="False" Visibility="Hidden" HorizontalAlignment="Right">Next chapter...</Button>
                <TextBox Height="23" Name="repl_input" FontFamily="Courier New" />
                <TextBox Height="183.37" Name="repl_history" HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Auto" IsEnabled="True" IsTabStop="False" IsUndoEnabled="False" IsReadOnly="True" Background="DarkGoldenrod" FontFamily="Courier New" />
            </StackPanel>
        </StackPanel>
    </StackPanel>
</Window>

EOF

	# Remove items added by the VS Xaml designer that are not needed here
    @@xaml = @@xaml.sub('x:Class="WpfApplication1.Window1"', '').sub('Loaded="Window_Loaded"', '').sub('<TreeView ', '<TreeView ItemTemplate="{StaticResource SectionTemplate}" ')

    def initialize tutorial = nil
        if tutorial
            @tutorial = tutorial
        else
            @tutorial = Tutorial.get_tutorial
        end

        scope = Object.new
        @bind = scope.instance_eval { binding }

        stringReader = System::IO::StringReader.new @@xaml
        xmlReader = System::Xml::XmlReader.Create stringReader
        @window = XamlReader.Load xmlReader
        
        @window.tutorial_name.Content = @tutorial.name
        @window.exercise.document = ToFlowDocument.convert(@tutorial.introduction)
        @window.chapters.ItemsSource = @tutorial.sections
        
        # Hook up events
        @window.chapters.mouse_left_button_up { |target, event_args| select_section_or_chapter target.SelectedItem }
        @window.repl_input.key_up { |target, event_args| on_repl_input event_args } # TODO - Should use TextChanged here
        @window.start_chapter.click { |target, event_args| on_start_chapter }
    end

    def select_next_task
        if @tasks.empty?
			if @chapter.next_item
				select_section_or_chapter @chapter.next_item
			else
				@window.exercise.document = ToFlowDocument.convert(@tutorial.summary)
			end
        else
            @task = @tasks.shift

            flowDoc = ToFlowDocument.convert(@task.description)
            flowDoc.Blocks.Add(Paragraph.new(Run.new("Enter the following code:")))
            p = Paragraph.new(Run.new(@task.code))
            p.font_family = FontFamily.new("Courier New")
            p.font_weight = FontWeights.Bold
            flowDoc.Blocks.Add(p)
            @window.exercise.document = flowDoc;
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
			tree_view.Items.each { |childItem|
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
			}
		end
		
		false
    end

    def select_section_or_chapter item
        if not item then return end
        WpfApp.select_item @window.chapters, item
        if item.kind_of? Tutorial::Chapter
			select_chapter item
		elsif item.kind_of? Tutorial::Section
			select_section item
		else
			raise "Unknown selection type: #{item}"
		end
	end
	
	def enable_start_chapter_button
        @window.start_chapter.visibility = Visibility.Visible
        @window.start_chapter.is_enabled = true
        # enable tab stop
	end
	
	def disable_start_chapter_button
        @window.start_chapter.visibility = Visibility.Hidden
        @window.start_chapter.is_enabled = false
        # disable tab stop
	end
	
	def on_start_chapter
		disable_start_chapter_button
		select_next_task
		@window.repl_input.focus
	end
	
	def select_chapter chapter
        @chapter = chapter
		@window.exercise.document = ToFlowDocument.convert(@chapter.introduction)
        enable_start_chapter_button
        @tasks = @chapter.tasks.clone
    end

	def select_section section
		@window.exercise.document = ToFlowDocument.convert(section.introduction)
    end

    def print_to_repl s, new_line = true
        @window.repl_history.text += s
        @window.repl_history.text += "\n" if new_line
        @window.repl_history.scroll_to_line(@window.repl_history.line_count - 1)
    end
    
    def on_repl_input event_args
        if event_args.Key == Key.Enter
            input = @window.repl_input.text
            print_to_repl "> " + input
            @window.repl_input.text = ""
            begin
                output = StringIO.new
                old_stdout, $stdout = $stdout, output
                result = nil
                result = eval(input.to_s, @bind) # TODO - to_s should not be needed here
            rescue => e
                print_to_repl output.string if not output.string.empty?
                print_to_repl e.to_s
            else
                print_to_repl(output.string, false) if not output.string.empty?
                print_to_repl "=> #{result.inspect}"
            ensure
                $stdout = old_stdout
            end
            if @task and @task.success?(Tutorial::InteractionResult.new(@bind, output.string, result, e))
                select_next_task
            end
        end
    end
    
    def run
        app = Application.new
        app.run @window
    end
end

if $0 == __FILE__
    WpfApp.new.run
end