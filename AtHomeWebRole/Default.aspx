<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs"
    Inherits="AtHomeWebRole._Default" ClientIDMode="Static" %>

<asp:Content ID="headContent" ContentPlaceHolderID="HeadContent" runat="server">
    <script type="text/javascript" src="http://ecn.dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=6.2"></script>
    <script type="text/javascript">
        var map = null;
        var pin = null;

        $(document).ready(function () {
            setTimeout("LoadMap()", 500);
        });

         function LoadMap() {

            if ($("#myMap").length == 0) return;

            var ipResolved = ($("#txtLatitude").val().length > 0) && ($("#txtLongitude").val().length > 0);
            latitude = ($("#txtLatitude").val().length == 0) ? 38.5 : $("#txtLatitude").val() * 1;
            longitude = ($("#txtLongitude").val().length == 0) ? -97.4 : $("#txtLongitude").val() * 1;

            // start up the map and center on client's IP
            map = new VEMap('myMap');
            map.LoadMap(new VELatLong(latitude, longitude), 4);
            if (ipResolved) SetLatLong(latitude, longitude);

            map.Resize(900, 480);
            map.AttachEvent("onclick", MouseClick);
            $("#myMap").children().first().css("cursor", "pointer");

            // create overlay with instruction to Shift+click
            control = document.createElement("div");
            control.innerHTML = "Shift+click to select your location";
            control.style.textAlign = "center";
            control.style.top = "1px";
            control.style.left = "671px";
            control.style.width = "220px";
            control.style.padding = "5px";
            control.style.background = "Black";
            control.style.color = "Yellow"
            map.AddControl(control);

            // move overlay stuff
            var shim = document.createElement("iframe");
            shim.frameBorder = "0";
            shim.style.position = "absolute";
            shim.style.zIndex = "1";
            shim.width = control.offsetWidth;
            shim.height = control.offsetHeight;
            shim.style.top = control.offsetTop;
            shim.style.left = control.offsetWidth;
            control.shimElement = shim;
            control.parentNode.insertBefore(shim, control);

        };

        function SetLatLong(lat, long) {
            $("#txtLatitude").val(lat);
            $("#txtLongitude").val(long);

            // hidden fields store the value because a disabled/readonly field
            // in WebForms does not post the value back to the page
            $("#txtLatitudeValue").val($("#txtLatitude").val());
            $("#txtLongitudeValue").val($("#txtLongitude").val());

            // move the pin
            var newPin = new VEShape(VEShapeType.Pushpin, new VELatLong(lat, long));
            if (pin != null) map.DeleteShape(pin)
            map.AddShape(newPin);
            pin = newPin;
        }

        function MouseClick(e) {
            if (e.ctrlKey | e.shiftKey) {

                // convert selected point to lat/long
                var ll = map.PixelToLatLong(new VEPixel(e.mapX, e.mapY));
                SetLatLong(Math.round(ll.Latitude * 10000) / 10000, Math.round(ll.Longitude * 10000) / 10000);
            }
        }

    </script>
</asp:Content>
<asp:Content ID="pageContent" ContentPlaceHolderID="MainContent" runat="server">
    <div>
        <p>
            Congratulations! You're on your way to joining thousands of people who have made
            a contribution to Stanford's <a href="http://folding.stanford.edu" target="_blank">Folding@home</a>,
            a distributed computing project leveraging CPU cycles for medical research.</p>
    </div>
    <form id="form1" runat="server">
    <div>
        <asp:Panel ID="pnlAccountForbidden" runat="server" Visible="False" CssClass="errorBox">
            <p>
                It appears the <span class="codeFont">AccountKey</span> for the Windows Azure storage
                account in the <span class="codeFont">DataConnectionString</span> configuration
                setting is not valid.
            </p>
            <p>
                You can modify the connection string via the <a href="http://windows.azure.com" target="portal">
                    Windows Azure Management Portal</a> without redeploying the application. <a href="http://www.c-sharpcorner.com/uploadfile/ae35ca/windows-azure-manually-edit-a-config-file-using-azure-management-portal/"
                        target="updateConfig">See how</a>.
            </p>
            <p>
                Once you've updated the configuration and your hosted service is back in the Ready
                state, refresh this page in your browser.</p>
        </asp:Panel>
        <asp:Panel ID="pnlAccountError" runat="server" Visible="False" CssClass="errorBox">
            <p>
                It appears <span class="codeFont">DataConnectionString</span> configuration setting
                for your Windows Azure storage account is not valid. The current setting is:</p>
            <p class="indentedCode truncate">
                <span class="codeFont">
                    <asp:Literal ID="litConnString" runat="server"></asp:Literal>
                </span>
            </p>
            <p>
                and it should look like</p>
            <p class="indentedCode">
                <span class="codeFont">DefaultEndpointsProtocol=https;AccountName=<strong>{Name}</strong>;AccountKey=<strong>{Base64Key}</strong></span>
            </p>
            <p>
                You can modify the connection string setting via the <a href="http://windows.azure.com"
                    target="portal">Windows Azure Management Portal</a> without redeploying the
                application. <a href="http://www.c-sharpcorner.com/uploadfile/ae35ca/windows-azure-manually-edit-a-config-file-using-azure-management-portal/"
                    target="updateConfig">See how</a>.
            </p>
            <p>
                Once you've updated the configuration and your hosted service is back in the Ready
                state, refresh this page in your browser.</p>
        </asp:Panel>
        <asp:Panel ID="pnlInput" runat="server" Visible="false">
            <p>
                Ready to get started? Three simple steps and you're good to go:
            </p>
            <ol>
                <li>Optionally modify the name field (whatever name you enter will appear on 
                    Stanford&#39;s
                    <a href="http://fah-web.stanford.edu/cgi-bin/main.py?qtype=teampage&teamnum=184157">
                        Folding@home results page</a>).</li>
                <li>Shift+Click on the map to reflect your current location (if it's not already set).</li>
                <li>Press the &quot;Start Folding&quot; button. From then on a status page will show your 
                    ongoing contribution to the effort.</li>
            </ol>
            <p>
                Any questions? <a href="http://distributedcomputing.cloudapp.net/Faq">Check out our
                    FAQ</a>.</p>
            <div id="entryDiv">
                <div id="latlong" class="left_justified">
                    Name:
                    <asp:TextBox ID="txtName" runat="server" CssClass="nameField"></asp:TextBox>
                    Lat/Long:
                    <asp:TextBox ID="txtLatitude" runat="server" CssClass="latlongField" ReadOnly="true" />
                    <asp:TextBox ID="txtLongitude" runat="server" CssClass="latlongField" ReadOnly="true" />
                </div>
                <div class="right_justified">
                    <asp:Button ID="cbStart" runat="server" OnClick="cbStart_Click" Text="Start Folding" CssClass="site-arrowboxcta" />
                </div>
                <asp:HiddenField ID="txtLatitudeValue" runat="server" />
                <asp:HiddenField ID="txtLongitudeValue" runat="server" />
            </div>
            <div id="validatorDiv">
                <div class="left_justified">
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="txtName"
                        CssClass="validator" SetFocusOnError="True">Please enter your name.</asp:RequiredFieldValidator>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtLatitude"
                        CssClass="validator">Shift+Click on map to indicate your location.</asp:RequiredFieldValidator>
                </div>
            </div>
            <div id="myMap" />
        </asp:Panel>
    </div>
    </form>
</asp:Content>
