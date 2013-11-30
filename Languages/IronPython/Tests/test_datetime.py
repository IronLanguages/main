# -*- coding: utf-8 -*-
#####################################################################################
#
#  Copyright (c) IronPython Contributors
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

import unittest
from datetime import datetime
from test import test_support

class TestDatetime(unittest.TestCase):

    def test_strptime_1(self):
        # cp34706
        d = datetime.strptime("2013-11-29T16:38:12.507000", "%Y-%m-%dT%H:%M:%S.%f")
        self.assertEquals(d, datetime(2013, 11, 29, 16, 38, 12, 507000))

    @unittest.skip("%f parsing uses .net DateType with miliseconds accuracy")
    def test_strptime_2(self):
        d = datetime.strptime("2013-11-29T16:38:12.507042", "%Y-%m-%dT%H:%M:%S.%f")
        self.assertEquals(d, datetime(2013, 11, 29, 16, 38, 12, 507042))

    def test_strptime_3(self):
        d = datetime.strptime("2013-11-29T16:38:12.507", "%Y-%m-%dT%H:%M:%S.%f")
        self.assertEquals(d, datetime(2013, 11, 29, 16, 38, 12, 507000))

    def test_strptime_4(self):
        d = datetime.strptime("2013-11-29T16:38:12.5", "%Y-%m-%dT%H:%M:%S.%f")
        self.assertEquals(d, datetime(2013, 11, 29, 16, 38, 12, 500000))

    def test_strftime_1(self):
        d = datetime(2013, 11, 29, 16, 38, 12, 507000)
        self.assertEquals(d.strftime("%Y-%m-%dT%H:%M:%S.%f"), "2013-11-29T16:38:12.507000")

    def test_strftime_2(self):
        d = datetime(2013, 11, 29, 16, 38, 12, 507042)
        self.assertEquals(d.strftime("%Y-%m-%dT%H:%M:%S.%f"), "2013-11-29T16:38:12.507042")

    def test_strftime_3(self):
        # cp32215
        d = datetime(2012,2,8,4,5,6,12314)
        self.assertEquals(d.strftime('%f'), "012314")


def test_main():
    from unittest import main
    main(module='test_datetime')

if __name__ == "__main__":
    test_main()


