//window.__pageloaded = false
//function onpageloaded() {
//  window.__pageloaded = true
//}

$(document).ready(function() {
 
  test("variables/functions are defined properly", function() {
    ok(window.DLR, "DLR is defined")
    ok(window.Silverlight, "Silverlight is defined")
    ok(Object.merge, "merge function exists")
    ok(DLR.parseSettings, "DLR.parseSettings function exists")
    ok(DLR.defaultSettings, "DLR.defaultSettings function exists")
    ok(DLR.settings, "DLR.settings object exists")
  });

  test("default options", function() {
    equals(DLR.autoAdd, true);
    equals(DLR.path, null); 
  });

  test("default settings", function() {
    ds = DLR.defaultSettings()
    equals(ds.width, 1)
    equals(ds.height, 1)
    equals(ds.reportErrors, 'errorLocation')
    equals(ds.source, 'dlr.xap')
    equals(ds.onError, 'Silverlight.default_error_handler')
    equals(ds.id, 'silverlightDlrObject_DOMOnly')
  });

  module("merging objects")

    test("unique", function() {
      a = { a: 'hi', b: 'bye' }
      b = { c: 'so long' }
      c = Object.merge(a, b)
      equals(c.a, a.a)
      equals(c.b, a.b)
      equals(c.c, b.c)
      equals(a.c, undefined)
      equals(b.a, undefined)
      equals(b.b, undefined)
    });

    test("full conflict", function() {
      a = { a: 'hi', b: 'bye' }
      b = { a: 'bye', b: 'hi' }
      c = Object.merge(a, b)
      equals(c.a, b.a)
      equals(c.b, b.b)
      equals(a.a, 'hi')
      equals(a.b, 'bye')
    });

    test('realistic', function() {
      a = { a: 'hi', b: 'bye', c: 'ciao' }
      b = { b: 'salve' }
      c = Object.merge(a, b)
      equals(c.a, a.a)
      equals(c.b, b.b)
      equals(c.c, a.c)
    });
  
  var temppath = null;
  module("options", {
    setup: function() {
      temppath = DLR.path
    },
    teardown: function() {
      DLR.path = temppath;
      temppath = null;
    }
  })

    test('DLR.path changes defaultSettings.source', function() {
      equals(DLR.path, null);
      equals(DLR.defaultSettings().source, 'dlr.xap');
      DLR.path = ".."
      equals(DLR.defaultSettings().source, "../dlr.xap");
    });

  module("parsing settings")

    test('no customization', function() {
      ds = DLR.defaultSettings()
      settings = DLR.parseSettings(ds, {})
      equals(settings.initParams, "reportErrors=" + ds.reportErrors)
      equals(settings.reportErrors, undefined)
      equals(settings.width, ds.width)
      equals(settings.height, ds.height)
      equals(settings.onerror, ds.onerror)
      equals(settings.source, ds.source)
      equals(settings.id, ds.id)
    });

    test('new DLR setting', function() {
      settings = DLR.parseSettings(DLR.defaultSettings(), {console: true})
      equals(settings.console, undefined)
      ok(/console=true/.test(settings.initParams), "console=true is in initParams")
    });

    test('new SL setting', function() {
      settings = DLR.parseSettings(DLR.defaultSettings(), {windowless: true})
      equals(settings.windowless, true)
    });

    test('merging settings', function() {
       settings = DLR.parseSettings(DLR.defaultSettings(), {height: '100%', width: '100%', source: 'tests.xap'})
       equals(settings.height, '100%')
       equals(settings.width, '100%')
       equals(settings.source, 'tests.xap')
    });

    test("DLR settings make their way into initParams", function() {
      news = {
        debug: true, console: true, start: 'foo.py', exceptionDetail: true, reportErrors: 'errorLocation', xamlid: 'foo'
      };
      settings = DLR.parseSettings(DLR.defaultSettings(), news);

      for(d in news) {
        equals(settings[d], undefined, d + " is undefined in settings")
        ok(new RegExp(d + "=" + news[d]).test(settings.initParams), d + "=" + news[d] + " is in initParams")
      }
    });

  module("Silverlight control loading")

    test("Silverlight control with default settings is added to the page automatically", function() {
      name = 'object#silverlightDlrObject_DOMOnly'
      waitForTrue(
        function() {
          return $(name).length > 0
        },
        function() {
          object = $(name)
          equals(object.length, 1)
          equals(object.attr('width'), "1")
          equals(object.attr('height'), "1")
          equals(object.attr('data'), "data:application/x-silverlight,")
          equals(object.attr('type'), "application/x-silverlight")
        }, 
        function() {
          ok(false, name + " not found")
        }
      );
    });

    test("Manually adding controls", function() {
      DLR.createObject();
      equals($('object#silverlightDLRObject1').length, 1)
      DLR.createObject();
      equals($('object#silverlightDLRObject2').length, 1)
    });

  var old_createObject = null;
  module('XAML script tags', {
    setup: function() {
      old_createObject = DLR.createObject
    },
    teardown: function() {
      DLR.createObject = old_createObject
    }
  })

    test('with ID produces a SL control with matching xamlid', function() {
      $(document.body).append("<script type=\"application/xml+xaml\" width=\"100\" height=\"150\" id=\"foo\"></script>")
      equals(DLR.__loaded, true)
      DLR.createObject = function(settings) {
        equals(settings.width, '100')
        equals(settings.height, '150')
        equals(settings.xamlid, 'foo')
      }
      DLR.__startup()
      $('script#foo').remove()
    })

    test('without ID produces a SL control with autogenerated xamlid and id', function() {
      $(document.body).append("<script type=\"application/xml+xaml\" width=\"200\" height=\"250\"></script>")
      equals(DLR.__loaded, true)
      id = DLR.__defaultXAMLId + DLR.__objectCount
      DLR.createObject = function(settings) {
        equals(settings.width, '200')
        equals(settings.height, '250')
        equals(settings.xamlid, id)
      }
      DLR.__startup()
      $('script#' + id).remove()
    });

});
