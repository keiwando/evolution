mergeInto(LibraryManager.library, {

    _openLink: function (url_pointer) {
        var url = Pointer_stringify(url_pointer)
        window.open(url, '_blank')
    },

    _openLinkOnMouseUp: function (url_pointer) {
        var url = Pointer_stringify(url_pointer)
        var onMouseUpBefore = document.onmouseup
        document.onmouseup = function () {
            window.open(url, '_blank')
            document.onmouseup = onMouseUpBefore
        }
    }
});