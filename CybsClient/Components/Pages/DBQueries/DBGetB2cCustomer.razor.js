function callCustDropDownNetMethod(itemChosen) {
    alert("IN callDropDownNetMethod");
    DotNet.invokeMethodAsync('CybsClient', 'NextAction', itemChosen)
        .then(data => {
            console.log(data);
        });
}