function jsProcessTransToken(tt) {
    alert("IN callInteriorJSTestNetMethod");
    DotNet.invokeMethodAsync('CybsClient', 'ProcessTransToken', tt)
        .then(data => {
            console.log(data);
        });
}