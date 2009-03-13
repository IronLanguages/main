module Fox
  #
  # The FXMatrix layout manager automatically arranges its child windows
  # in rows and columns. If the matrix style is +MATRIX_BY_ROWS+, then
  # the matrix will have the given number of rows and the number of columns
  # grows as more child windows are added; if the matrix style is +MATRIX_BY_COLUMNS+,
  # then the number of columns is fixed and the number of rows grows as more children
  # are added.  
  # If all children in a row (column) have the +LAYOUT_FILL_ROW+ (+LAYOUT_FILL_COLUMN+)
  # hint set, then the row (column) will be stretchable as the matrix layout manager
  # itself is resized.  If more than one row (column) is stretchable, the space is
  # apportioned to each stretchable row (column) proportionally.
  # Within each cell of the matrix, all other layout hints are observed.  
  # For example, a child having +LAYOUT_CENTER_Y+ and +LAYOUT_FILL_X+ hints will
  # be centered in the Y-direction, while being stretched in the X-direction.
  # Empty cells can be obtained by simply placing a borderless FXFrame widget
  # as a space-holder.
  #
  # === Matrix packing options
  #
  # +MATRIX_BY_ROWS+::		Fixed number of rows, add columns as needed
  # +MATRIX_BY_COLUMNS+::	Fixed number of columns, adding rows as needed
  #
  class FXMatrix < FXPacker

    # Matrix style [Integer]
    attr_accessor :matrixStyle
    
    # Number of rows [Integer]
    attr_accessor :numRows
    
    # Number of columns [Integer]
    attr_accessor :numColumns

    #
    # Construct a matrix layout manager with _n_ rows or columns
    #
    def initialize(parent, n=1, opts=MATRIX_BY_ROWS, x=0, y=0, width=0, height=0, padLeft=DEFAULT_SPACING, padRight=DEFAULT_SPACING, padTop=DEFAULT_SPACING, padBottom=DEFAULT_SPACING, hSpacing=DEFAULT_SPACING, vSpacing=DEFAULT_SPACING) # :yields: theMatrix
    end

    #
    # Obtain the child placed at a certain _row_ and _column_.
    #
    def childAtRowCol(row, column); end
    
    #
    # Return the row in which the given _child_ is placed.
    #
    def rowOfChild(child); end
    
    #
    # Return the column in which the given _child_ is placed.
    #
    def colOfChild(child); end
  end
end

