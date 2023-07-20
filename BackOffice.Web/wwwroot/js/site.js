window.elementClick = (Id) => {
    // Triggers click event of the element
    document.getElementById(Id).click();
};

function enableCtrlSave(objectReference, methodName) {
    var isCtrl = false;
    document.onkeyup = function (e) {
        if (e.keyCode == 17) isCtrl = false;
    }

    document.onkeydown = function (e) {
        if (e.keyCode == 17) isCtrl = true;
        if (e.keyCode == 83 && isCtrl == true) {
            //run code for CTRL+S -- ie, save!
            objectReference.invokeMethodAsync(methodName);
            return false;
        }
    }
}

function restoreDocumentKeys() {
    document.onkeyup = function () { };
    document.onkeydown = function () { };
}

function showDefaultToast() {
    $(".toast").toast('show');
}