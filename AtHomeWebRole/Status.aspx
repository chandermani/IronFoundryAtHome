<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Status.aspx.cs"
    Inherits="AtHomeWebRole.Status" ClientIDMode="Static" %>

<asp:Content ID="pageContent" ContentPlaceHolderID="MainContent" runat="server">
    <div>
        <div id="menucontainer">
            <ul id="menu">
                <li><a href="http://distributedcomputing.cloudapp.net/stats"   target="_blank">team stats</a></li>
                <li><a href="http://distributedcomputing.cloudapp.net/visual" target="_blank">world
                    map</a></li>
            </ul>
        </div>
        <form id="statusForm" runat="server">
        <div id="statusHeader" style="overflow: auto">
            <div id="statusGreeting" class="left_justified">
                <strong>
                    <asp:Literal ID="litName" runat="server" /></strong>, here's your current contribution
                to the Folding@home project.
            </div>
            <div class="right_justified">
                <asp:Button ID="Button1" runat="server" Text="Refresh this Page" 
                    CssClass="site-arrowboxcta" />
            </div>
        </div>
        <div class="progress_table">
            <p class="tableHeading">
                Work Units In-Progress</p>
            <asp:Literal ID="litNoProgress" runat="server">No work units are currently being processed. You can press the refresh button above to check for updates, which occur on the server per the <em>PollingInterval</em> (in minutes) setting of the <em>AtHomeWebRole</em>.</asp:Literal>
            <asp:Repeater ID="InProgress" runat="server">
                <ItemTemplate>
                    <div class="progress_row">
                        <div class="progress_name">
                            <%# DataBinder.Eval(Container.DataItem, "Name") %>
                            |
                            <%# DataBinder.Eval(Container.DataItem, "Tag") %>
                            <br />
                            <em>
                                <%# DataBinder.Eval(Container.DataItem, "InstanceId")%></em> </div><div class="progress_pct">
                                    <div class="outerbar">
                                        <div class="innerbar" style='width: <%# DataBinder.Eval(Container.DataItem, "Progress") %>%'>
                                            <span class="bartext">
                                                <%# DataBinder.Eval(Container.DataItem, "Progress") %>% </span>
                                        </div>
                                    </div>
                                </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
        <hr style="width: 80%; margin-top: 15px; margin-bottom: 15px;" />
        <div class="progress_table">
            <p class="tableHeading">
                <asp:Literal ID="litCompletedTitle" runat="server" Text="Work Units Completed"></asp:Literal>
            </p>
            <asp:GridView ID="GridViewCompleted" AutoGenerateColumns="False" runat="server" EnableViewState="False"
                CellSpacing="3" HorizontalAlign="Center" GridLines="None">
                <Columns>
                    <asp:BoundField DataField="Name" HeaderText="Name" />
                    <asp:BoundField DataField="Tag" HeaderText="Tag" />
                    <asp:BoundField DataField="StartTime" DataFormatString="{0:g}" HeaderText="Start Time"
                        ItemStyle-Width="180px" ItemStyle-HorizontalAlign="Center">
                        <ItemStyle HorizontalAlign="Center" Width="180px"></ItemStyle>
                    </asp:BoundField>
                    <asp:BoundField DataField="Duration" HeaderText="Duration" ItemStyle-Width="120px"
                        ItemStyle-HorizontalAlign="Center">
                        <ItemStyle HorizontalAlign="Center" Width="120px"></ItemStyle>
                    </asp:BoundField>
                </Columns>
                <HeaderStyle BorderStyle="None" Font-Bold="True" Font-Underline="True" ForeColor="Black" />
            </asp:GridView>
        </div>
        </form>
    </div>
</asp:Content>
