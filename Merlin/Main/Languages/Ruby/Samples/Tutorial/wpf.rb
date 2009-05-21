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

# Reference the WPF assemblies
require 'system.xml, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089' 
require 'PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
require 'PresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
require 'windowsbase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'

class System::Windows::FrameworkElement
    # Monkey-patch FrameworkElement to allow window.child_name instead of window.FindName("child_name")
    def method_missing name, *args
        child = FindName(name.to_clr_string)
        if child then return child end
        super
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
    def self.from_simple_markup text
        require 'rdoc/markup/simple_markup'
        require 'rdoc/markup/simple_markup/inline'

        if text =~ /\A(\s+)/
            text = "#{$1}dummy\n\n" + text # TODO - This is a workaround for http://ironruby.codeplex.com/WorkItem/View.aspx?WorkItemId=1301
        end
        
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
    # Include all the WPF namespaces for easy access. A user can just do "include Wpf" in their
    # class and get access to all the WPF types (without polluting the global namespace)
    include System::Windows
    include System::Windows::Documents
    include System::Windows::Controls
    include System::Windows::Input
    include System::Windows::Markup
    include System::Windows::Media

    # Utility class to convert from RDoc SimpleMarkup text to WPF FlowDocument.
    # It implements the same interface as SM::ToHtml, etc.
    class ToFlowDocument
        include System::Windows
        include System::Windows::Documents

        def start_accepting
            @@bold_mask = SM::Attribute.bitmap_for :BOLD
            @@italics_mask = SM::Attribute.bitmap_for :EM
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
                    when nil
                        paragraph.inlines.add(Run.new(item))
                    else
                        raise "unexpected active_attribute: #{active_attribute}"
                    end
                    
                when SM::AttrChanger
                    on_mask = item.turn_on
                    if not on_mask.zero?
                        active_attribute = on_mask
                    end

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
            raise NotImplementedError
        end

        def accept_list_start(am, fragment)
            raise NotImplementedError
        end

        def accept_list_end(am, fragment)
            raise NotImplementedError
        end

        def accept_list_item(am, fragment)
            raise NotImplementedError
        end

        def accept_blank_line(am, fragment)
        end

        def accept_heading(am, fragment)
            raise NotImplementedError
        end

        def accept_rule(am, fragment)
            raise NotImplementedError
        end
        
        def convert_special(special, paragraph)
            SM::Attribute.each_name_of(special.type) do |name|
                method_name = "handle_special_#{name}"
                if self.respond_to? method_name
                    return send(method_name, special, paragraph)
                end
            end
            raise "Unhandled special: #{special}"
        end

        def handle_special_HYPERLINK(special, paragraph)
            paragraph.inlines.add(Hyperlink.new(Run.new(special.text)))
        end

        def handle_special_TIDYLINK(special, paragraph)
            text = special.text
    #       unless text =~ /(\S+)\[(.*?)\]/
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