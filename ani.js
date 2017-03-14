var isOk=true;
    $(".treeBtn").click(function(){
        if(isOk){
            $(this).text(">>");
            $(".indexLeft").animate({
                width:"0%"
            },1000);
            $(".indexRight").animate({
                width:"98%",
            },1000);
            setTimeout(function(){
                $(".indexLeft").css("border","0px");
            },1000)
            $(".indexRight").css("float","left");
            isOk=!isOk;
        }else{
            $(this).text("<<");
            $(".indexLeft").css("border","1px solid #ccc");
            $(".indexLeft").animate({
                width:"20%"
            },1000);
            $(".indexRight").animate({
                width:"78%"
            },1000);
            $(".indexRight").css("float","right");
            isOk=!isOk;
        }
    })
