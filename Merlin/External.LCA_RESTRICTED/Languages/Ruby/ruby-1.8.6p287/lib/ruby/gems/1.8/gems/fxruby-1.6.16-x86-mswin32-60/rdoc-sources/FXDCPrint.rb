module Fox
  #
  # Bounding box
  #
  class FXPSBounds
  
    # Minimum x-coordinate [Float]
    attr_accessor :xmin
    
    # Maximum x-coordinate [Float]
    attr_accessor :xmax
    
    # Minimum y-coordinate [Float]
    attr_accessor :ymin
    
    # Maximum y-coordinate
    attr_accessor :ymax
    
    # Returns an initialized FXPSBounds instance
    def initialize; end
  end

  
  #
  # Describes printer
  #
  # === Printer flags
  #
  # +PRINT_DEST_PAPER+::	Send print to paper
  # +PRINT_DEST_FILE+::		Send print to file
  # +PRINT_PAGES_ALL+::		Print all pages
  # +PRINT_PAGES_EVEN+::	Print even pages only
  # +PRINT_PAGES_ODD+::		Print odd pages only
  # +PRINT_PAGES_RANGE+::	Print range of pages
  # +PRINT_COLLATE_NORMAL+::	Normal collate order
  # +PRINT_COLLATE_REVERSED+::	Reversed collate order
  # +PRINT_PORTRAIT+::		Portrait orientation
  # +PRINT_LANDSCAPE+::		Landscape orientation
  # +PRINT_BLACKANDWHITE+::	Black and white output
  # +PRINT_COLOR+::		Color output
  # +PRINT_NOBOUNDS+::		Must determine bounding box
  #
  # === Printer media size
  #
  # +MEDIA_CUSTOM+::		Custom paper size
  # +MEDIA_USLETTER+::		US Letter size
  # +MEDIA_LEGAL+::		US Legal size
  # +MEDIA_A4+::		A4
  # +MEDIA_ENVELOPE+::		#10 Envelope
  #
  class FXPrinter
    # Printer name [String]
    attr_accessor :name
    
    # First page that can be printed [Integer]
    attr_accessor :firstpage
    
    # Last page that can be printed [Integer]
    attr_accessor :lastpage
    
    # Current page to print [Integer]
    attr_accessor :currentpage
    
    # On output, this is the first page to print [Integer]
    attr_accessor :frompage
    
    # On output, last page to print [Integer]
    attr_accessor :topage
    
    #
    # Media size index, one of +MEDIA_CUSTOM+, +MEDIA_USLETTER+, +MEDIA_LEGAL+,
    # +MEDIA_A4+ or +MEDIA_ENVELOPE+ [Integer]
    #
    attr_accessor :mediasize
    
    # Width of paper in points (1/72 of an inch) [Float]
    attr_accessor :mediawidth
    
    # Height of paper in points [Float]
    attr_accessor :mediaheight
    
    # Left margin [Float]
    attr_accessor :leftmargin
    
    # Right margin [Float]
    attr_accessor :rightmargin
    
    # Top margin [Float]
    attr_accessor :topmargin
    
    # Bottom margin [Float]
    attr_accessor :bottommargin
    
    # Number of copies [Integer]
    attr_accessor :numcopies
    
    # Flags [Integer]
    attr_accessor :flags

    # Returns an initialized FXPrinter instance
    def initialize; end
  end

  #
  # Postscript Printer Device Context
  #
  class FXDCPrint < FXDC
    # Returns an initialized FXDCPrint instance.
    def initialize(app)
    end

    #
    # Generate print job epilog.
    # See also #beginPrint.
    #
    def endPrint(); end
  
    #
    # Generate end of page.
    # See also #beginPage.
    #
    def endPage(); end
    
    def setContentRange(pxmin, pymin, pxmax, pymax); end
  end
end

