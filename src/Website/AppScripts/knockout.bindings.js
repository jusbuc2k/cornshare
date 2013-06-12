ko.bindingHandlers.modalIsVisible = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        if (ko.utils.unwrapObservable(valueAccessor())) {
            $(element).modal({ persist: true, opacity: 50 });
        } else {
            $(element).hide();
        }
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here
    },
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        if (ko.utils.unwrapObservable(valueAccessor())) {
            $(element).modal({ persist: true, opacity: 50 });
        } else {
            $.modal.close();
        }
        // var zone = Dropzone.forElement(element);
        // This will be called once when the binding is first applied to an element,
        // and again whenever the associated observable changes value.
        // Update the DOM element based on the supplied values here.
    }
};