REM ****************************************************************************
REM
REM Copyright (c) Microsoft Corporation. 
REM
REM This source code is subject to terms and conditions of the Microsoft Public License. A 
REM copy of the license can be found in the License.html file at the root of this distribution. If 
REM you cannot locate the  Microsoft Public License, please send an email to 
REM ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
REM by the terms of the Microsoft Public License.
REM
REM You must not remove this notice, or any other, from this software.
REM
REM
REM ****************************************************************************

set PYTHON_EXE=%MERLIN_ROOT%\bin\debug\ipy.exe
%PYTHON_EXE% gen_block_ctrl_flow_first.py > template_block_ctrl_flow.rb
%PYTHON_EXE% gen_block_ctrl_flow_long.py %*