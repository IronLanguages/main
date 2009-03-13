# Help text
HELP = <<END_OF_HELP


                                FOX Text Editor On-line Help


Introduction.

TextEdit is a handy little editor which is fast and easy to use.
It makes for a great file viewer, as well as a convenient editor for most
common editing tasks.
TextEdit supports unlimited undo/redo, X selection, clipboard, XDND drag and drop
capability, continuous word wrapping, auto-indent capability, brace matching,
convenient search and replace methods, and statement block selections.
TextEdit also supports the mouse wheel.
A nice feature of textedit is the ability to show a File Browser side by
side with the text.  Switching to another file is as easy as clicking on an
icon in the File Browser.


Starting a New Document.

To start a new document, invoke the New menu, or press on the New button on
the toolbar.  If the current document has not yet been saved, you will be prompted
to either save or abandon the current text.


Opening and Saving Files the Old Fashioned Way.

To open a file, you can invoke the Open menu, or press on the Open button on
the toolbar.  This will bring up the standard File Dialog, which allows you
to select a file.
To save a file, you can either invoke the Save or the Save As menu option.
The former saves the file back to the same filename, while the latter prompts
for an alternative filename.
You can quickly navigate to the desired file by typing the first few letters of
the filename and then pressing Enter (Return); use Backspace to move up one directory
level.  Control-H moves to your home directory, Control-W moves back to the current
working directory.
A nice convenience of the File Dialog is the ability to set bookmarks, so once
bookmarked, you can quickly move back to a previously visited directory.


Opening Files Using the File Browser.

An alternative method to open files is the File Browser.
You can display the File Browser by invoking the File Browser option under
the View menu.
To open a file using the File Browser, simply click on the file.
If there are many files, you may want to limit the number of files displayed
by specifying a file pattern in the Filter typein field.
The pattern is can be any regular file wildcard expression such as "*.cpp".
By default, the File Browser shows all files, i.e. the pattern is "*".
You can switch patterns by means of the combo box under file File Browser;
additional patterns for the combo box (and File Dialog) can be specified
in the Preferences Dialog.


Opening Recently Visited Files.

The recent file menu shows files which have been recently visited.
You can quickly get back to a file you've been editing simply by selecting
one of these recent files.


Opening Files by Drag and Drop.

Using a file browser such as PathFinder or other Konqueror or other XDND
compatible file browsers, you can simply drop a file into the text pane and
have TextEdit read this file.


Opening a Selected Filename

Selecting any filename, possibly in another application, and invoking the
"Open Selected" option causes TextEdit to open the selected file.
When the selected filename is of the form:

   #include "filename.h"

or:

   #include <filename.h>

then TextEdit will search for this file in a sequence of include directories,
otherwise it will search in the same directory as the currently loaded file.
You can specify the list of include directories to search with the "Include Path"
option.
When the selected filename is of the form:

   filename.cpp:177

then TextEdit will not only load the filename, but also jump to the given line
number.  If this file has already been loaded, TextEdit will simply jump to the
given line number in the current file.
This option is very useful when fixing compiler errors.


Mouse Selection.

You can move the cursor by simply clicking on the desired location with the left mouse button.
To highlight some text, press the mouse and drag the mouse while holding the left button.
To select text a word at a time, you can double-click and drag;
to select text a line at a time, you can triple-click and drag.
Performing a shift-click extends the selection from the last cursor location to the
current one.
When selecting words, words are considered to extend from the clicked position up to
a blank or word-delimiting character.  The latter may depend on the programming language,
and so TextEdit offers a way to change the set of delimiter characters.
The default set of delimiters is "!"#\$%&'()*+,-./:;<=>?@[\\]^`{|}~".


Scrolling Text.

Using the right mouse button, you can grab the text and scroll it.
a right mouse drag is a very convenient way to scroll the text buffer by small amount
as the scrolling is exactly proportional to the mouse movement.
You can of course also use the scroll bars.  Because scrolling becomes awkward when
dealing with large amounts of text, you can do a fine scroll or vernier-scroll by
holding the shift or control keys while moving the scroll bars.
TextEdit can also take advantage of a wheel mouse; simply point the mouse inside the
text area and use the wheel to scroll it up and down.  Holding the Control-key while
operating the wheel makes the scrolling go faster, by smoothly scrolling one page at
a time.  To scroll horizontally, simply point the mouse at the horizontal scroll bar.
In fact, any scrollable control (including the File Browser), can be scrolled by
simply pointing the cursor over it and using the mouse wheel.
You can adjust the number of lines scrolled for each wheel notch by means of the
Preferences dialog.


The Clipboard.

After selecting some text, you can cut or copy this text to the clipboard.
A subsequent paste operation will then insert the contents of the clipboard at
the current cursor location.
If some text has been selected in another application, then you can paste this
text by placing the cursor at the right spot in your text and invoking the paste
command.


The X Selection.

When text is selected anywhere (possibly in another application), TextEdit can
paste this text into the current text buffer by means of the middle mouse button
or by pressing the wheel-button if you have a wheel mouse.  Note that while holding
the button, the insertion point can be moved by moving the mouse:- TextEdit will
only insert the text when you release the button.


Text Drag and Drop.

After selecting some text, you can drag this text to another location by pressing the
middle mouse button; because TextEdit is fully drag and drop enabled, you can not only
drag a selection from one place to another inside the text buffer, but also between different
TextEdit applications, or even from TextEdit to another drag and drop enabled application
or vice-versa.
Within the same text window, the drag defaults to a text-movement.  You can change this to
a text copy by holding down the Control key while you're dragging.
Between one text window and another, the drag defaults to a copy operation
you can change this to a text movement by holding down the Shift key while dragging.


Undo and Redo.

TextEdit support unlimited (well, the limit is large...) undo and redo capability.
Each time you insert, remove, or replace some text, TextEdit remembers what you did.
If you make a mistake, you can undo the last command, and the one before that, and so on.
Having invoked undo many times, it is sometimes desirable to invoke the redo command, i.e.
to perform the original editing operation again.  Thus, you can move backward or forward in
time.
However if, after undoing several commands, you decide edit the buffer in a different way, then
you will no longer be able to redo the undone commands:- you have now taken a different path.
When you first load a file, or just after you save it, TextEdit remembers that this version of
the text was special; while subsequent editing commands can be undone individually, you can always
quickly return to this special version of the text by means of the revert command.


Keyboard Bindings.

The following table lists the keyboard bindings.

Key:\tAction:
====\t=======

Up\tMove cursor up.
Shift+Up\tMove cursor up and extend selection.
Down\tMove cursor down.
Shift+Down\tMove cursor down and extend selection.
Left\tMove cursor left.
Shift+Left\tMove cursor left and extend selection.
Right\tMove cursor right.
Shift+Right\tMove cursor right and extend selection.
Home\tMove cursor to begin of line.
Shift+Home\tMove cursor to begin of line and extend selection.
Ctl+Home\tMove cursor to top of text.
End\tMove cursor to end of line.
Ctl+End\tMove cursor to bottom of text.
Shift+End\tMove cursor to end of line and extend selection.
Page Up\tMove cursor up one page.
Shift+Page Up\tMove cursor up one page and extend selection.
Page Down\tMove cursor down one page.
Shift+Page Down\tMove cursor down one page and extend selection.
Delete\tDelete character after cursor, or text selection.
Back Space\tDelete character before cursor, or text selection.
Ctl+A\tSelect all text.
Ctl+X\tCut selected text to clipboard.
Ctl+C\tCopy selected text to clipboard.
Ctl+V\tPaste text from clipboard.
Ctl+K\tDelete current line.
Ctl+H\tScroll current line to top of window.
Ctl+M\tScroll current line to center of window.
Ctl+L\tScroll current line to bottom of window.


Changing Font.

You can change font by invoking the Font Selection Dialog from the Font menu.
The Font Dialog displays four list boxes showing the font Family, Weight, Style,
and Size of each font. 
You can narrow down the number of fonts displayed by selecting a specific character
set, setwidth, pitch, and whether or not scalable fonts are to be listed only.
The All Fonts checkbutton causes all fonts to be listed. Use this feature if you
need to select old-style X11 bitmap fonts.
The Preview window shows a sample of text in the selected font.


Changing colors.

The four color wells on the status line determine the color of the
selection background, selection foreground, and normal background and foreground
respectively.  Double clicking on a colorwell will bring up the Color Selection
Dialog.
The Color Selection Dialog sypports several ways to select a color: 

1\tBy means of Red, Green, and Blue mixing.
2\tBy means of Hue, Saturation, Value mixing.
3\tBy means of Cyan, Magenta, and Yellow mixing.
4\tBy name.
5\tUsing one of the predefined color wells.

You can also drag and drop colors, and change the predefined colorwells
to hold your own custom color collections.


Configuration Issues.

The File Browser keeps an association list in the registry database to associate
a particular file extension to an icon and other descriptive information. 
The following is an example of how this could be filled in: 

  [SETTINGS]
  iconpath = /usr/share/icons:/home/jeroen/icons

  [FILETYPES]
  cpp = "/usr/local/bin/textedit %s &;C++ Source File;c_src.xpm;mini/c_src.xpm"


This example shows how the extension ".cpp" is bound to the program "textedit"
and is associated with two icons, a big icon "c_src.xpm" and a small icon "mini/c_src.xpm"
which are to be found in either directory "/usr/share/icons" or "/home/jeroen/icons".
END_OF_HELP

require 'fox16'

include Fox

class HelpWindow < FXDialogBox
  def initialize(owner)
    super(owner, "Help on TextEdit", DECOR_TITLE|DECOR_BORDER|DECOR_RESIZE,
      0, 0, 0, 0, 6, 6, 6, 6, 4, 4)
  
    # Bottom part
    closebox = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_BOTTOM|LAYOUT_FILL_X|PACK_UNIFORM_WIDTH)
    closeBtn = FXButton.new(closebox, "&Close", nil, self, FXDialogBox::ID_ACCEPT,
      LAYOUT_RIGHT|FRAME_RAISED|FRAME_THICK)
    closeBtn.padLeft = 20
    closeBtn.padRight = 20
    closeBtn.padTop = 5
    closeBtn.padBottom = 5

    # Editor part
    editbox = FXHorizontalFrame.new(self,
      LAYOUT_SIDE_TOP|LAYOUT_FILL_X|LAYOUT_FILL_Y|FRAME_SUNKEN|FRAME_THICK)
    editbox.padLeft = 0
    editbox.padRight = 0
    editbox.padTop = 0
    editbox.padBottom = 0
    helptext = FXText.new(editbox, nil, 0,
      TEXT_READONLY|TEXT_WORDWRAP|LAYOUT_FILL_X|LAYOUT_FILL_Y)
    helptext.visibleRows = 50
    helptext.visibleColumns = 60
  
    # Fill with help
    helptext.text = HELP
    helptext.tabColumns = 35
  end
end
