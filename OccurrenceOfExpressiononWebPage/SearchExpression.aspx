<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SearchExpression.aspx.cs" Inherits="OccurrenceOfExpressiononWebPage.SearchExpression" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Search occurence of Word on Website</title>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.3.1/jquery.min.js"></script>
    <link href="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/themes/smoothness/jquery-ui.css" rel="stylesheet" />
    <script src="https://ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js"></script>

    <script type="text/javascript" lang="js">
        $(document).ready(function () {
            debugger;
            $("#reportButton").click(function (e) {
                debugger;
                $("#reportButton").attr('disabled', true);
                $("[id*=reportGridView] tr").not($("[id*=reportGridView] tr:first-child")).remove();
                $('#reportGridView').css("display", "block");
                $.ajax({
                    type: "POST",
                    url: "SearchExpression.aspx/GetReport",
                    data: {},
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",

                    success: function (r) {
                        debugger;
                        for (var i = 0; i < r.d.length; i++) {
                            $("#reportGridView").append("<tr><td>" + r.d[i].Date + "</td><td>" + r.d[i].URL + "</td><td>"
                                + r.d[i].Printout + "</td><td>" + r.d[i].NoOfHits + "</td></tr>")

                        }
                       if (r.d == "Failure") {
                           alert("Due to technical issue your search could not be completed. Please try again..");
                            $("#SubmitButton").removeAttr('disabled', true);
                        }
                        $("#reportButton").removeAttr('disabled', true);
                    }


                });
                e.preventDefault();
            });

            $("#startButton").click(function (e) {
                debugger;
                $("#startButton").attr('disabled', true);
                alert("Please wait...");

                var urlofWebpage = $("#urlTextBox").val();
                var searchExpression = $("#expressionTextBox").val();

                var details = {
                    urlofWebpage: urlofWebpage, searchExpression: searchExpression
                }

                $.ajax({
                    type: "POST",
                    url: "SearchExpression.aspx/StartSearch",
                    data: JSON.stringify({ details: details }),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",

                    success: function (r) {
                        debugger;

                        if (r.d == "Success") {
                            alert("Results Saved to Database. To check, click on Report.");
                            $("#startButton").removeAttr('disabled', true);
                            $("#form1")[0].reset();
                            location.reload();
                        }
                        else if (r.d == "Failure") {
                            alert("Due to technical issue your search could not be completed. Please try again..");
                            $("#startButton").removeAttr('disabled', true);
                        }
                        $("#startButton").removeAttr('disabled', true);
                    }


                });
                e.preventDefault();
            });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <h1 style="color: orangered">Search what you need!</h1>
        <div>
            <table id="searchTable" style="width: unset">
                <tr>
                    <th>
                        <asp:Label ID="urlLabel" runat="server" Text="Provide the URL"></asp:Label></th>
                    <td>
                        <asp:TextBox ID="urlTextBox" runat="server" Width="500px" ClientIDMode="Static"></asp:TextBox></td>
                </tr>
                <tr>
                    <td>&nbsp;&nbsp;</td>
                </tr>
                <tr>
                    <th>
                        <asp:Label ID="descriptionLabel" runat="server" Text="Provide the Expression you want to search"></asp:Label></th>
                    <td>
                        <asp:TextBox ID="expressionTextBox" runat="server" Width="200px" ClientIDMode="Static"></asp:TextBox></td>
                </tr>

                <asp:Label ID="Label1" runat="server" Text=""></asp:Label>
            </table>
            
        </div>
        <br />
        <br />

        <input type="button" id="startButton" value="Start" />
        <div>
            <h1 style="color:forestgreen">Generate Report by Clicking on Report..</h1>
            <br />

            <input type="button" id="reportButton" value="Report" />
            <br /><br /><br />
            <asp:GridView ID="reportGridView" runat="server" style="display:none; border:0px"></asp:GridView>
        </div>
    </form>
</body>
</html>
