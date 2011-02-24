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

clr.AddReference('System.Management.Automation')
clr.AddReference('IronPython')

from System.Management.Automation import *
from System.Management.Automation.Host import *
from System.Management.Automation.Runspaces import *

import System

#Create a new runspace to execute powershell commands within
_runspace = RunspaceFactory.CreateRunspace()
_runspace.Open()
_intrinsics = _runspace.SessionStateProxy.GetVariable("ExecutionContext")

def translate(name):
    '''
    Utility function converts a string, name, to lowercase.
    Also replaces hyphens with underscores.
    '''
    name = name.lower()
    return name.replace('-', '_')

def fix_arg(arg):
    '''
    Utility function converts arg (of type string, PSObjectWrapper, or
    ShellOutput) to type string.
    '''
    if isinstance(arg, str):
        arg = _intrinsics.InvokeCommand.ExpandString(arg)
    elif isinstance(arg, PSObjectWrapper):
        arg = arg.data
    elif isinstance(arg, ShellOutput):
        arg = arg.data
    return arg

def InvokeCommand(_cmdName, _input=None, *args, **kws):
    '''
    Used to actually invoke a powershell command.
    '''
    #print 'invoke', _cmdName, _input
    cmd = Command(_cmdName)

    #Adds parameters to the command
    for arg in args:
        cmd.Parameters.Add(CommandParameter(None, fix_arg(arg)))

    for name, value in kws.items():
        cmd.Parameters.Add(CommandParameter(name, fix_arg(value)))

    #Create a pipeline to run the command within and invoke
    #the command.
    pipeline = _runspace.CreatePipeline()
    pipeline.Commands.Add(cmd)
    if _input:
        ret = pipeline.Invoke(fix_arg(_input))
    else:
        ret = pipeline.Invoke()
    
    #return the output of the command formatted special
    #using the ShellOutput class
    return ShellOutput(ret)

class Shell:
    '''
    Instances of this class are like pseudo PowerShell
    shells. That is, this class essentially has a method for
    each PowerShell command available.
    '''
    def __init__(self, data):
        '''
        Standard constructor. Just copies a dictionary mapping
        PowerShell commands to names as members of this class.
        '''
        for key, value in data.items():
            setattr(self, key, value)


class ShellCommand(object):
    '''
    Wrapper class for shell commands.
    '''
    def __init__(self, name, input=None):
        '''
        '''
        self.name = name
        self.input = input

    def __call__(self, *args, **kws):
        '''
        '''           
        return InvokeCommand(self.name, self.input, *args, **kws)

    def __get__(self, instance, context=None):
        '''
        '''
        return ShellCommand(self.name, instance)
    
    def __repr__(self):
        '''
        '''
        return "<ShellCommand %s>" % self.name


class ShellOutput(object):
    '''
    '''
    def __init__(self, data):
        '''
        '''
        self.data = data

    def __len__(self):
        '''
        '''
        return self.data.Count

    def __repr__(self):
        '''
        '''
        if self.data.Count == 0: return ''
        return str(self.out_string(Width=System.Console.BufferWidth-1)[0]).strip()

    def __getitem__(self, index):
        '''
        '''
        if index >= self.data.Count: raise IndexError
        ret = self.data[index]
        if isinstance(ret, PSObject):
            return PSObjectWrapper(ret)


class PSObjectWrapper(object):
    '''
    '''
    def __init__(self, data):
        '''
        '''
        self.data = data

    def __getattr__(self, name):
        '''
        '''
        member = self.data.Members[name]
        if member is not None:
            ret = member.Value
            if isinstance(ret, PSMethod):
                ret = InvokeToCallableAdapter(ret)
            return ret

        raise AttributeError(name)

    def __repr__(self):
        '''
        '''
        return self.data.Members['ToString'].Invoke()


def dump(o):
    '''
    '''
    print str(o.out_string(Width=System.Console.BufferWidth-1)[0]).strip()

class InvokeToCallableAdapter:
    '''
    '''
    def __init__(self, meth):
        '''
        '''
        self.meth = meth

    def __call__(self, *args):
        '''
        '''
        return self.meth.Invoke(*args)


def init_runspace():
    '''
    '''
    global shell
    
    #build up a dictionary of native PowerShell commands where 
    #each value consists of the PS command wrapped within 
    #the ShellCommand helper class
    cmds = {}
    for cmdlet in InvokeCommand('get-command'):
        cmds[translate(cmdlet.Name)] = ShellCommand(cmdlet.Name)
        
	#look for all aliases and for each of them that map directly
	#into a native PowerShell command, support them directly
	#from the dictionary
    for alias in InvokeCommand('get-alias'):
        cmdName = translate(alias.ReferencedCommand.Name)
        if cmdName in cmds:
            cmds[translate(alias.Name)] = cmds[cmdName]

    shell = Shell(cmds)
    for key in cmds.keys():
        setattr(ShellOutput, key, cmds[key])

init_runspace()

if __name__ == '__main__':
    print """Run \'dir(shell)\' to get a list of available PowerShell commands!
In general, IronPython PowerShell commands are accessed using the form:
    shell.get_process("cmd").select(First=2)
    """
