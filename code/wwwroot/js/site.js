// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {


    $('.book-items').slick({
        infinite: false,
        dots: true,
        arrows: true,
        responsive: [{
            breakpoint: 1024,
            settings: {
                slidesToShow: 3,
                infinite: false,
                arrows: true,
                draggable: true
            }
        }, {
            breakpoint: 600,
            settings: {
                slidesToShow: 2,
                dots: true,
                arrows: false,
                infinite: false,
                draggable: true


            }
        }, {
            breakpoint: 350,
            settings: {
                slidesToShow: 1,
                dots: true,
                draggable: true,
                infinite: false,
                arrows: false
            }
        }]
    });

    $('.video-items').slick({
        infinite: false,
        dots: true,
        arrows: true,
        responsive: [{
            breakpoint: 1024,
            settings: {
                arrows: true,
                draggable: true,
                slidesToShow: 2
            }
        }, {
            breakpoint: 800,
            settings: {
                slidesToShow: 2,
                dots: true,
                draggable: true,
                arrows: true

            }
        }, {
            breakpoint: 600,
            settings: {
                slidesToShow: 2,
                dots: true,
                draggable: true,
                arrows: false

            }
        }, {
            breakpoint: 350,
            settings: {
                slidesToShow: 2,
                dots: true,
                draggable: true,
                arrows: true
            }
        }]
    });
});