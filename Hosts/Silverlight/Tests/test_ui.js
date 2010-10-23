$(document).ready(function() {
    
  module('Error handling')

  test('basic python exception thrown', function() {
    execute('basic_python_exception_thrown', [{
      type: 'text/python', src: 'tests/regressions/fixtures/test2.py', defer: 'defer' }, {
      type: 'text/python', src: 'tests/regressions/fixtures/test.py'
    }]);

    test_dlr_exception({
      'type': "AssertionError",
      'file': 'tests/regressions/fixtures/test2.py',
      'line': 20,
      'message': 'AssertionError: Error raised at line 20 in test2.py.',
      'stack': [
        'at <module> in tests/regressions/fixtures/test2.py, line 20',
        'at f1 in tests/regressions/fixtures/test.py, line 5',
        'at <module> in tests/regressions/fixtures/test.py, line 7'
      ]
    });
  });

  test('basic ruby exception thrown', function() {
    execute('basic_ruby_exception_thrown', [{
      type: 'text/ruby', src: 'tests/regressions/fixtures/test2.rb', defer: 'defer' }, {
      type: 'text/ruby', src: 'tests/regressions/fixtures/test.rb'
    }]);
    // BUG: fix Ruby stack traces
    test_dlr_exception({
      'type': "RuntimeError",
      //'file': 'tests/regressions/fixtures/test2.py',
      //'line': 20,
      'message': 'RuntimeError: Error raised at line 20 in test2.rb.',
      //'stack': [
      //  'at <module> in tests/regressions/fixtures/test2.py, line 20',
      //  'at f1 in tests/regressions/fixtures/test.py, line 5',
      //  'at <module> in tests/regressions/fixtures/test.py, line 7'
      //]
    });
  });

  test('Attribute Error', function() {
    execute('x_attribute_error', [{
      type: 'text/python', src: 'tests/regressions/fixtures/x_attribute_error.py'
    }]);
    test_dlr_exception({
      type: 'AttributeError',
      message: "AttributeError: 'type' object has no attribute 'NonExistingItem'",
      file: "tests/regressions/fixtures/x_attribute_error.py",
      line: 6,
      stack: ['at <module> in tests/regressions/fixtures/x_attribute_error.py, line 6']
    });
  });

  test('Division by Zero Error', function() {
    execute('x_dividebyzero', [{
      type: 'text/python', src: 'tests/regressions/fixtures/x_dividebyzero.py'
    }]);
    test_dlr_exception({
      type: 'ZeroDivisionError',
      message: "ZeroDivisionError: Attempted to divide by zero."
    });
  });

  test('Import 1', function() {
    execute('x_import_1', [{
      type: 'text/python', src: 'tests/regressions/fixtures/x_import_1.py'
    }]);
    test_dlr_exception({
      type: 'ImportError',
      message: "ImportError: No module named SomethingNotExist"
    });
  });

  test('Import 2', function() {
    execute('x_import_2', [{
      type: 'text/python', src: 'tests/regressions/fixtures/module_with_syntaxerror.py', defer: 'defer' }, {
      type: 'text/python', src: 'tests/regressions/fixtures/x_import_2.py'
    }]);
    test_dlr_exception({
      type: 'SyntaxError',
      message: "SyntaxError: unexpected token ':'"
    });
  });

  test('Import 3', function() {
    execute('x_import_3', [{
      type: 'text/python', src: 'tests/regressions/fixtures/module_throw.py', defer: 'defer' }, {
      type: 'text/python', src: 'tests/regressions/fixtures/x_import_3.py'
    }]);
    test_dlr_exception({
      type: 'AssertionError',
      message: 'AssertionError: Silverlight test for throwing exception.'
    });
  });

  test('Rethrow', function() {
    execute('x_rethrow', [{
      type: 'text/python', src: 'tests/regressions/fixtures/x_rethrow.py'
    }]);
    test_dlr_exception({
      type: 'ZeroDivisionError'
    });
  });

  test('Syntax error', function() {
    execute('x_syntax_error', [{
      type: 'text/python', src: 'tests/regressions/fixtures/x_syntax_error.py'
    }]);
    test_dlr_exception({
      type: 'SyntaxError',
      message: "SyntaxError: unexpected token ':'",
      file: 'tests/regressions/fixtures/x_syntax_error.py',
      line: 3
    });
  });

});
