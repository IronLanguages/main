#
# Calendar widget, contributed by
# David Naseby (david.naseby@eonesolutions.com.au).
#
# Still could use methods for:
#
# * Get and set the day-of-the-month button properties (e.g. "poppy"
#   style buttons versus regular pushbuttons)
# * Setting the date explicitly, possibly also messages like
#   "advance/back up one day", "advance/back up one month", etc.
#
# * Should it have a display-only mode? In other words, buttons
#   aren't clickable, just labels?
#
# Harry Ohlsen (harryo@zip.com.au) also suggests a facility to jump
# backward or forward a year at a time.
#

require 'fox16/colors'

module Fox

  # Calendar-specific options
  CALENDAR_NORMAL = 0
  CALENDAR_READONLY = 0x00020000

  #
  # Calendar widget
  #
  # == Events
  #
  # The following messages are sent by FXCalendar to its target:
  #
  # +SEL_COMMAND+::	sent when a day button is clicked; the message data is a Time object indicating the selected date
  #
  # == Calendar options
  #
  # +CALENDAR_NORMAL+::		normal display mode
  # +CALENDAR_READONLY+::	read-only mode
  #
  class FXCalendar < FXVerticalFrame

    DAYS = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT']

    # Currently selected date [Time]
    attr_reader :selected
    
    # Font used for days [FXFont]
    attr_reader :dayLabelFont
    
    #
    # Returns an initialized calendar widget
    #
    def initialize(parent, initial_date=Time.now, tgt=nil, sel=0, opts=0, x=0, y=0, w=0, h=0, pl=DEFAULT_PAD, pr=DEFAULT_PAD, pt=DEFAULT_PAD, pb=DEFAULT_PAD)
      # Initialize the base class first
      super(parent, opts, x, y, w, h, pl, pr, pt, pb)

      # Save target and selector
      self.target = tgt
      self.selector = sel

      @headerBGColor = FXColor::Black
      @headerFGColor = FXColor::White
      @dayLabelFont = FXFont.new(getApp, "helvetica", 7)
      
      @date_showing = initial_date
      
      # Header row
      @header = FXHorizontalFrame.new(self, LAYOUT_FILL_X)
      @header.backColor = @headerBGColor
      @backBtn = FXArrowButton.new(@header, nil, 0, FRAME_RAISED|FRAME_THICK|ARROW_LEFT|ARROW_REPEAT)
      @backBtn.connect(SEL_COMMAND) do |send, sel, ev|
        @date_showing = _last_month
        _build_date_matrix
        @current_month.text = _header_date
      end  
      @current_month = FXLabel.new(@header, _header_date, nil,
        LAYOUT_FILL_X|JUSTIFY_CENTER_X|LAYOUT_FILL_Y)
      @current_month.backColor = @headerBGColor
      @current_month.textColor = @headerFGColor
      @foreBtn = FXArrowButton.new(@header, nil, 0, FRAME_RAISED|FRAME_THICK|ARROW_RIGHT|ARROW_REPEAT)
      @foreBtn.connect(SEL_COMMAND) do |send, sel, ev|
        @date_showing = _next_month
        _build_date_matrix
        @current_month.text = _header_date
      end
      
      @matrix = FXMatrix.new(self, 7,
        MATRIX_BY_COLUMNS|LAYOUT_FILL_X|LAYOUT_FILL_Y|PACK_UNIFORM_WIDTH|FRAME_RAISED,
        0, 0, 0, 0, 0, 0, 0, 0)
      DAYS.each { |day| _add_matrix_label(day) }
      (7*6 - 1).times do
        s = FXSwitcher.new(@matrix,
          LAYOUT_FILL_X|LAYOUT_FILL_Y|LAYOUT_FILL_ROW|LAYOUT_FILL_COLUMN, 0,0,0,0,0,0,0,0)
        FXFrame.new(s, LAYOUT_FILL_X|LAYOUT_FILL_Y, 0,0,0,0,0,0,0,0)
        btn = FXButton.new(s, '99', nil, nil, 0,
          LAYOUT_FILL_X|LAYOUT_FILL_Y|FRAME_RAISED|FRAME_THICK,
          0,0,0,0,0,0,0,0)
        btn.connect(SEL_COMMAND) do |send, sel, ev|
          @selected = Time.local(@date_showing.year, @date_showing.month,
                                 send.text.to_i)
          target.handle(self, MKUINT(selector, SEL_COMMAND), @selected) if target 
        end
      end
      _build_date_matrix()
    end
    
    # Return the current header background color
    def headerBackColor
      @headerBGColor
    end
    
    # Set the header background color
    def headerBackColor=(clr)
      @headerBGColor = clr
      @header.backColor = clr
      @current_month.backColor = clr
      DAYS.each_index { |i| @matrix.childAtIndex(i).backColor = clr }
    end
    
    # Return the current header text color
    def headerTextColor
      @headerFGColor
    end
    
    # Set the header text color
    def headerTextColor=(clr)
      @headerFGColor = clr
      @current_month.textColor = clr
      DAYS.each_index { |i| @matrix.childAtIndex(i).textColor = clr }
    end
    
    # Change the font used for the days of the weeks
    def dayLabelFont=(font)
      if @dayLabelFont != font
        @dayLabelFont = font
        DAYS.each_index { |i| @matrix.childAtIndex(i).font = font }
	update
      end
    end
    
    private   
    def _header_date()
      @date_showing.strftime("%B, %Y")
    end
    
    def _first_day
      Time.local(@date_showing.year, @date_showing.month, 1)
    end
    
    def _last_day
      year = @date_showing.year
      month = @date_showing.month+1
      if month > 12
        year += 1
        month = 1
      end
      Time.local(year, month, 1) - (60*60*24)
    end
    
    def _last_month
      year = @date_showing.year
      month = @date_showing.month - 1
      if month < 1
        year -= 1
        month = 12
      end
      Time.local(year, month)
    end
    
    def _next_month
      year = @date_showing.year
      month = @date_showing.month + 1
      if month > 12
        year +=1
        month = 1
      end
      Time.local(year, month)
    end
    
    def _build_date_matrix
      (0...6*7-1).each do |index|
        @matrix.childAtRowCol(index/7+1, index.modulo(7)).setCurrent(0)
      end
      (_first_day.wday... _last_day.day+_first_day.wday).each do |index|
        day = index - _first_day.wday + 1
        switcher = @matrix.childAtRowCol(index/7 + 1, index.modulo(7))
        switcher.setCurrent(1)
        switcher.childAtIndex(1).text = day.to_s
      end
    end
    
    def _add_matrix_label(label)
      l = FXLabel.new(@matrix, label, nil,
        LAYOUT_FILL_X|LAYOUT_FILL_COLUMN|JUSTIFY_CENTER_X|FRAME_SUNKEN)
      l.backColor = @headerBGColor
      l.textColor = @headerFGColor
      l.font = @dayLabelFont
    end
  end
end       

if __FILE__ == $0

  include Fox

  app = FXApp.new('Calendar', 'FXRuby')
  app.init(ARGV)
  mainwin = FXMainWindow.new(app, "Calendar Test", nil, nil, DECOR_ALL, 0, 0, 500, 500)
  calendar = FXCalendar.new(mainwin, Time.now, nil, 0, LAYOUT_FILL_X|LAYOUT_FILL_Y)
  calendar.connect(SEL_COMMAND) do |sender, sel, data|
    puts data.to_s
  end
  app.create
  mainwin.show(PLACEMENT_SCREEN)
  app.run
end
