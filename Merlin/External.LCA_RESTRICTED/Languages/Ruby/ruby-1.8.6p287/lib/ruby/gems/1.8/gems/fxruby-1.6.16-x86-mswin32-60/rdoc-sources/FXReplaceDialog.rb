module Fox
  #
  # Search and replace dialog box.
  #
  # === Message identifiers
  #
  # +ID_NEXT+::         x
  # +ID_PREV+::         x
  # +ID_SEARCH_UP+::    x
  # +ID_SEARCH_DN+::    x
  # +ID_REPLACE_UP+::   x
  # +ID_REPLACE_DN+::	x
  # +ID_ALL+::		x
  # +ID_DIR+::   	x
  # +ID_SEARCH_TEXT+::	x
  # +ID_REPLACE_TEXT+::	x
  # +ID_MODE+::		x
  #
  class FXReplaceDialog < FXDialogBox
    #
    # Search matching mode, one of the following:
    #
    # +DONE+::		Cancel search
    # +SEARCH+::	Search first occurrence
    # +REPLACE+::	Replace first occurrence
    # +SEARCH_NEXT+::	Search next occurrence
    # +REPLACE_NEXT+::	Replace next occurrence
    # +REPLACE_ALL+::	Replace all occurrences
    #
    attr_accessor :searchMode
    
    # Text or pattern to search for [String]
    attr_accessor :searchText
    
    # Replacement text [String]
    attr_accessor :replaceText
  
    #
    # Return an initialized FXReplaceDialog instance.
    #
    # ==== Parameters:
    #
    # +owner+::		the owner window for this dialog box [FXWindow]
    # +caption+::	the caption (title) string for this dialog box [String]
    # +ic+::		the icon [FXIcon]
    # +opts+::		the options [Integer]
    # +x+::		initial x-position [Integer]
    # +y+::		initial y-position [Integer]
    # +width+::		initial width [Integer]
    # +height+::		initial height [Integer]
    #
    def initialize(owner, caption, ic=nil, opts=0, x=0, y=0, width=0, height=0) # :yield: theReplaceDialog
    end
  end
end

