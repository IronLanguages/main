from sys import path
from System.IO.Path import Combine
from System.Environment import GetEnvironmentVariable as env
path.append(Combine(env('DLR_ROOT'), "External.LCA_RESTRICTED", "Languages", "IronPython", "27", "Lib"))