function callDropDownNetMethod(itemChosen) {
    alert("IN callDropDownNetMethod");
    DotNet.invokeMethodAsync('CybsClient', 'DropDownMethod', itemChosen)
        .then(data => {
            console.log(data);
        });
}

function callCustDropDownNetMethod(itemChosen) {
    alert("IN callDropDownNetMethod");
    DotNet.invokeMethodAsync('CybsClient', 'NextAction', itemChosen)
        .then(data => {
            console.log(data);
        });
}