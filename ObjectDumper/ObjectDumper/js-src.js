var data = "<ul id=\"finder\"><li>1<ul><li>3</li></ul></li><li>2</li></ul>";

if (!window.jQuery) {
    var jqscript = document.createElement('script');
    jqscript.setAttribute("type", "text/javascript");
    jqscript.setAttribute("src", "http://code.jquery.com/jquery.min.js");
    document.getElementsByTagName("head")[0].appendChild(jqscript);
};

(function timeout() {
    if (!window.jQuery) {
        setTimeout(timeout, 300);
    } else {
        $(function () {
            var $objectDumper = $('#finderparent');
            if ($objectDumper.length == 0) {
                $objectDumper = $("<div id=\"finderparent\"></div>").appendTo('body');
            }
            var $finder = $(data).appendTo($objectDumper);
            
            $('body').addClass('domenabled');

            var parentClass = 'parent';
            var showClass = 'shown';
            var hideClass = 'hidden';
            var openClass = 'open';

            $finder
                .find('ul').addClass(hideClass)
                .find('ul:only-child').before('[item]');

            var onClick = function () {
                var $a = $(this);
                var $li = $a.parent().parent();
                $li.find('ul').each(function () {
                    $(this).removeClass(showClass).addClass(hideClass);
                });
                $li.find('a').each(function () {
                    $(this).removeClass(openClass).addClass(parentClass);
                });
                $a.removeClass(parentClass).addClass(openClass);
                $a.next('ul').addClass(showClass);

                return false;
            };

            $finder.find('li:has(ul)').each(function () {
                var $name = $(this).contents().filter(function () { return this.nodeName.toUpperCase() != 'UL'; });
                var a = document.createElement('a');
                a.href = '#';
                a.className = parentClass;
                $name.wrapAll(a).parent().click(onClick);
            });
        });
    }
})();



