require 'fox16'
require 'fox16/undolist'

include Fox

# Undo record for text fragment
class FXTextCommand < FXCommand

  def initialize(txt, change)
    @text = txt
    @buffer = nil
    @pos = change.pos
    @numCharsDeleted = change.ndel
    @numCharsInserted = change.nins
  end

  def size
    (@buffer != nil) ? @buffer.size : 0
  end
end

# Insert command
class FXTextInsert < FXTextCommand

  def undoName
    "Undo insert"
  end

  def redoName
    "Redo insert"
  end

  # Undoing an insert removes the previously inserted text
  def undo
    @buffer = @text.extractText(@pos, @numCharsInserted)
    @text.removeText(@pos, @numCharsInserted)
    @text.cursorPos = @pos
    @text.makePositionVisible(@pos)
  end

  # Redoing an insert re-inserts the same text
  def redo
    @text.insertText(@pos, @buffer)
    @text.cursorPos = @pos + @numCharsInserted
    @text.makePositionVisible(@pos + @numCharsInserted)
    @buffer = nil
  end
end

# Delete command
class FXTextDelete < FXTextCommand
  def initialize(txt, change)
    super(txt, change)
    @buffer = change.del
  end

  def undoName
    "Undo delete"
  end

  def redoName
    "Redo delete"
  end

  # Undoing a delete re-inserts the deleted text
  def undo
    @text.insertText(@pos, @buffer)
    @text.cursorPos = @pos + @buffer.length
    @text.makePositionVisible(@pos + @buffer.length)
    @buffer = nil
  end

  # Redoing a delete removes it again
  def redo
    @buffer = @text.extractText(@pos, @numCharsDeleted)
    @text.removeText(@pos, @buffer.length)
    @text.cursorPos = @pos
    @text.makePositionVisible(@pos)
  end
end

# Replace command
class FXTextReplace < FXTextCommand
  def initialize(txt, change)
    super(txt, change)
    @buffer = change.del
  end

  def undoName
    "Undo replace"
  end

  def redoName
    "Redo replace"
  end

  # Undoing a replace reinserts the old text
  def undo
    tmp = @text.extractText(@pos, @numCharsInserted)
    @text.replaceText(@pos, @numCharsInserted, @buffer)
    @text.cursorPos = @pos + @buffer.length
    @text.makePositionVisible(@pos + @buffer.length)
    @buffer = tmp
  end

  # Redo a replace reinserts the new text
  def redo
    tmp = @text.extractText(@pos, @numCharsDeleted)
    @text.replaceText(@pos, @numCharsDeleted, @buffer)
    @text.cursorPos = @pos + @numCharsInserted
    @text.makePositionVisible(@pos + @numCharsInserted)
    @buffer = tmp
  end
end
