#! /usr/bin/env ruby

#
# This is version 0.1.1 of Gilles Filippini's FXIrb, an attempt to embed
# IRB into an FXRuby FXText widget. The master page for this is here:
#
#	http://www.rubygarden.org/ruby?FXIrb
#
# TODO
# - handle user input redirection
# - ^D
# - readonly everywhere but for the command line
# - readline

require "irb"
require "singleton"
require "fcntl"

module IRB
  def IRB.start_in_fxirb(redir)
    IRB.initialize(nil)
    IRB.parse_opts
    IRB.load_modules

    irb = Irb.new(nil, redir)

    @CONF[:IRB_RC].call(irb.context) if @CONF[:IRB_RC]
    @CONF[:MAIN_CONTEXT] = irb.context
    trap("SIGINT") do
      irb.signal_handle
    end
    
    catch(:IRB_EXIT) do
      irb.eval_input
    end
    print "\n"
  end
end

class Redirect < IRB::StdioInputMethod
	def initialize(dest)
		super()
		@dest = dest
	end

	def gets 
		close
		@dest.write(prompt)
		str = @dest.gets(@prompt)
		if /^exit/ =~ str
			exit
		end
		@line[@line_no += 1] = str
	end

	def close
		if @thread
			$defout = @old_out.dup
			$stderr = @old_err.dup
			$stdin = @old_in.dup
			@output[1].close
			@thread.join
		end
	end

	def redir
		@output = IO.pipe
		@old_out = $defout.dup
		@old_err = $stderr.dup
		@old_in = $stdin.dup
		$stdin = @dest.input[0]
		$defout = $stderr = @output[1]
		@output[1].fcntl(Fcntl::F_SETFL, File::NONBLOCK)
		@thread = Thread.new {
			while not @output[0].eof?
				select([@output[0]])
				@dest.write(@output[0].read)
			end
			@output[0].close
		}
	end
end

module Fox
  class FXIrb < FXText
	  include Singleton
	  include Responder

	  attr_reader :input

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

		  super
		  setFont(FXFont.new(FXApp.instance, "-misc-fixed-medium-r-semicondensed-*-*-120-*-*-c-*-iso8859-1"))
	  end

	  def create
		  super
		  setFocus
		  # IRB initialization
		  @redir = Redirect.new(self)
		  @input = IO.pipe
		  @irb = Thread.new {
			  IRB.start_in_fxirb(@redir)
		  }
	  end

	  def onKeyRelease(sender, sel, event)
		  if [Fox::KEY_Return, Fox::KEY_KP_Enter].include?(event.code)
			  newLineEntered
		  end
		  return 1
	  end

	  def newLineEntered
		  if @running
			  start = prevLine(getLength)
			  @input[1].puts(extractText(start, getLength - start))
		  else
			  processCommandLine(extractText(@anchor, getLength-@anchor))
		  end
	  end

	  def processCommandLine(cmd)
		  @redir.redir
		  @running = true
		  @input[1].puts cmd
	  end

	  def sendCommand(cmd)
		  setCursorPos(getLength)
		  makePositionVisible(getLength) unless isPosVisible(getLength)
		  cmd += "\n"
		  appendText(cmd)
		  processCommandLine(cmd)
	  end

	  def write(obj)
		  str = obj.to_s
		  appendText(str)
		  setCursorPos(getLength)
		  makePositionVisible(getLength) unless isPosVisible(getLength)
		  return str.length
	  end

	  def gets(prompt)
		  @running = false
		  @anchor = getLength
		  return @input[0].gets
	  end
  end
end

# Stand alone run
if __FILE__ == $0
  include Fox
  application = FXApp.new("FXIrb", "ruby")
  Thread.abort_on_exception = true
  application.init(ARGV)
  window = FXMainWindow.new(application, "FXIrb", nil, nil, DECOR_ALL, 0, 0, 580, 600)
  FXIrb.init(window, nil, 0, LAYOUT_FILL_X|LAYOUT_FILL_Y)
  application.create
  window.show(PLACEMENT_SCREEN)
  application.run
end
