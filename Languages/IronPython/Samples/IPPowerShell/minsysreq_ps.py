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

from powershell import *

#---------------------------------------------------------------

gameName = "Age of Empires III Trial"
minClockSpeed = 1400
minVideoRam   = 64000000L
gameProcName  = "age3"

print """This program will determine if your PC meets some of the 
minimum system requires to play the game, %s.
""" % (gameName)

#---------------------------------------------------------------
#see if the PC already has the game running

results =  shell.get_process("age3")

if len(results) != 0:
    
    memUsage = results[0].WS
    memUsage = str(memUsage/1045576L)
    
    procID =  results[0].ID
    
    print "It appears as if you're currently running", gameName, "!"
    print "The game is using %s megs of RAM and has a process ID of %s." % (memUsage, procID)
    print

#---------------------------------------------------------------
#see if the PC meets SOME minimum requirements to run the game

videoRam =  shell.get_wmiobject("Win32_VideoController")[0].AdapterRam
if videoRam == None:
    print "Cannot determine the amount of RAM on your video card.  Will assume there's enough."
    videoRam = minVideoRam + 1

maxClockSpeed = shell.get_wmiobject("Win32_Processor")[0].MaxClockSpeed


hasSound = False
try:
    soundResults = shell.get_wmiobject("Win32_SoundDevice")[0].Status
    
    if soundResults.upper()=="OK":
        hasSound = True
except:
    #if anything goes wrong we ASSUME this PC has no sound card
    hasSound = False

#---------------------------------------------------------------
#inform them if their system is capable of playing the game


if minClockSpeed > maxClockSpeed:
    print "Your system is too slow to play '" + gameName + "'."
    print "You need a CPU that operates at '" + str(minClockSpeed/1000.0) + "Ghz' or higher."
    print "Sorry!"
    from sys import exit
    exit(0)
else:
    print "Your CPU is fast enough (" + str(maxClockSpeed/1000.0) + "Ghz)!"
    print

if minVideoRam > videoRam:
    print "Your video card doesn't have enough memory to play '" + gameName + "'."
    print "You need a video card with at least '" + str(minVideoRam/1045576L) + "MB'."
    from sys import exit
    exit(0)
else:
    print str(videoRam/1045576L), "MB is enough video memory!"
    print

if hasSound==False:
    print "Unfortunately it appears as if you have no sound card."
    print "Playing", gameName, "would be a much better experience with sound!"
    
    
print "Have a nice day!"
