#####################################################################################
#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#
# This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Apache License, Version 2.0, please send an email to 
# ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Apache License, Version 2.0.
#
# You must not remove this notice, or any other, from this software.
#
#
#####################################################################################

import clr
clr.AddReferenceByPartialName("Microsoft.Office.Interop.Word")
from Microsoft.Office.Interop.Word import ApplicationClass

import System

clr.AddReference("System.Drawing")
import System.Drawing

clr.AddReference("System.Windows.Forms")
from System.Windows.Forms import Form

import sys
sys.path.append(r"c:\python24\lib")

## Let's us clean up the Word process on exit -- see _cleanup().
import atexit


###
### Accessing Word
###

_word_object = None

def _get_word_object ():
    global _word_object
    if not _word_object: _word_object = ApplicationClass()
    return _word_object

def _cleanup ():
    _word_object.Quit()
atexit.register(_cleanup)


###
### Public Operations
###

### Returns True if word is spelled correctly, otherwise false.
def check_word (word):
    w = _get_word_object()
    return _word_object.CheckSpelling(word)


### Returns a list of possible corrections for word.  Returns an empty list
### if word is spelled correctly.
def get_suggestions (word):
    w = _get_word_object()
    if w.Documents.Count < 1:
        ## Must have a document, or can't call GetSpellingSuggestions.
        w.Documents.Add()
    suggestions_objects = w.GetSpellingSuggestions(word)
    return [x.Name for x in suggestions_objects]


### Return word if it is correct, or interact with the user to get a correction.
### If the user ignores the word, or enters a custom correction, the returned
### word could be incorrect.
def correct_word (word):
    if check_word(word): return word
    words = get_suggestions(word)
    f = correction_dialog()
    f.TopMost = True
    for w in words: f.suggestionsListBox.Items.Add(w)
    f.show_word_in_context(word, word, 0)
    action = f.ShowDialog()
    if action == System.Windows.Forms.DialogResult.Ignore:
        ## Ignore button.
        return word
    elif action == System.Windows.Forms.DialogResult.Abort:
        ## Stop checking button.
        return None
    elif action == System.Windows.Forms.DialogResult.No:
        ## Ignore All button was pressed, save word in ignore list.
        return word
    elif action == System.Windows.Forms.DialogResult.OK or \
         action == System.Windows.Forms.DialogResult.Retry:
        ## Replace or Use Custom button was pressed.
        return f.result_text()
    #f.Close()


###
### UI
###

class correction_dialog (Form):
    def __init__(self):
        self._result_text = ""
        self._next_start = 0
        
        self.errorTextBox = System.Windows.Forms.RichTextBox()
        self.suggestionsListBox = System.Windows.Forms.ListBox()
        self.ignoreButton = System.Windows.Forms.Button()
        self.ignoreAllButton = System.Windows.Forms.Button()
        self.replaceButton = System.Windows.Forms.Button()
        self.fixesLabel = System.Windows.Forms.Label()
        self.stopButton = System.Windows.Forms.Button()
        self.customLabel = System.Windows.Forms.Label()
        self.customTextBox = System.Windows.Forms.TextBox()
        self.useCustomButton = System.Windows.Forms.Button()
        self.SuspendLayout()
        ## 
        ## errorTextBox
        ## 
        self.errorTextBox.Location = System.Drawing.Point(12, 12)
        self.errorTextBox.Name = "errorTextBox"
        self.errorTextBox.Size = System.Drawing.Size(426, 156)
        self.errorTextBox.TabIndex = 0
        self.errorTextBox.Text = ""
        ## 
        ## suggestionsListBox
        ## 
        self.suggestionsListBox.FormattingEnabled = True
        self.suggestionsListBox.Location = System.Drawing.Point(12, 195)
        self.suggestionsListBox.Name = "suggestionsListBox"
        self.suggestionsListBox.Size = System.Drawing.Size(345, 108)
        self.suggestionsListBox.TabIndex = 1
        self.suggestionsListBox.DoubleClick \
            += self.suggestionsListBox_DoubleClick
        self.suggestionsListBox.SelectedIndexChanged \
            += self.suggestionsListBox_SelectedIndexChanged
        ## 
        ## ignoreButton
        ## 
        self.ignoreButton.DialogResult \
            = System.Windows.Forms.DialogResult.Ignore
        self.ignoreButton.Location = System.Drawing.Point(363, 195)
        self.ignoreButton.Name = "ignoreButton"
        self.ignoreButton.Size = System.Drawing.Size(75, 23)
        self.ignoreButton.TabIndex = 3
        self.ignoreButton.Text = "&Ignore"
        ## 
        ## ignoreAllButton
        ## 
        self.ignoreAllButton.DialogResult \
            = System.Windows.Forms.DialogResult.No
        self.ignoreAllButton.Location = System.Drawing.Point(363, 224)
        self.ignoreAllButton.Name = "ignoreAllButton"
        self.ignoreAllButton.Size = System.Drawing.Size(75, 23)
        self.ignoreAllButton.TabIndex = 4
        self.ignoreAllButton.Text = "Ignore &All"
        ## 
        ## replaceButton
        ## 
        self.replaceButton.DialogResult = System.Windows.Forms.DialogResult.OK
        self.replaceButton.Location = System.Drawing.Point(363, 253)
        self.replaceButton.Name = "replaceButton"
        self.replaceButton.Size = System.Drawing.Size(75, 23)
        self.replaceButton.TabIndex = 5
        self.replaceButton.Text = "&Replace"
        ## 
        ## fixesLabel
        ## 
        self.fixesLabel.AutoSize = True
        self.fixesLabel.Location = System.Drawing.Point(12, 179)
        self.fixesLabel.Name = "fixesLabel"
        self.fixesLabel.Size = System.Drawing.Size(85, 13)
        self.fixesLabel.TabIndex = 6
        self.fixesLabel.Text = "&Suggested fixes:"
        ## 
        ## stopButton
        ## 
        self.stopButton.DialogResult = System.Windows.Forms.DialogResult.Abort
        self.stopButton.Location = System.Drawing.Point(363, 282)
        self.stopButton.Name = "stopButton"
        self.stopButton.Size = System.Drawing.Size(75, 23)
        self.stopButton.TabIndex = 7
        self.stopButton.Text = "Stop Check"
        ## 
        ## customLabel
        ## 
        self.customLabel.AutoSize = True
        self.customLabel.Location = System.Drawing.Point(11, 316)
        self.customLabel.Name = "customLabel"
        self.customLabel.Size = System.Drawing.Size(92, 13)
        self.customLabel.TabIndex = 8
        self.customLabel.Text = "Use custom word:"
        ## 
        ## customTextBox
        ## 
        self.customTextBox.Location = System.Drawing.Point(106, 313)
        self.customTextBox.Name = "customTextBox"
        self.customTextBox.Size = System.Drawing.Size(251, 20)
        self.customTextBox.TabIndex = 9
        ## 
        ## useCustomButton
        ## 
        self.useCustomButton.DialogResult \
            = System.Windows.Forms.DialogResult.Retry
        self.useCustomButton.Location = System.Drawing.Point(363, 311)
        self.useCustomButton.Name = "useCustomButton"
        self.useCustomButton.Size = System.Drawing.Size(75, 23)
        self.useCustomButton.TabIndex = 10
        self.useCustomButton.Text = "Use Custom"
        self.useCustomButton.Click \
            += self.useCustomButton_Click
        ## 
        ## CheckForm
        ## 
        self.AcceptButton = self.replaceButton
        self.AutoScaleDimensions = System.Drawing.SizeF(6.0, 13.0)
        self.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        self.CancelButton = self.ignoreButton
        self.ClientSize = System.Drawing.Size(450, 345)
        self.ControlBox = False
        self.Controls.Add(self.useCustomButton)
        self.Controls.Add(self.customTextBox)
        self.Controls.Add(self.customLabel)
        self.Controls.Add(self.stopButton)
        self.Controls.Add(self.fixesLabel)
        self.Controls.Add(self.replaceButton)
        self.Controls.Add(self.ignoreAllButton)
        self.Controls.Add(self.ignoreButton)
        self.Controls.Add(self.suggestionsListBox)
        self.Controls.Add(self.errorTextBox)
        self.Name = "CheckForm"
        self.ShowIcon = False
        self.ShowInTaskbar = False
        self.StartPosition \
            = System.Windows.Forms.FormStartPosition.CenterScreen
        self.Text = "Spell check"
        self.ResumeLayout(False)
        self.PerformLayout()

    def suggestionsListBox_DoubleClick (self, sender, event):
        self._result_text = sender.SelectedItem;
        self.DialogResult = System.Windows.Forms.DialogResult.OK;

    def suggestionsListBox_SelectedIndexChanged (self, *args):
        self._result_text = self.suggestionsListBox.Text

    def useCustomButton_Click (self, *args):
        self._result_text = self.customTextBox.Text

    ### Show the word, which occurs within line at line_start, as a bold, red,
    ### italic word so that it stands out within the line of text.
    def show_word_in_context (self, word, line, line_start):
        self.errorTextBox.Text = line;
        self.errorTextBox.Select(line_start, len(word));
        self.errorTextBox.SelectionColor = System.Drawing.Color.Red;
        self.errorTextBox.SelectionFont \
            = System.Drawing.Font \
                 (self.errorTextBox.SelectionFont, \
                  System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic);
        self.errorTextBox.Select(line_start, 0);
        self._next_start = line_start + len(word);

    def result_text (self):
        return self._result_text

    def next_start_position (self):
        return self._next_start



