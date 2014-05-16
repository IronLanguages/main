# -*- coding: utf-8 -*-
#####################################################################################
#
#  Copyright (c) Pawel Jasinski. All rights reserved.
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

##
## Test str/byte equivalence for built-in string methods
##
## Please, Note:
## i) All commented test cases are for bytes/extensible string combination
## ii) For version 3.x the str/byte mixing below become "Raises" test cases
##
##


from iptest.assert_util import *
from iptest.misc_util import ip_supported_encodings
import sys


class ExtensibleStringClass(str):
    pass

esa = ExtensibleStringClass("a")
esb = ExtensibleStringClass("b")
esc = ExtensibleStringClass("c")
esx = ExtensibleStringClass("x")


def test_contains():
    Assert(esa.__contains__("a"))
    Assert(esa.__contains__(b"a"))
    Assert(esa.__contains__(esa))
    Assert("a".__contains__("a"))
    Assert("a".__contains__(b"a"))
    Assert("a".__contains__(esa))
    Assert(b"a".__contains__("a"))
    Assert(b"a".__contains__(b"a"))
    Assert(b"a".__contains__(esa))

def test_format():
    AreEqual("%s" % b"a", "a")
    # AreEqual(b"%s" % b"a", b"a")
    # AreEqual("%s" % b"a", b"%s" % "a")

def test_count():
    AreEqual("aa".count(b"a"), 2)
    AreEqual("aa".count(b"a", 0), 2)
    AreEqual("aa".count(b"a", 0, 1), 1)

    AreEqual("aa".count(esa), 2)
    AreEqual("aa".count(esa, 0), 2)
    AreEqual("aa".count(esa, 0, 1), 1)

    AreEqual(b"aa".count("a"), 2)
    AreEqual(b"aa".count("a", 0), 2)
    AreEqual(b"aa".count("a", 0, 1), 1)

    # AreEqual(b"aa".count(esa), 2)
    # AreEqual(b"aa".count(esa, 0), 2)
    # AreEqual(b"aa".count(esa, 0, 1), 1)


def test_find():
    Assert("abc".find(b"b"))
    Assert("abc".find(b"b", 1))
    Assert("abc".find(b"b", 1, 2))
    Assert("abc".find(b"b", 1L))
    Assert("abc".find(b"b", 1L, 2L))

    Assert("abc".find(esb))
    Assert("abc".find(esb, 1))
    Assert("abc".find(esb, 1, 2))
    Assert("abc".find(esb, 1L))
    Assert("abc".find(esb, 1L, 2L))

    Assert(b"abc".find("b"))
    Assert(b"abc".find("b", 1))
    Assert(b"abc".find("b", 1, 2))

    # Assert(b"abc".find(esb))
    # Assert(b"abc".find(esb, 1))
    # Assert(b"abc".find(esb, 1, 2))

def test_lstrip():
    AreEqual("xa".lstrip(b"x"), "a")
    AreEqual("xa".lstrip(esx), "a")
    AreEqual(b"xa".lstrip("x"), b"a")
    # AreEqual(b"xa".lstrip(esx), b"a")

def test_partition():
    AreEqual("abc".partition(b"b"), ("a", "b", "c"))
    AreEqual("abc".partition(esb), ("a", "b", "c"))
    AreEqual(b"abc".partition("b"), (b"a", b"b", b"c"))
    # AreEqual(b"abc".partition(esb), (b"a", b"b", b"c"))

def test_replace():
    AreEqual("abc".replace(b"a", "x"), "xbc")
    AreEqual("abc".replace(b"a", b"x"), "xbc")
    AreEqual("abc".replace("a", b"x"), "xbc")
    AreEqual("abc".replace(b"a", "x", 1), "xbc")
    AreEqual("abc".replace(b"a", b"x", 1), "xbc")
    AreEqual("abc".replace("a", b"x", 1), "xbc")

    AreEqual("abc".replace(b"a", buffer("x")), "xbc")
    AreEqual("abc".replace(buffer("a"), "x"), "xbc")
    AreEqual("abc".replace(buffer("a"), buffer("x")), "xbc")
    AreEqual("abc".replace(b"a", bytearray(b"x")), "xbc")
    AreEqual("abc".replace(bytearray(b"a"), "x"), "xbc")
    AreEqual("abc".replace(bytearray(b"a"), bytearray(b"x")), "xbc")

    AreEqual("abc".replace("a", esx), "xbc")
    AreEqual("abc".replace(b"a", esx), "xbc")
    AreEqual("abc".replace(esa, esx), "xbc")
    AreEqual("abc".replace(esa, b"x"), "xbc")

    AreEqual("abc".replace("a", esx, 1), "xbc")
    AreEqual("abc".replace(b"a", esx, 1), "xbc")
    AreEqual("abc".replace(esa, esx, 1), "xbc")
    AreEqual("abc".replace("a", esx, 1), "xbc")

    AreEqual(b"abc".replace(b"a", "x"), "xbc")
    AreEqual(b"abc".replace("a", "x"), "xbc")
    AreEqual(b"abc".replace("a", b"x"), "xbc")
    AreEqual(b"abc".replace(b"a", "x", 1), "xbc")
    AreEqual(b"abc".replace("a", "x", 1), "xbc")
    AreEqual(b"abc".replace("a", b"x", 1), "xbc")

    # AreEqual(b"abc".replace("a", esx), "xbc")
    # AreEqual(b"abc".replace(b"a", esx), "xbc")
    # AreEqual(b"abc".replace(esa, esx), "xbc")
    # AreEqual(b"abc".replace(esa, b"x"), "xbc")

    # AreEqual(b"abc".replace("a", esx, 1), "xbc")
    # AreEqual(b"abc".replace(b"a", esx, 1), "xbc")
    # AreEqual(b"abc".replace(esa, esx, 1), "xbc")
    # AreEqual(b"abc".replace("a", esx, 1), "xbc")


def test_rfind():
    AreEqual("abc".rfind(b"c"), 2)
    AreEqual("abc".rfind(b"c", 1), 2)
    AreEqual("abc".rfind(b"c", 1, 3), 2)
    AreEqual("abc".rfind(b"c", 1L), 2)
    AreEqual("abc".rfind(b"c", 1L, 3L), 2)

    AreEqual("abc".rfind(esc), 2)
    AreEqual("abc".rfind(esc, 1), 2)
    AreEqual("abc".rfind(esc, 1, 3), 2)
    AreEqual("abc".rfind(esc, 1L), 2)
    AreEqual("abc".rfind(esc, 1L, 3L), 2)

    AreEqual(b"abc".rfind("c"), 2)
    AreEqual(b"abc".rfind("c", 1), 2)
    AreEqual(b"abc".rfind("c", 1, 3), 2)

    # AreEqual(b"abc".rfind(esc), 2)
    # AreEqual(b"abc".rfind(esc, 1), 2)
    # AreEqual(b"abc".rfind(esc, 1, 3), 2)


def test_rindex():
    AreEqual("abc".rindex(b"c"), 2)
    AreEqual("abc".rindex(b"c", 1), 2)
    AreEqual("abc".rindex(b"c", 1, 3), 2)
    AreEqual("abc".rindex(b"c", 1L), 2)
    AreEqual("abc".rindex(b"c", 1L, 3L), 2)

    AreEqual("abc".rindex(esc), 2)
    AreEqual("abc".rindex(esc, 1), 2)
    AreEqual("abc".rindex(esc, 1, 3), 2)
    AreEqual("abc".rindex(esc, 1L), 2)
    AreEqual("abc".rindex(esc, 1L, 3L), 2)

    AreEqual(b"abc".rindex("c"), 2)
    AreEqual(b"abc".rindex("c", 1), 2)
    AreEqual(b"abc".rindex("c", 1, 3), 2)

    # AreEqual(b"abc".rindex(esc), 2)
    # AreEqual(b"abc".rindex(esc, 1), 2)
    # AreEqual(b"abc".rindex(esc, 1, 3), 2)

def test_rpartition():
    AreEqual("abc".rpartition(b"b"), ("a", "b", "c"))
    AreEqual("abc".rpartition(esb), ("a", "b", "c"))
    AreEqual(b"abc".rpartition("b"), (b"a", b"b", b"c"))
    # AreEqual(b"abc".rpartition(esb), (b"a", b"b", b"c"))

def test_rsplit():
    AreEqual("abc".rsplit(b"b"), ["a", "c"])
    AreEqual("abc".rsplit(b"b", 1), ["a", "c"])
    AreEqual("abc".rsplit(esb), ["a", "c"])
    AreEqual("abc".rsplit(esb, 1), ["a", "c"])
    AreEqual(b"abc".rsplit("b"), [b"a", b"c"])
    AreEqual(b"abc".rsplit("b", 1), [b"a", b"c"])
    # AreEqual(b"abc".rsplit(esb), [b"a", b"c"])
    # AreEqual(b"abc".rsplit(esb, 1), [b"a", b"c"])

def test_rstrip():
    AreEqual("ax".rstrip(b"x"), "a")
    AreEqual("ax".rstrip(esx), "a")
    AreEqual(b"ax".rstrip("x"), b"a")
    # AreEqual(b"ax".rstrip(esx), b"a")

def test_split():
    AreEqual("abc".split(b"b"), ["a", "c"])
    AreEqual("abc".split(b"b", 1), ["a", "c"])
    AreEqual("abc".split(esb), ["a", "c"])
    AreEqual("abc".split(esb, 1), ["a", "c"])
    AreEqual(b"abc".split("b"), [b"a", b"c"])
    AreEqual(b"abc".split("b", 1), [b"a", b"c"])
    # AreEqual(b"abc".split(esb), [b"a", b"c"])
    # AreEqual(b"abc".split(esb, 1), [b"a", b"c"])

def test_strip():
    AreEqual("xax".strip(b"x"), "a")
    AreEqual("xax".strip(esx), "a")
    AreEqual(b"xax".strip("x"), b"a")
    # AreEqual(b"xax".strip(esx), b"a")

def test_startswith():
    Assert("abc".startswith(b"a"))
    Assert("abc".startswith(b"a", 0))
    Assert("abc".startswith(b"a", 0, 1))
    Assert("abc".startswith(esa))
    Assert("abc".startswith(esa, 0))
    Assert("abc".startswith(esa, 0, 1))
    Assert(b"abc".startswith("a"))
    Assert(b"abc".startswith("a", 0))
    Assert(b"abc".startswith("a", 0, 1))
    # Assert(b"abc".startswith(esa))
    # Assert(b"abc".startswith(esa, 0))
    # Assert(b"abc".startswith(esa, 0, 1))


def test_endswith():
    Assert("abc".endswith(b"c"))
    Assert("abc".endswith(b"c", 0))
    Assert("abc".endswith(b"c", 0, 3))
    Assert("abc".endswith(esc))
    Assert("abc".endswith(esc, 0))
    Assert("abc".endswith(esc, 0, 3))
    Assert(b"abc".endswith("c"))
    Assert(b"abc".endswith("c", 0))
    Assert(b"abc".endswith("c", 0, 3))
    # Assert(b"abc".endswith(esc))
    # Assert(b"abc".endswith(esc, 0))
    # Assert(b"abc".endswith(esc, 0, 3))

def test_join():
    AreEqual("abc", "b".join([b"a", b"c"]))
    AreEqual("b", "a".join([b"b"]))
    AreEqual("abc", "b".join([esa, esc]))
    AreEqual("b", "a".join([esb]))
    AreEqual(b"abc", b"b".join(["a", "c"]))
    AreEqual(b"b", b"a".join(["b"]))
    # AreEqual(b"abc", b"b".join([esb, esc]))
    # AreEqual(b"b", b"a".join([esb]))


run_test(__name__)
