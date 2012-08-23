$(function () {
    var $finder = $('#finder');
    if (!$finder) return;

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
