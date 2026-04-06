<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SuperAdminMembers.aspx.cs" Inherits="prjLibrarySystem.SuperAdminMembers" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Admin Management - Library System</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css?v=2.0" rel="stylesheet"/>
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css?v=2.0" rel="stylesheet"/>
    <style>
        .main-content { padding: 20px; }
        .action-buttons .btn { margin-right: 5px; }
        .card-body { min-height: 580px; max-height: 580px; overflow-y: auto; position: relative; padding-bottom: 0; }
        tr.pagination { display: none !important; }
        .pagination-bar { position: absolute; bottom: 0; left: 0; right: 0; height: 54px; display: flex; align-items: center; justify-content: flex-start; padding: 0 12px; border-top: 1px solid #f0f0f0; background: #fff; border-radius: 0 0 4px 4px; }
        .pagination-bar a, .pagination-bar span { color: #555; display: inline-block; padding: 6px 12px; text-decoration: none !important; border: 1px solid #ddd; margin: 0 2px; border-radius: 6px; font-weight: 500; font-size: 13px; transition: all .25s ease; }
        .pagination-bar a:hover { border-color: #1a237e !important; color: #1a237e !important; transform: translateY(-2px); background-color: transparent !important; }
        .pagination-bar span { background-color: #1a237e; color: white !important; border-color: #1a237e; cursor: default; }
        .btn-sa-save { background: linear-gradient(135deg,#1a237e 0%,#3949ab 100%); border: none; color: white; font-weight: 600; }
        .btn-sa-save:hover { background: linear-gradient(135deg,#283593 0%,#3f51b5 100%); color: white; }
        .modal-form-header { background: linear-gradient(135deg,#1a237e 0%,#283593 100%); color: white; }
        .modal-form-header .btn-close { filter: invert(1); }

        /* Status badge classes used in the grid */
        .status-active   { background-color: #28a745 !important; color: #fff !important; border: none; padding: 4px 8px; min-width: 70px; border-radius: 4px; cursor: pointer; }
        .status-inactive { background-color: #dc3545 !important; color: #fff !important; border: none; padding: 4px 8px; min-width: 70px; border-radius: 4px; cursor: pointer; }
        
        /* Autocomplete dropdown styles */
        .autocomplete-container {
            position: relative;
            width: 100%;
        }
        
        .autocomplete-dropdown {
            position: absolute;
            top: 100%;
            left: 0;
            right: 0;
            background: white;
            border: 1px solid #ddd;
            border-top: none;
            max-height: 200px;
            overflow-y: auto;
            z-index: 1000;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            display: none;
        }
        
        .autocomplete-item {
            padding: 10px 12px;
            cursor: pointer;
            border-bottom: 1px solid #f0f0f0;
            transition: background-color 0.2s;
        }
        
        .autocomplete-item:hover {
            background-color: #f8f9fa;
        }
        
        .autocomplete-item.selected {
            background-color: #e9ecef;
        }
        
        .autocomplete-item strong {
            color: #1a237e;
        }
        
        .autocomplete-no-results {
            padding: 10px 12px;
            color: #6c757d;
            font-style: italic;
        }
    </style>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
</head>
<body>
<form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true"></asp:ScriptManager>
    <asp:HiddenField ID="hfEditingAdminId" runat="server" />

    <div class="container-fluid">
        <div class="row">

            <asp:Literal ID="litSidebar" runat="server"></asp:Literal>

            <main class="col-12 col-md-9 col-lg-10 px-md-4 main-content">
                <div class="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
                    <h1 class="h2">Admin Management</h1>
                    <button type="button" class="btn btn-sa-save"
                        data-bs-toggle="modal" data-bs-target="#adminModal"
                        onclick="clearAdminForm()">
                        <i class="fas fa-plus me-1"></i> Add New Admin
                    </button>
                </div>

                <div class="row mb-3">
                    <div class="col-md-6">
                        <div class="autocomplete-container">
                            <div class="input-group">
                                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control"
                                    placeholder="Search by name, email, or User ID..."></asp:TextBox>
                                <asp:Button ID="btnSearch" runat="server" Text="Search"
                                    CssClass="btn btn-outline-secondary" OnClick="btnSearch_Click" CausesValidation="false" />
                            </div>
                            <div class="autocomplete-dropdown" id="searchSuggestions"></div>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged">
                            <asp:ListItem Value="">All Status</asp:ListItem>
                            <asp:ListItem Value="Active">Active</asp:ListItem>
                            <asp:ListItem Value="Inactive">Inactive</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="card shadow">
                    <div class="card-header py-3">
                        <h6 class="m-0 font-weight-bold" style="color:#1a237e;">Admins Directory</h6>
                    </div>
                    <div class="card-body">
                        <asp:GridView ID="gvAdmins" runat="server" CssClass="table table-hover"
                            AutoGenerateColumns="false" GridLines="None" AllowPaging="true"
                            PageSize="10" OnPageIndexChanging="gvAdmins_PageIndexChanging"
                            OnRowCommand="gvAdmins_RowCommand" DataKeyNames="UserID">
                            <PagerStyle CssClass="pagination" />
                            <Columns>
                                <asp:BoundField DataField="UserID"           HeaderText="User ID" />
                                <asp:BoundField DataField="FullName"         HeaderText="Full Name" />
                                <asp:BoundField DataField="Email"            HeaderText="Email" />
                                <asp:BoundField DataField="RegistrationDate" HeaderText="Created" DataFormatString="{0:yyyy-MM-dd}" />
                                <asp:TemplateField HeaderText="Status">
                                    <ItemTemplate>
                                        <%-- CssClass ternary — works correctly with ASP.NET data binding --%>
                                        <asp:LinkButton ID="btnToggleStatus" runat="server"
                                            CommandName="ToggleStatus"
                                            CommandArgument='<%# Container.DataItemIndex %>'
                                            CssClass='<%# Eval("Status").ToString() == "Active" ? "status-active" : "status-inactive" %>'>
                                            <%# Eval("Status") %>
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Actions">
                                    <ItemTemplate>
                                        <div class="action-buttons">
                                            <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditAdmin"
                                                CommandArgument='<%# Container.DataItemIndex %>' CssClass="btn btn-sm btn-warning">
                                                <i class="fas fa-edit"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteAdmin"
                                                CommandArgument='<%# Container.DataItemIndex %>' CssClass="btn btn-sm btn-danger"
                                                OnClientClick="return confirm('Are you sure you want to delete this admin?');">
                                                <i class="fas fa-trash"></i>
                                            </asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                        <div class="pagination-bar" id="customPager"></div>
                    </div>
                </div>
            </main>
        </div>
    </div>

    <%-- Add / Edit Admin Modal --%>
    <div class="modal fade" id="adminModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header modal-form-header">
                    <h5 class="modal-title">
                        <asp:Label ID="lblModalTitle" runat="server" Text="Add New Admin"></asp:Label>
                    </h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">User ID</label>
                        <asp:TextBox ID="txtUserId" runat="server" CssClass="form-control" placeholder="e.g. EMP-003" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Full Name</label>
                        <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Email</label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Password <small class="text-muted">(leave blank to keep existing)</small></label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" />
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                    <asp:Button ID="btnSaveAdmin" runat="server" Text="Save Admin"
                        CssClass="btn btn-sa-save" OnClick="btnSaveAdmin_Click" />
                </div>
            </div>
        </div>
    </div>

</form>
<script>
    window.addEventListener('DOMContentLoaded', function () {
        var builtIn = document.querySelector('tr.pagination td');
        var custom = document.getElementById('customPager');
        if (builtIn && custom) custom.innerHTML = builtIn.innerHTML;
        initializeAutocomplete();
    });

    // Autocomplete functionality
    var searchTimeout;
    var currentFocus = -1;

    function initializeAutocomplete() {
        var searchInput = document.getElementById('<%= txtSearch.ClientID %>');
        var suggestionsDiv = document.getElementById('searchSuggestions');

        if (!searchInput || !suggestionsDiv) return;

        // Input event handler
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            var query = this.value.trim();
            
            if (query.length < 2) {
                hideSuggestions();
                return;
            }

            searchTimeout = setTimeout(function() {
                fetchSearchSuggestions(query);
            }, 300);
        });

        // Keyboard navigation
        searchInput.addEventListener('keydown', function(e) {
            var items = suggestionsDiv.getElementsByClassName('autocomplete-item');
            
            if (e.keyCode === 40) { // Down arrow
                e.preventDefault();
                currentFocus++;
                addActive(items);
            } else if (e.keyCode === 38) { // Up arrow
                e.preventDefault();
                currentFocus--;
                addActive(items);
            } else if (e.keyCode === 13) { // Enter
                e.preventDefault();
                if (currentFocus > -1 && items[currentFocus]) {
                    items[currentFocus].click();
                }
            } else if (e.keyCode === 27) { // Escape
                hideSuggestions();
            }
        });

        // Hide suggestions when clicking outside
        document.addEventListener('click', function(e) {
            if (!searchInput.contains(e.target) && !suggestionsDiv.contains(e.target)) {
                hideSuggestions();
            }
        });
    }

    function addActive(items) {
        if (!items) return;
        removeActive(items);
        if (currentFocus >= items.length) currentFocus = 0;
        if (currentFocus < 0) currentFocus = items.length - 1;
        items[currentFocus].classList.add('selected');
    }

    function removeActive(items) {
        for (var i = 0; i < items.length; i++) {
            items[i].classList.remove('selected');
        }
    }

    function hideSuggestions() {
        var suggestionsDiv = document.getElementById('searchSuggestions');
        if (suggestionsDiv) {
            suggestionsDiv.style.display = 'none';
        }
        currentFocus = -1;
    }

    function showSuggestions(suggestions) {
        var suggestionsDiv = document.getElementById('searchSuggestions');
        if (!suggestionsDiv) return;

        suggestionsDiv.innerHTML = '';
        
        if (suggestions.length === 0) {
            suggestionsDiv.innerHTML = '<div class="autocomplete-no-results">No suggestions found</div>';
        } else {
            suggestions.forEach(function(suggestion) {
                var item = document.createElement('div');
                item.className = 'autocomplete-item';
                item.innerHTML = suggestion.displayText;
                item.addEventListener('click', function() {
                    selectSuggestion(suggestion.value);
                });
                suggestionsDiv.appendChild(item);
            });
        }
        
        suggestionsDiv.style.display = 'block';
    }

    function selectSuggestion(value) {
        var searchInput = document.getElementById('<%= txtSearch.ClientID %>');
        if (searchInput) {
            searchInput.value = value;
        }
        hideSuggestions();
        
        // Trigger search
        __doPostBack('<%= btnSearch.UniqueID %>', '');
    }

    function fetchSearchSuggestions(query) {
        if (query.length < 2) {
            hideSuggestions();
            return;
        }

        PageMethods.GetSearchSuggestions(query, function(suggestions) {
            showSuggestions(suggestions);
        }, function(error) {
            showSuggestions([]);
        });
    }

    function clearAdminForm() {
        document.getElementById('<%= txtUserId.ClientID %>').value = '';
        document.getElementById('<%= txtFullName.ClientID %>').value = '';
        document.getElementById('<%= txtEmail.ClientID %>').value = '';
        document.getElementById('<%= txtPassword.ClientID %>').value = '';
        document.getElementById('<%= hfEditingAdminId.ClientID %>').value = '';
        document.getElementById('<%= lblModalTitle.ClientID %>').innerText = 'Add New Admin';
    }
</script>
</body>
</html>
