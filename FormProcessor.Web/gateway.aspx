<%@ Page Language="C#" Trace="false" CodeBehind="gateway.aspx.cs" EnableViewState="false" Inherits="FormProcessor.Gateway" AutoEventWireup="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Bellevue College Form Processor</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
			<p>
				<%= UserMessage %>
			</p>
			<p>
<%-- NOTE: Only one of the following link formats will be displayed: --%>
				<%-- If only the raw URL is supplied in the settings (DO NOT CHANGE the 'ID', 'Visible' or 'runat' attributes) --%>
				<a href="" ID="ReturnUrl" Visible="false" runat="server">&lt;&lt; Return</a>
				<%-- If a custom anchor tag is supplied (this entire tag will be replaced with the specified anchor tag) --%>
				<asp:Literal ID="ReturnLink" Visible="false" runat="server"></asp:Literal>
			</p>
    </div>
    </form>
</body>
</html>
