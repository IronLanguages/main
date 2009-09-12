//window.__pageloaded = false
//function onpageloaded() {
//  window.__pageloaded = true
//}

$(document).ready(function() {
 
  test("variables/functions are defined properly", function() {
    ok(window.DLR, "DLR is defined")
    ok(window.Silverlight, "Silverlight is defined")
    ok(Object.merge, "merge function exists")
    ok(DLR.parseSettings, "DLR.parseSettings exists")
    ok(DLR.defaultSettings, "DLR.defaultSettings exists")
    ok(DLR.settings, "DLR.settings exists")
  });

  test("default settings", function() {
    equals(DLR.defaultSettings.autoAdd, true)
    equals(DLR.defaultSettings.width, 1)
    equals(DLR.defaultSettings.height, 1)
    equals(DLR.defaultSettings.reportErrors, 'errorLocation')
    equals(DLR.defaultSettings.source, 'dlr.xap')
    equals(DLR.defaultSettings.onerror, 'Silverlight.default_error_handler')
    equals(DLR.defaultSettings.id, 'silverlightDlrObject_DOMOnly')
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

  module("parsing settings")

    test('no customization', function() {
      settings = DLR.parseSettings(DLR.defaultSettings, {})
      equals(settings.autoAdd, true)
      equals(DLR.autoAdd, DLR.defaultSettings.autoAdd)
      equals(settings.initParams, "reportErrors=" + DLR.defaultSettings.reportErrors)
      equals(settings.reportErrors, undefined)
      equals(settings.width, DLR.defaultSettings.width)
      equals(settings.height, DLR.defaultSettings.height)
      equals(settings.onerror, DLR.defaultSettings.onerror)
      equals(settings.source, DLR.defaultSettings.source)
      equals(settings.id, DLR.defaultSettings.id)
    });

    test('new DLR setting', function() {
      settings = DLR.parseSettings(DLR.defaultSettings, {console: true})
      equals(settings.console, undefined)
      ok(/console=true/.test(settings.initParams), "console=true is in initParams")
    });

    test('new SL setting', function() {
      settings = DLR.parseSettings(DLR.defaultSettings, {windowless: true})
      equals(settings.windowless, true)
    });

    test('merging settings', function() {
       settings = DLR.parseSettings(DLR.defaultSettings, {height: '100%', width: '100%', source: 'tests.xap'})
       equals(settings.height, '100%')
       equals(settings.width, '100%')
       equals(settings.source, 'tests.xap')
    });

    test("DLR settings make their way into initParams", function() {
      news = {
        autoAdd: false, debug: true, console: true, start: 'foo.py', exceptionDetail: true, reportErrors: 'errorLocation'
      };
      settings = DLR.parseSettings(DLR.defaultSettings, news);
      equals(settings.autoAdd, false)
      delete news.autoAdd

      for(d in news) {
        equals(settings[d], undefined, d + " is undefined in settings")
        ok(new RegExp(d + "=" + news[d]).test(settings.initParams), d + "=" + news[d] + " is in initParams")
      }
    });

  module("Silverlight control loading")

    test("Silverlight control with default settings is added to the page automatically", function() {
      name = 'object#' + DLR.defaultSettings.id 
      waitForElement(name,
        function() {
          object = $(name)
          equals(object.length, 1)
          equals(object.attr('width'), "1")
          equals(object.attr('height'), "1")
          equals(object.attr('data'), "data:application/x-silverlight,")
          equals(object.attr('type'), "application/x-silverlight")
        }, 
        function() {
          ok(false, "element not found")
        }
      );
    });

    test("Manually adding a control", function() {
      DLR.createObject();
      name1 = 'object#silverlightDLRObject1'
      name2 = 'object#silverlightDLRObject2'
      waitForElement(name1, 
        function() {
          equals($(name1).length, 1)
        }, 
        function() { ok(false, name1 + " not found") }
      );
      DLR.createObject();
      waitForElement(name2, 
        function() {
          equals($(name2).length, 1)
        }, 
        function() { ok(false, name2 + " not found") }
      );
    });
   
});
