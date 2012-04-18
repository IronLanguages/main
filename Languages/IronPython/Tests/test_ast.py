import sys, itertools, unittest
from test import test_support
import ast

def to_tuple(t):
    if t is None or isinstance(t, (basestring, int, long, complex)):
        return t
    elif isinstance(t, list):
        return [to_tuple(e) for e in t]
    result = [t.__class__.__name__]
    if hasattr(t, 'lineno') and hasattr(t, 'col_offset'):
        result.append((t.lineno, t.col_offset))
    if t._fields is None:
        return tuple(result)
    for f in t._fields:
        result.append(to_tuple(getattr(t, f)))
    return tuple(result)


# These tests are compiled through "exec"
# There should be atleast one test per statement
exec_tests = [
    # FunctionDef
    "def f(): pass",
    # ClassDef
    "class C:pass",
    # Return
    "def f():return 1",
    # Delete
    "del v",
    # Assign
    "v = 1",
    # AugAssign
    "v += 1",
    # Print
    "print >>f, 1, ",
    # For
    "for v in v:pass",
    # While
    "while v:pass",
    # If
    "if v:pass",
    # Raise
    "raise Exception, 'string'",
    # TryExcept
    "try:\n  pass\nexcept Exception:\n  pass",
    # TryFinally
    "try:\n  pass\nfinally:\n  pass",
    # Assert
    "assert v",
    # Import
    "import sys",
    # ImportFrom
    "from sys import v",
    # Exec
    "exec 'v'",
    # Global
    "global v",
    # Expr
    "1",
    # Pass,
    "pass",
    # Break
    # "break", doesn't work outside a loop
    "while x: break",
    # Continue
    # "continue", doesn't work outside a loop
    "while x: continue",
    # for statements with naked tuples (see http://bugs.python.org/issue6704)
    "for a,b in c: pass",
    "[(a,b) for a,b in c]",
    "((a,b) for a,b in c)",
]

# These are compiled through "single"
# because of overlap with "eval", it just tests what
# can't be tested with "eval"
single_tests = [
    "1+2"
]

# These are compiled through "eval"
# It should test all expressions
eval_tests = [
  # BoolOp
  "a and b",
  # BinOp
  "a + b",
  # UnaryOp
  "not v",
  # Lambda
  "lambda:None",
  # Dict
  "{ 1:2 }",
  # ListComp
  "[a for b in c if d]",
  # GeneratorExp
  "(a for b in c if d)",
  # Yield - yield expressions can't work outside a function
  #
  # Compare
  "1 < 2 < 3",
  # Call
  "f(1,2,c=3,*d,**e)",
  # Repr
  "`v`",
  # Num
  "10L",
  # Str
  "'string'",
  # Attribute
  "a.b",
  # Subscript
  "a[b:c]",
  # Name
  "v",
  # List
  "[1,2,3]",
  # Tuple
  "1,2,3",
  # Combination
  "a.b.c.d(a.b[1:2])",
  # ellipsis
  "a[...]",

]

# TODO: expr_context, slice, boolop, operator, unaryop, cmpop, comprehension
# excepthandler, arguments, keywords, alias

class AST_Tests(unittest.TestCase):

    def _assertTrueorder(self, ast_node, parent_pos):
        if not isinstance(ast_node, ast.AST) or ast_node._fields is None:
            return
        if isinstance(ast_node, (ast.expr, ast.stmt, ast.excepthandler)):
            node_pos = (ast_node.lineno, ast_node.col_offset)
            self.assertTrue(node_pos >= parent_pos)
            parent_pos = (ast_node.lineno, ast_node.col_offset)
        for name in ast_node._fields:
            value = getattr(ast_node, name)
            if isinstance(value, list):
                for child in value:
                    self._assertTrueorder(child, parent_pos)
            elif value is not None:
                self._assertTrueorder(value, parent_pos)

    def test_snippets(self):
        self.maxDiff = 2048
        # Things which diverted from cpython:
        # - col_offset of list comprehension in ironpython uses opening bracket, cpython points to first expr
        # - same for generator
        # - Slice in iron has col_offset and lineno set, in cpython both are not set
        for input, output, kind in ((exec_tests, exec_results, "exec"),
                                    (single_tests, single_results, "single"),
                                    (eval_tests, eval_results, "eval")):
            for i, o in itertools.izip(input, output):
                ast_tree = compile(i, "?", kind, ast.PyCF_ONLY_AST)
                self.assertEqual(to_tuple(ast_tree), o)
                self._assertTrueorder(ast_tree, (0, 0))

    def test_slicex(self):
        slc = ast.parse("x[1:2:3]").body[0].value.slice
        self.assertEqual(slc.lower.n, 1)
        self.assertEqual(slc.upper.n, 2)
        self.assertEqual(slc.step.n, 3)

    def test_slice(self):
        slc = ast.parse("x[::]").body[0].value.slice
        self.assertIsNone(slc.upper)
        self.assertIsNone(slc.lower)
        self.assertIsInstance(slc.step, ast.Name)
        self.assertEqual(slc.step.id, "None")

    def test_from_import(self):
        im = ast.parse("from . import y").body[0]
        self.assertIsNone(im.module)

    def test_base_classes(self):
        self.assertTrue(issubclass(ast.For, ast.stmt))
        self.assertTrue(issubclass(ast.Name, ast.expr))
        self.assertTrue(issubclass(ast.stmt, ast.AST))
        self.assertTrue(issubclass(ast.expr, ast.AST))
        self.assertTrue(issubclass(ast.comprehension, ast.AST))
        self.assertTrue(issubclass(ast.Gt, ast.AST))

    def test_nodeclasses(self):
        # IronPyhon performs argument typechecking
        l=ast.Str('A')
        o=ast.Mult()
        r=ast.Num('13')
        x=ast.BinOp(l,o,r,lineno=42)
        self.assertEqual(x.left, l)
        self.assertEqual(x.op, o)
        self.assertEqual(x.right, r)
        self.assertEqual(x.lineno, 42)

        # node raises exception when not given enough arguments
        self.assertRaises(TypeError, ast.BinOp, l, o)

        # can set attributes through kwargs too
        x = ast.BinOp(left=l, op=o, right=r, lineno=42)
        self.assertEqual(x.left, l)
        self.assertEqual(x.op, o)
        self.assertEqual(x.right, r)
        self.assertEqual(x.lineno, 42)

        # this used to fail because Sub._fields was None
        x = ast.Sub()

    def test_docexample(self):
        # used to fail on ironpython for various reason
        node = ast.UnaryOp(ast.USub(), ast.Num(5, lineno=0, col_offset=0),
                            lineno=0, col_offset=0)

        # the same with zero argument constructors
        node = ast.UnaryOp()
        node.op = ast.USub()
        node.operand = ast.Num()
        node.operand.n = 5
        node.operand.lineno = 0
        node.operand.col_offset = 0
        node.lineno = 0
        node.col_offset = 0

    def test_example_from_net(self):
        node = ast.Expression(ast.BinOp(ast.Str('xy'), ast.Mult(), ast.Num(3)))

    def _test_extra_attribute(self):
        n=ast.Num()
        n.extra_attribute=2
        self.assertTrue(hasattr(n,'extra_attribute'))

    def test_operators(self):
        boolop0 = ast.BoolOp()
        boolop1 = ast.BoolOp(ast.And(), 
                          [ ast.Name('True', ast.Load()), ast.Name('False', ast.Load()), ast.Name('a',ast.Load())])
        boolop2 = ast.BoolOp(ast.And(), 
                          [ ast.Name('True', ast.Load()), ast.Name('False', ast.Load()), ast.Name('a',ast.Load())],
                          0, 0)
        binop0 = ast.BinOp()
        binop1 = ast.BinOp(ast.Str('xy'), ast.Mult(), ast.Num(3))
        binop2 = ast.BinOp(ast.Str('xy'), ast.Mult(), ast.Num(3), 0, 0)

        unaryop0 = ast.UnaryOp()
        unaryop1 = ast.UnaryOp(ast.Not(), ast.Name('True',ast.Load())) 
        unaryop2 = ast.UnaryOp(ast.Not(), ast.Name('True',ast.Load()), 0, 0)

        lambda0 = ast.Lambda()
        lambda1 = ast.Lambda(ast.arguments([ast.Name('x', ast.Param())], None, None, []), ast.Name('x', ast.Load()))
        
        ifexp0 = ast.IfExp()
        ifexp1 = ast.IfExp(ast.Name('True',ast.Load()), ast.Num(1), ast.Num(0))
        ifexp2 = ast.IfExp(ast.Name('True',ast.Load()), ast.Num(1), ast.Num(0), 0, 0)

        dict0 = ast.Dict()
        dict1 = ast.Dict([ast.Num(1), ast.Num(2)], [ast.Str('a'), ast.Str('b')])
        dict2 = ast.Dict([ast.Num(1), ast.Num(2)], [ast.Str('a'), ast.Str('b')], 0, 0)

        set0 = ast.Set()
        set1 = ast.Set([ast.Num(1), ast.Num(2)])
        set2 = ast.Set([ast.Num(1), ast.Num(2)], 0, 0)

        lc0 = ast.ListComp()
        lc1 = ast.ListComp( ast.Name('x',ast.Load()), 
                   [ast.comprehension(ast.Name('x', ast.Store()), 
                                      ast.Tuple([ast.Num(1), ast.Num(2)], ast.Load()), [])])
        lc2 = ast.ListComp( ast.Name('x',ast.Load()), 
                   [ast.comprehension(ast.Name('x', ast.Store()), 
                                      ast.Tuple([ast.Num(1), ast.Num(2)], ast.Load()), [])], 0, 0)


        setcomp0 = ast.SetComp()
        setcomp1 = ast.SetComp(ast.Name('x', ast.Load()), 
                   [ast.comprehension(ast.Name('x', ast.Store()), ast.Str('abracadabra'), 
                                      [ast.Compare(ast.Name('x', ast.Load()), [ast.NotIn()], 
                                                   [ast.Str('abc')])])])


        comprehension0 = ast.comprehension()
        comprehension1 = ast.comprehension(ast.Name('x', ast.Store()), 
                                           ast.Tuple([ast.Num(1), ast.Num(2)], ast.Load()), [])


        # "{i : chr(65+i) for i in (1,2)}")
        dictcomp0 = ast.DictComp()
        dictcomp1 = ast.DictComp(ast.Name('i', ast.Load()), 
                                 ast.Call(ast.Name('chr', ast.Load()), 
                                          [ast.BinOp(ast.Num(65), ast.Add(), ast.Name('i', ast.Load()))],
                                          [], None, None), 
                                 [ast.comprehension(ast.Name('i', ast.Store()), 
                                                    ast.Tuple([ast.Num(1), ast.Num(n=2)], ast.Load()), [])])
        dictcomp2 = ast.DictComp(ast.Name('i', ast.Load()), 
                                 ast.Call(ast.Name('chr', ast.Load()), 
                                          [ast.BinOp(ast.Num(65), ast.Add(), ast.Name('i', ast.Load()))],
                                          [], None, None), 
                                 [ast.comprehension(ast.Name('i', ast.Store()), 
                                                    ast.Tuple([ast.Num(1), ast.Num(n=2)], ast.Load()), [])],0,0)

        # (x for x in (1,2))
        genexp0 = ast.GeneratorExp()
        genexp1 = ast.GeneratorExp(ast.Name('x', ast.Load()), 
                                   [ast.comprehension(ast.Name('x', ast.Store()), 
                                                      ast.Tuple([ast.Num(1), ast.Num(2)], ast.Load()), [])])
        genexp2 = ast.GeneratorExp(ast.Name('x', ast.Load()), 
                                   [ast.comprehension(ast.Name('x', ast.Store()), 
                                                      ast.Tuple([ast.Num(1), ast.Num(2)], ast.Load()), [])],0,0)

        # yield 2
        yield0 = ast.Yield()
        yield1 = ast.Yield(ast.Num(2))
        yield2 = ast.Yield(ast.Num(2),0,0)
        yield20 = ast.Yield(lineno=0, col_offset=0)

        # a>0
        compare0 = ast.Compare()
        compare1 = ast.Compare(ast.Name('a', ast.Load()), [ast.Gt()], [ast.Num(0)])
        compare2 = ast.Compare(ast.Name('a', ast.Load()), [ast.Gt()], [ast.Num(0)],0,0)

        # chr(65)
        call0 = ast.Call()
        call1 = ast.Call(ast.Name('chr', ast.Load()), [ast.Num(65)], [], None, None)
        call2 = ast.Call(ast.Name('chr', ast.Load()), [ast.Num(65)], [], None, None, 0, 0)
        call20 = ast.Call(ast.Name('f', ast.Load()), [ast.Num(0)], [])
        call21 = ast.Call(ast.Name('f', ast.Load()), [ast.Num(0)], [], lineno=0, col_offset=0)

        # 0
        num0 = ast.Num()
        num1 = ast.Num(0)
        num2 = ast.Num(0,0,0)

        # "foo"
        str0 = ast.Str()
        str1 = ast.Str("foo")
        str2 = ast.Str("foo",0,0)

        # TODO: come back
        repr0 = ast.Repr()
        repr1 = ast.Repr(ast.Num(0))
        repr2 = ast.Repr(ast.Num(0),0,0)

        # foo.bar
        attr0 = ast.Attribute()
        attr1 = ast.Attribute(ast.Name('foo', ast.Load()), 'bar', ast.Load())
        attr2 = ast.Attribute(ast.Name('foo', ast.Load()), 'bar', ast.Load(), 0,0)

        # a[1:2]
        subscript0 = ast.Subscript()
        subscript1 = ast.Subscript(ast.Name('a', ast.Load()), ast.Slice(ast.Num(1), ast.Num(2)), ast.Load())
        subscript2 = ast.Subscript(ast.Name('a', ast.Load()), ast.ExtSlice([ast.Num(1), ast.Num(2)]), ast.Load(), 0, 0)

        # name
        name0 = ast.Name()
        name1 = ast.Name("name", ast.Load())
        name2 = ast.Name("name", ast.Load(),0,0)

        # [1,2]
        list0 = ast.List()
        list1 = ast.List([ast.Num(1), ast.Num(2)], ast.Load())
        list2 = ast.List([ast.Num(1), ast.Num(2)], ast.Load(),0,0)

        # (1,2)
        tuple0 = ast.Tuple()
        tuple1 = ast.Tuple([ast.Num(1), ast.Num(2)], ast.Load())
        tuple2 = ast.Tuple([ast.Num(1), ast.Num(2)], ast.Load(), 0, 0)


    def test_stmt(self):

        # def foo():
        #   pass
        fundef0 = ast.FunctionDef()
        fundef1 = ast.FunctionDef('foo', ast.arguments([], None, None, []), [ast.Pass()], [])
        fundef2 = ast.FunctionDef('foo', ast.arguments([], None, None, []), [ast.Pass()], [], 0,0 )

        # class foo(object):
        #   pass
        classdef0 = ast.ClassDef()
        classdef1 = ast.ClassDef('foo', [ast.Name('object', ast.Load())], [ast.Pass()], [])
        classdef1 = ast.ClassDef('foo', [ast.Name('object', ast.Load())], [ast.Pass()], [], 0,0)

        # return 0
        return0 = ast.Return()
        return1 = ast.Return(ast.Num(0))
        return2 = ast.Return(ast.Num(0),0,0)
        return20 = ast.Return(lineno=0, col_offset=0)

        # del d[1]
        del0 = ast.Delete()
        del1 = ast.Delete([ast.Subscript(ast.Name('d', ast.Load()), ast.Index(ast.Num(1)), ast.Del())])
        del2 = ast.Delete([ast.Subscript(ast.Name('d', ast.Load()), ast.Index(ast.Num(1)), ast.Del())],0,0)

        # a=1
        assign0=ast.Assign()
        assign1=ast.Assign([ast.Name('a', ast.Store())], ast.Num(1))
        assign2=ast.Assign([ast.Name('a', ast.Store())], ast.Num(1),0,0)

        # a+=1
        augassign0=ast.AugAssign()
        augassign1=ast.AugAssign(ast.Name('a', ast.Store()), ast.Add(), ast.Num(1))
        augassign2=ast.AugAssign(ast.Name('a', ast.Store()), ast.Add(), ast.Num(1),0,0)
        
        # print 1
        print0 = ast.Print()
        print1 = ast.Print(None, [ast.Num(1)], True)
        print2 = ast.Print(None, [ast.Num(1)], True, 0, 0)
        print20 = ast.Print( values=[ast.Num(1)], nl=True)

        # for i in l:
        #   print i
        # else:
        #   pass
        for0 = ast.For()
        for1 = ast.For(ast.Name('i', ast.Store()), 
                       ast.Name('l', ast.Load()), 
                       [ast.Print(None, [ast.Name('i', ast.Load())], True)], 
                       [ast.Pass()])
        for2 = ast.For(ast.Name('i', ast.Store()), 
                       ast.Name('l', ast.Load()), 
                       [ast.Print(None, [ast.Name('i', ast.Load())], True)], 
                       [ast.Pass()],0,0)

        # while True:
        #   pass
        # else:
        #   pass
        while0 = ast.While()
        while1 = ast.While(ast.Name('True', ast.Load()), [ast.Pass()], [ast.Pass()])
        while2 = ast.While(ast.Name('True', ast.Load()), [ast.Pass()], [ast.Pass()], 0,0 )

        # if a:
        #   pass
        # else:
        #   pass
        if0 = ast.If()
        if1 = ast.If(ast.Name('a', ast.Load()), [ast.Pass()], [ast.Pass()])
        if2 = ast.If(ast.Name('a', ast.Load()), [ast.Pass()], [ast.Pass()] ,0,0)

        # with with open("foo") as f:
        #   pass
        with0 = ast.With()
        with0 = ast.With(ast.Call(ast.Name('open', ast.Load()), [ast.Str('foo')], []), 
                         ast.Name('f', ast.Store()), 
                         [ast.Pass()])

        # raise Exception()
        raise0 = ast.Raise()
        raise1 = ast.Raise(ast.Call(ast.Name('Exception', ast.Load()), [], []), None, None)
        raise2 = ast.Raise(ast.Call(ast.Name('Exception', ast.Load()), [], []), None, None, 0, 0)

    def test_attributes(self):
        # assert True, "bad"
        assert0 = ast.Assert()
        self.assertFalse(hasattr(assert0, 'lineno'))
        self.assertFalse(hasattr(assert0, 'col_offset'))
        assert1 = ast.Assert(ast.Name('True', ast.Load()), ast.Str('bad'))
        self.assertFalse(hasattr(assert1, 'lineno'))
        self.assertFalse(hasattr(assert1, 'col_offset'))
        try:
            tmp=assert1.lineno
        except Exception as e:
            self.assertTrue(isinstance(e,AttributeError))
        try:
            tmp=assert1.col_offset
        except Exception as e:
            self.assertTrue(isinstance(e,AttributeError))
        assert2 = ast.Assert(ast.Name('True', ast.Load()), ast.Str('bad'),2,3)
        self.assertEqual(assert2.lineno,2)
        self.assertEqual(assert2.col_offset,3)

    def test_compare(self):
        # 
        c0 = to_tuple(ast.parse("a<b>c"))
        c1 = to_tuple(ast.parse("(a<b)>c"))
        c2 = to_tuple(ast.parse("a<(b>c)"))
        self.assertNotEqual(c0,c1)
        self.assertNotEqual(c1,c2)
        self.assertNotEqual(c0,c2)


    def test_pickling(self):
        import pickle
        mods = [pickle]
        try:
            import cPickle
            mods.append(cPickle)
        except ImportError:
            pass
        protocols = [0, 1, 2]
        for mod in mods:
            for protocol in protocols:
                for ast in (compile(i, "?", "exec", 0x400) for i in exec_tests):
                    ast2 = mod.loads(mod.dumps(ast, protocol))
                    self.assertEquals(to_tuple(ast2), to_tuple(ast))


class ASTHelpers_Test(unittest.TestCase):

    def test_parse(self):
        a = ast.parse('foo(1 + 1)')
        b = compile('foo(1 + 1)', '<unknown>', 'exec', ast.PyCF_ONLY_AST)
        self.assertEqual(ast.dump(a), ast.dump(b))

    def test_dump(self):
        node = ast.parse('spam(eggs, "and cheese")')
        self.assertEqual(ast.dump(node),
            "Module(body=[Expr(value=Call(func=Name(id='spam', ctx=Load()), "
            "args=[Name(id='eggs', ctx=Load()), Str(s='and cheese')], "
            "keywords=[], starargs=None, kwargs=None))])"
        )
        self.assertEqual(ast.dump(node, annotate_fields=False),
            "Module([Expr(Call(Name('spam', Load()), [Name('eggs', Load()), "
            "Str('and cheese')], [], None, None))])"
        )
        self.assertEqual(ast.dump(node, include_attributes=True),
            "Module(body=[Expr(value=Call(func=Name(id='spam', ctx=Load(), "
            "lineno=1, col_offset=0), args=[Name(id='eggs', ctx=Load(), "
            "lineno=1, col_offset=5), Str(s='and cheese', lineno=1, "
            "col_offset=11)], keywords=[], starargs=None, kwargs=None, "
            "lineno=1, col_offset=0), lineno=1, col_offset=0)])"
        )

    def test_copy_location(self):
        src = ast.parse('1 + 1', mode='eval')
        src.body.right = ast.copy_location(ast.Num(2), src.body.right)
        self.assertEqual(ast.dump(src, include_attributes=True),
            'Expression(body=BinOp(left=Num(n=1, lineno=1, col_offset=0), '
            'op=Add(), right=Num(n=2, lineno=1, col_offset=4), lineno=1, '
            'col_offset=0))'
        )

    def test_fix_missing_locations(self):
        self.maxDiff = 2048
        src = ast.parse('write("spam")')
        src.body.append(ast.Expr(ast.Call(ast.Name('spam', ast.Load()),
                                          [ast.Str('eggs')], [], None, None)))
        self.assertEqual(src, ast.fix_missing_locations(src))
        self.assertEqual(ast.dump(src, include_attributes=True),
            "Module(body=[Expr(value=Call(func=Name(id='write', ctx=Load(), "
            "lineno=1, col_offset=0), args=[Str(s='spam', lineno=1, "
            "col_offset=6)], keywords=[], starargs=None, kwargs=None, "
            "lineno=1, col_offset=0), lineno=1, col_offset=0), "
            "Expr(value=Call(func=Name(id='spam', ctx=Load(), lineno=1, "
            "col_offset=0), args=[Str(s='eggs', lineno=1, col_offset=0)], "
            "keywords=[], starargs=None, kwargs=None, lineno=1, "
            "col_offset=0), lineno=1, col_offset=0)])"
        )

    def test_increment_lineno(self):
        src = ast.parse('1 + 1', mode='eval')
        self.assertEqual(ast.increment_lineno(src, n=3), src)
        self.assertEqual(ast.dump(src, include_attributes=True),
            'Expression(body=BinOp(left=Num(n=1, lineno=4, col_offset=0), '
            'op=Add(), right=Num(n=1, lineno=4, col_offset=4), lineno=4, '
            'col_offset=0))'
        )
        # issue10869: do not increment lineno of root twice
        src = ast.parse('1 + 1', mode='eval')
        self.assertEqual(ast.increment_lineno(src.body, n=3), src.body)
        self.assertEqual(ast.dump(src, include_attributes=True),
            'Expression(body=BinOp(left=Num(n=1, lineno=4, col_offset=0), '
            'op=Add(), right=Num(n=1, lineno=4, col_offset=4), lineno=4, '
            'col_offset=0))'
        )

    def test_iter_fields(self):
        node = ast.parse('foo()', mode='eval')
        d = dict(ast.iter_fields(node.body))
        self.assertEqual(d.pop('func').id, 'foo')
        self.assertEqual(d, {'keywords': [], 'kwargs': None,
                             'args': [], 'starargs': None})

    def test_iter_child_nodes(self):
        node = ast.parse("spam(23, 42, eggs='leek')", mode='eval')
        self.assertEqual(len(list(ast.iter_child_nodes(node.body))), 4)
        iterator = ast.iter_child_nodes(node.body)
        self.assertEqual(next(iterator).id, 'spam')
        self.assertEqual(next(iterator).n, 23)
        self.assertEqual(next(iterator).n, 42)
        self.assertEqual(ast.dump(next(iterator)),
            "keyword(arg='eggs', value=Str(s='leek'))"
        )

    def test_get_docstring(self):
        node = ast.parse('def foo():\n  """line one\n  line two"""')
        self.assertEqual(ast.get_docstring(node.body[0]),
                         'line one\nline two')

    def test_literal_eval(self):
        self.assertEqual(ast.literal_eval('[1, 2, 3]'), [1, 2, 3])
        self.assertEqual(ast.literal_eval('{"foo": 42}'), {"foo": 42})
        self.assertEqual(ast.literal_eval('(True, False, None)'), (True, False, None))
        self.assertRaises(ValueError, ast.literal_eval, 'foo()')

    def test_literal_eval_issue4907(self):
        self.assertEqual(ast.literal_eval('2j'), 2j)
        self.assertEqual(ast.literal_eval('10 + 2j'), 10 + 2j)
        self.assertEqual(ast.literal_eval('1.5 - 2j'), 1.5 - 2j)
        self.assertRaises(ValueError, ast.literal_eval, '2 + (3 + 4j)')


def test_main():
    with test_support.check_py3k_warnings(("backquote not supported",
                                             SyntaxWarning)):
        test_support.run_unittest(AST_Tests, ASTHelpers_Test)

def main():
    if __name__ != '__main__':
        return
    if sys.argv[1:] == ['-g']:
        for statements, kind in ((exec_tests, "exec"), (single_tests, "single"),
                                 (eval_tests, "eval")):
            print kind+"_results = ["
            for s in statements:
                print repr(to_tuple(compile(s, "?", kind, 0x400)))+","
            print "]"
        print "main()"
        raise SystemExit
    test_main()


#### GENERATED FOR IRONPYTHON ####
exec_results = [
('Module', [('FunctionDef', (1, 0), 'f', ('arguments', [], None, None, []), [('Pass', (1, 9))], [])]),
('Module', [('ClassDef', (1, 0), 'C', [], [('Pass', (1, 8))], [])]),
('Module', [('FunctionDef', (1, 0), 'f', ('arguments', [], None, None, []), [('Return', (1, 8), ('Num', (1, 15), 1))], [])]),
('Module', [('Delete', (1, 0), [('Name', (1, 4), 'v', ('Del',))])]),
('Module', [('Assign', (1, 0), [('Name', (1, 0), 'v', ('Store',))], ('Num', (1, 4), 1))]),
('Module', [('AugAssign', (1, 0), ('Name', (1, 0), 'v', ('Store',)), ('Add',), ('Num', (1, 5), 1))]),
('Module', [('Print', (1, 0), ('Name', (1, 8), 'f', ('Load',)), [('Num', (1, 11), 1)], False)]),
('Module', [('For', (1, 0), ('Name', (1, 4), 'v', ('Store',)), ('Name', (1, 9), 'v', ('Load',)), [('Pass', (1, 11))], [])]),
('Module', [('While', (1, 0), ('Name', (1, 6), 'v', ('Load',)), [('Pass', (1, 8))], [])]),
('Module', [('If', (1, 0), ('Name', (1, 3), 'v', ('Load',)), [('Pass', (1, 5))], [])]),
('Module', [('Raise', (1, 0), ('Name', (1, 6), 'Exception', ('Load',)), ('Str', (1, 17), 'string'), None)]),
('Module', [('TryExcept', (1, 0), [('Pass', (2, 2))], [('ExceptHandler', (3, 0), ('Name', (3, 7), 'Exception', ('Load',)), None, [('Pass', (4, 2))])], [])]),
('Module', [('TryFinally', (1, 0), [('Pass', (2, 2))], [('Pass', (4, 2))])]),
('Module', [('Assert', (1, 0), ('Name', (1, 7), 'v', ('Load',)), None)]),
('Module', [('Import', (1, 0), [('alias', 'sys', None)])]),
('Module', [('ImportFrom', (1, 0), 'sys', [('alias', 'v', None)], 0)]),
('Module', [('Exec', (1, 0), ('Str', (1, 5), 'v'), None, None)]),
('Module', [('Global', (1, 0), ['v'])]),
('Module', [('Expr', (1, 0), ('Num', (1, 0), 1))]),
('Module', [('Pass', (1, 0))]),
('Module', [('While', (1, 0), ('Name', (1, 6), 'x', ('Load',)), [('Break', (1, 9))], [])]),
('Module', [('While', (1, 0), ('Name', (1, 6), 'x', ('Load',)), [('Continue', (1, 9))], [])]),
('Module', [('For', (1, 0), ('Tuple', (1, 4), [('Name', (1, 4), 'a', ('Store',)), ('Name', (1, 6), 'b', ('Store',))], ('Store',)), ('Name', (1, 11), 'c', ('Load',)), [('Pass', (1, 14))], [])]),
('Module', [('Expr', (1, 0), ('ListComp', (1, 0), ('Tuple', (1, 1), [('Name', (1, 2), 'a', ('Load',)), ('Name', (1, 4), 'b', ('Load',))], ('Load',)), [('comprehension', ('Tuple', (1, 11), [('Name', (1, 11), 'a', ('Store',)), ('Name', (1, 13), 'b', ('Store',))], ('Store',)), ('Name', (1, 18), 'c', ('Load',)), [])]))]),
('Module', [('Expr', (1, 0), ('GeneratorExp', (1, 0), ('Tuple', (1, 1), [('Name', (1, 2), 'a', ('Load',)), ('Name', (1, 4), 'b', ('Load',))], ('Load',)), [('comprehension', ('Tuple', (1, 11), [('Name', (1, 11), 'a', ('Store',)), ('Name', (1, 13), 'b', ('Store',))], ('Store',)), ('Name', (1, 18), 'c', ('Load',)), [])]))]),
]
single_results = [
('Interactive', [('Expr', (1, 0), ('BinOp', (1, 0), ('Num', (1, 0), 1), ('Add',), ('Num', (1, 2), 2)))]),
]
eval_results = [
('Expression', ('BoolOp', (1, 0), ('And',), [('Name', (1, 0), 'a', ('Load',)), ('Name', (1, 6), 'b', ('Load',))])),
('Expression', ('BinOp', (1, 0), ('Name', (1, 0), 'a', ('Load',)), ('Add',), ('Name', (1, 4), 'b', ('Load',)))),
('Expression', ('UnaryOp', (1, 0), ('Not',), ('Name', (1, 4), 'v', ('Load',)))),
('Expression', ('Lambda', (1, 0), ('arguments', [], None, None, []), ('Name', (1, 7), 'None', ('Load',)))),
('Expression', ('Dict', (1, 0), [('Num', (1, 2), 1)], [('Num', (1, 4), 2)])),
('Expression', ('ListComp', (1, 0), ('Name', (1, 1), 'a', ('Load',)), [('comprehension', ('Name', (1, 7), 'b', ('Store',)), ('Name', (1, 12), 'c', ('Load',)), [('Name', (1, 17), 'd', ('Load',))])])),
('Expression', ('GeneratorExp', (1, 0), ('Name', (1, 1), 'a', ('Load',)), [('comprehension', ('Name', (1, 7), 'b', ('Store',)), ('Name', (1, 12), 'c', ('Load',)), [('Name', (1, 17), 'd', ('Load',))])])),
('Expression', ('Compare', (1, 0), ('Num', (1, 0), 1), [('Lt',), ('Lt',)], [('Num', (1, 4), 2), ('Num', (1, 8), 3)])),
('Expression', ('Call', (1, 0), ('Name', (1, 0), 'f', ('Load',)), [('Num', (1, 2), 1), ('Num', (1, 4), 2)], [('keyword', 'c', ('Num', (1, 8), 3))], ('Name', (1, 11), 'd', ('Load',)), ('Name', (1, 15), 'e', ('Load',)))),
('Expression', ('Repr', (1, 0), ('Name', (1, 1), 'v', ('Load',)))),
('Expression', ('Num', (1, 0), 10L)),
('Expression', ('Str', (1, 0), 'string')),
('Expression', ('Attribute', (1, 0), ('Name', (1, 0), 'a', ('Load',)), 'b', ('Load',))),
('Expression', ('Subscript', (1, 0), ('Name', (1, 0), 'a', ('Load',)), ('Slice', (1, 1), ('Name', (1, 2), 'b', ('Load',)), ('Name', (1, 4), 'c', ('Load',)), None), ('Load',))),
('Expression', ('Name', (1, 0), 'v', ('Load',))),
('Expression', ('List', (1, 0), [('Num', (1, 1), 1), ('Num', (1, 3), 2), ('Num', (1, 5), 3)], ('Load',))),
('Expression', ('Tuple', (1, 0), [('Num', (1, 0), 1), ('Num', (1, 2), 2), ('Num', (1, 4), 3)], ('Load',))),
('Expression', ('Call', (1, 0), ('Attribute', (1, 0), ('Attribute', (1, 0), ('Attribute', (1, 0), ('Name', (1, 0), 'a', ('Load',)), 'b', ('Load',)), 'c', ('Load',)), 'd', ('Load',)), [('Subscript', (1, 8), ('Attribute', (1, 8), ('Name', (1, 8), 'a', ('Load',)), 'b', ('Load',)), ('Slice', (1, 11), ('Num', (1, 12), 1), ('Num', (1, 14), 2), None), ('Load',))], [], None, None)),
('Expression', ('Subscript', (1, 0), ('Name', (1, 0), 'a', ('Load',)), ('Ellipsis', (1, 1)), ('Load',))),
]
main()