Merb::BootLoader.after_app_loads do
  module Merb
    class Dispatcher
      # :api: private
      module DefaultExceptionHelper
      
        # :api: private
        def humanize_exception(e)
          e.class.name.split("::").last.gsub(/([a-z])([A-Z])/, '\1 \2')
        end

        # :api: private
        def error_codes(exception)
          if @show_details
            message, message_details = exception.message.split("\n", 2)
            "<h2>#{escape_html(message)}</h2><p>#{escape_html(message_details)}</p>"
          else
            "<h2>Sorry about that...</h2>"
          end
        end

        # :api: private
        def frame_details(line)
          if (match = line.match(/^(.+):(\d+):(.+)$/))
            filename = match[1]
            lineno = match[2]
            location = match[3]
            if filename.index(Merb.framework_root) == 0
              type = "framework"
              shortname = Pathname.new(filename).relative_path_from(Pathname.new(Merb.framework_root))
            elsif filename.index(Merb.root) == 0
              type = "app"
              shortname = Pathname.new(filename).relative_path_from(Pathname.new(Merb.root))
            elsif path = Gem.path.find {|p| filename.index(p) == 0}
              type = "gem"
              shortname = Pathname.new(filename).relative_path_from(Pathname.new(path))
            else
              type = "other"
              shortname = filename
            end
            [type, shortname, filename, lineno, location]
          else
            ['', '', '', nil, nil]
          end
        end

        # :api: private
        def listing(key, value, arr)
          ret   =  []
          ret   << "<table class=\"listing\" style=\"display: none\">"
          ret   << "  <thead>"
          ret   << "    <tr><th width='25%'>#{key}</th><th width='75%'>#{value}</th></tr>"
          ret   << "  </thead>"
          ret   << "  <tbody>"
          (arr || []).each_with_index do |(key, val), i|
            klass = i % 2 == 0 ? "even" : "odd"
            ret << "    <tr class=\"#{klass}\"><td>#{key}</td><td>#{val.inspect}</td></tr>"
          end
          if arr.blank?
            ret << "    <tr class='odd'><td colspan='2'>None</td></tr>"
          end
          ret   << "  </tbody>"
          ret   << "</table>"
          ret.join("\n")
        end
      
        def jar?(filename)
          filename.match(/jar\!/)
        end
      
        # :api: private
        def textmate_url(filename, line)
          "<a href='txmt://open?url=file://#{filename}&amp;line=#{line}'>#{line}</a>"
        end
      
        # :api: private
        def render_source(filename, line)
          line = line.to_i
          ret   =  []
          ret   << "<tr class='source'>"
          ret   << "  <td class='collapse'></td>"
          str   =  "  <td class='code' colspan='2'><div>"
        
          __caller_lines__(filename, line, 5) do |lline, lcode|
            str << "<a href='txmt://open?url=file://#{filename}&amp;line=#{lline}'>#{lline}</a>"
            str << "<em>" if line == lline
            str << Erubis::XmlHelper.escape_xml(lcode)
            str << "</em>" if line == lline
            str << "\n"
          end
          str   << "</div></td>"
          ret   << str
          ret   << "</tr>"
          ret.join("\n")
        end

        def jar?(filename)
          filename.match(/jar\!/)
        end
      end
    
      # :api: private
      class DefaultException < Merb::Controller
        self._template_root = File.dirname(__FILE__) / "views"
      
        # :api: private
        def _template_location(context, type = nil, controller = controller_name)
          "#{context}.#{type}"
        end
      
        # :api: private
        def index
          @exceptions = request.exceptions
          @show_details = Merb::Config[:exception_details]
          render :format => :html
        end
      end
    end
  end
end