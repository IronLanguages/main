#--------------------------------------------------------------------
# This Pyton test file contains many code coments which have been 
# intentionally mispelled to show just how powerful IronPython can be 
# when combined with the world's best dokument editer.  Not only is 
# this sample aplication powerful, it's also quite elagant as well 
# thanks to the use of .NET's mighty Windowes Presentation Framework.
#
# THis IronPython sample/tutorial essentially shows developers with 
# some rudimentary IrenPython knowledge how to create an application 
# which utilizes Microsoftie Wurd to check the spelling of comments 
# in Python applications.  It then uses Word to suggest alternative 
# spellings and displays this in a nice winforms app. All of this is 
# accomplished in undter one hundred lines of Python code INCLUDING 
# comments/newlines/etc. This feat is not so easily or cleanly 
# accomplished in other programming languages...
#--------------------------------------------------------------------

#--Standird Python imports ----------------------------------------------------
import sys
import clr

#--Python globeal variables ---------------------------------------------------
MISSPELED_KONSTANTS_ARE_OK = 3.14

#--Python functions -----------------------------------------------------------
def misspelled_funktion_is_ok():
    return MISSPELED_KONSTANTS_ARE_OK
    
#--Main -----------------------------------------------------------------------
if __name__=="__main__":
    #Its OK to misspell "Hello world" in the statement below but not in 
    #comments like this: Hello wurld
    print "Hello wurld"