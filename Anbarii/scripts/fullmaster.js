$(document).ready(function () {
    var offset = 200,
//browser window scroll (in pixels) after which the "back to top" link opacity is reduced
offset_opacity = 1100,
//duration of the top scrolling animation (in ms)
        $back_to_top = $('.cd-top');

    scroll_top_duration = 2000;
    $(window).scroll(function () {
        ($(this).scrollTop() > offset) ? $back_to_top.addClass('cd-is-visible') : $back_to_top.removeClass('cd-is-visible cd-fade-out');
        if ($(this).scrollTop() > offset_opacity) {
            $back_to_top.addClass('cd-fade-out');
        }
    });

    //smooth scroll to top
    $back_to_top.on('click', function (event) {
        event.preventDefault();
        $('body,html').animate({
            scrollTop: 0,
        }, scroll_top_duration
        );
    });
});