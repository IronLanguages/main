.. highlightlang:: c


.. hosting-sourcecodekind:

**************
SourceCodeKind
**************

This enum identifies parsing hints to languages for ScriptSource objects.  For example, some languages need to know if they are parsing a Statement or an Expression, or they may allow special syntax or variables for InteractiveCode.

Members
=======

The Summary section shows the type as it is defined (to indicate values of members), and this section documents the intent of the members.

===================== ====================================================================
   Enum Value            Description
--------------------- --------------------------------------------------------------------
Unspecified	             Should not be used.
Expression	             Start parsing an expression.
Statements	             Start parsing one or more statements if there's special syntax for multiple statements.
SingleStatement	         Start parsing a single statement, guaranteeing there's only one if that is significant to the language.
File	                 Start parsing at the beginning of a file.
InteractiveCode	         Start parsing at a legal input to a REPL.
AutoDetect	             The language best determines how to parse the input.  It may choose Interactive (supporting special syntax or variagles), Expression, or Statement.
===================== ====================================================================
