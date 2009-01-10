# ****************************************************************************
#
# Copyright (c) Microsoft Corporation. 
#
# This source code is subject to terms and conditions of the Microsoft Public License. A 
# copy of the license can be found in the License.html file at the root of this distribution. If 
# you cannot locate the  Microsoft Public License, please send an email to 
# ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
# by the terms of the Microsoft Public License.
#
# You must not remove this notice, or any other, from this software.
#
#
# ****************************************************************************
# Configuration file for IronRuby developers

class UserEnvironment
  # Location of your MRI installation. This is REQUIRED until
  # we start packaging and shipping the MRI libs ourselves.
  # We will find your Ruby install directory based on where 
  # ruby.exe is on your path. If you want to define it explicitly
  # yourself, uncomment the following line:
  #
  # MRI      = 'c:\ruby'

  # These constants must be defined if you want to run the RubySpec specs.
  #
  # CONFIG   = '~/default.mspec'
  # TAGS     = 'c:/dev/ironruby-tags'
  # RUBYSPEC = 'c:/dev/rubyspec'
  # MSPEC    = 'c:/dev/mspec'
end
