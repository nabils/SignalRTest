<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebApplication2.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    
    <script type="text/javascript" src="Scripts/jquery-1.6.4.js"></script>
    <script type="text/javascript" src="Scripts/jquery.signalR-0.5.1.js"></script>
    <script src="/signalr/hubs" type="text/javascript"></script>

    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
        <script type="text/javascript">
            $(function () {
                // Proxy created on the fly
                var chat = $.connection.chat;

                // Declare a function on the chat hub so the server can invoke it
                chat.addMessage = function (message) {
                    $('#messages').append('<li>' + message + '</li>');
                };

                $("#broadcast").click(function () {
                    // Call the chat method on the server
                    chat.send($('#msg').val());
                });

                // Start the connection
                $.connection.hub.start();
            });
        </script>
        <input type="text" id="msg" />
        <input type="button" id="broadcast" />
        <ul id="messages"></ul>
    </div>
    </form>
</body>
</html>
