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

#--IMPORTS---------------------------------------------------------------------
import sys
import clr
from   spelling import *

clr.AddReferenceByPartialName("PresentationCore")
clr.AddReferenceByPartialName("PresentationFramework") 
from System.Windows import Window, Application, HorizontalAlignment
from System.Windows.Controls import FlowDocumentScrollViewer, ComboBox
from System.Windows.Documents import FlowDocument, Paragraph, Run
from System.Windows.Media.Brushes import Black, PaleGreen, Red

#--GLOBALS---------------------------------------------------------------------
_temp_file = open(sys.argv[1], "r")
lines_in_file = _temp_file.readlines()
_temp_file.close()

main_window = Window()

#--FUNCTIONS-------------------------------------------------------------------
def create_paragraph(file_lines):
    #Create a "Paragraph" object to hold all text from the file
    paragraph = Paragraph()
    #Text in the paragraph will have a font size of 16
    paragraph.FontSize = 16.0

    #Go through every line in the file passed in from the command
    #line...
    for x in file_lines:
        #...looking for Python/Ruby/PowerShell style comments
        if "#" in x:

            #Split the line whenever a comment is found
            first, second = x.split("#", 1)

            #Add the first part of the line to the paragraph
            paragraph.Inlines.Add(Run(first + "#"))
            
            #Split the second part of the line based on whitespace
            for y in second.split():
                #If the word is misspelled...
                if not check_word(y):
                    #Add a drop down menu to offer alternative
                    #spelling suggestions
                    c = ComboBox()
                    c.Background = Red
                    c.HorizontalAlignment = HorizontalAlignment.Left
                    #Add the misspelled word to the drop down menu
                    #and make is the first item
                    c.AddText(y)
                    c.SelectedItem = y
                    #Offer alternative spellings in the drop down menu
                    for suggest in suggestions(y):
                        c.AddText(suggest)
                    #Add the combo box to the drop down menu
                    paragraph.Inlines.Add(c)
                else:
                    paragraph.Inlines.Add(Run(y))
                paragraph.Inlines.Add(" ")
            paragraph.Inlines.Add(Run("\n"))
        else:
            #If there's no comment in the line, just directly
            #add it to the paragraph
            paragraph.Inlines.Add(Run(x))
    return paragraph

#--MAIN------------------------------------------------------------------------
main_window.Title = "IronPython Comment Checker"
main_window.Background = Black
main_window.Foreground = PaleGreen
main_window.Content = FlowDocumentScrollViewer()
main_window.Content.Document = FlowDocument()
main_window.Content.Document.Blocks.Add(create_paragraph(lines_in_file))
main_window.Show()
Application().Run()
ms_word.Quit()
