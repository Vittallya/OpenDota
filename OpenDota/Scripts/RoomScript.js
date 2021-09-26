
//$("#replaceMe1").ready(() => {
//    $("#connectLink").click();
    
//});

$("#container").ready(() => {

    window.onbeforeunload = async function () {

        var pr = window
            .location
            .search
            .replace('?', '')
            .split('&')
            .reduce(
                function (p, e) {
                    var a = e.split('=');
                    p[decodeURIComponent(a[0])] = decodeURIComponent(a[1]);
                    return p;
                },
                {}
        );
        
        await fetch('/Home/OnUserUnload?RoomId=' + pr['RoomId']+'&isCreator='+pr['IsCreator'], {
            method: 'POST'
        });
    };

    $("#connectLink").click(); 

    //var resp = await fetch("home/StartMatch?isCreator=" + pr['isCreator'] + "&roomId="+pr['roomId'])
    //if (resp.ok) {
        
    //}
});

function RefreshHeroes() {
    $("#actualHeroesLink").click(); 
}

function StartRefresh() {
    $("#awaitLink").click();  

}
