.. highlightlang:: c


.. hosting-parseresult:

*********************
ScriptCodeParseResult
*********************

This enum identifies final parsing state for a ScriptSource objects.  It helps with interactive tool support.

Members
=======

===================== ====================================================================
   Enum Value            Description
--------------------- --------------------------------------------------------------------
Complete                There is no reportable state after parsing.
Invalid                 The source is syntactically invalid and cannot be parsed.
IncompleteToken	        The source ended on an incomplete token that aborted parsing.
IncompleteStatement  	The source ended on an incomplete statement that aborted parsing.
Empty	                The source is either empty, all whitespace, or all comments.
===================== ====================================================================


