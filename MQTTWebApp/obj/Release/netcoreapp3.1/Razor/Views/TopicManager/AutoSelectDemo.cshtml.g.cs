#pragma checksum "C:\Users\vantr\Desktop\netzero\MQTTWebApp\Views\TopicManager\AutoSelectDemo.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "24185b61279495f34277c744143a51bd4a5f1fc8"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_TopicManager_AutoSelectDemo), @"mvc.1.0.view", @"/Views/TopicManager/AutoSelectDemo.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "C:\Users\vantr\Desktop\netzero\MQTTWebApp\Views\_ViewImports.cshtml"
using MQTTWebApp;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "C:\Users\vantr\Desktop\netzero\MQTTWebApp\Views\_ViewImports.cshtml"
using MQTTWebApp.Models;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"24185b61279495f34277c744143a51bd4a5f1fc8", @"/Views/TopicManager/AutoSelectDemo.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"14b89f13efb73e62b556075767f46ec67efd2a8c", @"/Views/_ViewImports.cshtml")]
    public class Views_TopicManager_AutoSelectDemo : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("\r\n");
#nullable restore
#line 2 "C:\Users\vantr\Desktop\netzero\MQTTWebApp\Views\TopicManager\AutoSelectDemo.cshtml"
  
    ViewData["Title"] = "AutoSelectDemo";

#line default
#line hidden
#nullable disable
            WriteLiteral("\r\n");
            DefineSection("Links", async() => {
                WriteLiteral("\r\n    <link rel=\"stylesheet\" href=\"http://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css\">\r\n");
                WriteLiteral(@"    <link rel=""stylesheet"" type=""text/css"" href=""https://cdn.datatables.net/1.10.24/css/jquery.dataTables.css"">
    <link rel=""stylesheet"" type=""text/css"" href=""https://cdn.datatables.net/1.10.24/css/jquery.dataTables.min.css"" />
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css"">
");
            }
            );
            WriteLiteral(@"
<h3>Sensor Status Alert</h3>
<div class=""container-fluid"">
    <div class=""text-right mt-3 mb-3 d-fixed"">
        <a href=""./AutoSelectDemo""
           target=""_blank""
           class=""btn btn-success mr-2"">
            <span class=""btn-text""> Refresh </span>
        </a>
    </div>
</div>

<style>
    .table.dataTable {
        font-family: Verdana, Geneva, Tahoma, sans-serif; 
    }
    th.dt-center, td.dt-center {
        text-align: center;
    }
 
</style>


<!--<button id=""button"">Row count</button>-->
<div class=""container mt-2 mb-2"">
    <table id=""warningTable"" class=""display"" style=""width: 100%; font-size: 10px "">
        <thead>
            <tr>
                <th>Id</th>
                <th>Timestamp</th>
                <th>Model</th>
                <th>Campus</th>
                <th>Building</th>
                <th>Room</th>
                <th>SetTemp</th>
                <th>RoomTemp</th>
                <th>Humidity</th>
                <th>FanSpeed<");
            WriteLiteral(@"/th>
                <th>Mode</th>
                <th>Battery</th>
                <th>Power</th>
                <th>Reason</th>
            </tr>
        </thead>
        <tfoot>
            <tr>
                <th>Id</th>
                <th>Time</th>
                <th>Model</th>
                <th>Campus</th>
                <th>Building</th>
                <th>Room</th>
                <th>SetTemp</th>
                <th>RoomTemp</th>
                <th>Humidity</th>
                <th>FanSpeed</th>
                <th>Mode</th>
                <th>Battery</th>
                <th>Power</th>
                <th>Reason</th>

            </tr>
        </tfoot>
    </table>
</div>
<br />

<div class=""row"">
    <div class=""col-10"">
    </div>
    <div class=""col-2"">

        <table border=""0"" width=""100%"" style=""color: royalblue; font-size: 13px;"">
            <tr>
                <th>Code</th>
                <th>Reason</th>
            </tr>
            <tr>");
            WriteLiteral(@"
                <td>BL</td>
                <td>Battery Low</td>
            </tr>
            <tr>
                <td>BD</td>
                <td>Battery Dead</td>
            </tr>
            <tr>
                <td>OH</td>
                <td>Over Heat</td>
            </tr>
            <tr>
                <td>OC</td>
                <td>Over Cool</td>
            </tr>
            <tr>
                <td>LC</td>
                <td>Lost Connection</td>
            </tr>
            <tr>
                <td>TE</td>
                <td>Temperature Error</td>
            </tr>

            <tr>
                <td>AO</td>
                <td>Abnormal Operation</td>
            </tr>
        </table>
    </div>
</div>


<div id=""main"" style=""width: 100%; height: 900px;""></div> 
");
            DefineSection("Scripts", async() => {
                WriteLiteral(@"
    <script src=""https://code.jquery.com/ui/1.12.1/jquery-ui.js""></script>
    <script type=""text/javascript"" charset=""utf8"" src=""https://cdn.datatables.net/1.10.24/js/jquery.dataTables.js""></script>
    <script type=""text/javascript"" charset=""utf8"" src=""https://cdn.datatables.net/colreorder/1.5.4/js/dataTables.colReorder.min.js""></script>
    <script type=""text/javascript"" charset=""utf8"" src=""https://cdn.datatables.net/select/1.3.3/js/dataTables.select.min.js""></script>

    <script type=""text/javascript"" charset=""utf8"" src=""https://cdn.datatables.net/buttons/2.0.0/js/dataTables.buttons.min.js""></script>
    <script type=""text/javascript"" charset=""utf8"" src=""https://cdnjs.cloudflare.com/ajax/libs/jszip/3.1.3/jszip.min.js""></script>
    <script type=""text/javascript"" charset=""utf8"" src=""https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.1.53/pdfmake.min.js""></script>
    <script type=""text/javascript"" charset=""utf8"" src=""https://cdnjs.cloudflare.com/ajax/libs/pdfmake/0.1.53/vfs_fonts.js""></script>
  ");
                WriteLiteral(@"  <script type=""text/javascript"" charset=""utf8"" src=""https://cdn.datatables.net/buttons/2.0.0/js/buttons.html5.min.js""></script>
    <script type=""text/javascript"" charset=""utf8"" src=""https://cdn.datatables.net/buttons/2.0.0/js/buttons.print.min.js""></script>

    <script>
        function getCookie(cookieName) {
            let cookie = {};
            document.cookie.split(';').forEach(function (el) {
                let [key, value] = el.split('=');
                cookie[key.trim()] = value;
            })
            return cookie[cookieName];
        }

        const user = getCookie('user')
        if (user === null || user === '' || user === undefined) {
            location.href = ""/Login"";
        }

        var warningresult;
        var myCallback = function () {
            this.api().columns().every(function () {
                var column = this;
                var select = $('<select><option value=""""></option></select>')
                    .appendTo($(column.footer()).");
                WriteLiteral(@"empty())
                    .on('change', function () {
                        var val = $.fn.dataTable.util.escapeRegex(
                            $(this).val()
                        );
                        column
                            .search(val ? '^' + val + '$' : '', true, false)
                            .draw();
                    });
                column.data().unique().sort().each(function (d, j) {
                    select.append('<option value=""' + d + '"">' + d + '</option>')
                });
            });
        };
        //$.get(`/TopicManager/GetPowerStatistics`).then((res) => {
        $.get(`/TopicManager/GetWarningList`).then((res) => {
            
            warningresult = JSON.parse(res)
            console.log(warningresult)
            var table = $('#warningTable').DataTable();

            table.rows.add(warningresult).draw();

        })
        // scrollX: true,
        //     scrollY: '90vh',
        //    scrollCollapse: true,");
                WriteLiteral(@"

        $.get(`/TopicManager/GetSensorDataCustom?date=1672888939`).then((res) => {
            warningresult = JSON.parse(res)
            console.log(warningresult)

        })
        // scrollX: true,
        //     scrollY: '90vh',
        //    scrollCollapse: true,



        $(document).ready(function () {

            var table = $('#warningTable').removeAttr('width').DataTable({
                ""destroy"": false,
                ""bJQueryUI"": true,
                select: true,
               
                colReorder: true,
                dom: 'Bfrtip',

                buttons: [
                    {
                        extend: 'copy',
                        orientation: 'landscape',
                        pageSize: 'LEGAL',
                        title: 'LTU HVAC Sensor Alert Report',
                        text: 'COPY TABLE'
                    },
                    {
                        extend: 'csv',
                        orientation: 'landsca");
                WriteLiteral(@"pe',
                        pageSize: 'LEGAL',
                        title: 'LTU HVAC Sensor Alert Report',
                        text: 'DOWNLOAD CSV'
                    },
                    {
                        extend: ""pdfHtml5"",
                        orientation: 'landscape',
                        pageSize: 'LEGAL',
                        title: 'LTU HVAC Sensor Alert Report',
                        text: 'DOWNLOAD PDF',
                        messageBottom: ""\r\nCode Reason: \r\n \r\n BL - Battery Low; \r\n BD- Battery Dead; \r\n OH- Over Heat; \r\n OC - Over Cool; \r\n LC - Lost Connection; \r\n TE - Temperature Error; \r\n AO - Abnormal Operation"",
                        customize: function (doc) {
                            doc.content[1].table.body[0].forEach(function (h) {
                                h.fillColor ='#ff6666';
                            });
                        } 
                    }
                ],
                columnDefs: [
   ");
                WriteLiteral(@"                 { width: 80, targets: 0, className: 'dt-body-center'},
                    { width: 150, targets: 1, className: 'dt-body-center'},
                    { width: 80, targets: 2, className: 'dt-body-center' },
                    { width: 80, targets: 3, className: 'dt-body-center'},
                    { width: 80, targets: 4, className: 'dt-body-center'},
                    { width: 100, targets: 5, className: 'dt-body-center'},
                    { width: 70, targets: 6, className: 'dt-body-center' },
                    { width: 70, targets: 7, className: 'dt-body-center' },
                    { width: 80, targets: 8, className: 'dt-body-center' },
                    { width: 80, targets: 9, className: 'dt-body-center' },
                    { width: 80, targets: 10, className: 'dt-body-center' },
                    { width: 80, targets: 11, className: 'dt-body-center' },
                    { width: 80, targets: 12, className: 'dt-body-center' },
                    { widt");
                WriteLiteral(@"h: 200, targets: 13 }
                ],
                fixedColumns: true,

                ""pageLength"": 25,
                ""columns"": [
                    { ""data"": ""SensorId"" },
                    { ""data"": ""Timestamp"" },
                    { ""data"": ""Vendor"" },
                    { ""data"": ""Campus"" },
                    { ""data"": ""Building"" },
                    { ""data"": ""Room"" },
                    { ""data"": ""SetTemperature"" },
                    { ""data"": ""RoomTemperature"" },
                    { ""data"": ""Humidity"" },
                    { ""data"": ""FanSpeed"" },
                    { ""data"": ""Mode"" },
                    { ""data"": ""BatteryPercentage"" },
                    { ""data"": ""PowerOnOff"" },
                    { ""data"": ""Reason"" }
                ]
            });
            $('#warningTable tbody').on('click', 'tr', function () {
                $(this).toggleClass('selected');
            });
            $('#warningTable').DataTable().draw();
            ");
                WriteLiteral(@"$('#button').click(function () {
                var data = table.row('.selected').data();
                alert(table.rows('.selected').data().length + ' row(s) selected');
            });
        });                                                                                                                           //  }, 3000);
                                                                                                                                       //  setInterval(function () {
    </script>


    <script type=""text/javascript"">

        /*   var config = {
               grid: {
                   name: 'grid',
                   url: '/TopicManager/GetWarningList',
                   show: {
                       footer: true,
                       toolbar: true,
                       lineNumbers: true
                   },
                   limit: 50,
                   columns: [
                       { field: 'NickName', text: 'NickName', size: '100px' },
 ");
                WriteLiteral(@"                      { field: 'Vendor', text: 'Vendor', size: '100px', searchable: 'text' },
                       { field: 'Campus', text: 'Campus', size: '100px', searchable: 'text' },
                       { field: 'Building', text: 'Building', size: '100px' }
                   ],
                   onLoad: function (event) {
                       let data = JSON.parse(event.xhr.responseText)
                       event.xhr.responseText = data
                   }
               }
           };

           function refreshGrid(auto) {
               w2ui.grid.autoLoad = auto;
               w2ui.grid.skip(0);
           }

           $(function () {
               $('#main').w2grid(config.grid);
           });*/
    </script>

");
            }
            );
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591
