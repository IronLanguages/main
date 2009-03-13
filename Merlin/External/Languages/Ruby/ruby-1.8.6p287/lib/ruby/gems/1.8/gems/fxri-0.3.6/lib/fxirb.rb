#! /usr/bin/env ruby

# Credits:
# - Initial linux version:  Gilles Filippini
# - Initial windows port : Marco Frailis
# - Currently maintained and developed by 
#     Martin DeMello <martindemello@gmail.com>

# fxri already includes Fox
#require "fox12"
require "irb"
require "singleton"
require "English"
require 'thread'

include Fox

STDOUT.sync = true

class FXIRBInputMethod < IRB::StdioInputMethod

	attr_accessor :print_prompt, :gets_mode

	def initialize
		super 
		@history = 1
		@begin = nil
		@end = nil
		@print_prompt = true
		@continued_from = nil
		@gets_mode = false
	end

	def gets 
		if @gets_mode
			return FXIrb.instance.get_line
		end

		if (a = @prompt.match(/(\d+)[>*]/))
			level = a[1].to_i
			continued = @prompt =~ /\*\s*$/
		else
			level = 0
		end

		if level > 0 or continued
			@continued_from ||= @line_no
		elsif @continued_from
			merge_last(@line_no-@continued_from+1)
			@continued_from = nil
		end

		l = @line.length
		@line = @line.reverse.uniq.reverse
		delta = l - @line.length
		@line_no -= delta
		@history -= delta

		if print_prompt
			print @prompt

			#indentation
			print "  "*level
		end

		str = FXIrb.instance.get_line

		@line_no += 1
		@history = @line_no + 1
		@line[@line_no] = str

		str
	end

	# merge a block spanning several lines into one \n-separated line
	def merge_last(i)
		return unless i > 1
		range = -i..-1
		@line[range] = @line[range].join
		@line_no -= (i-1)
		@history -= (i-1)
	end

	def prev_cmd
		return "" if @gets_mode

		if @line_no > 0
			@history -= 1 unless @history <= 1
			return line(@history)
		end
		return ""
	end

	def next_cmd
		return "" if @gets_mode

		if (@line_no > 0) && (@history < @line_no)
			@history += 1
			return line(@history)
		end
		return ""
	end

end

module IRB

	def IRB.start_in_fxirb(im)
		if RUBY_VERSION < "1.7.3"
			IRB.initialize(nil)
			IRB.parse_opts
			IRB.load_modules
		else
			IRB.setup(nil)
		end

		irb = Irb.new(nil, im)    

		@CONF[:IRB_RC].call(irb.context) if @CONF[:IRB_RC]
		@CONF[:MAIN_CONTEXT] = irb.context
		trap("SIGINT") do
			irb.signal_handle
		end

		class << irb.context.workspace.main
			def gets
				inp = IRB.conf[:MAIN_CONTEXT].io
				inp.gets_mode = true
				retval = IRB.conf[:MAIN_CONTEXT].io.gets
				inp.gets_mode = false
				retval
			end
		end

		catch(:IRB_EXIT) do
			irb.eval_input
		end
		print "\n"
	end

end

class FXEvent
	def ctrl?
		(self.state & CONTROLMASK) != 0 
	end

	def shift?
		(self.state & SHIFTMASK) != 0 
	end
end


class FXIrb < FXText
	include Singleton
	include Responder

	attr_reader :input
	attr_accessor :multiline

	def FXIrb.init(p, tgt, sel, opts)
		unless @__instance__
			Thread.critical = true
			begin
				@__instance__ ||= new(p, tgt, sel, opts)
			ensure
				Thread.critical = false
			end
		end
		return @__instance__
	end

	def initialize(p, tgt, sel, opts)
		FXMAPFUNC(SEL_KEYRELEASE, 0, "onKeyRelease")
		FXMAPFUNC(SEL_KEYPRESS, 0, "onKeyPress")
		FXMAPFUNC(SEL_LEFTBUTTONPRESS,0,"onLeftBtnPress")
		FXMAPFUNC(SEL_MIDDLEBUTTONPRESS,0,"onMiddleBtnPress")
		FXMAPFUNC(SEL_LEFTBUTTONRELEASE,0,"onLeftBtnRelease")

		super
		setFont(FXFont.new(FXApp.instance, "lucida console", 9))
		@anchor = 0
	end

	def create
		super
		setFocus
		# IRB initialization
		@inputAdded = 0
		@input = IO.pipe
		$DEFAULT_OUTPUT = self

		@im = FXIRBInputMethod.new
		@irb = Thread.new {
			IRB.start_in_fxirb(@im)
			self.crash
		}

		@multiline = false

		@exit_proc = lambda {exit}
	end

	def on_exit(&block)
		@exit_proc = block
	end

	private

	def crash
		instance_eval(&@exit_proc)
	end

	def onLeftBtnPress(sender,sel,event)
		@store_anchor = @anchor
		setFocus
		super
	end

	def onLeftBtnRelease(sender,sel,event)
		super
		@anchor = @store_anchor
		setCursorPos(getLength)
	end

	def onMiddleBtnPress(sender,sel,event)
		pos = getPosAt(event.win_x,event.win_y)
		if pos >= @anchor
			super
		end
	end

	def onKeyRelease(sender, sel, event)
		case event.code
		when KEY_Return, KEY_KP_Enter
			new_line_entered unless empty_frame?
		end
		return 1
	end

	def onKeyPress(sender,sel,event)
		case event.code
		when KEY_Return, KEY_KP_Enter
			move_to_end_of_frame
			super unless empty_frame?

		when KEY_Up,KEY_KP_Up
			multiline = true if get_from_start_of_line =~ /\n/
			multiline ? super : history(:prev)
			move_to_start_of_line if invalid_pos?

		when KEY_Down,KEY_KP_Down
			multiline = true if get_to_end_of_line =~ /\n/
			multiline ? super : history(:next)

		when KEY_Left,KEY_KP_Left
			super if can_move_left?

		when KEY_Delete,KEY_KP_Delete,KEY_BackSpace
			if event.shift? or event.ctrl?
				event.code == KEY_BackSpace ? 
					delete_from_start_of_line :
					delete_to_end_of_line
			elsif can_move_left?
				super
			end

		when KEY_Home, KEY_KP_Home
			move_to_start_of_line

		when KEY_End, KEY_KP_End
			move_to_end_of_line

		when KEY_Page_Up, KEY_KP_Page_Up
			history(:prev)

		when KEY_Page_Down, KEY_KP_Page_Down
			history(:next)

		when KEY_bracketright, KEY_braceright
			#auto-auto_dedent if the } or ] is on a line by itself
			auto_dedent if empty_frame? and indented?
			super

		when KEY_u
			event.ctrl? ? delete_from_start_of_line : super

		when KEY_k
			event.ctrl? ? delete_to_end_of_line :	super

		when KEY_d
			if event.ctrl? and empty_frame?
				quit_irb
			else
				# test for 'end' so we can auto_dedent
				if (get_frame == "en") and indented?
					auto_dedent
				end
				super
			end

		else
			super
		end
	end

	def auto_dedent
		str = get_frame
		clear_frame
		@anchor -= 2
		appendText(str)
		setCursorPos(getLength)
	end

	def history(dir)
		str = (dir == :prev) ? @im.prev_cmd.chomp : @im.next_cmd.chomp
		if str != ""
			clear_frame
			write(str)
		end  
	end

	def quit_irb
		clear_frame
		appendText("exit")
		new_line_entered
	end

	def get_frame
		extractText(@anchor, getLength-@anchor)
	end

	def invalid_pos?
		getCursorPos < @anchor
	end

	def can_move_left? 
		getCursorPos > @anchor
	end

	def move_to_start_of_frame
		setCursorPos(@anchor)
	end

	def move_to_end_of_frame
		setCursorPos(getLength)
	end

	def move_to_start_of_line
		if multiline
			cur = getCursorPos
			pos = lineStart(cur)
			pos = @anchor if pos < @anchor
		else
			pos = @anchor
		end
		setCursorPos(pos)
	end

	def move_to_end_of_line
		if multiline
			cur = getCursorPos
			pos = lineEnd(cur)
		else
			pos = getLength
		end
		setCursorPos(pos)
	end

	def get_from_start_of_line
		extractText(@anchor, getCursorPos-@anchor)
	end
	
	def get_to_end_of_line
		extractText(getCursorPos, getLength - getCursorPos)
	end

	def clear_frame
		removeText(@anchor, getLength-@anchor)
	end

	def delete_from_start_of_line
		str = get_to_end_of_line
		clear_frame
		appendText(str)
		setCursorPos(@anchor)
	end

	def delete_to_end_of_line
		str = get_from_start_of_line
		clear_frame
		appendText(str)
		setCursorPos(getLength)
	end

	def empty_frame?
		get_frame == ""
	end

	def indented?
		extractText(@anchor-2, 2) == "  "
	end

	def new_line_entered
		process_commandline(extractText(@anchor, getLength-@anchor))
	end

	def process_commandline(cmd)
		multiline = false
		lines = cmd.split(/\n/)
		lines.each {|i| 
			@input[1].puts i
			@inputAdded += 1
		}

		while (@inputAdded > 0) do
			@irb.run
		end
	end

	public

	def send_command(cmd)
		setCursorPos(getLength)
		makePositionVisible(getLength) unless isPosVisible(getLength)
		cmd += "\n"
		appendText(cmd)
		process_commandline(cmd)
	end

	def write(obj)
		str = obj.to_s
		appendText(str)
		setCursorPos(getLength)
		makePositionVisible(getLength) unless isPosVisible(getLength)
		return str.length
	end

	def get_line
		@anchor = getLength
		if @inputAdded == 0
			Thread.stop
		end
		@inputAdded -= 1
		retval = @input[0].gets
		# don't print every prompt for multiline input
		@im.print_prompt = (@inputAdded == 0) 
		return retval
	end
end

# Stand alone run
if __FILE__ == $0
	application = FXApp.new("FXIrb", "ruby")
	application.threadsEnabled = true
	Thread.abort_on_exception = true
	window = FXMainWindow.new(application, "FXIrb", 
														nil, nil, DECOR_ALL, 0, 0, 580, 500)
	fxirb = FXIrb.init(window, nil, 0, 
										 LAYOUT_FILL_X|LAYOUT_FILL_Y|TEXT_WORDWRAP|TEXT_SHOWACTIVE)
	application.create
	window.show(PLACEMENT_SCREEN)
	fxirb.on_exit {exit}
	application.run
end
