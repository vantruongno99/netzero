//"use strict";

@{
    ViewData["Title"] = "Home Page";
}

@section Links{
    <link rel="stylesheet" href="https://cdn.datatables.net/1.10.21/css/jquery.dataTables.min.css" />
}



<div class="container">
    <div class="row">&nbsp;</div>
    <div class="row">
        <div class="col-2">SensorID</div>
        <div class="col-4"><input type="text" id="userInput" /></div>
    </div>
    <div class="row">
        <div class="col-2">Message</div>
        <div class="col-4"><input type="text" id="messageInput" /></div>
    </div>
    <div class="row">&nbsp;</div>
    <div class="row">
        <div class="col-6">
            <input type="button" id="sendButton" value="Send Message" />
        </div>
    </div>
</div>
    <div class="row">
        <div class="col-12">
            <hr />
        </div>
    </div>


    <body>
        <table class="table table-striped table-bordered" id="example">
            <thead>
                <tr>
                    <th></th>
                    <th>Name</th>
                    <th>User Name</th>
                    <th>EMail</th>
                    <th>Phone</th>
                    <th>Website</th>
                </tr>
            </thead>
        </table>
    </body>

    <table id="examplecode" class="display" style="width:100%">
        <thead>
            <tr>
                <th>Name</th>
                <th>Position</th>
                <th>Office</th>
            </tr>
        </thead>
        <tbody>
        </tbody>
        <tfoot>
            <tr>
                <th>Name</th>
                <th>Position</th>
                <th>Office</th>
            </tr>
        </tfoot>
    </table>


    <table id="table1" class="table table-striped">
        <thead style="background-color: silver">
            <tr>
                <th>Sensor Id1</th>
            </tr>
        </thead>
        <tr id="messagesList">
        </tr>

    </table>

@section Scripts{
    <script src="https://cdn.datatables.net/1.10.21/js/jquery.dataTables.min.js"></script>
        <script>

            $(document).ready(function () {
            var tableData =
                [{
                "SensorName": "TB001",
                    "Position": "PY",
                    "Office": "CTI"
                }]

            tableData.push({
                "SensorName": "002",
                "Position": "CT",
                "Office": "CTI"
            })

            var tbl = $('#examplecode').DataTable({
                "data": tableData,
                "columns": [
                    {"data": "SensorName" },
                    {"data": "Position" },
                    {"data": "Office" }
                ]
            });

            setTimeout(() => {
                tbl.row.add({
                    "SensorName": "0001",
                    "Position": "SC",
                    "Office": "CTI"
                })

                tbl.rows().invalidate("data").draw(false);
            }, 3000)



            var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

            //Disable send button until connection is established
            document.getElementById("sendButton").disabled = true;

            connection.on("ReceiveMessage", function (user, message) {
                var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = user + " sends " + msg;
    var tr2 = document.createElement("tr");
    tr2.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(tr2);
});


connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});
             
        });

 


    </script >
}

<script src="~/js/signalr/dist/browser/signalr.js"></script>

    <script src="~/js/chat.js"></script>





