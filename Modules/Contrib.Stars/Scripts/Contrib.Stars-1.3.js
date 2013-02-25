(function ($) {
    $.fn.extend({
        helpfullySetTheStars: function () {
            var __cookieScope = "__stars";
            var __cookiePath = document.location.pathname;
            return $(this).each(function () {
                var _this = $(this);
                var forms = _this.find("form");
                if (forms.length != 1) {
                    return _this;
                }

                var clearVote = $("<span class=\"stars-clear\">x</span>");
                $(".stars-clear").live(
                        "click",
                        function (e) {
                            var _clear_this = $(this);
                            _clear_this.addClass("active");

                            var form = _clear_this.closest(".stars-rating").find("form").first();
                            form.find('[name="rating"]').val(-1);
                            $.post(
                                form.attr("action"),
                                form.serialize()
                            );
                            form = null;

                            $.orchard.cookie(__cookieScope, null, { path: __cookiePath });

                            var resultDisplay = _clear_this.closest(".stars-current-result").first();
                            var existingUserRating = resultDisplay.attr("class").match(/\bstars-user-rating-\d+\b/);
                            if (existingUserRating && existingUserRating.length > 0) {
                                resultDisplay.removeClass(existingUserRating[0]);
                            }
                            removeClearVoteUI(_clear_this);

                            e.preventDefault();
                            return false;
                        })
                    .live(
                        "mouseenter",
                        function () { $(this).addClass("mousey"); }
                        )
                    .live(
                        "mouseleave",
                        function () { $(this).removeClass("mousey"); }
                    );

                function addClearVoteUI(fromHere) {
                    fromHere.closest(".stars-current-result").first().append(clearVote);
                }

                function removeClearVoteUI(fromHere) {
                    fromHere.closest(".stars-current-result").first().children(".stars-clear").removeClass("mousey").removeClass("active").remove();
                }

                _this.find(".a-star")
                    .click(function () {
                        var _thisStar = $(this);
                        var ratingMatch = _thisStar.attr("class").match(/\bstar-(\d+)\b/);
                        if (!ratingMatch || ratingMatch.length < 2) {
                            return;
                        }

                        var rating = _thisStar.attr("class").match(/\bstar-(\d+)\b/)[1];

                        var form = _thisStar.closest(".stars-rating").find("form");
                        form.find('[name="rating"]').val(rating);
                        $.post(
                            form.attr("action"),
                            form.serialize()
                        );
                        form = null;

                        $.orchard.cookie(__cookieScope, rating, { expires: $.orchard.__cookieExpiration, path: __cookiePath });

                        // not bothering to update the display for a failed vote. first use case to implement might be for a user who's auth session has expired
                        var resultDisplay = _this.find(".stars-current-result").first();
                        var existingUserRating = resultDisplay.attr("class").match(/\bstars-user-rating-\d+\b/);
                        if (existingUserRating && existingUserRating.length > 0) {
                            resultDisplay.removeClass(existingUserRating[0]);
                        }

                        resultDisplay.addClass("stars-user-rating-" + rating)
                        resultDisplay = null;

                        addClearVoteUI(_thisStar);
                    })
                    .hover(
                        function () { // mouseenter
                            var _thisStar = $(this);
                            _this.addClass(_thisStar.attr("class").match(/\bstar-\d+\b/)[0]);
                        },
                        function () { // mouseleave
                            var _thisStar = $(this);
                            _this.removeClass(_thisStar.attr("class").match(/\bstar-\d+\b/)[0]);
                        });

                // add the "clear vote" bit
                var result;
                if ((result = _this.find(".stars-current-result").first()) !== undefined && result.attr("class").match(/\bstars-user-rating-\d+\b/)) {
                    addClearVoteUI(result);
                }

                return _this;
            });
        }
    });
    $(function () {
        $(".stars-rating").helpfullySetTheStars();
    });
})(jQuery);