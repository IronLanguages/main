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

class Etc
  class << self
    def getlogin
      ENV['USERNAME']
    end
    
    def endgrent(*args)
      nil
    end

    [:endpwent, :getgrent, :getgrgid, :getgrnam, :getpwent, 
     :getpwnam, :getpwuid, :group, :passwd, :setgrent,
     :setpwent].each do |method|
      alias_method method, :endgrent
    end
  end
end
