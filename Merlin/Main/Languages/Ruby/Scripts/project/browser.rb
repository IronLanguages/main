require 'rubygems'
require 'watir'
require 'logger'

class Browser
  attr_reader :ie
  
  PROGNAME = "BROWSER"

  def initialize(url, log = Logger.new(STDOUT), hidden = true, speed = :zippy)
    @log = log
    @log.debug(PROGNAME){ "starting" }
    if @log.debug?
      hidden = false
      speed = :slow
    end
    Watir::IE.set_options :visible => !hidden, :speed => speed
    @ie = Watir::IE.start(url)
  end

  def finish_loading(state = :normal)
    @ie.wait
    iter = 0
    max_iters = 20
    while yield(@ie)
      if iter >= max_iters
        msg = "Loading timed out!"
        @log.fatal msg
        raise msg
      end
      @log.debug(PROGNAME){ "loading" } if iter == 0 && %W(start normal).include?(state.to_s)
      sleep 0.1
      iter += 1
    end
  end

  def done
    @log.debug(PROGNAME){ "closing" }
    @ie.close
  end

  def method_missing(m, *args)
    begin
      @ie.send(m, *args)
    rescue
      super(m, *args)
    end
  end
end
