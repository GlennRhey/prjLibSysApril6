<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="prjLibrarySystem.Settings" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Settings - Library Management System</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" rel="stylesheet" />
    <style>
        /* ── Base ── */
        body { background-color: #f4f6f9; }
        .main-content { padding: 24px 28px; }

        /* ── Page header ── */
        .page-header {
            border-bottom: 2px solid #dee2e6;
            padding-bottom: 14px;
            margin-bottom: 22px;
        }

        /* ── Alert message (inline in header) ── */
        .alert-inline {
            padding: 5px 14px;
            font-size: .85rem;
            border-radius: 6px;
            margin: 0;
            white-space: nowrap;
        }

        /* ── Tabs ── */
        .nav-tabs { border-bottom: 2px solid #dee2e6; }
        .nav-tabs .nav-link {
            color: #6c757d;
            font-weight: 500;
            padding: 10px 18px;
            border: none;
            border-bottom: 3px solid transparent;
            border-radius: 0;
            cursor: pointer;
            transition: color .15s, border-color .15s;
        }
        .nav-tabs .nav-link:hover { color: #0d6efd; }
        .nav-tabs .nav-link.active {
            color: #0d6efd;
            font-weight: 600;
            border-bottom: 3px solid #0d6efd;
            background: transparent;
        }

        /* ── Tab content ── */
        .tab-content { padding-top: 24px; }

        /* ── Add-item section ── */
        .add-section {
            background: #fff;
            border: 1px solid #dee2e6;
            border-radius: 8px;
            padding: 16px 20px;
            margin-bottom: 16px;
        }
        .add-section h6 {
            font-size: .88rem;
            font-weight: 600;
            color: #343a40;
            margin: 0 0 12px 0;
        }
        /* Input + button always on one line */
        .add-input-row {
            display: flex;
            gap: 10px;
            align-items: center;
        }
        .add-input-row input {
            flex: 1;
            min-width: 0;
        }
        .add-input-row .btn {
            white-space: nowrap;
            flex-shrink: 0;
        }

        /* ── Table ── */
        .table-wrapper {
            background: #fff;
            border: 1px solid #dee2e6;
            border-radius: 8px;
            overflow: hidden;
        }
        .table { margin-bottom: 0; }
        .table thead th {
            background-color: #343a40;
            color: #fff;
            border-color: #454d55;
            font-size: .83rem;
            font-weight: 600;
            padding: 11px 14px;
            white-space: nowrap;
        }
        .table tbody td {
            vertical-align: middle;
            padding: 9px 14px;
            font-size: .875rem;
            border-color: #f0f0f0;
        }
        .table tbody tr:hover { background-color: #f8f9fa; }

        /* ── Status badges ── */
        .badge-active {
            display: inline-block;
            background-color: #198754;
            color: #fff;
            padding: 3px 10px;
            border-radius: 20px;
            font-size: .73rem;
            font-weight: 600;
            letter-spacing: .3px;
        }
        .badge-inactive {
            display: inline-block;
            background-color: #adb5bd;
            color: #fff;
            padding: 3px 10px;
            border-radius: 20px;
            font-size: .73rem;
            font-weight: 600;
            letter-spacing: .3px;
        }

        /* ── Action buttons ── */
        .action-cell { white-space: nowrap; display: flex; gap: 4px; align-items: center; }
        .action-cell .btn { flex-shrink: 0; }

        /* ── Modal ── */
        .modal-header {
            background-color: #0d6efd;
            color: #fff;
            border-radius: 6px 6px 0 0;
            padding: 12px 16px;
        }
        .modal-header h5 { margin: 0; font-size: .95rem; }
        .modal-header .btn-close { filter: invert(1); }
        .modal-body { padding: 16px 20px; }
        .modal-body label { font-size: .85rem; font-weight: 600; margin-bottom: 6px; display: block; }

        /* ── Borrow Policy Cards ── */
        .policy-cards { display: flex; gap: 20px; flex-wrap: wrap; margin-bottom: 20px; }
        .policy-card {
            flex: 1; min-width: 260px;
            background: #fff; border-radius: 10px;
            border: 1px solid #dee2e6; overflow: hidden;
        }
        .policy-card-header {
            padding: 14px 20px; font-weight: 700; font-size: .9rem;
            letter-spacing: .03em; display: flex; align-items: center; gap: 8px;
        }
        .policy-card-header.student { background: linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%); color: #1565c0; }
        .policy-card-header.teacher { background: linear-gradient(135deg, #f3e5f5 0%, #e1bee7 100%); color: #6a1b9a; }
        .policy-card-body { padding: 20px; }
        .policy-field { margin-bottom: 16px; }
        .policy-field:last-child { margin-bottom: 0; }
        .policy-field label {
            font-size: .82rem; font-weight: 600; color: #555;
            margin-bottom: 6px; display: block;
        }
        .policy-input-wrap { position: relative; }
        .policy-input-wrap input {
            width: 100%; padding: 8px 50px 8px 14px;
            border: 1px solid #ced4da; border-radius: 6px;
            font-size: .95rem; outline: none;
            transition: border-color .15s, box-shadow .15s;
        }
        .policy-input-wrap input:focus {
            border-color: #0d6efd; box-shadow: 0 0 0 .2rem rgba(13,110,253,.15);
        }
        .policy-input-wrap .unit-badge {
            position: absolute; right: 10px; top: 50%; transform: translateY(-50%);
            font-size: .75rem; color: #888; pointer-events: none;
        }


        .policy-save-row { text-align: right; }
        .btn-save-policy {
            background: linear-gradient(135deg, #1565c0 0%, #1976d2 100%);
            border: none; color: #fff; padding: 9px 28px;
            border-radius: 6px; font-weight: 600; font-size: .88rem;
            transition: background .15s, transform .1s;
        }
        .btn-save-policy:hover { background: linear-gradient(135deg, #0d47a1 0%, #1565c0 100%); color: #fff; transform: translateY(-1px); }
        .modal-footer { padding: 10px 16px; }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <div class="container-fluid">
        <div class="row flex-nowrap">

            <!-- ================= SIDEBAR ================= -->
            <asp:Literal ID="litSidebar" runat="server"></asp:Literal>

            <!-- ================= MAIN CONTENT ================= -->
            <main class="col-12 col-md-9 col-lg-10 px-md-4 main-content">

                <!-- Page Header -->
                <div class="d-flex justify-content-between align-items-center page-header">
                    <h1 class="h4 mb-0 fw-bold">
                        <i class="fas fa-cogs me-2 text-primary"></i>Settings
                    </h1>
                    <div class="d-flex align-items-center gap-3">
                        <asp:Label ID="lblMsg" runat="server" CssClass="alert-inline"></asp:Label>

                        <!-- Notification Bell -->
                        <div class="dropdown">
                            <button class="btn btn-sm btn-outline-secondary dropdown-toggle position-relative"
                                    type="button" id="adminNotificationDropdown"
                                    data-bs-toggle="dropdown" aria-expanded="false">
                                <i class="fas fa-bell"></i>
                                <asp:Label ID="adminNotificationBadge" runat="server"
                                    CssClass="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-warning text-dark">0</asp:Label>
                            </button>
                            <ul class="dropdown-menu dropdown-menu-end shadow-sm">
                                <li><h6 class="dropdown-header"><i class="fas fa-bell me-1"></i>System Notifications</h6></li>
                                <li><hr class="dropdown-divider my-1" /></li>
                                <li>
                                    <a class="dropdown-item" href="#" onclick="sendDueDateReminders(); return false;">
                                        <i class="fas fa-envelope me-2 text-primary"></i>Send Due Date Reminders
                                    </a>
                                </li>
                                <li>
                                    <a class="dropdown-item" href="#">
                                        <i class="fas fa-book me-2 text-warning"></i>Low Stock Alerts
                                    </a>
                                </li>
                                <li>
                                    <a class="dropdown-item" href="#">
                                        <i class="fas fa-chart-line me-2 text-success"></i>Usage Report
                                    </a>
                                </li>
                                <li><hr class="dropdown-divider my-1" /></li>
                                <li>
                                    <a class="dropdown-item text-center text-primary fw-semibold" href="#"
                                       data-bs-toggle="modal" data-bs-target="#notificationsModal">
                                        View All Notifications
                                    </a>
                                </li>
                            </ul>
                        </div>
                    </div>
                </div>

                <!-- ================= TABS ================= -->
                <ul class="nav nav-tabs" id="settingsTabs" role="tablist">
                    <li class="nav-item" role="presentation">
                        <button type="button" class="nav-link active" id="tab-categories"
                                data-bs-toggle="tab" data-bs-target="#categories"
                                role="tab" aria-controls="categories" aria-selected="true">
                            <i class="fas fa-tags me-1"></i>Book Categories
                        </button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button type="button" class="nav-link" id="tab-yearlevel"
                                data-bs-toggle="tab" data-bs-target="#yearlevel"
                                role="tab" aria-controls="yearlevel" aria-selected="false">
                            <i class="fas fa-layer-group me-1"></i>Year Level
                        </button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button type="button" class="nav-link" id="tab-course"
                                data-bs-toggle="tab" data-bs-target="#course"
                                role="tab" aria-controls="course" aria-selected="false">
                            <i class="fas fa-graduation-cap me-1"></i>Course
                        </button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button type="button" class="nav-link" id="tab-policy"
                                data-bs-toggle="tab" data-bs-target="#policy"
                                role="tab" aria-controls="policy" aria-selected="false">
                            <i class="fas fa-scroll me-1"></i>Borrow Policy
                        </button>
                    </li>
                </ul>

                <div class="tab-content" id="settingsTabContent">

                    <!-- ===== BOOK CATEGORIES ===== -->
                    <div class="tab-pane fade show active" id="categories"
                         role="tabpanel" aria-labelledby="tab-categories">

                        <div class="add-section">
                            <h6><i class="fas fa-plus-circle me-2 text-primary"></i>Add New Category</h6>
                            <div class="add-input-row">
                                <asp:TextBox ID="txtCategory" runat="server"
                                    CssClass="form-control form-control-sm"
                                    placeholder="e.g. Science, Fiction, History"></asp:TextBox>
                                <asp:Button ID="btnAddCategory" runat="server"
                                    Text="Add Category"
                                    CssClass="btn btn-primary btn-sm"
                                    OnClick="btnAddCategory_Click" />
                            </div>
                        </div>

                        <div class="table-wrapper">
                            <asp:GridView ID="gvCategories" runat="server"
                                AutoGenerateColumns="false"
                                CssClass="table table-hover"
                                GridLines="None" Width="100%">
                                <Columns>
                                    <asp:BoundField DataField="CategoryName" HeaderText="Category Name" />
                                    <asp:TemplateField HeaderText="Status" ItemStyle-Width="110px">
                                        <ItemTemplate>
                                            <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge-active" : "badge-inactive" %>'>
                                                <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="210px" ItemStyle-CssClass="action-cell">
                                        <ItemTemplate>
                                            <asp:Button runat="server" Text="Edit"
                                                CssClass="btn btn-info btn-sm text-white"
                                                CommandArgument='<%# Eval("CategoryID") %>'
                                                OnClick="btnEditCategory_Click" />
                                            <asp:Button runat="server"
                                                Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>'
                                                CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "btn btn-warning btn-sm" : "btn btn-success btn-sm" %>'
                                                CommandArgument='<%# Eval("CategoryID") %>'
                                                OnClick="btnToggleCategory_Click" />
                                            <asp:Button runat="server" Text="Delete"
                                                CssClass="btn btn-danger btn-sm"
                                                CommandArgument='<%# Eval("CategoryID") %>'
                                                OnClick="btnDeleteCategory_Click"
                                                OnClientClick="return confirm('Delete this category?');" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>

                    <!-- ===== YEAR LEVEL ===== -->
                    <div class="tab-pane fade" id="yearlevel"
                         role="tabpanel" aria-labelledby="tab-yearlevel">

                        <div class="add-section">
                            <h6><i class="fas fa-plus-circle me-2 text-primary"></i>Add New Year Level</h6>
                            <div class="add-input-row">
                                <asp:TextBox ID="txtYearLevel" runat="server"
                                    CssClass="form-control form-control-sm"
                                    placeholder="e.g. 1st Year, 2nd Year, 3rd Year"></asp:TextBox>
                                <asp:Button ID="btnAddYear" runat="server"
                                    Text="Add Year Level"
                                    CssClass="btn btn-primary btn-sm"
                                    OnClick="btnAddYear_Click" />
                            </div>
                        </div>

                        <div class="table-wrapper">
                            <asp:GridView ID="gvYearLevel" runat="server"
                                AutoGenerateColumns="false"
                                CssClass="table table-hover"
                                GridLines="None" Width="100%">
                                <Columns>
                                    <asp:BoundField DataField="YearLevelName" HeaderText="Year Level" />
                                    <asp:TemplateField HeaderText="Status" ItemStyle-Width="110px">
                                        <ItemTemplate>
                                            <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge-active" : "badge-inactive" %>'>
                                                <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="210px" ItemStyle-CssClass="action-cell">
                                        <ItemTemplate>
                                            <asp:Button runat="server" Text="Edit"
                                                CssClass="btn btn-info btn-sm text-white"
                                                CommandArgument='<%# Eval("YearLevelID") %>'
                                                OnClick="btnEditYear_Click" />
                                            <asp:Button runat="server"
                                                Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>'
                                                CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "btn btn-warning btn-sm" : "btn btn-success btn-sm" %>'
                                                CommandArgument='<%# Eval("YearLevelID") %>'
                                                OnClick="btnToggleYear_Click" />
                                            <asp:Button runat="server" Text="Delete"
                                                CssClass="btn btn-danger btn-sm"
                                                CommandArgument='<%# Eval("YearLevelID") %>'
                                                OnClick="btnDeleteYear_Click"
                                                OnClientClick="return confirm('Delete this year level?');" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>

                    <!-- ===== COURSE ===== -->
                    <div class="tab-pane fade" id="course"
                         role="tabpanel" aria-labelledby="tab-course">

                        <div class="add-section">
                            <h6><i class="fas fa-plus-circle me-2 text-primary"></i>Add New Course</h6>
                            <div class="add-input-row">
                                <asp:TextBox ID="txtCourse" runat="server"
                                    CssClass="form-control form-control-sm"
                                    placeholder="e.g. BSCS, BSIT, BSED"></asp:TextBox>
                                <asp:Button ID="btnAddCourse" runat="server"
                                    Text="Add Course"
                                    CssClass="btn btn-primary btn-sm"
                                    OnClick="btnAddCourse_Click" />
                            </div>
                        </div>

                        <div class="table-wrapper">
                            <asp:GridView ID="gvCourse" runat="server"
                                AutoGenerateColumns="false"
                                CssClass="table table-hover"
                                GridLines="None" Width="100%">
                                <Columns>
                                    <asp:BoundField DataField="CourseName" HeaderText="Course" />
                                    <asp:TemplateField HeaderText="Status" ItemStyle-Width="110px">
                                        <ItemTemplate>
                                            <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge-active" : "badge-inactive" %>'>
                                                <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="210px" ItemStyle-CssClass="action-cell">
                                        <ItemTemplate>
                                            <asp:Button runat="server" Text="Edit"
                                                CssClass="btn btn-info btn-sm text-white"
                                                CommandArgument='<%# Eval("CourseID") %>'
                                                OnClick="btnEditCourse_Click" />
                                            <asp:Button runat="server"
                                                Text='<%# Convert.ToBoolean(Eval("IsActive")) ? "Deactivate" : "Activate" %>'
                                                CssClass='<%# Convert.ToBoolean(Eval("IsActive")) ? "btn btn-warning btn-sm" : "btn btn-success btn-sm" %>'
                                                CommandArgument='<%# Eval("CourseID") %>'
                                                OnClick="btnToggleCourse_Click" />
                                            <asp:Button runat="server" Text="Delete"
                                                CssClass="btn btn-danger btn-sm"
                                                CommandArgument='<%# Eval("CourseID") %>'
                                                OnClick="btnDeleteCourse_Click"
                                                OnClientClick="return confirm('Delete this course?');" />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>

                    <!-- ===== BORROW POLICY ===== -->
                    <div class="tab-pane fade" id="policy"
                         role="tabpanel" aria-labelledby="tab-policy">

                        <div class="policy-cards">
                            <!-- Student Card -->
                            <div class="policy-card">
                                <div class="policy-card-header student">
                                    <i class="fas fa-user-graduate"></i> Student
                                </div>
                                <div class="policy-card-body">
                                    <div class="policy-field">
                                        <label>Max Books to Borrow</label>
                                        <div class="policy-input-wrap">
                                            <asp:TextBox ID="txtStuMax" runat="server" TextMode="Number" CssClass="" />
                                            <span class="unit-badge">books</span>
                                        </div>
                                    </div>
                                    <div class="policy-field">
                                        <label>Borrow Duration</label>
                                        <div class="policy-input-wrap">
                                            <asp:TextBox ID="txtStuDays" runat="server" TextMode="Number" CssClass="" />
                                            <span class="unit-badge">days</span>
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <!-- Teacher Card -->
                            <div class="policy-card">
                                <div class="policy-card-header teacher">
                                    <i class="fas fa-chalkboard-teacher"></i> Teacher
                                </div>
                                <div class="policy-card-body">
                                    <div class="policy-field">
                                        <label>Max Books to Borrow</label>
                                        <div class="policy-input-wrap">
                                            <asp:TextBox ID="txtTchMax" runat="server" TextMode="Number" CssClass="" />
                                            <span class="unit-badge">books</span>
                                        </div>
                                    </div>
                                    <div class="policy-field">
                                        <label>Borrow Duration</label>
                                        <div class="policy-input-wrap">
                                            <asp:TextBox ID="txtTchDays" runat="server" TextMode="Number" CssClass="" />
                                            <span class="unit-badge">days</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="policy-save-row">
                            <asp:Button ID="btnSavePolicy" runat="server" Text="Save Changes"
                                CssClass="btn-save-policy"
                                OnClick="btnSavePolicy_Click" />
                        </div>
                    </div>

                </div><!-- /tab-content -->

                <!-- ================= EDIT MODAL ================= -->
                <div class="modal fade" id="editModal" tabindex="-1"
                     aria-labelledby="editModalLabel" aria-hidden="true">
                    <div class="modal-dialog modal-dialog-centered modal-sm">
                        <div class="modal-content shadow">
                            <div class="modal-header">
                                <h5 class="modal-title" id="editModalLabel">
                                    <i class="fas fa-edit me-2"></i>Edit Value
                                </h5>
                                <button type="button" class="btn-close"
                                        data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                <asp:HiddenField ID="hfID" runat="server" />
                                <label class="form-label">New Value</label>
                                <asp:TextBox ID="txtEditValue" runat="server"
                                    CssClass="form-control form-control-sm" />
                            </div>
                            <div class="modal-footer py-2">
                                <button type="button" class="btn btn-secondary btn-sm"
                                        data-bs-dismiss="modal">
                                    <i class="fas fa-times me-1"></i>Cancel
                                </button>
                                <asp:Button ID="btnUpdate" runat="server" Text="Update"
                                    CssClass="btn btn-primary btn-sm"
                                    OnClick="btnUpdate_Click" />
                            </div>
                        </div>
                    </div>
                </div>

<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
<script>
    function sendDueDateReminders() { __doPostBack('btnSendReminders', ''); }

    // Persist active tab across postbacks
    (function () {
        var saved = sessionStorage.getItem('settingsActiveTab');
        if (saved) {
            var el = document.querySelector('[data-bs-target="' + saved + '"]');
            if (el) new bootstrap.Tab(el).show();
        }
        document.querySelectorAll('#settingsTabs button').forEach(function (btn) {
            btn.addEventListener('shown.bs.tab', function (e) {
                sessionStorage.setItem('settingsActiveTab', e.target.getAttribute('data-bs-target'));
            });
        });
    })();
</script>

            </main>
        </div>
    </div>
</form>

</body>
</html>
