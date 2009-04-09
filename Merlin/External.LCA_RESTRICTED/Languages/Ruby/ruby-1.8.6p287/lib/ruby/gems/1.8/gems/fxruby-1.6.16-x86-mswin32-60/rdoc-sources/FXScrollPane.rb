module Fox
  #
  # A Scroll Pane is a menu pane which provides scrolling of menu entries.
  # It is useful when menus are populated programmatically and it is not
  # known in advance how many entries will be added.
  #
  class FXScrollPane < FXMenuPane

    # Index of top-most menu item [Integer]
    attr_accessor :topItem
    
    # Number of visible items [Integer]
    attr_accessor :numVisible

    #
    # Return an initialized FXScrollPane instance.
    #
    # ==== Parameters:
    #
    # +owner+::	owner window for this menu pane [FXWindow]
    # +nvis+::	maximum number of visible items [Integer]
    # +opts+::	options [Integer]
    #
    def initialize(owner, nvis, opts=0) # :yields: theScrollPane
    end
  end
end

