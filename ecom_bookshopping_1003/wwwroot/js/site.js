// Interactive 3D Card Tilt & Parallax System for BookKart

$(document).ready(function () {
    // Interactive 3D Cursor Tilt Effect for Product Cards
    $(document).on("mousemove", ".book-card", function (e) {
        var card = $(this);
        var cardOffset = card.offset();
        var cardWidth = card.outerWidth();
        var cardHeight = card.outerHeight();

        // Calculate cursor offset relative to card center
        var mouseX = e.pageX - cardOffset.left;
        var mouseY = e.pageY - cardOffset.top;

        var rotateY = ((mouseX / cardWidth) - 0.5) * 16; // Max 8 deg rotation
        var rotateX = ((mouseY / cardHeight) - 0.5) * -16; // Max 8 deg rotation

        card.css({
            "transform": "perspective(1000px) rotateX(" + rotateX + "deg) rotateY(" + rotateY + "deg) scale3d(1.02, 1.02, 1.02)",
            "transition": "transform 0.08s ease-out"
        });
    });

    // Reset 3D transform on mouse leave
    $(document).on("mouseleave", ".book-card", function () {
        $(this).css({
            "transform": "perspective(1000px) rotateX(0deg) rotateY(0deg) scale3d(1, 1, 1)",
            "transition": "transform 0.4s ease-out"
        });
    });
});
