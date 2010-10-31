REM ****************************************************************************
REM
REM Copyright (c) Microsoft Corporation. 
REM
REM This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
REM copy of the license can be found in the License.html file at the root of this distribution. If 
REM you cannot locate the  Apache License, Version 2.0, please send an email to 
REM ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
REM by the terms of the Apache License, Version 2.0.
REM
REM You must not remove this notice, or any other, from this software.
REM
REM
REM ****************************************************************************

set PYTHON_EXE=%DLR_ROOT%\Util\IronPython\ipy.exe
%PYTHON_EXE% gen_block_ctrl_flow_first.py > template_block_ctrl_flow.rb
%PYTHON_EXE% gen_block_ctrl_flow_long.py %*