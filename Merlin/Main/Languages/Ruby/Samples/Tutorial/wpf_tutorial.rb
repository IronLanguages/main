require "tutorial"
require "stringio"

# Reference the WPF assemblies
require 'system.xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' 
require 'PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
require 'PresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
require 'windowsbase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'

# Initialization Constants
Window = System::Windows::Window
Application = System::Windows::Application
Button = System::Windows::Controls::Button
StackPanel = System::Windows::Controls::StackPanel
Label = System::Windows::Controls::Label
Thickness = System::Windows::Thickness
DropShadowBitmapEffect = System::Windows::Media::Effects::DropShadowBitmapEffect
XamlReader = System::Windows::Markup::XamlReader
Key = System::Windows::Input::Key

class System::Windows::FrameworkElement
    def method_missing name, *args
        child = FindName(name.to_clr_string)
        if child then return child end
        super
    end
end

class WpfApp
    def initialize tutorial=nil
        if tutorial
            @tutorial = tutorial
        else
            require File.expand_path("Tutorials/ironruby_tutorial", File.dirname(__FILE__))
            @tutorial = IronRubyTutorial.new
        end
        
        @xaml = <<EOF
<Window x:Class="WpfApplication1.Window1"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    Title="Window1" Height="829" Width="868.507" Loaded="Window_Loaded">
    <StackPanel Height="786.824" Name="stackPanel1" Width="825.165" Background="BurlyWood">
        <StackPanel Height="100.02" Name="stackPanel2" Width="746.816" Orientation="Horizontal">
            <Label Height="86.684" Name="tutorial_name" Width="403.414" HorizontalAlignment="Left" VerticalAlignment="Center" FontSize="36">Tutorial Name</Label>
            <Image Height="71.681" Name="tutorial_icon" Stretch="Fill" Width="116.69" VerticalAlignment="Center" HorizontalAlignment="Right" />
        </StackPanel>
        <StackPanel Height="666.8" Name="stackPanel3" Width="760.152" Orientation="Horizontal">
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
            <TreeView Height="580.116" Name="chapters" Width="185.037" HorizontalAlignment="Left" VerticalAlignment="Top" />
            <StackPanel Height="570.114" Name="stackPanel4" Width="631.793" VerticalAlignment="Top">
                <Label Height="83.35" Name="chapter_content" Width="208.375">Chapter content</Label>
                <StackPanel Height="475.095" Name="stackPanel5" Width="571.781" Orientation="Horizontal">
                    <StackPanel Height="328.399" Name="stackPanel6" Width="450.09">
                        <Label Height="83.35" Name="exercise" Width="300.06">Exercise instructions</Label>
                        <TextBox Height="183.37" Name="repl_history" Width="270.054" HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Auto" IsEnabled="True" IsTabStop="False" IsUndoEnabled="False" IsReadOnly="True" Background="DarkGoldenrod" />
                        <TextBox Height="23" Name="repl_input" Width="215.043" />
                    </StackPanel>
                    <StackPanel Height="348.403" Name="stackPanel7" Width="126.692" IsEnabled="False" Visibility="Hidden">
                        <Button Height="23.338" Name="hint_button" Width="75.015">Hint</Button>
                        <Label Height="290.058" Name="hint" Width="120.024">Label</Label>
                    </StackPanel>
                </StackPanel>
            </StackPanel>
        </StackPanel>
    </StackPanel>
</Window>
EOF
        
        @xaml = @xaml.sub('x:Class="WpfApplication1.Window1"', '').sub('Loaded="Window_Loaded"', '').sub('<TreeView ', '<TreeView ItemTemplate="{StaticResource SectionTemplate}" ')
    end

    def select_next_task
        if @tasks.empty?
            @window.exercise.content = "Chapter completed successfully!"
        else
            @task = @tasks.shift
            @window.exercise.content = @task.description + "\nEnter the following code:\n" + @task.hint
        end
    end
    
    def select_chapter chapter
        if not chapter then return end
        if not chapter.kind_of? Chapter then return end
        @chapter = chapter
        @window.chapter_content.content = chapter.description
        @tasks = @chapter.tasks.clone
        select_next_task
        scope = Object.new
        @bind = scope.instance_eval { binding }
    end

    def print_to_repl s
        @window.repl_history.text = @window.repl_history.text + "\n" + s
        @window.repl_history.select((@window.repl_history.text.length) - 1, 1)
    end
    
    def on_repl_input event_args
        if event_args.Key == Key.Enter
            input = @window.repl_input.text
            print_to_repl "> " + input
            @window.repl_input.text = ""
            begin
                output = StringIO.new
                old_stdout, $stdout = $stdout, output
                result = eval(input.to_s, @bind) ############## to_s should not be needed here
            rescue => e
                print_to_repl output.string if not output.string.empty?
                print_to_repl e.to_s
            else
                print_to_repl output.string if not output.string.empty?
                print_to_repl result.to_s
            ensure
                $stdout = old_stdout
            end
            if @task.success?(@bind, output.string, result)
                select_next_task
            end
        end
    end
    
    def run
        stringReader = System::IO::StringReader.new @xaml
        xmlReader = System::Xml::XmlReader.Create stringReader
        @window = XamlReader.Load xmlReader
        @app = Application.new        
        @window.tutorial_name.Content = @tutorial.name
        @window.chapters.ItemsSource = @tutorial.sections
        @window.chapters.mouse_left_button_up { |target, event_args| select_chapter target.SelectedItem }
        @window.repl_input.key_up { |target, event_args| on_repl_input event_args }
        @app.run @window
    end
end

if $0 == __FILE__
    WpfApp.new.run
end